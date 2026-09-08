using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mafi.Unity.InstancedRendering;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class WorldAssets
{
    private const string Root="Assets/RecursiveIndustry/World";
    private static readonly Color White=new Color(0.79f,0.84f,0.83f,0);
    private static readonly Color Dark=new Color(0.13f,0.19f,0.20f,0);
    private static readonly Color Steel=new Color(0.44f,0.51f,0.54f,0);
    private static readonly Color Teal=new Color(0.10f,0.46f,0.48f,0);
    private static readonly Color Gold=new Color(0.88f,0.62f,0.20f,0);
    private static readonly Color Coral=new Color(0.66f,0.26f,0.21f,0);
    private static readonly Color Green=new Color(0.36f,0.55f,0.31f,0);
    private static readonly Color Light=new Color(0.34f,0.72f,0.67f,1);
    private static readonly Color[] CargoPalette={White,Dark,Steel,Teal,Gold,Coral,Green,Light};
    private static Material surface;
    private static Material fiberParts;
    private static Material fiberFlow;
    private static Material fiberGlass;
    private static string publicRoot;

    [Serializable] private sealed class Spec
    {
        public string key;
        public string category;
        public string family;
        public float width;
        public float height;
        public float depth;
        public float base_height;
        public float origin_z;
        public float origin_x;
        public int tier;
        public float wheel_radius;
        public float bogie_offset;
        public bool animate_wheels;
        public int payload_count;
        public string[] states;
    }
    [Serializable] private sealed class Catalog { public int schema_version; public string art_version; public Spec[] models; }
    [Serializable] private sealed class FileRecord { public string path; public long size_bytes; public string sha256; }
    [Serializable] private sealed class ModelRecord
    {
        public string key;
        public string category;
        public string family;
        public string asset_path;
        public float[] bounds_center;
        public float[] bounds_size;
        public int[] triangles_per_lod;
        public int[] renderers_per_lod;
        public string[] required_paths;
        public string[] required_renderers;
        public string[] animation_states;
        public bool sampled_animation_motion;
        public bool native_rig_contract;
        public bool readable_meshes;
        public bool root_collider;
        public bool native_mesh_material_extraction;
        public string[] cargo_meshes;
        public string[] cargo_textures;
        public string cargo_import_preview;
        public int cargo_import_pixels;
        public string preview_path;
        public int nonblank_pixels;
    }
    [Serializable] private sealed class BundleDependency { public string name; public string[] dependencies; }
    [Serializable] private sealed class AssemblyRecord
    {
        public string key;
        public string[] models;
        public string preview_path;
        public int nonblank_pixels;
        public bool sampled_state_changes;
    }
    [Serializable] private sealed class ControlRecord
    {
        public string key;
        public string property;
        public string[] previews;
        public bool distinct_pixels;
    }
    [Serializable] private sealed class Manifest
    {
        public int schema_version=1;
        public string art_version="0.27.0a";
        public FileRecord catalog;
        public FileRecord[] generators;
        public FileRecord[] bundles;
        public FileRecord[] previews;
        public BundleDependency[] dependencies;
        public string[] asset_paths;
        public ModelRecord[] models;
        public AssemblyRecord[] assemblies;
        public ControlRecord[] controls;
        public string provenance="Original procedural geometry, PBR textures, shaders and animations. No exported game artwork.";
    }

    private sealed class Geometry
    {
        public readonly List<Vector3> vertices=new List<Vector3>();
        public readonly List<int> triangles=new List<int>();
        public readonly List<Vector2> uvs=new List<Vector2>();
        public readonly List<Color> colors=new List<Color>();
        public void Add(ReconstructionAssets.Geometry geometry,Color color,Matrix4x4 transform)
        {
            int offset=vertices.Count;
            vertices.AddRange(geometry.vertices.Select(transform.MultiplyPoint3x4));
            triangles.AddRange(geometry.triangles.Select(index=>index+offset));
            uvs.AddRange(geometry.uvs);
            colors.AddRange(Enumerable.Repeat(color,geometry.vertices.Count));
        }
        public void Box(Vector3 center,Vector3 size,Color color)
        {
            var geometry=new ReconstructionAssets.Geometry();
            geometry.Box(center,size);
            Add(geometry,color,Matrix4x4.identity);
        }
        public void Drum(Vector3 center,float radius,float height,Color color,int sides=16,Quaternion? rotation=null)
        {
            var geometry=new ReconstructionAssets.Geometry();
            geometry.Drum(Vector3.zero,radius,height,sides);
            for(int index=0;index<geometry.uvs.Count;index++)
                geometry.uvs[index]=new Vector2(geometry.vertices[index].x/(2*radius)+0.5f,geometry.vertices[index].z/(2*radius)+0.5f);
            Add(geometry,color,Matrix4x4.TRS(center,rotation??Quaternion.identity,Vector3.one));
            for(int index=0;index<sides;index++)
            {
                float angle=index*Mathf.PI*2/sides;
                float next=(index+1)*Mathf.PI*2/sides;
                var cap=new ReconstructionAssets.Geometry();
                cap.vertices.AddRange(new[] {Vector3.down*height/2,new Vector3(Mathf.Cos(angle)*radius,-height/2,Mathf.Sin(angle)*radius),new Vector3(Mathf.Cos(next)*radius,-height/2,Mathf.Sin(next)*radius)});
                cap.triangles.AddRange(new[] {0,1,2});
                cap.uvs.AddRange(new[] {new Vector2(0.5f,0.5f),Vector2.zero,Vector2.one});
                Add(cap,color,Matrix4x4.TRS(center,rotation??Quaternion.identity,Vector3.one));
            }
        }
        public void Beam(Vector3 from,Vector3 to,float width,float depth,Color color)
        {
            var geometry=new ReconstructionAssets.Geometry();
            geometry.Box(Vector3.zero,new Vector3(width,depth,Vector3.Distance(from,to)));
            Add(geometry,color,Matrix4x4.TRS((from+to)/2,Quaternion.LookRotation(to-from),Vector3.one));
        }
        public Mesh Mesh(string name)
        {
            var mesh=new Mesh {name=name,indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.SetUVs(0,uvs); mesh.SetColors(colors);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            return mesh;
        }
    }

    private static T Persist<T>(T value,string path) where T:UnityEngine.Object
    {
        T existing=AssetDatabase.LoadAssetAtPath<T>(path);
        if(existing==null) { AssetDatabase.CreateAsset(value,path); return value; }
        EditorUtility.CopySerialized(value,existing);
        UnityEngine.Object.DestroyImmediate(value);
        return existing;
    }
    private static Transform EnsurePath(Transform root,string path)
    {
        Transform current=root;
        foreach(string segment in path.Split('/'))
        {
            Transform next=current.Find(segment);
            if(next==null) { next=new GameObject(segment).transform; next.SetParent(current,false); }
            current=next;
        }
        return current;
    }
    private static MeshRenderer Attach(GameObject root,string path,Geometry geometry,Material material,string assetName)
    {
        Transform node=EnsurePath(root.transform,path);
        Mesh mesh=Persist(geometry.Mesh(assetName),Root+"/"+assetName+".asset");
        node.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        MeshRenderer renderer=node.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        renderer.shadowCastingMode=ShadowCastingMode.On;
        return renderer;
    }

    private static GameObject CreateModel(Spec spec,out ModelRecord record)
    {
        if(spec.category=="vehicle" || spec.category=="train" || spec.category=="attachment")
            return CreateDynamic(spec,out record);
        var root=new GameObject(spec.key);
        var levels=new List<LOD>();
        var meshPaths=new List<string>();
        var texturePaths=new List<string>();
        int lodCount=spec.category=="cargo" || spec.category=="fiber"?5:3;
        Material material=surface;
        if(spec.category=="cargo") material=CargoMaterial(spec,texturePaths);
        if(spec.category=="fiber") material=spec.family=="flow_glass"?fiberGlass:spec.family=="flow"?fiberFlow:fiberParts;
        for(int lod=0;lod<lodCount;lod++)
        {
            Geometry geometry=spec.category=="cargo"?Cargo(spec,lod):spec.category=="fiber"?Fiber(spec,lod):Rack(spec,lod);
            if(spec.category=="cargo" || spec.category=="fiber" && spec.family!="flow" && spec.family!="flow_glass") BakeCargoUvs(geometry);
            string name=spec.key+"_lod"+lod;
            MeshRenderer renderer=Attach(root,"visual_"+(lod==0?32:lod==1?16:lod==2?8:lod==3?4:1)+"ppm_LOD"+lod,geometry,material,name);
            levels.Add(new LOD(lod==0?0.22f:lod==1?0.10f:lod==2?0.04f:lod==3?0.016f:0.003f,new Renderer[] {renderer}));
            if(spec.category=="cargo")
            {
                string path=Root+"/"+spec.key+"-LOD"+lod+".obj";
                WriteObj(renderer.GetComponent<MeshFilter>().sharedMesh,path);
                meshPaths.Add(path);
            }
        }
        string[] required=Array.Empty<string>();
        if(spec.category=="rack")
        {
            required=new[] {"DataCenter_Rack1","DataCenter_Rack2","DataCenter_Rack3","DataCenter_Rack4"};
            var shared=new List<Renderer>();
            for(int index=0;index<required.Length;index++)
            {
                var panel=new Geometry();
                panel.Box(new Vector3(0,spec.base_height+0.68f+index*0.76f,spec.origin_z-spec.depth*0.43f),new Vector3(spec.width*0.7f,0.42f,0.025f),Light);
                shared.Add(Attach(root,required[index],panel,surface,spec.key+"_panel"+index));
            }
            levels=levels.Select(level=>new LOD(level.screenRelativeTransitionHeight,level.renderers.Concat(shared).ToArray())).ToList();
        }
        LODGroup group=root.AddComponent<LODGroup>(); group.SetLODs(levels.ToArray()); group.RecalculateBounds();
        Bounds bounds=BoundsOf(root);
        if(spec.category=="cargo" && (bounds.size.x>1.001f || bounds.size.y>1.001f || bounds.size.z>1.001f))
            throw new InvalidOperationException("Cargo exceeds normalized envelope: "+spec.key);
        if(spec.category!="cargo")
        {
            var collider=root.AddComponent<BoxCollider>(); collider.center=bounds.center; collider.size=bounds.size;
        }
        record=new ModelRecord {
            key=spec.key,category=spec.category,family=spec.family,asset_path=Root+"/"+spec.key+".prefab",
            bounds_center=Vector(bounds.center),bounds_size=Vector(bounds.size),
            triangles_per_lod=levels.Select(level=>level.renderers.Sum(renderer=>renderer.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3)).ToArray(),
            renderers_per_lod=levels.Select(level=>level.renderers.Length).ToArray(),required_paths=required,
            required_renderers=required,animation_states=Array.Empty<string>(),
            readable_meshes=true,root_collider=spec.category!="cargo",cargo_meshes=meshPaths.ToArray(),cargo_textures=texturePaths.ToArray(),
            preview_path="art/RecursiveIndustry/World/previews/"+spec.key+".png",
        };
        PrefabUtility.SaveAsPrefabAsset(root,record.asset_path);
        return root;
    }

    private static Geometry Cargo(Spec spec,int lod)
    {
        var geometry=new Geometry();
        Color primary=spec.family=="dossier"?Green:spec.family=="instrument"?Gold:spec.family=="project"?Coral:spec.family=="provisions"?Green:Teal;
        switch(spec.family)
        {
            case "instrument":
                geometry.Box(Vector3.zero,new Vector3(0.92f,0.38f,0.76f),primary);
                if(lod<4) geometry.Box(new Vector3(0,0.22f,0),new Vector3(0.65f,0.08f,0.48f),White);
                if(lod<3) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.43f,0,0),new Vector3(0.12f,0.48f,0.83f),Dark);
                if(lod<2) geometry.Beam(new Vector3(-0.19f,0.32f,0),new Vector3(0.19f,0.32f,0),0.07f,0.06f,Steel);
                if(lod==0) for(int index=0;index<4;index++) geometry.Box(new Vector3(-0.18f+index*0.12f,0.28f,0.15f),new Vector3(0.045f,0.025f,0.06f),Dark);
                break;
            case "dossier":
                geometry.Box(Vector3.zero,new Vector3(0.80f,0.25f,0.96f),primary);
                if(lod<4) geometry.Box(new Vector3(0.035f,0.015f,0),new Vector3(0.69f,0.17f,0.88f),White);
                if(lod<3) geometry.Box(new Vector3(0,0.16f,0),new Vector3(0.83f,0.075f,0.98f),primary);
                if(lod<2) geometry.Box(new Vector3(0.32f,0.18f,0),new Vector3(0.14f,0.075f,0.34f),Gold);
                if(lod==0) for(int index=0;index<4;index++) geometry.Box(new Vector3(0.414f,0.01f,-0.3f+index*0.2f),new Vector3(0.02f,0.13f,0.03f),Steel);
                break;
            case "command":
                geometry.Box(Vector3.zero,new Vector3(0.9f,0.33f,0.78f),Dark);
                if(lod<4) geometry.Box(new Vector3(0,0.18f,0),new Vector3(0.70f,0.07f,0.65f),primary);
                if(lod<3) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.41f,0.03f,0),new Vector3(0.10f,0.4f,0.84f),Steel);
                if(lod<2) geometry.Box(new Vector3(0,0,-0.42f),new Vector3(0.55f,0.15f,0.08f),Gold);
                if(lod==0) for(int index=0;index<5;index++) geometry.Box(new Vector3(-0.2f+index*0.10f,0.23f,0.13f),new Vector3(0.045f,0.025f,0.26f),White);
                break;
            case "project":
                geometry.Box(Vector3.zero,new Vector3(0.84f,0.70f,0.84f),primary);
                if(lod<4) geometry.Box(new Vector3(0,0.39f,0),new Vector3(0.94f,0.08f,0.94f),Dark);
                if(lod<3) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.36f,0,0),new Vector3(0.09f,0.83f,0.96f),Steel);
                if(lod<2) geometry.Box(new Vector3(0,0,-0.45f),new Vector3(0.44f,0.40f,0.05f),White);
                if(lod==0) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(0,0,side*0.36f),new Vector3(0.96f,0.83f,0.09f),Dark);
                break;
            case "calibration":
                geometry.Drum(Vector3.zero,0.48f,0.16f,primary,Math.Max(6,20-lod*3));
                if(lod<4) geometry.Drum(new Vector3(0,0.12f,0),0.36f,0.09f,Gold,Math.Max(6,16-lod*2));
                if(lod<3) geometry.Box(new Vector3(0,0.20f,0),new Vector3(0.34f,0.08f,0.34f),White);
                if(lod<2) geometry.Box(new Vector3(0,0,-0.40f),new Vector3(0.2f,0.25f,0.1f),Steel);
                break;
            case "provisions":
                geometry.Box(Vector3.zero,new Vector3(0.88f,0.64f,0.69f),primary);
                if(lod<4) geometry.Box(new Vector3(0,0.35f,0),new Vector3(0.72f,0.07f,0.65f),White);
                if(lod<3) geometry.Box(new Vector3(0,0.04f,-0.37f),new Vector3(0.65f,0.36f,0.04f),White);
                if(lod<2) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.42f,0,0),new Vector3(0.055f,0.72f,0.74f),Gold);
                if(lod==0) geometry.Box(new Vector3(0,0.42f,0),new Vector3(0.32f,0.06f,0.10f),Dark);
                break;
            default: throw new InvalidOperationException("Unknown cargo family "+spec.family);
        }
        return geometry;
    }

    private static Geometry Rack(Spec spec,int lod)
    {
        var geometry=new Geometry();
        float bottom=spec.base_height;
        float front=spec.origin_z-spec.depth*0.42f;
        geometry.Box(new Vector3(0,bottom+spec.height*0.5f,spec.origin_z),new Vector3(spec.width*0.91f,spec.height*0.96f,spec.depth*0.82f),Dark);
        geometry.Box(new Vector3(0,bottom+spec.height*0.5f,front),new Vector3(spec.width*0.82f,spec.height*0.91f,0.055f),spec.tier==1?White:spec.tier==2?Teal:Steel);
        geometry.Box(new Vector3(0,bottom+0.09f,spec.origin_z),new Vector3(spec.width*0.98f,0.18f,spec.depth*0.94f),Steel);
        int blades=lod==0?spec.tier*4+4:lod==1?4:1;
        for(int index=0;index<blades;index++)
            geometry.Box(new Vector3(0,bottom+0.48f+index*2.78f/Math.Max(1,blades-1),front-0.03f),new Vector3(spec.width*0.77f,0.11f,0.05f),Dark);
        if(lod<2)
            for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.78f,bottom+1.95f,spec.origin_z-0.5f),new Vector3(0.06f,3.42f,0.9f),spec.tier==3?Gold:Steel);
        if(lod==0 && spec.tier>=2)
            for(int index=0;index<spec.tier;index++) geometry.Drum(new Vector3(-0.4f+index*0.8f/(spec.tier-1),bottom+3.7f,spec.origin_z),0.19f,0.08f,Steel,12);
        return geometry;
    }

    private static Material CargoMaterial(Spec spec,List<string> paths)
    {
        string albedo=Root+"/"+spec.key+"-albedo.png";
        string normal=Root+"/cargo-normals.png";
        string smooth=Root+"/cargo-smoothmetal.png";
        var texture=new Texture2D(512,512,TextureFormat.RGBA32,false);
        var pixels=new Color32[512*512];
        var icon=new Texture2D(2,2,TextureFormat.RGBA32,false);
        icon.LoadImage(File.ReadAllBytes(Path.Combine(publicRoot,"art/RecursiveIndustry/UiIcons/exports/"+spec.key+".png")));
        Color32[] iconPixels=icon.GetPixels32();
        for(int row=0;row<512;row++)
            for(int column=0;column<512;column++)
            {
                int tileColumn=column%128;
                int tileRow=row%256;
                int tile=column/128+(row/256)*4;
                Color tint=CargoPalette[tile];
                Color color=tileRow<12 || tileRow>243 || tileColumn<6 || tileColumn>121?tint*0.68f:tint;
                if(tileColumn>=30 && tileColumn<98 && tileRow>=76 && tileRow<180)
                {
                    Color iconColor=iconPixels[((tileRow-76)*icon.height/104)*icon.width+(tileColumn-30)*icon.width/68];
                    color=Color.Lerp(color,iconColor,iconColor.a*0.95f);
                }
                color.a=1;
                pixels[row*512+column]=color;
            }
        texture.SetPixels32(pixels); texture.Apply(); File.WriteAllBytes(albedo,texture.EncodeToPNG());
        texture.SetPixels(Enumerable.Repeat(new Color(0.5f,0.5f,1,1),512*512).ToArray()); texture.Apply(); File.WriteAllBytes(normal,texture.EncodeToPNG());
        texture.SetPixels(Enumerable.Repeat(new Color(0.3f,0,0,0.45f),512*512).ToArray()); texture.Apply(); File.WriteAllBytes(smooth,texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(icon);
        AssetDatabase.ImportAsset(albedo,ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(normal,ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(smooth,ImportAssetOptions.ForceSynchronousImport);
        var material=new Material(surface) {name=spec.key+"_cargo"};
        material.SetColor("_EmissionColor",Color.clear);
        material.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
        material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normal));
        material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(smooth));
        paths.AddRange(new[] {albedo,normal,smooth});
        return Persist(material,Root+"/"+spec.key+"_cargo.mat");
    }

    private static void BakeCargoUvs(Geometry geometry)
    {
        for(int index=0;index<geometry.vertices.Count;index++)
        {
            int tile=Array.IndexOf(CargoPalette,geometry.colors[index]);
            if(tile<0) throw new InvalidOperationException("Cargo color lacks a texture-atlas entry");
            Vector2 uv=geometry.uvs[index];
            geometry.uvs[index]=new Vector2(((tile%4)*128+3+Mathf.Clamp01(uv.x)*122)/512,
                ((tile/4)*256+3+Mathf.Clamp01(uv.y)*250)/512);
            geometry.colors[index]=new Color(1,1,1,0);
        }
    }

    private static void WriteObj(Mesh mesh,string path)
    {
        var builder=new StringBuilder();
        foreach(Vector3 vertex in mesh.vertices) builder.AppendFormat(CultureInfo.InvariantCulture,"v {0:R} {1:R} {2:R}\n",vertex.x,vertex.y,vertex.z);
        foreach(Vector2 uv in mesh.uv) builder.AppendFormat(CultureInfo.InvariantCulture,"vt {0:R} {1:R}\n",uv.x,uv.y);
        foreach(Vector3 normal in mesh.normals) builder.AppendFormat(CultureInfo.InvariantCulture,"vn {0:R} {1:R} {2:R}\n",normal.x,normal.y,normal.z);
        int[] triangles=mesh.triangles;
        for(int index=0;index<triangles.Length;index+=3)
        {
            builder.Append("f");
            for(int offset=0;offset<3;offset++)
            {
                int vertex=triangles[index+offset]+1;
                builder.AppendFormat(CultureInfo.InvariantCulture," {0}/{0}/{0}",vertex);
            }
            builder.Append('\n');
        }
        File.WriteAllText(path,builder.ToString(),new UTF8Encoding(false));
    }
    private static Bounds BoundsOf(GameObject root)
    {
        Renderer[] renderers=root.GetComponentsInChildren<Renderer>(true);
        Bounds bounds=renderers[0].bounds;
        foreach(Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
    private static float[] Vector(Vector3 value) => new[] {value.x,value.y,value.z};

    public static void Build()
    {
        publicRoot=Environment.GetEnvironmentVariable("RI_PUBLIC_ROOT");
        if(string.IsNullOrEmpty(publicRoot)) throw new InvalidOperationException("RI_PUBLIC_ROOT required");
        Catalog catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(publicRoot,"data/world-art.json")));
        if(catalog.schema_version!=1 || catalog.models.Select(spec=>spec.key).Distinct().Count()!=catalog.models.Length)
            throw new InvalidOperationException("Invalid original world-art catalog");
        Directory.CreateDirectory(Root); Directory.CreateDirectory(Path.Combine(publicRoot,"art/RecursiveIndustry/World/previews"));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Shader shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/WorldSurface.shader");
        if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("World shader failed");
        surface=Persist(new Material(shader) {name="world_surface"},Root+"/world_surface.mat");
        CreateFiberMaterials();
        CreateDynamicMaterials();
        var models=new List<ModelRecord>();
        foreach(Spec spec in catalog.models)
        {
            GameObject root=CreateModel(spec,out ModelRecord model);
            models.Add(model); UnityEngine.Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string path in AssetDatabase.FindAssets("",new[]{Root}).Select(AssetDatabase.GUIDToAssetPath))
        {
            if(Directory.Exists(path)) continue;
            AssetImporter importer=AssetImporter.GetAtPath(path);
            importer.assetBundleName="recursiveindustry_world";
            if(importer is ModelImporter model)
            {
                model.isReadable=true; model.globalScale=1; model.importNormals=ModelImporterNormals.Import;
                model.importTangents=ModelImporterTangents.CalculateMikk; model.importCameras=false; model.importLights=false;
                model.addCollider=false; model.generateSecondaryUV=false; model.materialImportMode=ModelImporterMaterialImportMode.None;
            }
            if(importer is TextureImporter texture)
            {
                bool normal=path.EndsWith("-normals.png",StringComparison.Ordinal);
                texture.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
                texture.sRGBTexture=!normal && !path.EndsWith("-smoothmetal.png",StringComparison.Ordinal);
                texture.alphaSource=TextureImporterAlphaSource.FromInput; texture.maxTextureSize=512; texture.mipmapEnabled=true;
                texture.wrapMode=path.Contains("fiber-") || path.Contains("track-treads")?TextureWrapMode.Repeat:TextureWrapMode.Clamp; texture.filterMode=FilterMode.Bilinear;
                texture.textureCompression=TextureImporterCompression.Uncompressed;
            }
            importer.SaveAndReimport();
        }
        if(!AssetBundlesHelpers.TryBuildAssetBundles("AssetBundles",string.Empty,BuildTarget.StandaloneWindows64,cleanBuild:false,
            out var bundleManifest,out var bundleToDlc,out var error)) throw new InvalidOperationException("World bundles: "+error);
        string[] all=bundleManifest.GetAllAssetBundles();
        string shared=all.Single(name=>name.StartsWith("world_",StringComparison.Ordinal));
        string[] own=new[]{shared}.Concat(models.Select(model=>all.Single(name=>name.Length==model.key.Length+5 && name.StartsWith(model.key+"_",StringComparison.Ordinal)))).ToArray();
        foreach(string name in own)
        {
            if(bundleManifest.GetAllDependencies(name).Any(dependency=>!own.Contains(dependency))) throw new InvalidOperationException("Foreign world-art dependency: "+name);
            File.Copy(Path.Combine("AssetBundles",name),Path.Combine(publicRoot,"mods/RecursiveIndustry/AssetBundles/"+name),true);
        }
        string playerManifest=Path.Combine(publicRoot,"mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest");
        string[] old=File.ReadAllLines(playerManifest).Where(line=>!string.IsNullOrWhiteSpace(line) && !line.StartsWith("+",StringComparison.Ordinal)).ToArray();
        string[] roots=old.Concat(own).Distinct().OrderBy(name=>name,StringComparer.Ordinal).ToArray();
        if(!AssetBundlesHelpers.TryGenerateMafiBundlesManifest(bundleManifest,roots,isDlc:false,validateBundle:_=>true,"AssetBundles/mafi_bundles.manifest",out error))
            throw new InvalidOperationException("World manifest: "+error);
        File.Copy("AssetBundles/mafi_bundles.manifest",playerManifest,true);
        AssetBundle[] loaded=own.Select(name=>AssetBundle.LoadFromFile(Path.GetFullPath(Path.Combine("AssetBundles",name)))).ToArray();
        if(loaded.Any(bundle=>bundle==null)) throw new InvalidOperationException("World bundle failed loading");
        foreach(ModelRecord model in models)
        {
            GameObject prefab=loaded.Select(bundle=>bundle.LoadAsset<GameObject>(model.asset_path)).FirstOrDefault(asset=>asset!=null);
            if(prefab==null || prefab.GetComponentsInChildren<MeshFilter>(true).Any(filter=>filter.sharedMesh==null || !filter.sharedMesh.isReadable))
                throw new InvalidOperationException("Unreadable or missing final world model: "+model.key);
            foreach(string path in model.required_paths)
                if(prefab.transform.Find(path)==null) throw new InvalidOperationException("Missing native model node "+model.key+":"+path);
            foreach(string path in model.required_renderers)
                if(prefab.transform.Find(path)?.GetComponent<MeshRenderer>()==null) throw new InvalidOperationException("Missing native controlled renderer "+model.key+":"+path);
            if(model.root_collider && prefab.GetComponent<Collider>()==null) throw new InvalidOperationException("Missing root collider: "+model.key);
            if(model.category=="vehicle" || model.category=="train" || model.category=="attachment") ValidateDynamic(prefab,model);
            if(model.category=="fiber")
            {
                for(int lod=0;lod<model.triangles_per_lod.Length;lod++)
                {
                    GameObject node=prefab.GetComponentsInChildren<MeshRenderer>().Single(renderer=>renderer.name.EndsWith("LOD"+lod,StringComparison.Ordinal)).gameObject;
                    if(!InstancingUtils.TryGetSharedMatMesh(node,true,out Mesh mesh,out Material material,out string extractionError))
                        throw new InvalidOperationException("Native Fiber extraction failed: "+model.key+" "+extractionError);
                    var copied=new Material(Shader.Find("Standard"));
                    InstancingUtils.CopyTextures(copied,material);
                    if(copied.GetTexture("_MainTex")==null || mesh.vertexCount==0)
                        throw new InvalidOperationException("Native Fiber texture/mesh contract missing: "+model.key);
                    UnityEngine.Object.DestroyImmediate(copied);
                }
                model.native_mesh_material_extraction=true;
            }
            foreach(string path in model.cargo_meshes)
            {
                GameObject imported=loaded.Select(bundle=>bundle.LoadAsset<GameObject>(path)).FirstOrDefault(asset=>asset!=null);
                if(imported==null || imported.GetComponentsInChildren<MeshFilter>().Any(filter=>filter.sharedMesh==null || !filter.sharedMesh.isReadable))
                    throw new InvalidOperationException("Cargo import cannot provide a readable mesh: "+path);
            }
            if(model.category=="cargo")
            {
                GameObject imported=loaded.Select(bundle=>bundle.LoadAsset<GameObject>(model.cargo_meshes[0])).FirstOrDefault(asset=>asset!=null);
                GameObject cargo=UnityEngine.Object.Instantiate(imported);
                try
                {
                    Material material=prefab.GetComponentsInChildren<MeshRenderer>().Single(renderer=>renderer.name.EndsWith("LOD0",StringComparison.Ordinal)).sharedMaterial;
                    foreach(MeshRenderer renderer in cargo.GetComponentsInChildren<MeshRenderer>()) renderer.sharedMaterial=material;
                    Bounds importedBounds=BoundsOf(cargo);
                    if(importedBounds.size.x>1.001f || importedBounds.size.y>1.001f || importedBounds.size.z>1.001f)
                        throw new InvalidOperationException("Imported cargo mesh escaped its normalized envelope: "+model.key);
                    model.cargo_import_preview="art/RecursiveIndustry/World/previews/imported-"+model.key+".png";
                    model.cargo_import_pixels=Render(cargo,Path.Combine(publicRoot,model.cargo_import_preview));
                }
                finally { UnityEngine.Object.DestroyImmediate(cargo); }
            }
            GameObject instance=UnityEngine.Object.Instantiate(prefab);
            try { model.nonblank_pixels=Render(instance,Path.Combine(publicRoot,model.preview_path)); }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        AssemblyRecord[] assemblies=RenderAssemblies(loaded,models);
        ControlRecord[] controls=RenderNativeControls(loaded);
        var manifest=new Manifest {
            catalog=Identity("data/world-art.json"),generators=Directory.GetFiles(Path.Combine(publicRoot,"art/RecursiveIndustry/World/Editor"),"*.cs").OrderBy(path=>path,StringComparer.Ordinal).Select(path=>Identity(path.Substring(publicRoot.Length+1).Replace('\\','/'))).ToArray(),
            bundles=own.Select(name=>Identity("mods/RecursiveIndustry/AssetBundles/"+name)).ToArray(),
            previews=models.Select(model=>Identity(model.preview_path)).Concat(models.Where(model=>model.category=="cargo").Select(model=>Identity(model.cargo_import_preview))).Concat(assemblies.Select(assembly=>Identity(assembly.preview_path))).Concat(controls.SelectMany(control=>control.previews).Select(Identity)).ToArray(),models=models.ToArray(),assemblies=assemblies,controls=controls,
            dependencies=own.Select(name=>new BundleDependency {name=name,dependencies=bundleManifest.GetAllDependencies(name)}).ToArray(),
            asset_paths=loaded.SelectMany(bundle=>bundle.GetAllAssetNames()).Distinct().OrderBy(path=>path,StringComparer.Ordinal).ToArray(),
        };
        foreach(AssetBundle bundle in loaded.Reverse()) bundle.Unload(true);
        File.WriteAllText(Path.Combine(publicRoot,"art/RecursiveIndustry/World/asset-manifest.json"),JsonUtility.ToJson(manifest,true)+"\n");
        Debug.Log("RI_WORLD_ART_COMPLETE models="+models.Count+" bundles="+own.Length);
    }

    private static int Render(GameObject root,string path)
    {
        foreach(LODGroup group in root.GetComponentsInChildren<LODGroup>()) group.ForceLOD(0);
        Bounds bounds=BoundsOf(root);
        var cameraObject=new GameObject("WorldPreviewCamera");
        Camera camera=cameraObject.AddComponent<Camera>(); camera.orthographic=true;
        camera.orthographicSize=Mathf.Max(bounds.size.x*0.63f,bounds.size.z*0.8f,bounds.size.y*0.7f);
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(0.19f,0.215f,0.225f);
        float distance=Mathf.Max(30,bounds.size.magnitude*3);
        camera.transform.position=bounds.center+new Vector3(1.15f,0.90f,-1.35f).normalized*distance;
        camera.transform.LookAt(bounds.center); camera.nearClipPlane=0.02f; camera.farClipPlane=distance*4;
        var lightObject=new GameObject("WorldPreviewSun"); Light light=lightObject.AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.15f; light.transform.rotation=Quaternion.Euler(48,-35,0);
        RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(0.52f,0.56f,0.6f);
        var target=new RenderTexture(840,600,24); camera.targetTexture=target; camera.Render(); RenderTexture.active=target;
        var texture=new Texture2D(840,600,TextureFormat.RGB24,false); texture.ReadPixels(new Rect(0,0,840,600),0,0); texture.Apply();
        Color32[] pixels=texture.GetPixels32(); Color32 background=pixels[0];
        int count=pixels.Count(color=>Math.Abs(color.r-background.r)+Math.Abs(color.g-background.g)+Math.Abs(color.b-background.b)>35);
        if(count<5000 || pixels.Count(color=>color.r>210 && color.b>210 && color.g<60)>pixels.Length/100)
            throw new InvalidOperationException("Blank or broken final world-model preview: "+root.name);
        File.WriteAllBytes(path,texture.EncodeToPNG());
        RenderTexture.active=null; camera.targetTexture=null;
        UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(lightObject);
        return count;
    }
    private static FileRecord Identity(string relative)
    {
        byte[] bytes=File.ReadAllBytes(Path.Combine(publicRoot,relative));
        using(SHA256 hash=SHA256.Create()) return new FileRecord {path=relative,size_bytes=bytes.Length,sha256=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-",string.Empty)};
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ReconstructionAssets
{
    private const string AssetRoot = "Assets/RecursiveIndustry/Reconstruction";
    private static readonly Color[] Palette = {
        new Color(0.76f, 0.80f, 0.79f), new Color(0.14f, 0.20f, 0.22f),
        new Color(0.08f, 0.29f, 0.33f), new Color(0.20f, 0.55f, 0.42f),
        new Color(0.92f, 0.64f, 0.22f), new Color(0.34f, 0.40f, 0.40f),
        new Color(0.37f, 0.48f, 0.30f), new Color(0.53f, 0.40f, 0.28f),
    };
    private static readonly string[] MaterialNames = { "concrete", "frame", "glass", "civic", "safety", "roof", "planting", "timber" };
    private static Material[] materials;

    [Serializable] private sealed class ModelRecord
    {
        public string name;
        public string asset_path;
        public float[] center;
        public float[] size;
        public int vertices;
        public int triangles;
        public int renderer_count;
        public bool root_collider;
        public int emissive_renderers;
        public string preview_path;
    }

    [Serializable] private sealed class FileRecord
    {
        public string path;
        public long size_bytes;
        public string sha256;
    }

    [Serializable] private sealed class Manifest
    {
        public int schema_version = 1;
        public string bundle_name;
        public FileRecord bundle;
        public FileRecord[] bundles;
        public string[] dependencies;
        public string[] asset_paths;
        public ModelRecord[] models;
        public FileRecord[] icons;
        public string origin = "Original procedural geometry, shader, and bitmap icons; no game assets included.";
    }

    internal sealed class Geometry
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<int> triangles = new List<int>();
        public readonly List<Vector2> uvs = new List<Vector2>();

        public void Box(Vector3 center, Vector3 size)
        {
            Vector3 half = size * 0.5f;
            Vector3[] corners = {
                center + new Vector3(-half.x,-half.y,-half.z), center + new Vector3(half.x,-half.y,-half.z),
                center + new Vector3(half.x,half.y,-half.z), center + new Vector3(-half.x,half.y,-half.z),
                center + new Vector3(-half.x,-half.y,half.z), center + new Vector3(half.x,-half.y,half.z),
                center + new Vector3(half.x,half.y,half.z), center + new Vector3(-half.x,half.y,half.z),
            };
            int[][] faces = {
                new[] {0,3,2,1}, new[] {5,6,7,4}, new[] {4,7,3,0},
                new[] {1,2,6,5}, new[] {3,7,6,2}, new[] {4,0,1,5},
            };
            foreach (int[] face in faces)
            {
                int start = vertices.Count;
                foreach (int index in face) vertices.Add(corners[index]);
                uvs.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
                triangles.AddRange(new[] { start, start+1, start+2, start, start+2, start+3 });
            }
        }

        public void Drum(Vector3 center, float radius, float height, int sides = 16)
        {
            for (int index = 0; index < sides; index++)
            {
                float angle = index * Mathf.PI * 2 / sides;
                float next = (index + 1) * Mathf.PI * 2 / sides;
                Vector3 lower = center + new Vector3(Mathf.Cos(angle)*radius, -height/2, Mathf.Sin(angle)*radius);
                Vector3 upper = center + new Vector3(Mathf.Cos(next)*radius, -height/2, Mathf.Sin(next)*radius);
                int start = vertices.Count;
                vertices.AddRange(new[] { lower, lower + Vector3.up*height, upper + Vector3.up*height, upper,
                    center + Vector3.up*height/2, lower + Vector3.up*height, upper + Vector3.up*height });
                for (int point = 0; point < 7; point++) uvs.Add(Vector2.zero);
                triangles.AddRange(new[] { start, start+1, start+2, start, start+2, start+3, start+4, start+6, start+5 });
            }
        }
    }

    private sealed class Building
    {
        public readonly Geometry[] groups = Enumerable.Range(0, Palette.Length).Select(_ => new Geometry()).ToArray();
        public void Box(int material, float x, float y, float z, float width, float height, float depth)
        {
            groups[material].Box(new Vector3(x,y,z), new Vector3(width,height,depth));
        }

        public void Windows(float left, float right, float bottom, float top, float front, float spacing, float width, float height)
        {
            for (float position = left; position <= right; position += spacing)
                for (float level = bottom; level <= top; level += height + 0.55f)
                    Box(2, position, level, front, width, height, 0.08f);
        }

        public GameObject Save(string name)
        {
            var root = new GameObject(name);
            for (int index = 0; index < groups.Length; index++)
            {
                Geometry geometry = groups[index];
                if (geometry.vertices.Count == 0) continue;
                var mesh = new Mesh { name = name + "_" + MaterialNames[index], indexFormat = IndexFormat.UInt32 };
                mesh.SetVertices(geometry.vertices);
                mesh.SetTriangles(geometry.triangles, 0);
                mesh.SetUVs(0, geometry.uvs);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                Mesh persistent = SaveAsset(mesh, AssetRoot + "/" + mesh.name + ".asset");
                var part = new GameObject(MaterialNames[index]);
                part.transform.SetParent(root.transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = persistent;
                MeshRenderer renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials[index];
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = bounds.size;
            PrefabUtility.SaveAsPrefabAsset(root, AssetRoot + "/" + name + ".prefab");
            return root;
        }
    }

    private static T SaveAsset<T>(T value, string path) where T : UnityEngine.Object
    {
        T previous = AssetDatabase.LoadAssetAtPath<T>(path);
        if (previous == null)
        {
            AssetDatabase.CreateAsset(value, path);
            return value;
        }
        EditorUtility.CopySerialized(value, previous);
        UnityEngine.Object.DestroyImmediate(value);
        return previous;
    }

    public static void Build()
    {
        string publicRoot = Environment.GetEnvironmentVariable("RI_PUBLIC_ROOT");
        if (string.IsNullOrEmpty(publicRoot)) throw new InvalidOperationException("RI_PUBLIC_ROOT is required");
        string artRoot = Path.Combine(publicRoot, "art/RecursiveIndustry/Reconstruction");
        Directory.CreateDirectory(AssetRoot);
        Directory.CreateDirectory(Path.Combine(artRoot, "previews"));
        Directory.CreateDirectory(Path.Combine(artRoot, "icons"));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetRoot + "/IndustrialSurface.shader");
        if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Original surface shader failed");
        materials = new Material[Palette.Length];
        for (int index = 0; index < materials.Length; index++)
        {
            var material = new Material(shader) { name = MaterialNames[index] };
            material.SetColor("_Color", Palette[index]);
            material.SetFloat("_Metallic", index == 1 || index == 5 ? 0.6f : 0.05f);
            material.SetFloat("_Glossiness", index == 2 ? 0.75f : 0.28f);
            if (index == 2)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.12f, 0.18f, 0.15f));
            }
            materials[index] = SaveAsset(material, AssetRoot + "/" + MaterialNames[index] + ".mat");
        }

        var models = new List<ModelRecord>();
        models.Add(Finish(Integration().Save("electronics_integration"), artRoot, new Vector3(14,6,8)));
        models.Add(Finish(CivicCenter().Save("civic_model_center"), artRoot, new Vector3(12,8,10)));
        models.Add(Finish(Commons().Save("knowledge_commons"), artRoot, new Vector3(22,8,14)));
        models.Add(Finish(Planetary().Save("planetary_coordination_center"), artRoot, new Vector3(76,32,30)));
        string[] iconNames = { "civic_knowledge_stream", "civic_model_center", "knowledge_commons" };
        var iconRecords = new List<FileRecord>();
        for (int index = 0; index < iconNames.Length; index++)
        {
            byte[] png = Icon(index).EncodeToPNG();
            string path = AssetRoot + "/" + iconNames[index] + ".png";
            File.WriteAllBytes(path, png);
            string publicPath = "art/RecursiveIndustry/Reconstruction/icons/" + iconNames[index] + ".png";
            File.WriteAllBytes(Path.Combine(publicRoot, publicPath), png);
            iconRecords.Add(Identity(publicRoot, publicPath));
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in AssetDatabase.FindAssets("", new[] { AssetRoot }).Select(AssetDatabase.GUIDToAssetPath))
        {
            if (Directory.Exists(path)) continue;
            AssetImporter importer = AssetImporter.GetAtPath(path);
            importer.assetBundleName = "recursiveindustry_reconstruction";
            if (importer is TextureImporter texture)
            {
                texture.textureType = TextureImporterType.Sprite;
                texture.spriteImportMode = SpriteImportMode.Single;
                texture.alphaIsTransparency = true;
                texture.mipmapEnabled = false;
                texture.maxTextureSize = 512;
                texture.textureCompression = TextureImporterCompression.Uncompressed;
            }
            importer.SaveAndReimport();
        }
        if (!AssetBundlesHelpers.TryBuildAssetBundles("AssetBundles", string.Empty,
                BuildTarget.StandaloneWindows64, cleanBuild: false, out var manifest, out var bundleToDlc, out var error))
            throw new InvalidOperationException("Reconstruction asset build: " + error);
        string bundleName = manifest.GetAllAssetBundles().Single(name => name.StartsWith("reconstruction_", StringComparison.Ordinal));
        string[] dependencies = manifest.GetAllDependencies(bundleName);
        if (dependencies.Length != 0) throw new InvalidOperationException("Reconstruction bundle must be dependency-free");
        var ownBundles = new List<string> { bundleName };
        foreach (ModelRecord model in models)
            ownBundles.Add(manifest.GetAllAssetBundles().Single(name => name.StartsWith(model.name + "_", StringComparison.Ordinal)));
        foreach (string name in ownBundles)
        {
            if (manifest.GetAllDependencies(name).Any(dependency => !ownBundles.Contains(dependency)))
                throw new InvalidOperationException("Unexpected model dependency: " + name);
            File.Copy(Path.Combine("AssetBundles", name), Path.Combine(publicRoot, "mods/RecursiveIndustry/AssetBundles/" + name), true);
        }
        string[] roots = new[] { "producticons_84e1", "cartridge_c874", "uiicons_5287" }.Concat(ownBundles).ToArray();
        if (!AssetBundlesHelpers.TryGenerateMafiBundlesManifest(manifest, roots,
                isDlc: false, validateBundle: _ => true, "AssetBundles/mafi_bundles.manifest", out error))
            throw new InvalidOperationException("Reconstruction bundle manifest: " + error);
        File.Copy("AssetBundles/mafi_bundles.manifest", Path.Combine(publicRoot, "mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest"), true);
        var loaded = ownBundles.Select(name => AssetBundle.LoadFromFile(Path.GetFullPath(Path.Combine("AssetBundles", name)))).ToArray();
        if (loaded.Any(bundle => bundle == null)) throw new InvalidOperationException("Built bundle cannot be loaded");
        var record = new Manifest {
            bundle_name = bundleName, bundle = Identity(publicRoot, "mods/RecursiveIndustry/AssetBundles/" + bundleName), dependencies = dependencies,
            bundles = ownBundles.Select(name => Identity(publicRoot, "mods/RecursiveIndustry/AssetBundles/" + name)).ToArray(),
            asset_paths = loaded.SelectMany(bundle => bundle.GetAllAssetNames()).Distinct().OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            models = models.ToArray(), icons = iconRecords.ToArray(),
        };
        foreach (ModelRecord model in models)
        {
            GameObject finalPrefab = loaded.Select(bundle => bundle.LoadAsset<GameObject>(model.asset_path)).FirstOrDefault(prefab => prefab != null);
            if (finalPrefab == null || finalPrefab.GetComponentsInChildren<MeshFilter>().Any(filter => filter.sharedMesh == null))
                throw new InvalidOperationException("Prefab missing from final bundle: " + model.asset_path);
            if (finalPrefab.GetComponent<Collider>() == null)
                throw new InvalidOperationException("Root collider missing: " + model.asset_path);
            if (!finalPrefab.GetComponentsInChildren<MeshRenderer>().Any(renderer =>
                renderer.sharedMaterial.IsKeywordEnabled("_EMISSION") && renderer.sharedMaterial.HasProperty("_EmissionColor")))
                throw new InvalidOperationException("Native emission contract missing: " + model.asset_path);
            GameObject instance = UnityEngine.Object.Instantiate(finalPrefab);
            try
            {
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
                Render(instance, bounds, Path.Combine(publicRoot, model.preview_path));
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        foreach (AssetBundle bundle in loaded.Reverse()) bundle.Unload(true);
        File.WriteAllText(Path.Combine(artRoot, "asset-manifest.json"), JsonUtility.ToJson(record, true) + "\n");
        Debug.Log("RI_RECONSTRUCTION_ASSETS_COMPLETE bundle=" + bundleName + " models=4 icons=3");
    }

    private static Building Integration()
    {
        var model = new Building();
        model.Box(5,0,0.16f,0,13.6f,0.32f,5.8f);
        model.Box(0,0,1.8f,0,12.8f,3.4f,5.6f);
        model.Box(1,0,3.55f,0,13.2f,0.25f,5.9f);
        model.Box(0,0,4.15f,0.3f,5.7f,1.1f,3.4f);
        model.Windows(-5.4f,5.4f,2.25f,2.25f,-2.83f,1.35f,1.05f,1.3f);
        model.Box(4,0,3.15f,-2.87f,12.8f,0.18f,0.15f);
        model.Box(2,0,4.3f,-1.42f,5.2f,0.65f,0.1f);
        for (int side = -1; side <= 1; side += 2)
        {
            model.Box(1,side*6.48f,1.15f,0,0.18f,2.1f,2.2f);
            model.Box(4,side*6.52f,2.3f,0,0.18f,0.2f,2.4f);
            model.groups[5].Drum(new Vector3(side*4.6f,3.94f,0.7f),0.75f,0.45f);
            model.Box(1,side*4.6f,4.23f,0.7f,1.05f,0.12f,0.14f);
        }
        model.Box(3,-4.7f,0.8f,3.15f,1.5f,1.5f,0.55f);
        model.Box(5,0,0.3f,3.2f,8.8f,0.6f,0.6f);
        return model;
    }

    private static Building CivicCenter()
    {
        var model = new Building();
        model.Box(5,0,0.18f,0,11.7f,0.36f,9.7f);
        model.Box(0,-1.3f,2.3f,0,8.4f,4.2f,8.8f);
        model.Box(1,3.7f,2.05f,0,2.7f,3.8f,8.8f);
        model.Box(0,-1.3f,4.7f,0.5f,7.6f,0.4f,8);
        model.Windows(-4.3f,1.6f,1.55f,3.3f,-4.46f,1.6f,1.25f,1.2f);
        for (int index=0; index<7; index++) model.Box(3,5.1f,2.05f,-3.5f+index,0.18f,3.5f,0.3f);
        model.Box(2,-1.4f,5.25f,0.6f,4.8f,0.65f,5.9f);
        model.Box(5,0.8f,6.0f,2.3f,2.6f,0.6f,2.8f);
        model.groups[5].Drum(new Vector3(-2.8f,5.4f,2.5f),0.8f,0.65f);
        model.Box(4,5.7f,1.1f,0,0.25f,1.8f,1.2f);
        return model;
    }

    private static Building Commons()
    {
        var model = new Building();
        model.Box(5,0,0.15f,0,21.5f,0.3f,13.5f);
        model.Box(0,-7.2f,2.45f,0,5.7f,4.6f,12.3f);
        model.Box(0,7.2f,2.45f,0,5.7f,4.6f,12.3f);
        model.Box(0,0,2.45f,4.3f,14.5f,4.6f,3.6f);
        model.Box(3,-7.2f,4.95f,0,5.9f,0.45f,12.5f);
        model.Box(3,7.2f,4.95f,0,5.9f,0.45f,12.5f);
        model.Box(3,0,4.95f,4.3f,14.5f,0.45f,3.8f);
        model.Windows(-9.1f,-5.5f,2.2f,2.2f,-6.22f,1.2f,0.9f,2.4f);
        model.Windows(5.4f,9.1f,2.2f,2.2f,-6.22f,1.2f,0.9f,2.4f);
        model.Windows(-5.5f,5.5f,2.2f,2.2f,2.45f,1.5f,1.15f,2.4f);
        model.Box(7,0,0.38f,-0.8f,7.5f,0.35f,5.4f);
        for (int side=-1; side<=1; side+=2)
        {
            model.Box(0,side*3.0f,0.6f,-4.7f,1.4f,0.6f,1.4f);
            model.Box(6,side*3.0f,1.4f,-4.7f,1.2f,1.05f,1.2f);
            model.Box(7,side*2.8f,0.72f,0.4f,1.8f,0.25f,0.55f);
        }
        model.Box(2,0,5.4f,4.2f,7.4f,0.45f,2.4f);
        return model;
    }

    private static Building Planetary()
    {
        var model = new Building();
        model.Box(5,0,0.35f,0,68,0.7f,27.5f);
        model.Box(0,0,2.8f,0,62,5.1f,23);
        model.Box(0,-20,9.4f,2,18,8.5f,18);
        model.Box(0,20,9.4f,2,18,8.5f,18);
        model.Box(1,0,15,2,18,20,18);
        model.Box(0,0,26.2f,2,21,2.4f,20);
        model.Box(3,0,27.6f,2,21.4f,0.55f,20.4f);
        model.Box(5,0,28.3f,2,12,0.9f,12);
        model.Windows(-29,29,2.7f,2.7f,-11.6f,2.4f,1.6f,2.7f);
        model.Windows(-27,-13,7.6f,12.2f,-7.1f,2.5f,1.65f,2.2f);
        model.Windows(13,27,7.6f,12.2f,-7.1f,2.5f,1.65f,2.2f);
        model.Windows(-7.2f,7.2f,7.1f,24,-7.1f,2.4f,1.7f,2.4f);
        for(int index=0;index<7;index++) model.Box(0,-8.4f+index*2.8f,15.2f,-7.25f,0.22f,20.5f,0.25f);
        model.Box(3,0,5.7f,-8.6f,32,0.8f,6.6f);
        for(int side=-1;side<=1;side+=2)
        {
            model.Box(2,side*20,14,2,14,0.45f,13.5f);
            model.Box(4,side*31.2f,2.1f,0,0.22f,3.4f,4.0f);
            model.Box(6,side*28,6.2f,5,4.6f,0.9f,6.0f);
        }
        return model;
    }

    private static ModelRecord Finish(GameObject root, string artRoot, Vector3 maximum)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        if (bounds.min.y < -0.01f || bounds.size.x > maximum.x || bounds.size.y > maximum.y || bounds.size.z > maximum.z)
            throw new InvalidOperationException("Model exceeds its declared envelope: " + root.name + " " + bounds);
        string preview = "art/RecursiveIndustry/Reconstruction/previews/" + root.name + ".png";
        var meshes = root.GetComponentsInChildren<MeshFilter>().Select(filter => filter.sharedMesh).ToArray();
        var record = new ModelRecord {
            name = root.name, asset_path = AssetRoot + "/" + root.name + ".prefab",
            center = new[] { bounds.center.x, bounds.center.y, bounds.center.z },
            size = new[] { bounds.size.x, bounds.size.y, bounds.size.z },
            vertices = meshes.Sum(mesh => mesh.vertexCount), triangles = meshes.Sum(mesh => mesh.triangles.Length/3),
            renderer_count = renderers.Length, preview_path = preview,
            root_collider = root.GetComponent<Collider>() != null,
            emissive_renderers = renderers.Count(renderer => renderer.sharedMaterial.IsKeywordEnabled("_EMISSION")),
        };
        UnityEngine.Object.DestroyImmediate(root);
        return record;
    }

    private static void Render(GameObject root, Bounds bounds, string path)
    {
        var cameraObject = new GameObject("ReconstructionPreviewCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(bounds.size.z, bounds.size.x*0.65f, bounds.size.y*1.15f) * 0.76f;
        camera.backgroundColor = new Color(0.13f,0.16f,0.17f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = bounds.center + new Vector3(1.1f,0.85f,-1.35f).normalized * 180;
        camera.transform.LookAt(bounds.center);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 400;
        var lightObject = new GameObject("ReconstructionPreviewLight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(45,-30,0);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f,0.58f,0.6f);
        var target = new RenderTexture(1000,720,24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var pixels = new Texture2D(1000,720,TextureFormat.RGB24,false);
        pixels.ReadPixels(new Rect(0,0,1000,720),0,0);
        pixels.Apply();
        Color32[] colors = pixels.GetPixels32();
        Color32 background = colors[0];
        int changed = colors.Count(color => Math.Abs(color.r-background.r)+Math.Abs(color.g-background.g)+Math.Abs(color.b-background.b)>30);
        if (changed < colors.Length/50) throw new InvalidOperationException("Blank model preview: " + root.name);
        File.WriteAllBytes(path, pixels.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(pixels);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        UnityEngine.Object.DestroyImmediate(lightObject);
    }

    private static Texture2D Icon(int kind)
    {
        var texture = new Texture2D(512,512,TextureFormat.RGBA32,false);
        var pixels = new Color32[512*512];
        Color32 ink = new Color(0.83f,0.97f,0.89f);
        Color32 accent = new Color(0.26f,0.72f,0.53f);
        Action<Vector2,Vector2,float,Color32> line = (start,end,width,color) => {
            Vector2 direction = end-start;
            for(int row=Mathf.Max(0,(int)(Mathf.Min(start.y,end.y)-width));row<Mathf.Min(512,Mathf.Max(start.y,end.y)+width);row++)
                for(int column=Mathf.Max(0,(int)(Mathf.Min(start.x,end.x)-width));column<Mathf.Min(512,Mathf.Max(start.x,end.x)+width);column++)
                {
                    Vector2 offset = new Vector2(column,row)-start;
                    float fraction = Mathf.Clamp01(Vector2.Dot(offset,direction)/Mathf.Max(1,direction.sqrMagnitude));
                    if ((offset-direction*fraction).sqrMagnitude < width*width) pixels[row*512+column]=color;
                }
        };
        if(kind==0 || kind==2)
        {
            line(new Vector2(100,150),new Vector2(100,350),16,ink);
            line(new Vector2(412,150),new Vector2(412,350),16,ink);
            line(new Vector2(100,350),new Vector2(256,315),16,ink);
            line(new Vector2(412,350),new Vector2(256,315),16,ink);
            line(new Vector2(100,150),new Vector2(256,115),16,ink);
            line(new Vector2(412,150),new Vector2(256,115),16,ink);
            line(new Vector2(256,115),new Vector2(256,315),16,accent);
        }
        if(kind==1)
        {
            line(new Vector2(100,115),new Vector2(100,330),17,ink);
            line(new Vector2(412,115),new Vector2(412,330),17,ink);
            line(new Vector2(100,115),new Vector2(412,115),17,ink);
            line(new Vector2(100,330),new Vector2(256,415),18,ink);
            line(new Vector2(256,415),new Vector2(412,330),18,ink);
            for(int column=0;column<3;column++) line(new Vector2(165+column*90,190),new Vector2(165+column*90,280),18,accent);
        }
        if(kind==0) for(int index=0;index<3;index++) line(new Vector2(165+index*90,407),new Vector2(165+index*90,420),19,accent);
        if(kind==2)
        {
            line(new Vector2(115,405),new Vector2(397,405),17,accent);
            line(new Vector2(150,405),new Vector2(150,440),17,accent);
            line(new Vector2(362,405),new Vector2(362,440),17,accent);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private static FileRecord Identity(string root, string relative)
    {
        byte[] payload = File.ReadAllBytes(Path.Combine(root,relative));
        using (SHA256 hash = SHA256.Create()) return new FileRecord {
            path = relative, size_bytes = payload.Length,
            sha256 = BitConverter.ToString(hash.ComputeHash(payload)).Replace("-",string.Empty),
        };
    }
}
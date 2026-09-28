using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

public static class AllBuildingAssets
{
    private const string Root = "Assets/RecursiveIndustry/Buildings";
    private static readonly Color Concrete = new Color(0.71f,0.75f,0.74f);
    private static readonly Color White = new Color(0.88f,0.90f,0.87f);
    private static readonly Color Metal = new Color(0.24f,0.30f,0.31f);
    private static readonly Color Steel = new Color(0.51f,0.59f,0.60f);
    private static readonly Color Teal = new Color(0.16f,0.50f,0.53f);
    private static readonly Color Red = new Color(0.66f,0.24f,0.18f);
    private static readonly Color Green = new Color(0.35f,0.52f,0.28f);
    private static readonly Color Yellow = new Color(0.91f,0.66f,0.18f);
    private static readonly Color Copper = new Color(0.64f,0.45f,0.29f);
    private static readonly Color Glass = new Color(0.22f,0.43f,0.46f);
    private static Material surface;
    private static Material lighting;
    private static Material signMaterial;
    private static Material glazingMaterial;

    [Serializable] private sealed class Spec
    {
        public string key;
        public string member;
        public string category;
        public string family;
        public float width;
        public float depth;
        public float height;
        public float origin_z;
        public int tier;
        public bool sign;
        public bool animated;
        public bool universal;
    }
    [Serializable] private sealed class Catalog { public int schema_version; public Spec[] models; }
    [Serializable] private sealed class FileRecord { public string path; public long size_bytes; public string sha256; }
    [Serializable] private sealed class ModelRecord
    {
        public string key;
        public string family;
        public string member;
        public string category;
        public string asset_path;
        public float[] envelope;
        public float[] bounds_center;
        public float[] bounds_size;
        public int[] triangles_per_lod;
        public int[] renderers_per_lod;
        public bool root_collider;
        public bool native_emission;
        public bool recipe_sign;
        public bool native_animation;
        public bool sampled_animation_motion;
        public string geometry_sha256;
        public string preview_path;
        public int nonblank_pixels;
    }
    [Serializable] private sealed class BundleDependency { public string name; public string[] dependencies; }
    [Serializable] private sealed class Manifest
    {
        public int schema_version = 1;
        public int new_model_count;
        public int total_building_count = 55;
        public FileRecord catalog;
        public FileRecord generator;
        public FileRecord[] bundles;
        public FileRecord[] previews;
        public BundleDependency[] dependencies;
        public string[] asset_paths;
        public ModelRecord[] models;
        public string provenance = "Original procedural building geometry, shaders and materials; no redistributed game artwork.";
    }

    private sealed class MeshData
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<int> triangles = new List<int>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<Color> colors = new List<Color>();
        public void Append(ReconstructionAssets.Geometry shape, Matrix4x4 transform, Color color)
        {
            int offset = vertices.Count;
            vertices.AddRange(shape.vertices.Select(transform.MultiplyPoint3x4));
            triangles.AddRange(shape.triangles.Select(index => index+offset));
            uvs.AddRange(shape.uvs);
            colors.AddRange(Enumerable.Repeat(color, shape.vertices.Count));
        }
        public Mesh Mesh(string name)
        {
            var mesh = new Mesh { name=name, indexFormat=IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0,uvs);
            mesh.SetColors(colors);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    private sealed class Design
    {
        public readonly Spec spec;
        public readonly int detail;
        public readonly MeshData solid = new MeshData();
        public readonly MeshData glow = new MeshData();
        public readonly MeshData glazing = new MeshData();
        private float Width => spec.width * 0.96f;
        private float Depth => spec.depth * 0.94f;
        private float Height => spec.height * 0.94f;
        public Design(Spec value, int level) { spec=value; detail=level; }
        public Vector3 Point(float centerX,float centerY,float centerZ) => new Vector3(centerX*Width,centerY*Height,centerZ*Depth+spec.origin_z);
        public void Box(Color color,float centerX,float centerY,float centerZ,float width,float height,float depth,bool emissive=false)
        {
            var shape = new ReconstructionAssets.Geometry();
            shape.Box(Point(centerX,centerY,centerZ),new Vector3(width*Width,height*Height,depth*Depth));
            (emissive?glow:solid).Append(shape,Matrix4x4.identity,color);
        }
        public void Tank(Color color,float centerX,float bottom,float centerZ,float radius,float height)
        {
            var shape = new ReconstructionAssets.Geometry();
            shape.Drum(Point(centerX,bottom+height/2,centerZ),radius*Mathf.Min(Width,Depth),height*Height,detail==0?16:detail==1?10:6);
            solid.Append(shape,Matrix4x4.identity,color);
        }
        public void Beam(Color color,Vector3 from,Vector3 to,float thickness)
        {
            var shape = new ReconstructionAssets.Geometry();
            shape.Box(Vector3.zero,new Vector3(thickness,thickness,Vector3.Distance(from,to)));
            solid.Append(shape,Matrix4x4.TRS((from+to)/2,Quaternion.LookRotation(to-from),Vector3.one),color);
        }
        public void Pipe(Color color,float centerX,float centerY,float centerZ,float length,float radius,bool alongX=true)
        {
            var shape = new ReconstructionAssets.Geometry();
            shape.Drum(Vector3.zero,radius*Mathf.Min(Width,Depth),length*(alongX?Width:Depth),detail==0?12:detail==1?8:6);
            solid.Append(shape,Matrix4x4.TRS(Point(centerX,centerY,centerZ),Quaternion.Euler(alongX?0:90,0,alongX?90:0),Vector3.one),color);
        }
        public void Windows(float centerX,float centerZ,float width,float bottom,float top)
        {
            int count = detail==0?Math.Max(3,(int)(width*Width/1.4f)):detail==1?4:1;
            int floors = detail==2?1:Math.Max(1,(int)((top-bottom)*Height/2));
            for(int row=0;row<floors;row++)
                for(int index=0;index<count;index++)
                    Box(Glass,centerX-width/2+(index+0.5f)*width/count,bottom+(row+0.5f)*(top-bottom)/floors,centerZ,
                        width/count*0.68f,(top-bottom)/floors*0.65f,0.006f,true);
        }
        public void ServicePanel()
        {
            Box(Glass,0.29f,0.23f,-0.365f,0.14f,0.10f,0.009f,true);
            if(detail==0) Box(Yellow,0.29f,0.15f,-0.37f,0.14f,0.014f,0.01f);
        }
        public void Hall(Color color,float height=0.46f,float width=0.82f,float depth=0.70f)
        {
            Box(color,0,height/2+0.03f,0,width,height,depth);
            Box(Metal,0,height+0.055f,0,width+0.03f,0.035f,depth+0.035f);
            Windows(0,-depth/2-0.006f,width*0.78f,0.17f,height-0.01f);
            if(detail<2)
            {
                Box(Yellow,-0.43f,0.25f,0,0.015f,0.02f,0.35f);
                Box(Metal,-0.42f,0.13f,0,0.02f,0.20f,0.23f);
            }
        }
        public void RoofEquipment(float baseHeight,int count=2)
        {
            for(int index=0;index<count;index++)
            {
                float position=-0.25f+index*0.5f/Math.Max(1,count-1);
                Box(White,position,baseHeight+0.055f,0.20f,0.16f,0.11f,0.20f);
                Tank(Metal,position,baseHeight+0.11f,0.20f,0.055f,0.015f);
                if(detail==0) for(int fin=0;fin<4;fin++) Box(Steel,position-0.052f+fin*0.035f,baseHeight+0.14f,0.2f,0.01f,0.02f,0.12f);
            }
        }
        public void Gantry(float height,Color color)
        {
            for(int side=-1;side<=1;side+=2)
                Box(color,side*0.30f,height/2,0.05f,0.04f,height,0.07f);
            Box(color,0,height,0.05f,0.67f,0.055f,0.08f);
            Box(Metal,0.12f,height-0.08f,0.05f,0.1f,0.12f,0.07f);
            if(detail<2) Beam(Steel,Point(0.12f,height-0.13f,0.05f),Point(0.12f,height-0.35f,0.05f),0.07f);
        }
        public void Pane(Vector3 first,Vector3 second,Vector3 third,Vector3 fourth)
        {
            var shape=new ReconstructionAssets.Geometry();
            shape.vertices.AddRange(new[] {first,second,third,fourth});
            shape.triangles.AddRange(new[] {0,1,2,0,2,3});
            shape.uvs.AddRange(new[] {Vector2.zero,Vector2.right,Vector2.one,Vector2.up});
            glazing.Append(shape,Matrix4x4.identity,Color.white);
        }
    }

    private static void Shape(Design design)
    {
        Spec spec = design.spec;
        int detail = design.detail;
        if(spec.family=="greenhouse") { Greenhouse(design); return; }
        if(spec.family=="poultry") { Poultry(design); return; }
        if(spec.family=="office") { Office(design); return; }
        if(spec.family=="care") { Care(design); return; }
        design.Box(Concrete,0,0.02f,0,0.96f,0.04f,0.94f);
        if(spec.key=="local_deployment_controller")
        {
            design.Box(White,0,0.31f,0.02f,0.70f,0.54f,0.56f);
            design.Box(Metal,0,0.60f,0.02f,0.76f,0.04f,0.61f);
            design.Box(Glass,0,0.40f,-0.268f,0.46f,0.22f,0.01f,true);
            design.Box(Teal,-0.21f,0.20f,-0.274f,0.13f,0.08f,0.014f,true);
            design.Tank(Steel,0.23f,0.63f,0.17f,0.048f,0.13f);
            if(detail<2) for(int slot=0;slot<4;slot++) design.Box(Metal,-0.23f+slot*0.15f,0.67f,0,0.04f,0.10f,0.32f);
            design.ServicePanel();
            return;
        }
        if(spec.key=="alloy_glass_works")
        {
            design.Box(White,0,0.16f,0.12f,0.85f,0.24f,0.52f);
            design.Tank(Red,-0.26f,0.29f,0.12f,0.13f,0.31f);
            design.Tank(Metal,-0.26f,0.60f,0.12f,0.14f,0.05f);
            design.Tank(Steel,0.27f,0.29f,0.14f,0.095f,0.47f);
            design.Tank(Teal,0.27f,0.74f,0.14f,0.085f,0.08f);
            design.Box(Metal,0,0.15f,-0.28f,0.82f,0.15f,0.18f);
            design.Box(Glass,0.10f,0.24f,-0.28f,0.55f,0.025f,0.14f,true);
            if(detail<2) design.Pipe(Copper,0,0.57f,0.36f,0.70f,0.025f);
            design.ServicePanel();
            return;
        }
        if(spec.key=="process_water_chiller")
        {
            design.Box(Metal,0,0.09f,0.06f,0.84f,0.10f,0.66f);
            for(int side=-1;side<=1;side+=2)
            {
                design.Box(White,side*0.23f,0.32f,0.12f,0.33f,0.40f,0.54f);
                design.Tank(Metal,side*0.23f,0.54f,0.12f,0.13f,0.07f);
                design.Tank(Steel,side*0.23f,0.61f,0.12f,0.055f,0.02f);
                if(detail<2) for(int fin=0;fin<6;fin++) design.Box(Steel,side*0.23f,0.17f+fin*0.05f,-0.157f,0.28f,0.012f,0.012f);
            }
            design.Pipe(Teal,0,0.14f,-0.32f,0.78f,0.034f);
            design.Pipe(Copper,0,0.25f,-0.32f,0.78f,0.025f);
            design.ServicePanel();
            return;
        }
        switch(spec.family)
        {
            case "assembly":
            case "robotics":
            case "manufacturing":
            case "fabrication":
                design.Hall(spec.family=="fabrication"?Steel:White,0.42f);
                design.Box(Teal,-0.24f,0.63f,0.12f,0.20f,0.33f,0.32f);
                design.Box(Glass,-0.24f,0.66f,-0.048f,0.16f,0.17f,0.009f,true);
                if(spec.family=="fabrication" || spec.family=="robotics") design.Gantry(0.81f,Yellow);
                else design.RoofEquipment(0.48f,spec.key=="accelerator_works"?3:2);
                if(spec.key=="autonomous_construction_nexus")
                {
                    design.Box(Red,0.20f,0.62f,-0.12f,0.22f,0.27f,0.27f);
                    design.Pipe(Steel,0.19f,0.77f,-0.1f,0.34f,0.06f,false);
                }
                if(detail==0) for(int index=0;index<4;index++) design.Box(Metal,0.15f+index*0.05f,0.13f,-0.40f,0.03f,0.1f,0.09f);
                break;
            case "archive":
            case "laboratory":
            case "assurance":
                design.Hall(White,0.43f);
                design.Box(Teal,-0.24f,0.60f,0.12f,0.25f,0.31f,0.30f);
                design.Box(White,0.24f,0.60f,0.12f,0.25f,0.31f,0.30f);
                design.Box(Glass,0,0.51f,-0.05f,0.26f,0.1f,0.32f,true);
                if(spec.family=="laboratory") design.Tank(Steel,0.29f,0.04f,0.38f,0.065f,0.5f);
                if(spec.family=="archive") for(int index=0;index<(detail==0?6:3);index++) design.Box(Copper,-0.3f+index*0.6f/(detail==0?5:2),0.34f,-0.358f,0.025f,0.4f,0.025f);
                if(spec.family=="assurance") design.Box(Teal,0,0.60f,0.15f,0.20f,0.13f,0.20f);
                break;
            case "compute":
            case "integration":
            case "gateway":
                design.Hall(White,spec.family=="gateway"?0.32f:0.42f,0.84f,0.72f);
                int banks=spec.key=="recursive_integration_array"?3:2;
                for(int bank=0;bank<banks;bank++)
                    design.Box(Metal,-0.26f+bank*0.52f/Math.Max(1,banks-1),0.59f,0.12f,0.20f,0.25f,0.52f);
                if(detail<2) for(int index=0;index<6;index++) design.Box(Teal,-0.34f+index*0.136f,0.68f,0.1f,0.025f,0.10f,0.43f);
                if(spec.family=="gateway") { design.Tank(Steel,-0.25f,0.40f,0.08f,0.08f,0.40f); design.Tank(Teal,-0.25f,0.8f,0.08f,0.12f,0.03f); }
                if(spec.family=="integration")
                {
                    design.Box(Teal,0,0.68f,-0.12f,0.18f,0.39f,0.29f);
                    design.Box(Glass,0,0.72f,-0.272f,0.14f,0.21f,0.01f,true);
                    design.Box(Steel,0,0.75f,0.16f,0.65f,0.065f,0.12f);
                }
                break;
            case "cleanroom":
                design.Hall(White,0.57f,0.82f,0.70f);
                design.RoofEquipment(0.65f,3);
                for(int index=0;index<3;index++) design.Tank(Steel,-0.31f+index*0.20f,0.08f,0.40f,0.045f,0.60f);
                break;
            case "pilot":
                design.Hall(White,0.32f,0.80f,0.62f);
                for(int index=0;index<3;index++) design.Tank(index==1?Copper:Steel,-0.28f+index*0.28f,0.37f,0.10f,0.09f,0.40f);
                if(detail<2) design.Pipe(Yellow,0,0.70f,0.14f,0.72f,0.016f);
                break;
            case "crusher":
                design.Box(Metal,-0.20f,0.3f,0.12f,0.40f,0.48f,0.48f);
                design.Box(Yellow,-0.20f,0.55f,0.12f,0.48f,0.12f,0.54f);
                design.Box(Concrete,0.24f,0.18f,-0.12f,0.34f,0.26f,0.62f);
                design.Beam(Steel,design.Point(-0.15f,0.42f,-0.1f),design.Point(0.35f,0.17f,-0.2f),0.9f);
                if(detail<2) design.Gantry(0.80f,Metal);
                break;
            case "kiln":
                design.Pipe(Steel,0,0.34f,0.12f,0.72f,0.14f);
                for(int side=-1;side<=1;side+=2) design.Box(Red,side*0.24f,0.14f,0.12f,0.12f,0.26f,0.37f);
                design.Tank(Concrete,0.30f,0.04f,-0.27f,0.09f,0.76f);
                design.Tank(Steel,0.11f,0.04f,-0.27f,0.09f,0.64f);
                if(detail<2) design.Pipe(Copper,-0.23f,0.52f,0.1f,0.3f,0.035f,false);
                break;
            case "arc":
            case "blast":
                design.Tank(Red,-0.12f,0.06f,0.10f,0.25f,0.52f);
                design.Tank(Metal,-0.12f,0.58f,0.10f,0.26f,0.055f);
                design.Box(White,0.28f,0.18f,-0.2f,0.27f,0.26f,0.50f);
                if(spec.family=="arc") for(int index=0;index<3;index++) design.Tank(Steel,-0.27f+index*0.14f,0.59f,0.10f,0.038f,0.28f);
                else { design.Tank(Metal,0.26f,0.28f,0.19f,0.06f,0.61f); design.Pipe(Copper,0.09f,0.61f,0.17f,0.35f,0.05f); }
                if(detail<2) design.Gantry(0.88f,Yellow);
                break;
            case "electrolysis":
                for(int index=0;index<3;index++)
                {
                    design.Box(White,-0.26f+index*0.26f,0.22f,0.05f,0.19f,0.32f,0.72f);
                    design.Box(Metal,-0.26f+index*0.26f,0.4f,0.05f,0.17f,0.05f,0.70f);
                    if(detail==0) for(int plate=0;plate<7;plate++) design.Box(Copper,-0.26f+index*0.26f,0.47f,-0.23f+plate*0.09f,0.17f,0.10f,0.025f);
                }
                design.Gantry(0.72f,Yellow);
                break;
            case "casting":
                design.Box(White,0,0.20f,0.15f,0.84f,0.30f,0.35f);
                design.Pipe(Metal,0,0.18f,-0.19f,0.72f,0.08f);
                design.Tank(Red,-0.24f,0.35f,0.13f,0.14f,0.28f);
                design.Gantry(0.79f,Yellow);
                break;
            case "refinery":
            case "gas":
            case "chemical":
            case "scrubber":
                int columns=spec.family=="chemical"?4:3;
                for(int index=0;index<columns;index++)
                {
                    float position=-0.28f+index*0.56f/(columns-1);
                    float height=0.84f-index*0.13f;
                    design.Tank(index==1?Teal:Steel,position,0.04f,0.18f,0.075f,height);
                    if(detail<2) for(int ring=1;ring<=3;ring++) design.Tank(Metal,position,height*ring/4,0.18f,0.088f,0.018f);
                }
                design.Box(White,0,0.16f,-0.22f,0.81f,0.24f,0.24f);
                design.Pipe(Copper,0,0.30f,-0.12f,0.69f,0.034f);
                if(spec.family=="gas") design.Pipe(White,0.18f,0.36f,-0.27f,0.32f,0.11f);
                if(spec.family=="scrubber") design.Tank(Concrete,0.29f,0.04f,-0.1f,0.12f,0.52f);
                break;
            case "medical":
            case "food":
            case "packing":
                design.Hall(White,spec.family=="medical"?0.48f:0.39f);
                design.Box(spec.family=="medical"?Teal:Green,0,0.44f,-0.358f,0.79f,0.045f,0.012f);
                design.RoofEquipment(spec.family=="medical"?0.53f:0.45f,3);
                if(spec.family=="food") for(int index=0;index<2;index++) design.Tank(White,-0.3f+index*0.22f,0.46f,0.23f,0.07f,0.34f);
                if(spec.family=="packing") for(int index=0;index<3;index++) design.Box(Metal,-0.22f+index*0.22f,0.17f,-0.37f,0.14f,0.2f,0.06f);
                break;
            case "fermentation":
            case "digester":
                for(int side=-1;side<=1;side+=2)
                {
                    design.Tank(spec.family=="digester"?Green:White,side*0.21f,0.04f,0.10f,0.18f,0.48f);
                    design.Tank(Metal,side*0.21f,0.53f,0.10f,0.13f,0.08f);
                }
                design.Box(White,0,0.14f,-0.31f,0.75f,0.20f,0.20f);
                design.Pipe(Yellow,0,0.31f,-0.13f,0.68f,0.02f);
                if(spec.family=="fermentation") design.Tank(Steel,-0.31f,0.52f,0.23f,0.032f,0.27f);
                break;
            case "water":
                for(int side=-1;side<=1;side+=2)
                {
                    design.Tank(White,side*0.22f,0.05f,0.14f,0.20f,0.18f);
                    design.Tank(Teal,side*0.22f,0.22f,0.14f,0.17f,0.015f);
                    if(detail<2) design.Beam(Steel,design.Point(side*0.22f-0.15f,0.28f,0.14f),design.Point(side*0.22f+0.15f,0.28f,0.14f),0.12f);
                }
                design.Box(White,0,0.20f,-0.25f,0.82f,0.3f,0.24f);
                design.RoofEquipment(0.38f,2);
                break;
            case "desalination":
                for(int index=0;index<3;index++)
                {
                    design.Pipe(White,0,0.22f+index*0.15f,0.14f,0.74f,0.066f);
                }
                design.Box(Metal,-0.32f,0.34f,0.14f,0.04f,0.58f,0.20f);
                design.Box(Metal,0.32f,0.34f,0.14f,0.04f,0.58f,0.20f);
                design.Box(Teal,0.1f,0.18f,-0.29f,0.61f,0.26f,0.19f);
                break;
            case "recovery":
                design.Hall(Concrete,0.33f,0.64f,0.55f);
                design.Box(Yellow,-0.15f,0.42f,0.04f,0.40f,0.13f,0.36f);
                design.Box(Green,0.30f,0.18f,0.27f,0.21f,0.27f,0.22f);
                design.Beam(Metal,design.Point(-0.30f,0.38f,-0.12f),design.Point(0.25f,0.15f,-0.37f),0.7f);
                if(spec.key=="electronics_reclaimer")
                {
                    design.Tank(White,0.10f,0.4f,0.05f,0.10f,0.36f);
                    design.Box(Teal,-0.28f,0.54f,0.14f,0.16f,0.30f,0.18f);
                    design.Pipe(Copper,-0.12f,0.62f,0.10f,0.35f,0.025f);
                    for(int side=-1;side<=1;side+=2)
                    {
                        design.Box(White,-0.415f,0.15f,side*0.35f,0.11f,0.22f,0.17f);
                        design.Box(Teal,-0.473f,0.17f,side*0.35f,0.012f,0.12f,0.10f,true);
                    }
                }
                if(detail==0) for(int index=0;index<3;index++) design.Box(Steel,-0.3f+index*0.23f,0.10f,0.38f,0.16f,0.10f,0.13f);
                break;
            case "enrichment":
            case "hotcells":
            case "fuelrods":
                design.Hall(Concrete,spec.family=="hotcells"?0.59f:0.42f);
                if(spec.family=="enrichment")
                    for(int index=0;index<(detail==2?3:6);index++) design.Tank(Steel,-0.31f+index*0.62f/(detail==2?2:5),0.46f,0.15f,0.043f,0.35f);
                if(spec.family=="hotcells") { design.Box(Yellow,0,0.68f,0.05f,0.75f,0.045f,0.36f); design.Tank(Metal,0.3f,0.61f,0.24f,0.045f,0.25f); }
                if(spec.family=="fuelrods") for(int index=0;index<3;index++) design.Box(Yellow,-0.22f+index*0.22f,0.6f,0.12f,0.12f,0.26f,0.30f);
                break;
            case "crystal":
                design.Hall(White,0.31f,0.76f,0.58f);
                for(int side=-1;side<=1;side+=2) { design.Tank(Metal,side*0.2f,0.35f,0.03f,0.115f,0.46f); design.Tank(Teal,side*0.2f,0.75f,0.03f,0.10f,0.09f); }
                design.Pipe(Copper,0,0.34f,0.37f,0.76f,0.02f);
                break;
            case "orbital":
            case "mission":
            case "project":
                design.Hall(White,spec.family=="project"?0.60f:0.43f,0.79f,0.72f);
                if(spec.family=="project") { design.Gantry(0.84f,Yellow); design.Box(Teal,0,0.27f,-0.36f,0.55f,0.45f,0.07f); }
                else Dish(design,-0.2f,0.50f,0.1f,0.16f);
                design.RoofEquipment(spec.family=="project"?0.68f:0.50f,2);
                break;
            case "receiver":
                design.Box(White,0,0.19f,0,0.76f,0.30f,0.64f);
                Dish(design,0,0.37f,0,0.33f);
                for(int side=-1;side<=1;side+=2) design.Box(Teal,side*0.36f,0.21f,-0.25f,0.12f,0.28f,0.23f);
                break;
            default: throw new InvalidOperationException("Unknown building art family: " + spec.family);
        }
        design.ServicePanel();
    }

    private static void Dish(Design design,float centerX,float baseHeight,float centerZ,float radius)
    {
        design.Tank(Metal,centerX,baseHeight,centerZ,radius*0.17f,0.22f);
        int segments = design.detail==0?16:design.detail==1?10:6;
        for(int index=0;index<segments;index++)
        {
            float angle=index*Mathf.PI*2/segments;
            float next=(index+1)*Mathf.PI*2/segments;
            Vector3 center=design.Point(centerX,baseHeight+0.19f,centerZ);
            Vector3 outer=design.Point(centerX+Mathf.Cos(angle)*radius,baseHeight+0.40f,centerZ+Mathf.Sin(angle)*radius);
            Vector3 end=design.Point(centerX+Mathf.Cos(next)*radius,baseHeight+0.40f,centerZ+Mathf.Sin(next)*radius);
            var shape=new ReconstructionAssets.Geometry();
            shape.vertices.AddRange(new[] { center,outer,end,center,end,outer });
            shape.triangles.AddRange(new[] {0,1,2,3,4,5});
            shape.uvs.AddRange(Enumerable.Repeat(Vector2.zero,6));
            design.solid.Append(shape,Matrix4x4.identity,index%2==0?White:Steel);
            if(design.detail<2) design.Beam(Metal,outer,end,0.055f);
        }
        design.Tank(Copper,centerX,baseHeight+0.20f,centerZ,radius*0.035f,0.28f);
    }

    private static void Office(Design design)
    {
        int tier=Math.Max(1,design.spec.tier);
        design.Box(Concrete,0,0.015f,0,0.91f,0.03f,0.93f);
        design.Box(White,0,0.16f,0.05f,0.84f,0.29f,0.71f);
        design.Box(Metal,0,0.32f,0.05f,0.87f,0.025f,0.75f);
        design.Box(Teal,-0.23f,0.51f,0.11f,0.22f,0.38f,0.57f);
        design.Box(White,0.22f,0.45f,0.11f,0.21f,0.26f,0.57f);
        design.Box(Glass,0,0.38f,0,0.25f,0.12f,0.50f,true);
        if(tier>=2) design.Box(White,-0.05f,0.63f,0.14f,0.20f,0.51f,0.48f);
        if(tier>=3) design.Box(Teal,0.17f,0.73f,0.15f,0.16f,0.32f,0.46f);
        design.Windows(0,-0.309f,0.80f,0.06f,0.29f);
        design.Windows(-0.23f,-0.18f,0.20f,0.34f,0.69f);
        design.Windows(0.22f,-0.18f,0.19f,0.34f,0.56f);
        if(tier>=2) design.Windows(-0.05f,-0.108f,0.17f,0.39f,0.88f);
        if(tier>=3) design.Windows(0.17f,-0.088f,0.13f,0.62f,0.87f);
        design.Box(Copper,0,0.06f,-0.42f,0.21f,0.09f,0.14f);
        if(design.detail<2) for(int index=0;index<7;index++) design.Box(Green,-0.36f+index*0.12f,0.045f,0.43f,0.07f,0.06f,0.05f);
    }

    private static void Care(Design design)
    {
        design.Box(Concrete,0,0.02f,0,0.95f,0.04f,0.93f);
        design.Box(White,0,0.23f,0.23f,0.85f,0.40f,0.32f);
        design.Box(Green,0,0.45f,0.23f,0.88f,0.05f,0.36f);
        design.Box(White,-0.33f,0.24f,-0.1f,0.22f,0.43f,0.44f);
        design.Box(Green,-0.33f,0.48f,-0.1f,0.24f,0.04f,0.47f);
        design.Windows(0,0.062f,0.79f,0.13f,0.39f);
        design.Box(Green,0.13f,0.045f,-0.19f,0.50f,0.022f,0.42f);
        if(design.detail<2)
        {
            for(int index=0;index<8;index++) design.Box(Metal,-0.12f+index*0.077f,0.15f,-0.40f,0.008f,0.21f,0.008f);
            design.Box(Steel,0.15f,0.23f,-0.4f,0.56f,0.02f,0.012f);
            design.Box(Copper,0.11f,0.12f,-0.2f,0.18f,0.03f,0.06f);
        }
    }

    private static void Greenhouse(Design design)
    {
        for(int side=-1;side<=1;side+=2)
        {
            design.Box(Concrete,side*0.492f,0.025f,0,0.012f,0.05f,0.98f);
            design.Box(Concrete,0,0.025f,side*0.492f,0.98f,0.05f,0.012f);
        }
        design.Box(White,0.431f,0.28f,-0.35f,0.12f,0.53f,0.28f);
        design.Box(Teal,0.431f,0.56f,-0.35f,0.122f,0.035f,0.285f);
        design.Windows(0.431f,-0.496f,0.10f,0.16f,0.48f);
        design.Box(Steel,-0.474f,0.20f,-0.35f,0.042f,0.35f,0.28f);
        int bays=7;
        int frames=design.detail==0?8:design.detail==1?5:3;
        for(int bay=0;bay<=bays;bay++)
        {
            float center=bay==bays?0.43f:-0.43f+(bay+0.5f)*0.80f/bays;
            float half=bay==bays?0.056f:0.39f/bays;
            float front=bay==bays?-0.18f:-0.46f;
            float back=0.47f;
            for(int frame=0;frame<frames;frame++)
            {
                float position=front+frame*(back-front)/(frames-1);
                Vector3 left=design.Point(center-half,0.04f,position);
                Vector3 upperLeft=design.Point(center-half,0.58f,position);
                Vector3 ridge=design.Point(center,0.84f,position);
                Vector3 upperRight=design.Point(center+half,0.58f,position);
                Vector3 right=design.Point(center+half,0.04f,position);
                design.Beam(White,left,upperLeft,0.24f);
                design.Beam(White,upperLeft,ridge,0.28f);
                design.Beam(White,ridge,upperRight,0.28f);
                design.Beam(White,upperRight,right,0.24f);
            }
            design.Beam(Steel,design.Point(center,0.84f,front),design.Point(center,0.84f,back),0.22f);
            design.Pane(design.Point(center-half,0.58f,front),design.Point(center,0.84f,front),
                design.Point(center,0.84f,back),design.Point(center-half,0.58f,back));
            design.Pane(design.Point(center,0.84f,front),design.Point(center+half,0.58f,front),
                design.Point(center+half,0.58f,back),design.Point(center,0.84f,back));
            if(design.detail==0)
                design.Beam(Teal,design.Point(center-half,0.58f,front),design.Point(center-half,0.58f,back),0.18f);
        }
    }

    private static void Poultry(Design design)
    {
        design.Box(Concrete,0,0.02f,0.085f,0.96f,0.04f,0.78f);
        design.Box(Concrete,-0.25f,0.02f,-0.40f,0.46f,0.04f,0.18f);
        design.Box(White,-0.27f,0.43f,-0.22f,0.32f,0.78f,0.46f);
        design.Box(Green,-0.27f,0.84f,-0.22f,0.35f,0.055f,0.49f);
        design.Windows(-0.27f,-0.46f,0.27f,0.2f,0.72f);
        design.Tank(White,-0.44f,0.07f,-0.14f,0.035f,0.80f);
        design.Box(Green,0.13f,0.044f,0.10f,0.63f,0.013f,0.70f);
        int pens=design.detail==0?4:2;
        for(int pen=0;pen<pens;pen++)
        {
            float position=-0.16f+pen*0.64f/(pens-1);
            design.Beam(Steel,design.Point(-0.12f,0.3f,position),design.Point(0.45f,0.3f,position),0.11f);
            if(design.detail<2) for(int post=0;post<8;post++) design.Box(Metal,-0.12f+post*0.081f,0.17f,position,0.008f,0.27f,0.008f);
        }
        design.Box(White,0.24f,0.30f,0.35f,0.36f,0.09f,0.19f);
        design.Box(Teal,0.44f,0.14f,0.10f,0.016f,0.18f,0.17f);
    }

    private static T Persist<T>(T value,string path) where T:UnityEngine.Object
    {
        T old=AssetDatabase.LoadAssetAtPath<T>(path);
        if(old==null) { AssetDatabase.CreateAsset(value,path); return value; }
        EditorUtility.CopySerialized(value,old);
        UnityEngine.Object.DestroyImmediate(value);
        return old;
    }
    private static MeshRenderer Attach(GameObject root,MeshData data,string name,Material material)
    {
        var child=new GameObject(name);
        child.transform.SetParent(root.transform,false);
        Mesh mesh=Persist(data.Mesh(root.name+"_"+name),Root+"/"+root.name+"_"+name+".asset");
        child.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=child.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        renderer.shadowCastingMode=ShadowCastingMode.On;
        return renderer;
    }

    private static ModelRecord Create(Spec spec,string publicRoot)
    {
        var root=new GameObject(spec.key);
        var lodGroup=root.AddComponent<LODGroup>();
        var levels=new List<LOD>();
        var sharedRenderers=new List<Renderer>();
        string geometryHash=null;
        for(int detail=0;detail<3;detail++)
        {
            var design=new Design(spec,detail);
            Shape(design);
            if(design.glow.vertices.Count==0) design.ServicePanel();
            var renderers=new List<Renderer> {
                Attach(root,design.solid,"structure_lod"+detail,surface),
                Attach(root,design.glow,"illumination_lod"+detail,lighting),
            };
            if(design.glazing.vertices.Count>0) renderers.Add(Attach(root,design.glazing,"glazing_lod"+detail,glazingMaterial));
            if(detail==0) geometryHash=GeometryIdentity(design);
            levels.Add(new LOD(detail==0?0.16f:detail==1?0.055f:0.004f,renderers.ToArray()));
        }
        if(spec.sign)
        {
            var geometry=new ReconstructionAssets.Geometry();
            geometry.Box(new Vector3(0,spec.height*0.47f,-spec.depth*0.36f+spec.origin_z),new Vector3(1.5f,1.5f,0.10f));
            var sign=new MeshData();
            sign.Append(geometry,Matrix4x4.identity,White);
            sharedRenderers.Add(Attach(root,sign,"sign",signMaterial));
        }
        if(spec.animated) sharedRenderers.Add(AnimateVentilation(root,spec));
        LOD[] finalLevels=levels.Select(level=>new LOD(level.screenRelativeTransitionHeight,
            level.renderers.Concat(sharedRenderers).ToArray())).ToArray();
        lodGroup.SetLODs(finalLevels);
        lodGroup.RecalculateBounds();
        Bounds bounds=new Bounds();
        bool first=true;
        foreach(Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            if(first) { bounds=renderer.bounds; first=false; } else bounds.Encapsulate(renderer.bounds);
        }
        if(bounds.min.y < -0.001f || bounds.size.x>spec.width+0.01f || bounds.size.z>spec.depth+0.01f || bounds.max.y>spec.height+0.01f)
            throw new InvalidOperationException("Building exceeds preserved envelope: "+spec.key+" "+bounds);
        var collider=root.AddComponent<BoxCollider>();
        collider.center=bounds.center;
        collider.size=bounds.size;
        string assetPath=Root+"/"+spec.key+".prefab";
        PrefabUtility.SaveAsPrefabAsset(root,assetPath);
        string preview="art/RecursiveIndustry/Buildings/previews/"+spec.key+".png";
        var record=new ModelRecord {
            key=spec.key,family=spec.family,member=spec.member,category=spec.category,asset_path=assetPath,
            envelope=new[]{spec.width,spec.height,spec.depth},bounds_center=new[]{bounds.center.x,bounds.center.y,bounds.center.z},
            bounds_size=new[]{bounds.size.x,bounds.size.y,bounds.size.z},
            triangles_per_lod=finalLevels.Select(level=>level.renderers.Sum(renderer=>renderer.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3)).ToArray(),
            renderers_per_lod=finalLevels.Select(level=>level.renderers.Length).ToArray(),
            root_collider=true,native_emission=true,recipe_sign=spec.sign,native_animation=spec.animated,
            geometry_sha256=geometryHash,
            preview_path=preview,
        };
        UnityEngine.Object.DestroyImmediate(root);
        return record;
    }

    private static string GeometryIdentity(Design design)
    {
        using(var stream=new MemoryStream())
        {
            using(var writer=new BinaryWriter(stream,System.Text.Encoding.UTF8,true))
                foreach(MeshData data in new[] {design.solid,design.glow,design.glazing})
                {
                    writer.Write(data.vertices.Count);
                    foreach(Vector3 vertex in data.vertices) { writer.Write(vertex.x); writer.Write(vertex.y); writer.Write(vertex.z); }
                    writer.Write(data.triangles.Count);
                    foreach(int index in data.triangles) writer.Write(index);
                }
            using(SHA256 hash=SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(stream.ToArray())).Replace("-",string.Empty);
        }
    }

    private static MeshRenderer AnimateVentilation(GameObject root,Spec spec)
    {
        var geometry=new ReconstructionAssets.Geometry();
        geometry.Box(Vector3.zero,new Vector3(1.6f,0.1f,0.23f));
        geometry.Box(Vector3.zero,new Vector3(0.23f,0.1f,1.6f));
        var data=new MeshData();
        data.Append(geometry,Matrix4x4.identity,Metal);
        MeshRenderer rotor=Attach(root,data,"ventilation_rotor",surface);
        rotor.transform.localPosition=spec.family=="poultry"
            ? new Vector3(-0.27f*spec.width*0.96f,spec.height*0.86f,-0.22f*spec.depth*0.94f+spec.origin_z)
            : new Vector3(0,spec.height*0.48f,0.23f*spec.depth*0.94f+spec.origin_z);
        var clip=new AnimationClip { name="Main",frameRate=30,wrapMode=WrapMode.Loop };
        clip.SetCurve("ventilation_rotor",typeof(Transform),"localEulerAnglesRaw.y",AnimationCurve.Linear(0,0,4,360));
        AnimationClipSettings settings=AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime=true;
        AnimationUtility.SetAnimationClipSettings(clip,settings);
        clip=Persist(clip,Root+"/"+spec.key+"_main.anim");
        string controllerPath=Root+"/"+spec.key+".controller";
        AnimatorController controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if(controller==null) controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine states=controller.layers[0].stateMachine;
        AnimatorState state=states.states.Select(child=>child.state).SingleOrDefault(value=>value.name=="Main") ?? states.AddState("Main");
        state.motion=clip;
        states.defaultState=state;
        EditorUtility.SetDirty(controller);
        var animator=root.AddComponent<Animator>();
        animator.runtimeAnimatorController=controller;
        animator.applyRootMotion=false;
        animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
        return rotor;
    }

    private static void ValidateAnimation(GameObject prefab,ModelRecord record)
    {
        GameObject instance=UnityEngine.Object.Instantiate(prefab);
        try
        {
            Animator[] animators=instance.GetComponentsInChildren<Animator>();
            if(animators.Length!=1) throw new InvalidOperationException("Expected one native Animator: "+record.key);
            Animator animator=animators[0];
            animator.Rebind();
            animator.Update(0);
            AnimationClip[] clips=animator.runtimeAnimatorController.animationClips;
            if(!animator.HasState(0,Animator.StringToHash("Main")) || clips.Length!=1 || !clips[0].isLooping || clips[0].length<=0)
                throw new InvalidOperationException("Missing native looping Main state: "+record.key);
            Transform rotor=instance.transform.Find("ventilation_rotor");
            clips[0].SampleAnimation(instance,0);
            Quaternion first=rotor.localRotation;
            clips[0].SampleAnimation(instance,1);
            if(Quaternion.Angle(first,rotor.localRotation)<20)
                throw new InvalidOperationException("Animation has no sampled motion: "+record.key);
            record.sampled_animation_motion=true;
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }

    private static int Render(GameObject root,Bounds bounds,string path)
    {
        root.GetComponent<LODGroup>().ForceLOD(0);
        var cameraObject=new GameObject("BuildingPreviewCamera");
        Camera camera=cameraObject.AddComponent<Camera>();
        camera.orthographic=true;
        camera.orthographicSize=Mathf.Max(bounds.size.x*0.73f,bounds.size.z*0.72f,bounds.size.y*1.20f);
        camera.aspect=1.4f;
        camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(0.19f,0.215f,0.225f);
        float distance=Mathf.Max(200,bounds.size.magnitude*3);
        camera.transform.position=bounds.center+new Vector3(1.1f,0.95f,-1.25f).normalized*distance;
        camera.transform.LookAt(bounds.center);
        camera.nearClipPlane=0.1f; camera.farClipPlane=distance*3;
        var lightObject=new GameObject("BuildingPreviewSun");
        Light light=lightObject.AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.25f;
        light.transform.rotation=Quaternion.Euler(45,-30,0);
        RenderSettings.ambientMode=AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(0.57f,0.60f,0.64f);
        var target=new RenderTexture(840,600,24);
        camera.targetTexture=target;
        camera.Render();
        RenderTexture.active=target;
        var texture=new Texture2D(840,600,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,840,600),0,0); texture.Apply();
        Color32[] pixels=texture.GetPixels32(); Color32 backdrop=pixels[0];
        int changed=pixels.Count(color=>Math.Abs(color.r-backdrop.r)+Math.Abs(color.g-backdrop.g)+Math.Abs(color.b-backdrop.b)>35);
        if(changed<pixels.Length/70) throw new InvalidOperationException("Blank building preview: "+root.name);
        if(pixels.Count(color=>color.r>210 && color.b>210 && color.g<50)>pixels.Length/100)
            throw new InvalidOperationException("Pink shader failure in "+root.name);
        File.WriteAllBytes(path,texture.EncodeToPNG());
        RenderTexture.active=null; camera.targetTexture=null;
        UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(lightObject);
        root.GetComponent<LODGroup>().ForceLOD(-1);
        return changed;
    }

    public static void Build()
    {
        string publicRoot=Environment.GetEnvironmentVariable("RI_PUBLIC_ROOT");
        if(string.IsNullOrEmpty(publicRoot)) throw new InvalidOperationException("RI_PUBLIC_ROOT is required");
        Catalog catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(publicRoot,"data/building-models.json")));
        if(catalog.schema_version!=1 || catalog.models.Length!=51 || catalog.models.Select(model=>model.key).Distinct().Count()!=51)
            throw new InvalidOperationException("Expected exact 51-model catalog");
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Path.Combine(publicRoot,"art/RecursiveIndustry/Buildings/previews"));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Shader shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/BuildingSurface.shader");
        Shader signShader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/BuildingSign.shader");
        Shader glazingShader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/BuildingGlazing.shader");
        if(shader==null || signShader==null || glazingShader==null || ShaderUtil.ShaderHasError(shader) || ShaderUtil.ShaderHasError(signShader) || ShaderUtil.ShaderHasError(glazingShader))
            throw new InvalidOperationException("Original building shader failed");
        var baseMaterial=new Material(shader) { name="building_surface" };
        baseMaterial.SetColor("_Color",Color.white);
        surface=Persist(baseMaterial,Root+"/building_surface.mat");
        var lightMaterial=new Material(shader) { name="building_windows" };
        lightMaterial.EnableKeyword("_EMISSION");
        lightMaterial.SetColor("_EmissionColor",new Color(0.17f,0.25f,0.23f));
        lighting=Persist(lightMaterial,Root+"/building_windows.mat");
        signMaterial=Persist(new Material(signShader) { name="building_sign" },Root+"/building_sign.mat");
        glazingMaterial=Persist(new Material(glazingShader) { name="greenhouse_glazing" },Root+"/greenhouse_glazing.mat");
        ModelRecord[] models=catalog.models.Select(spec=>Create(spec,publicRoot)).ToArray();
        if(models.Select(model=>model.geometry_sha256).Distinct().Count()!=models.Length)
            throw new InvalidOperationException("Duplicate building geometry; every building needs its own equipment or silhouette");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string path in AssetDatabase.FindAssets("",new[]{Root}).Select(AssetDatabase.GUIDToAssetPath))
        {
            if(Directory.Exists(path)) continue;
            AssetImporter importer=AssetImporter.GetAtPath(path);
            importer.assetBundleName="recursiveindustry_buildings";
            importer.SaveAndReimport();
        }
        if(!AssetBundlesHelpers.TryBuildAssetBundles("AssetBundles",string.Empty,BuildTarget.StandaloneWindows64,
            cleanBuild:false,out var bundleManifest,out var bundleToDlc,out var error))
            throw new InvalidOperationException("Building bundle build: "+error);
        var bundleNames=new List<string> { bundleManifest.GetAllAssetBundles().Single(name=>name.StartsWith("buildings_",StringComparison.Ordinal)) };
        foreach(Spec spec in catalog.models)
            bundleNames.Add(bundleManifest.GetAllAssetBundles().Single(name=>name.StartsWith(spec.key+"_",StringComparison.Ordinal)));
        foreach(string name in bundleNames)
        {
            if(bundleManifest.GetAllDependencies(name).Any(dependency=>!bundleNames.Contains(dependency)))
                throw new InvalidOperationException("Unexpected outside dependency: "+name);
            File.Copy(Path.Combine("AssetBundles",name),Path.Combine(publicRoot,"mods/RecursiveIndustry/AssetBundles/"+name),true);
        }
        var oldNames=File.ReadAllLines(Path.Combine(publicRoot,"mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest"))
            .Where(name=>!string.IsNullOrWhiteSpace(name) && !name.StartsWith("+",StringComparison.Ordinal));
        string[] roots=oldNames.Concat(bundleNames).Distinct().OrderBy(name=>name,StringComparer.Ordinal).ToArray();
        if(!AssetBundlesHelpers.TryGenerateMafiBundlesManifest(bundleManifest,roots,isDlc:false,validateBundle:_=>true,
            "AssetBundles/mafi_bundles.manifest",out error)) throw new InvalidOperationException("Building manifest: "+error);
        File.Copy("AssetBundles/mafi_bundles.manifest",Path.Combine(publicRoot,"mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest"),true);
        var loaded=bundleNames.Select(name=>AssetBundle.LoadFromFile(Path.GetFullPath(Path.Combine("AssetBundles",name)))).ToArray();
        if(loaded.Any(bundle=>bundle==null)) throw new InvalidOperationException("Built model bundle cannot load");
        foreach(ModelRecord model in models)
        {
            GameObject prefab=loaded.Select(bundle=>bundle.LoadAsset<GameObject>(model.asset_path)).FirstOrDefault(value=>value!=null);
            if(prefab==null || prefab.GetComponent<Collider>()==null || prefab.GetComponent<LODGroup>()==null)
                throw new InvalidOperationException("Final root collider or LOD missing: "+model.key);
            if(prefab.GetComponent<LODGroup>().GetLODs().Length!=3 || prefab.GetComponentsInChildren<MeshFilter>().Any(filter=>filter.sharedMesh==null))
                throw new InvalidOperationException("Incomplete final geometry: "+model.key);
            if(!prefab.GetComponentsInChildren<MeshRenderer>().Any(renderer=>renderer.sharedMaterial.IsKeywordEnabled("_EMISSION") && renderer.sharedMaterial.HasProperty("_EmissionColor")))
                throw new InvalidOperationException("Native emission missing: "+model.key);
            if(model.recipe_sign)
            {
                MeshRenderer sign=prefab.transform.Find("sign")?.GetComponent<MeshRenderer>();
                if(sign==null || new[] {"_IconTex","_IconScale","_IconAlpha"}.Any(property=>!sign.sharedMaterial.HasProperty(property)))
                    throw new InvalidOperationException("Native recipe sign renderer or shader contract missing: "+model.key);
            }
            if(model.native_animation) ValidateAnimation(prefab,model);
            GameObject instance=UnityEngine.Object.Instantiate(prefab);
            try
            {
                Renderer[] renderers=instance.GetComponentsInChildren<Renderer>();
                Bounds finalBounds=renderers[0].bounds;
                foreach(Renderer renderer in renderers.Skip(1)) finalBounds.Encapsulate(renderer.bounds);
                BoxCollider collider=instance.GetComponent<BoxCollider>();
                if(Vector3.Distance(finalBounds.center,collider.bounds.center)>0.01f || Vector3.Distance(finalBounds.size,collider.bounds.size)>0.01f)
                    throw new InvalidOperationException("Final collider does not enclose geometry: "+model.key);
                if(finalBounds.min.y < -0.001f || finalBounds.size.x>model.envelope[0]+0.01f || finalBounds.max.y>model.envelope[1]+0.01f || finalBounds.size.z>model.envelope[2]+0.01f)
                    throw new InvalidOperationException("Final geometry escaped envelope: "+model.key);
                model.nonblank_pixels=Render(instance,finalBounds,Path.Combine(publicRoot,model.preview_path));
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        var manifest=new Manifest {
            new_model_count=models.Length,catalog=Identity(publicRoot,"data/building-models.json"),
            generator=Identity(publicRoot,"art/RecursiveIndustry/Buildings/Editor/AllBuildingAssets.cs"),
            bundles=bundleNames.Select(name=>Identity(publicRoot,"mods/RecursiveIndustry/AssetBundles/"+name)).ToArray(),
            previews=models.Select(model=>Identity(publicRoot,model.preview_path)).ToArray(),
            dependencies=bundleNames.Select(name=>new BundleDependency { name=name,dependencies=bundleManifest.GetAllDependencies(name) }).ToArray(),
            asset_paths=loaded.SelectMany(bundle=>bundle.GetAllAssetNames()).Distinct().OrderBy(path=>path,StringComparer.Ordinal).ToArray(),models=models,
        };
        foreach(AssetBundle bundle in loaded.Reverse()) bundle.Unload(true);
        File.WriteAllText(Path.Combine(publicRoot,"art/RecursiveIndustry/Buildings/asset-manifest.json"),JsonUtility.ToJson(manifest,true)+"\n");
        Debug.Log("RI_ALL_BUILDING_ART_COMPLETE models="+models.Length+" lods=3 root_colliders="+models.Length+" emission_contracts="+models.Length);
    }

    private static FileRecord Identity(string root,string relative)
    {
        byte[] bytes=File.ReadAllBytes(Path.Combine(root,relative));
        using(SHA256 hash=SHA256.Create()) return new FileRecord {path=relative,size_bytes=bytes.Length,sha256=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-",string.Empty)};
    }
}
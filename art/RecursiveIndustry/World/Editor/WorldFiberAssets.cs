using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class WorldAssets
{
    private static Geometry Fiber(Spec spec,int lod)
    {
        var geometry=new Geometry();
        if(spec.family.StartsWith("port",StringComparison.Ordinal))
        {
            bool closed=spec.family.Contains("closed");
            float depth=spec.family.EndsWith("far",StringComparison.Ordinal)?0.18f:0.30f;
            geometry.Box(new Vector3(1,1+spec.height/2,0),new Vector3(depth,0.10f,spec.depth),Steel);
            geometry.Box(new Vector3(1,1-spec.height/2,0),new Vector3(depth,0.10f,spec.depth),Steel);
            for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(1,1,side*spec.depth*0.45f),new Vector3(depth,spec.height,0.10f),Teal);
            if(closed) geometry.Box(new Vector3(1.08f,1,0),new Vector3(0.07f,spec.height*0.72f,spec.depth*0.75f),Dark);
            if(lod<2 && !spec.family.EndsWith("far",StringComparison.Ordinal))
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(1.17f,1.20f,side*0.3f),new Vector3(0.035f,0.04f,0.08f),Gold);
        }
        else if(spec.family=="junction")
        {
            geometry.Box(new Vector3(0,0.52f,0),new Vector3(1.25f,0.88f,1.25f),Dark);
            geometry.Box(new Vector3(0,1.02f,0),new Vector3(1.34f,0.14f,1.34f),Teal);
            for(int axis=0;axis<2;axis++)
                for(int side=-1;side<=1;side+=2)
                    geometry.Box(new Vector3(axis==0?side*0.78f:0,1,axis==1?side*0.78f:0),new Vector3(axis==0?0.42f:0.62f,0.30f,axis==1?0.42f:0.62f),Steel);
            if(lod<2)
            {
                geometry.Box(new Vector3(0,1.11f,0),new Vector3(0.76f,0.03f,0.14f),White);
                geometry.Box(new Vector3(0,1.12f,0),new Vector3(0.14f,0.03f,0.76f),White);
            }
        }
        else if(spec.family=="vertical")
        {
            geometry.Box(Vector3.zero,new Vector3(0.94f,0.24f,0.60f),Steel);
            geometry.Box(new Vector3(0,0.08f,0),new Vector3(0.64f,0.11f,0.95f),Teal);
            if(lod<2) for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*0.35f,0.14f,0),new Vector3(0.07f,0.025f,0.38f),Gold);
        }
        else if(spec.family=="mount")
        {
            geometry.Box(new Vector3(0,0.24f,0),new Vector3(1.84f,0.17f,1.84f),Steel);
            geometry.Box(new Vector3(0,0.73f,0),new Vector3(0.32f,0.82f,0.32f),Dark);
            geometry.Box(new Vector3(0,1.15f,0),new Vector3(1.22f,0.17f,0.70f),Teal);
            if(lod<2) for(int side=-1;side<=1;side+=2) geometry.Beam(new Vector3(side*0.72f,0.3f,0),new Vector3(0,1.08f,0),0.10f,0.14f,Steel);
        }
        else if(spec.family=="flow_frame")
        {
            for(int side=-1;side<=1;side+=2)
            {
                geometry.Box(new Vector3(side*(spec.width/2-0.10f),0,0),new Vector3(0.20f,spec.height,spec.depth),Steel);
                geometry.Box(new Vector3(0,-spec.height*0.30f,side*spec.depth*0.43f),new Vector3(spec.width,0.14f,spec.depth*0.14f),Dark);
            }
            if(lod<2) geometry.Box(new Vector3(0,-spec.height*0.42f,0),new Vector3(spec.width*0.80f,0.06f,spec.depth*0.6f),Teal);
        }
        else if(spec.family=="flow" || spec.family=="flow_glass")
        {
            geometry.Box(new Vector3(0,spec.base_height,0),new Vector3(spec.width,spec.height,spec.depth),White);
            for(int index=0;index<geometry.colors.Count;index++) geometry.colors[index]=new Color(1,1,1,0);
        }
        else throw new InvalidOperationException("Unknown Fiber hardware family "+spec.family);
        return geometry;
    }

    private static void CreateFiberMaterials()
    {
        foreach(string name in new[]{"parts","line","flow","glass"})
        {
            var texture=new Texture2D(512,512,TextureFormat.RGBA32,false);
            var pixels=new Color[512*512];
            for(int row=0;row<512;row++)
                for(int column=0;column<512;column++)
                {
                    Color color;
                    if(name=="parts")
                    {
                        color=CargoPalette[column/128+(row/256)*4];
                        if(row%256<10 || row%256>245 || column%128<5 || column%128>122) color*=0.75f;
                    }
                    else if(name=="line")
                    {
                        color=row%128<15?White:row%128<24?Dark:Teal;
                        if(column%128<12) color*=0.78f;
                    }
                    else color=name=="glass"?new Color(0.68f,0.84f,0.85f):((column+(row/2))%128<42?White:Dark);
                    color.a=name=="glass"?0.24f:1;
                    pixels[row*512+column]=color;
                }
            texture.SetPixels(pixels); texture.Apply();
            string albedo=Root+"/fiber-"+name+"-albedo.png";
            File.WriteAllBytes(albedo,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(albedo,ImportAssetOptions.ForceSynchronousImport);
            Shader shader=name=="glass"?AssetDatabase.LoadAssetAtPath<Shader>(Root+"/WorldGlass.shader"):surface.shader;
            if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Fiber surface shader failed: "+name);
            var material=new Material(shader) {name="fiber_"+name};
            material.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
            material.SetColor("_EmissionColor",Color.clear);
            material=Persist(material,Root+"/fiber_"+name+".mat");
            if(name=="parts") fiberParts=material;
            if(name=="flow") fiberFlow=material;
            if(name=="glass") fiberGlass=material;
        }
    }
}
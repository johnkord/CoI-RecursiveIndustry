using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using Mafi.Unity.Utils;
using UnityEngine;

public static partial class WorldAssets
{
    private static Material tracksMaterial;
    private static Material signsMaterial;
    private static Material trainSignsMaterial;

    private sealed class Rig
    {
        public readonly GameObject root;
        public readonly Spec spec;
        public readonly List<Renderer>[] levels={new List<Renderer>(),new List<Renderer>(),new List<Renderer>()};
        public readonly List<string> required=new List<string>();
        public readonly List<string> controlled=new List<string>();
        public readonly List<string> animated=new List<string>();
        public readonly List<string> wheels=new List<string>();
        public Rig(Spec value) { spec=value; root=new GameObject(value.key); }

        public Transform Part(string path,Vector3 position,Func<int,Geometry> build,Material material=null,bool direct=false)
        {
            Transform parent=EnsurePath(root.transform,path); parent.localPosition=position;
            required.Add(path);
            if(direct)
            {
                MeshRenderer renderer=Attach(root,path,build(1),material??surface,spec.key+"_"+path.Replace('/','_'));
                controlled.Add(path);
                foreach(List<Renderer> level in levels) level.Add(renderer);
            }
            else
                for(int lod=0;lod<3;lod++) levels[lod].Add(Attach(root,path+"/visual_"+path.Replace('/','_')+"_"+(lod==0?"32":lod==1?"12":"4")+"ppm_LOD"+lod,build(lod),material??surface,spec.key+"_"+path.Replace('/','_')+"_lod"+lod));
            return parent;
        }
        public Transform Socket(string path,Vector3 position)
        {
            Transform node=EnsurePath(root.transform,path); node.localPosition=position;
            required.Add(path); return node;
        }
        public void Wheel(string path,Vector3 position,float radius,float width)
        {
            Part(path,position,lod=>
            {
                var geometry=new Geometry();
                geometry.Drum(Vector3.zero,radius,width,Dark,lod==0?20:lod==1?12:8,Quaternion.Euler(90,0,0));
                if(lod<2)
                    for(int side=-1;side<=1;side+=2) geometry.Drum(new Vector3(0,0,side*width*0.505f),radius*0.62f,0.04f,Steel,12,Quaternion.Euler(90,0,0));
                if(lod==0) geometry.Box(new Vector3(0,0,width*0.54f),new Vector3(radius*0.2f,radius*1.3f,0.025f),Teal);
                return geometry;
            });
            wheels.Add(path);
        }
        public void Tracks(float length,float depth,float height)
        {
            for(int side=-1;side<=1;side+=2)
            {
                string path=side<0?"track_left":"track_right";
                Part(path,new Vector3(0,height*0.51f,side*depth*0.40f),lod=>
                {
                    var geometry=new Geometry();
                    geometry.Box(Vector3.zero,new Vector3(length,height,depth*0.18f),Steel);
                    for(int index=0;index<5;index++) geometry.Drum(new Vector3(-length*0.38f+index*length*0.19f,0,side*depth*0.095f),height*0.32f,0.08f,Dark,10,Quaternion.Euler(90,0,0));
                    return geometry;
                },tracksMaterial,true);
            }
        }
    }

    private static GameObject CreateDynamic(Spec spec,out ModelRecord record)
    {
        var rig=new Rig(spec);
        if(spec.category=="train") Train(rig);
        else if(spec.category=="attachment") Attachment(rig);
        else if(spec.family=="truck" || spec.family=="haul" || spec.family=="amphibious") Truck(rig);
        else WorkingVehicle(rig);
        string[] objectNames=rig.root.GetComponentsInChildren<Transform>(true).Select(transform=>transform.name).ToArray();
        if(objectNames.Distinct(StringComparer.Ordinal).Count()!=objectNames.Length)
            throw new InvalidOperationException("Repeated rig node names prevent stable prefab IDs: "+spec.key);
        LOD[] levels=rig.levels.Select((renderers,index)=>new LOD(index==0?0.18f:index==1?0.06f:0.004f,renderers.ToArray())).ToArray();
        LODGroup group=rig.root.AddComponent<LODGroup>(); group.SetLODs(levels); group.RecalculateBounds();
        Bounds bounds=BoundsOf(rig.root);
        var collider=rig.root.AddComponent<BoxCollider>(); collider.center=bounds.center; collider.size=bounds.size;
        record=new ModelRecord {
            key=spec.key,category=spec.category,family=spec.family,asset_path=Root+"/"+spec.key+".prefab",
            bounds_center=Vector(bounds.center),bounds_size=Vector(bounds.size),
            triangles_per_lod=levels.Select(level=>level.renderers.Sum(renderer=>renderer.GetComponent<MeshFilter>()?.sharedMesh.triangles.Length/3??0)).ToArray(),
            renderers_per_lod=levels.Select(level=>level.renderers.Length).ToArray(),
            required_paths=rig.required.Distinct().ToArray(),required_renderers=rig.controlled.Distinct().ToArray(),
            animation_states=rig.animated.Distinct().ToArray(),readable_meshes=true,root_collider=true,
            cargo_meshes=Array.Empty<string>(),cargo_textures=Array.Empty<string>(),
            preview_path="art/RecursiveIndustry/World/previews/"+spec.key+".png",
        };
        PrefabUtility.SaveAsPrefabAsset(rig.root,record.asset_path);
        return rig.root;
    }

    private static void Truck(Rig rig)
    {
        Spec spec=rig.spec;
        float length=spec.width;
        bool amphibious=spec.family=="amphibious";
        float deck=amphibious?2.8f:spec.family=="haul"?2.1f:1.1f;
        rig.Part("chassis",Vector3.zero,lod=>
        {
            var geometry=new Geometry();
            geometry.Box(new Vector3(spec.origin_x,deck,0),new Vector3(length*0.94f,0.40f,spec.depth*0.65f),Dark);
            geometry.Box(new Vector3(spec.origin_x+length*0.33f,deck+spec.height*0.28f,0),new Vector3(length*0.23f,spec.height*0.42f,spec.depth*0.78f),White);
            geometry.Box(new Vector3(spec.origin_x+length*0.415f,deck+spec.height*0.33f,0),new Vector3(0.1f,spec.height*0.17f,spec.depth*0.66f),Teal);
            geometry.Box(new Vector3(spec.origin_x+length*0.30f,deck+spec.height*0.54f,0),new Vector3(length*0.23f,0.13f,spec.depth*0.84f),spec.tier==4?Gold:Teal);
            if(lod<2)
            {
                geometry.Box(new Vector3(spec.origin_x+length*0.16f,deck+spec.height*0.54f,0),new Vector3(0.25f,0.22f,spec.depth*0.64f),Steel);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(spec.origin_x+length*0.46f,deck+0.33f,side*spec.depth*0.28f),new Vector3(0.09f,0.24f,0.35f),Light);
            }
            if(lod==0)
            {
                geometry.Drum(new Vector3(spec.origin_x+length*0.31f,deck+spec.height*0.61f,0),0.22f,0.18f,Dark,12);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(spec.origin_x+length*0.19f,deck+spec.height*0.20f,side*spec.depth*0.48f),new Vector3(0.1f,0.65f,0.16f),Steel);
            }
            return geometry;
        });
        if(amphibious)
        {
            rig.Tracks(length*0.89f,spec.depth,1.9f);
            OriginalParticles(rig,"WaterSplash",new Vector3(0,1,0),new Color(0.8f,0.88f,0.9f,0.25f));
        }
        else if(spec.family=="haul")
        {
            rig.Wheel("wheel_front_left",new Vector3(3,1.9f,-2.5f),1.9f,1.65f);
            rig.Wheel("wheel_front_right",new Vector3(3,1.9f,2.5f),1.9f,1.65f);
            rig.Part("wheel_back",new Vector3(-3,1.9f,0),lod=>
            {
                var geometry=new Geometry();
                for(int side=-1;side<=1;side+=2) geometry.Drum(new Vector3(0,0,side*2.5f),1.9f,2.0f,Dark,lod==0?20:lod==1?12:8,Quaternion.Euler(90,0,0));
                return geometry;
            });
        }
        else
        {
            rig.Wheel("wheel_front_left",new Vector3(4.6f,spec.wheel_radius,-1.1f),spec.wheel_radius,0.38f);
            rig.Wheel("wheel_front_right",new Vector3(4.6f,spec.wheel_radius,1.1f),spec.wheel_radius,0.38f);
            string[] names={"middle","back1","back2"};
            for(int index=0;index<names.Length;index++)
                for(int side=-1;side<=1;side+=2)
                    rig.Wheel("wheel_"+names[index]+(side<0?"_left":"_right"),new Vector3(1-index*1.15f,spec.wheel_radius,side*1.1f),spec.wheel_radius,0.38f);
        }
    }

    private static void WorkingVehicle(Rig rig)
    {
        Spec spec=rig.spec;
        bool excavator=spec.family=="excavator";
        float length=excavator?spec.width*0.47f:spec.width*0.65f;
        float bodyHeight=excavator?2.8f:1.8f;
        rig.Tracks(length,spec.depth,excavator?2.0f:1.2f);
        rig.Part("chassis",new Vector3(0,bodyHeight*0.67f,0),lod=>
        {
            var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(length*0.92f,0.55f,spec.depth*0.65f),Dark);
            if(lod<2) geometry.Drum(new Vector3(0,0.4f,0),spec.depth*0.27f,0.35f,Steel,lod==0?20:10);
            return geometry;
        });
        rig.Part("cabin",new Vector3(0,bodyHeight,0),lod=>
        {
            var geometry=new Geometry();
            geometry.Box(new Vector3(-length*0.17f,0.70f,0),new Vector3(length*0.66f,1.6f,spec.depth*0.55f),White);
            geometry.Box(new Vector3(-length*0.17f,1.6f,0),new Vector3(length*0.71f,0.22f,spec.depth*0.59f),excavator?Teal:Green);
            geometry.Box(new Vector3(length*0.15f,0.9f,-spec.depth*0.22f),new Vector3(0.9f,0.9f,0.65f),Teal);
            if(lod<2) for(int side=-1;side<=1;side+=2) geometry.Drum(new Vector3(-length*0.30f,1.45f,side*spec.depth*0.16f),0.45f,length*0.32f,Steel,12,Quaternion.Euler(0,0,90));
            if(lod==0) for(int index=0;index<5;index++) geometry.Box(new Vector3(-length*0.22f+index*0.24f,1.75f,0),new Vector3(0.12f,0.12f,spec.depth*0.44f),Dark);
            return geometry;
        });
        float boomLength=excavator?spec.width*0.34f:spec.width*0.42f;
        float stickLength=excavator?spec.width*0.31f:spec.width*0.42f;
        rig.Part("cabin/boom",new Vector3(length*0.12f,1.05f,0),lod=>Arm(boomLength,excavator?0.85f:0.5f,lod,Gold));
        rig.Part("cabin/boom/stick",new Vector3(boomLength,0,0),lod=>Arm(stickLength,excavator?0.68f:0.42f,lod,White));
        string end=excavator?"bucket":"head";
        string endPath="cabin/boom/stick/"+end;
        rig.Part(endPath,new Vector3(stickLength,0,0),lod=>
        {
            var geometry=new Geometry();
            if(excavator)
            {
                geometry.Box(new Vector3(0.65f,-0.38f,0),new Vector3(1.55f,0.24f,2.2f),Steel);
                geometry.Box(new Vector3(0,-0.05f,0),new Vector3(0.25f,1.0f,2.2f),Dark);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(0.5f,-0.02f,side*1.06f),new Vector3(1.3f,0.74f,0.14f),Gold);
                if(lod<2) for(int tooth=0;tooth<5;tooth++) geometry.Box(new Vector3(1.50f,-0.40f,-0.84f+tooth*0.42f),new Vector3(0.4f,0.16f,0.18f),White);
            }
            else
            {
                geometry.Box(new Vector3(0,-0.3f,0),new Vector3(0.8f,1.2f,0.75f),spec.family=="planter"?Green:Gold);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(0.2f,-0.8f,side*0.5f),new Vector3(0.55f,0.35f,0.17f),Steel);
                if(lod==0) geometry.Drum(new Vector3(0.3f,-0.75f,0),0.34f,0.14f,Dark,16,Quaternion.Euler(90,0,0));
            }
            return geometry;
        });
        if(excavator)
            rig.Part(endPath+"/payload",new Vector3(0.55f,-0.18f,0),lod=>Pile(1.15f,0.50f,1.7f,lod),surface,true);
        else if(spec.family=="harvester") rig.Socket(endPath+"/TreeHolder",new Vector3(0.3f,-0.7f,0));
        else
            for(int index=1;index<=spec.payload_count;index++)
                rig.Part("seedling"+index,new Vector3(-length*0.25f+(index%4)*0.5f,bodyHeight+1.0f,(index<=4?-0.8f:0.8f)),lod=>
                {
                    var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(0.08f,0.9f,0.08f),Gold);
                    geometry.Drum(new Vector3(0,0.3f,0),0.18f,0.45f,Green,lod==0?8:5);
                    return geometry;
                });
        var clips=new Dictionary<string,AnimationClip>();
        foreach(string state in spec.states)
        {
            float duration=state=="Planting"?16.333334f:state.Contains("Mine")?spec.tier==3?2.833333f:2.0f:state.Contains("Dump")?spec.tier==3?4f:2.1f:1.5f;
            bool idle=state=="Idle";
            var clip=new AnimationClip {name=state,frameRate=30};
            float boom=idle?35:state.Contains("DeepMax")?-48:state.Contains("Deep")?-22:state.Contains("Dump")?48:state.Contains("TreeOn") || state.Contains("Truck")?20:8;
            float stick=idle?-85:state.Contains("DeepMax")?-35:state.Contains("Dump")?-48:-35;
            float initial=state.StartsWith("Prepare",StringComparison.Ordinal) || state=="LayArmToTree"?35:boom-8;
            RotationCurve(clip,"cabin/boom","z",duration,initial,boom,idle);
            RotationCurve(clip,"cabin/boom/stick","z",duration,idle?-85:stick-15,stick,idle);
            RotationCurve(clip,endPath,"z",duration,state.Contains("Dump")?0:-25,state.Contains("Dump")?-90:25,idle);
            if(state=="Planting")
            {
                clip.SetCurve("cabin/boom",typeof(Transform),"localEulerAnglesRaw.z",new AnimationCurve(new Keyframe(0,35),new Keyframe(5,-15),new Keyframe(10,-15),new Keyframe(duration,35)));
                clip.SetCurve("cabin/boom/stick",typeof(Transform),"localEulerAnglesRaw.z",new AnimationCurve(new Keyframe(0,-85),new Keyframe(5,-40),new Keyframe(10,-40),new Keyframe(duration,-85)));
            }
            clips.Add(state,clip);
        }
        clips["Idle"].SampleAnimation(rig.root,0);
        Controller(rig,rig.root,clips,"Idle",false);
    }

    private static Geometry Arm(float length,float thickness,int lod,Color color)
    {
        var geometry=new Geometry(); geometry.Box(new Vector3(length/2,0,0),new Vector3(length,thickness,thickness*0.8f),color);
        if(lod<2)
        {
            geometry.Drum(Vector3.zero,thickness*0.60f,thickness,Dark,lod==0?16:8,Quaternion.Euler(90,0,0));
            geometry.Beam(new Vector3(length*0.08f,thickness,0),new Vector3(length*0.82f,thickness*0.25f,0),thickness*0.23f,thickness*0.23f,Steel);
        }
        if(lod==0) geometry.Box(new Vector3(length*0.5f,thickness*0.54f,0),new Vector3(length*0.72f,0.06f,thickness*0.32f),Teal);
        return geometry;
    }

    private static Geometry Pile(float length,float height,float depth,int lod)
    {
        var geometry=new Geometry();
        geometry.Box(Vector3.zero,new Vector3(length,height*0.52f,depth),Steel);
        if(lod<2) geometry.Box(new Vector3(0,height*0.3f,0),new Vector3(length*0.78f,height*0.30f,depth*0.75f),Steel);
        if(lod==0) geometry.Box(new Vector3(-length*0.1f,height*0.52f,0),new Vector3(length*0.4f,height*0.16f,depth*0.45f),Steel);
        for(int index=0;index<geometry.vertices.Count;index++) geometry.vertices[index]+=Vector3.up*height*0.26f;
        return geometry;
    }

    private static void Attachment(Rig rig)
    {
        Spec spec=rig.spec;
        float length=spec.width, depth=spec.depth, bottom=spec.base_height;
        if(spec.family=="tank")
        {
            rig.Part("tank",new Vector3(spec.origin_x,bottom+spec.height/2,0),lod=>
            {
                var geometry=new Geometry();
                geometry.Drum(Vector3.zero,Mathf.Min(depth*0.46f,spec.height*0.46f),length*0.90f,White,lod==0?24:lod==1?14:8,Quaternion.Euler(0,0,90));
                if(lod<2) for(int side=-1;side<=1;side+=2) geometry.Drum(new Vector3(side*length*0.31f,0,0),Mathf.Min(depth*0.47f,spec.height*0.47f),0.16f,Teal,16,Quaternion.Euler(0,0,90));
                if(lod==0) geometry.Box(new Vector3(0,spec.height*0.45f,0),new Vector3(length*0.55f,0.18f,0.38f),Steel);
                if(spec.tier==3)
                    for(int index=0;index<geometry.vertices.Count;index++)
                        geometry.vertices[index]=Vector3.Scale(geometry.vertices[index],new Vector3(1,1,1.7f));
                return geometry;
            },surface,true);
            rig.Part("icons",new Vector3(spec.origin_x,bottom+spec.height*0.6f,spec.tier==3?-3.03f:-depth*0.475f),lod=>
            {
                var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(Mathf.Min(1.6f,length*0.25f),Mathf.Min(1.2f,spec.height*0.3f),0.04f),White); return geometry;
            },signsMaterial,true);
        }
        else if(spec.family=="flatbed")
        {
            rig.Part("shelf",new Vector3(spec.origin_x,bottom,0),lod=>
            {
                var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(length,0.30f,depth),Steel);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(0,0.35f,side*depth*0.48f),new Vector3(length,0.20f,0.1f),Teal);
                if(lod<2) for(int index=0;index<6;index++) geometry.Box(new Vector3(-length*0.4f+index*length*0.16f,0.4f,depth*0.48f),new Vector3(0.1f,0.66f,0.1f),Steel);
                return geometry;
            },surface,true);
        }
        else
        {
            float hinge=spec.origin_x-length*0.45f;
            if(spec.tier==3) length*=0.74f;
            rig.Part("bed",new Vector3(hinge,bottom,0),lod=>
            {
                var geometry=new Geometry();
                geometry.Box(new Vector3(length*0.44f,0,0),new Vector3(length*0.92f,0.28f,depth),Steel);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(length*0.44f,spec.height*0.27f,side*depth*0.47f),new Vector3(length*0.94f,spec.height*0.55f,depth*0.06f),Teal);
                geometry.Box(new Vector3(length*0.90f,spec.height*0.27f,0),new Vector3(0.17f,spec.height*0.55f,depth),White);
                if(lod<2) for(int index=0;index<5;index++) geometry.Box(new Vector3(length*0.1f+index*length*0.17f,spec.height*0.27f,-depth*0.515f),new Vector3(0.08f,spec.height*0.52f,0.06f),Gold);
                return geometry;
            });
            rig.Part("bed/PileSmooth",new Vector3(length*0.44f,0.28f,0),lod=>Pile(length*0.76f,spec.height*0.4f,depth*0.80f,lod),surface,true);
            rig.Part("bed/PileRough",new Vector3(length*0.44f,0.28f,0),lod=>Pile(length*0.76f,spec.height*0.4f,depth*0.80f,lod),surface,true);
            var up=new AnimationClip {name="Up",frameRate=30}; RotationCurve(up,"bed","z",2,0,52,false);
            var down=new AnimationClip {name="Down",frameRate=30}; RotationCurve(down,"bed","z",2,52,0,false);
            Controller(rig,rig.root,new Dictionary<string,AnimationClip> {{"Up",up},{"Down",down}},"Down",false);
            foreach(string name in new[]{"PileSmooth","PileRough"})
            {
                var clip=new AnimationClip {name="Main",frameRate=30};
                clip.SetCurve(string.Empty,typeof(Transform),"localScale.y",AnimationCurve.Linear(0,0.05f,1,1.0f));
                Controller(rig,rig.root.transform.Find("bed/"+name).gameObject,new Dictionary<string,AnimationClip> {{"Main",clip}},"Main",false);
            }
        }
    }

    private static void Train(Rig rig)
    {
        Spec spec=rig.spec;
        float length=spec.width, depth=spec.depth, height=spec.height;
        bool tender=spec.family.EndsWith("tender",StringComparison.Ordinal);
        bool boiler=spec.family=="steam" || spec.family=="fireless";
        rig.Part("body",Vector3.zero,lod=>
        {
            var geometry=new Geometry();
            geometry.Box(new Vector3(0,1.15f,0),new Vector3(length*0.90f,0.48f,depth*0.84f),Dark);
            if(boiler)
            {
                geometry.Drum(new Vector3(length*0.04f,2.25f,0),depth*0.34f,length*0.60f,White,lod==0?24:lod==1?14:8,Quaternion.Euler(0,0,90));
                geometry.Box(new Vector3(-length*0.32f,2.5f,0),new Vector3(length*0.20f,2.35f,depth*0.85f),Teal);
                geometry.Box(new Vector3(-length*0.32f,3.72f,0),new Vector3(length*0.23f,0.17f,depth*0.96f),Dark);
                if(spec.family=="steam") geometry.Drum(new Vector3(length*0.28f,3.35f,0),0.27f,0.9f,Dark,lod==0?16:8);
                if(lod<2) for(int index=0;index<3;index++) geometry.Drum(new Vector3(-length*0.18f+index*length*0.19f,2.25f,0),depth*0.355f,0.12f,Gold,12,Quaternion.Euler(0,0,90));
            }
            else if(tender)
            {
                if(spec.family=="turbine_tender") geometry.Drum(new Vector3(0,2.75f,0),depth*0.40f,length*0.80f,White,lod==0?24:lod==1?14:8,Quaternion.Euler(0,0,90));
                else
                {
                    geometry.Box(new Vector3(0,2.0f,0),new Vector3(length*0.83f,1.45f,depth*0.85f),Teal);
                    for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(0,3.0f,side*depth*0.42f),new Vector3(length*0.83f,1.0f,0.15f),White);
                }
            }
            else if(spec.family=="nuclear_reactor")
            {
                geometry.Box(new Vector3(0,2.2f,0),new Vector3(length*0.85f,1.7f,depth*0.86f),White);
                geometry.Drum(new Vector3(0,3.15f,0),depth*0.44f,1.7f,Steel,lod==0?24:lod==1?14:8);
                for(int side=-1;side<=1;side+=2) geometry.Box(new Vector3(side*length*0.31f,3.08f,0),new Vector3(length*0.15f,1.8f,depth*0.78f),Teal);
            }
            else
            {
                float roof=spec.family=="electric"?height*0.66f:height*0.86f;
                geometry.Box(new Vector3(0,(roof+1.4f)/2,0),new Vector3(length*0.86f,roof-1.4f,depth*0.82f),White);
                geometry.Box(new Vector3(length*0.32f,roof*0.83f,0),new Vector3(length*0.22f,roof*0.35f,depth*0.91f),Teal);
                geometry.Box(new Vector3(length*0.438f,roof*0.86f,0),new Vector3(0.05f,roof*0.16f,depth*0.75f),Dark);
                geometry.Box(new Vector3(-length*0.05f,roof+0.10f,0),new Vector3(length*0.66f,0.20f,depth*0.9f),Steel);
                if(spec.family=="hydrogen")
                    for(int side=-1;side<=1;side+=2) geometry.Drum(new Vector3(-length*0.12f,roof-0.14f,side*depth*0.27f),depth*0.14f,length*0.51f,Teal,lod==0?16:lod==1?10:6,Quaternion.Euler(0,0,90));
                if(spec.family=="electric")
                    for(int sign=-1;sign<=1;sign+=2)
                    {
                        float center=sign*length*0.20f;
                        geometry.Beam(new Vector3(center-0.7f,roof+0.2f,0),new Vector3(center,roof+1.0f,0),0.08f,0.11f,Dark);
                        geometry.Beam(new Vector3(center+0.7f,roof+0.2f,0),new Vector3(center,roof+1.0f,0),0.08f,0.11f,Dark);
                        geometry.Box(new Vector3(center,roof+1.10f,0),new Vector3(1.4f,0.12f,depth*0.70f),Gold);
                    }
                if(spec.family=="nuclear_condenser" || spec.family=="turbine")
                    for(int index=0;index<(lod==2?2:4);index++) geometry.Drum(new Vector3(-length*0.28f+index*length*0.15f,roof+0.32f,0),depth*0.30f,0.22f,Dark,lod==0?16:8);
                if(lod<2)
                    for(int side=-1;side<=1;side+=2)
                        for(int index=0;index<5;index++) geometry.Box(new Vector3(-length*0.28f+index*length*0.11f,roof*0.69f,side*depth*0.419f),new Vector3(length*0.06f,roof*0.22f,0.03f),spec.family=="captains"?Gold:Dark);
            }
            if(lod==0)
                for(int side=-1;side<=1;side+=2)
                {
                    geometry.Box(new Vector3(0,1.50f,side*depth*0.47f),new Vector3(length*0.85f,0.08f,0.06f),Gold);
                    geometry.Box(new Vector3(length*0.42f,1.9f,side*depth*0.31f),new Vector3(0.08f,0.21f,0.25f),Light);
                }
            return geometry;
        });
        for(int side=-1;side<=1;side+=2)
        {
            string name=side>0?"bogie_front":"bogie_rear";
            rig.Part(name,new Vector3(side*spec.bogie_offset,0,0),lod=>
            {
                var geometry=new Geometry(); geometry.Box(new Vector3(0,0.72f,0),new Vector3(spec.tier==1?2.5f:3.8f,0.44f,depth*0.65f),Dark); return geometry;
            });
            int axles=spec.tier==1?2:3;
            for(int axle=0;axle<axles;axle++)
                for(int wheelSide=-1;wheelSide<=1;wheelSide+=2)
                    rig.Wheel(name+"/wheel_"+name+"_"+axle+(wheelSide<0?"_left":"_right"),new Vector3((axle-(axles-1)/2f)*(spec.tier==1?1.5f:1.25f),spec.wheel_radius,wheelSide*0.82f),spec.wheel_radius,0.20f);
            rig.Part(side>0?"coupler_front":"coupler_rear",new Vector3(side*(length/2-0.72f),0.9f,0),lod=>
            {
                var geometry=new Geometry(); geometry.Box(new Vector3(side*0.32f,0,0),new Vector3(0.68f,0.22f,0.32f),Steel);
                if(lod<2) geometry.Box(new Vector3(side*0.63f,0,0),new Vector3(0.16f,0.35f,0.40f),Dark); return geometry;
            });
            rig.Part(side>0?"connector_front":"connector_rear",new Vector3(side*(length/2-0.55f),1.8f,0),lod=>
            {
                var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(0.35f,0.72f,0.72f),Dark); return geometry;
            });
        }
        rig.Part("sign",new Vector3(length*0.22f,2.2f,-depth*0.47f),lod=>
        {
            var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(1.1f,0.46f,0.025f),White); return geometry;
        },trainSignsMaterial,true);
        rig.Part("engine_rotor",new Vector3(-length*0.23f,height*0.78f,0),lod=>
        {
            var geometry=new Geometry(); geometry.Box(Vector3.zero,new Vector3(1.0f,0.12f,0.20f),Dark);
            if(lod<2) geometry.Box(Vector3.zero,new Vector3(0.20f,0.12f,1.0f),Dark); return geometry;
        });
        if(spec.animate_wheels || spec.family.StartsWith("nuclear_",StringComparison.Ordinal))
        {
            var main=new AnimationClip {name="Main",frameRate=30};
            if(spec.animate_wheels)
                foreach(string wheel in rig.wheels) RotationCurve(main,wheel,"z",1,0,-360,false);
            else RotationCurve(main,"engine_rotor","y",2,0,360,false);
            Controller(rig,rig.root,new Dictionary<string,AnimationClip> {{"Main",main}},"Main",true);
        }
        if(spec.family=="steam_tender")
        {
            Transform payload=rig.Part("payload",new Vector3(0,3.1f,0),lod=>Pile(length*0.7f,0.65f,depth*0.7f,lod),surface,true);
            var clip=new AnimationClip {name="Main",frameRate=30}; clip.SetCurve(string.Empty,typeof(Transform),"localScale.y",AnimationCurve.Linear(0,0.01f,1,1));
            Controller(rig,payload.gameObject,new Dictionary<string,AnimationClip> {{"Main",clip}},"Main",false);
        }
        foreach(string name in new[]{"mainSmoke","Exhaust","Exhaust2","Smoke_left","Smoke_right"})
            OriginalParticles(rig,name,new Vector3(length*0.20f,height*0.94f,name.EndsWith("left",StringComparison.Ordinal)?-depth*0.4f:name.EndsWith("right",StringComparison.Ordinal)?depth*0.4f:0),new Color(0.7f,0.73f,0.73f,0.25f));
    }

    private static void OriginalParticles(Rig rig,string path,Vector3 position,Color color)
    {
        Transform node=rig.Socket(path,position);
        ParticleSystem particles=node.gameObject.AddComponent<ParticleSystem>();
        var main=particles.main; main.playOnAwake=false; main.loop=true; main.startLifetime=2; main.startSpeed=0.8f; main.startSize=0.28f; main.startColor=color; main.maxParticles=80;
        var emission=particles.emission; emission.rateOverTime=5;
        var shape=particles.shape; shape.shapeType=ParticleSystemShapeType.Cone; shape.radius=0.08f; shape.angle=12;
        node.localRotation=Quaternion.Euler(-90,0,0);
        node.GetComponent<ParticleSystemRenderer>().sharedMaterial=surface;
        particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private static void RotationCurve(AnimationClip clip,string path,string axis,float duration,float first,float last,bool constant)
    {
        clip.SetCurve(path,typeof(Transform),"localEulerAnglesRaw."+axis,AnimationCurve.Linear(0,first,duration,constant?first:last));
    }
    private static void Controller(Rig rig,GameObject target,Dictionary<string,AnimationClip> clips,string initial,bool loop)
    {
        string suffix=target==rig.root?"root":target.name;
        string path=Root+"/"+rig.spec.key+"_"+suffix+".controller";
        AnimatorController controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path)??AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorStateMachine stateMachine=controller.layers[0].stateMachine;
        if(controller.parameters.All(parameter=>parameter.name!="SpeedMult")) controller.AddParameter("SpeedMult",AnimatorControllerParameterType.Float);
        var parameters=controller.parameters; foreach(AnimatorControllerParameter parameter in parameters) if(parameter.name=="SpeedMult") parameter.defaultFloat=1; controller.parameters=parameters;
        foreach(var entry in clips)
        {
            AnimationClip clip=entry.Value;
            AnimationClipSettings settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
            clip=Persist(clip,Root+"/"+rig.spec.key+"_"+suffix+"_"+entry.Key+".anim");
            AnimatorState state=stateMachine.states.Select(child=>child.state).SingleOrDefault(state=>state.name==entry.Key)??stateMachine.AddState(entry.Key);
            state.motion=clip; state.speed=1; state.speedParameter="SpeedMult"; state.speedParameterActive=loop;
            if(entry.Key==initial) stateMachine.defaultState=state;
            rig.animated.Add((target==rig.root?string.Empty:target.transform.parent==rig.root.transform?target.name:"bed/"+target.name)+":"+entry.Key);
        }
        EditorUtility.SetDirty(controller);
        Animator animator=target.AddComponent<Animator>(); animator.runtimeAnimatorController=controller; animator.applyRootMotion=false;
        animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    }

    private static void ValidateDynamic(GameObject prefab,ModelRecord record)
    {
        GameObject instance=UnityEngine.Object.Instantiate(prefab);
        try
        {
            foreach(string entry in record.animation_states)
            {
                string[] values=entry.Split(':');
                GameObject node=values[0].Length==0?instance:instance.transform.Find(values[0]).gameObject;
                Animator animator=node.GetComponent<Animator>();
                if(animator==null || animator.runtimeAnimatorController==null) throw new InvalidOperationException("Missing rig Animator: "+record.key+entry);
                animator.Rebind(); animator.Update(0);
                if(!animator.HasState(0,Animator.StringToHash(values[1]))) throw new InvalidOperationException("Missing native rig state: "+record.key+entry);
                animator.Play(values[1],0,0); animator.Update(0);
                AnimatorClipInfo[] playing=animator.GetCurrentAnimatorClipInfo(0);
                if(playing.Length!=1) throw new InvalidOperationException("Native state must bind one original clip: "+record.key+entry);
                AnimationClip clip=playing[0].clip;
                if(clip.length<=0) throw new InvalidOperationException("Empty native rig state: "+record.key+entry);
                if(values[1]=="Idle") continue;
                Transform[] transforms=node.GetComponentsInChildren<Transform>(true);
                clip.SampleAnimation(node,0); Matrix4x4[] first=transforms.Select(transform=>transform.localToWorldMatrix).ToArray();
                clip.SampleAnimation(node,clip.length*0.39f);
                if(!transforms.Where((transform,index)=>transform.localToWorldMatrix!=first[index]).Any()) throw new InvalidOperationException("Native rig state does not move: "+record.key+entry);
            }
            foreach(string path in record.required_renderers)
            {
                MeshRenderer renderer=instance.transform.Find(path).GetComponent<MeshRenderer>();
                if(path.StartsWith("track_",StringComparison.Ordinal) && !renderer.sharedMaterial.HasProperty("_TexOffset")) throw new InvalidOperationException("Missing native track control: "+record.key);
                if(path=="icons" && !renderer.sharedMaterial.HasProperty("_IconTex")) throw new InvalidOperationException("Missing native cargo product icon: "+record.key);
                if(path=="sign" && !renderer.sharedMaterial.HasProperty("_LocoNumber")) throw new InvalidOperationException("Missing native locomotive number: "+record.key);
            }
            if(record.category=="train")
            {
                LOD[] levels=instance.GetComponent<LODGroup>().GetLODs();
                for(int index=0;index<levels.Length;index++)
                    if(LodUtils.ParsePpmOrDefault(levels,index)!=(index==0?32:index==1?12:4))
                        throw new InvalidOperationException("Native train LOD recognition failed: "+record.key);
            }
            record.sampled_animation_motion=record.animation_states.Length>0;
            record.native_rig_contract=true;
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }

    private static void CreateDynamicMaterials()
    {
        var texture=new Texture2D(128,128,TextureFormat.RGBA32,false);
        texture.SetPixels(Enumerable.Range(0,128*128).Select(index=>(index%128)%24<7?new Color(0.12f,0.16f,0.17f):new Color(0.53f,0.56f,0.55f)).ToArray()); texture.Apply();
        string path=Root+"/track-treads-albedo.png"; File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.wrapMode=TextureWrapMode.Repeat; importer.SaveAndReimport();
        var material=new Material(surface) {name="track_treads"}; material.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        tracksMaterial=Persist(material,Root+"/track_treads.mat");
        Shader shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/WorldSign.shader");
        if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Dynamic sign shader failed");
        signsMaterial=Persist(new Material(shader) {name="world_signs"},Root+"/world_signs.mat");
        var numbered=new Material(shader) {name="train_signs"}; numbered.SetFloat("_NumberMode",1);
        trainSignsMaterial=Persist(numbered,Root+"/train_signs.mat");
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static partial class WorldAssets
{
    private static ControlRecord[] RenderNativeControls(AssetBundle[] bundles)
    {
        string[][] cases={
            new[]{"locomotive_number","autonomous_diesel_locomotive_i","sign","_LocoNumber","101","888"},
            new[]{"track_motion","autonomous_amphibious_hauler","track_left","_TexOffset","0","0.13"},
            new[]{"vehicle_lighting","autonomous_hauler","chassis","_EmissionStrength","0","8"},
        };
        var result=new List<ControlRecord>();
        foreach(string[] entry in cases)
        {
            GameObject prefab=bundles.Select(bundle=>bundle.LoadAsset<GameObject>(Root+"/"+entry[1]+".prefab")).FirstOrDefault(value=>value!=null);
            GameObject instance=UnityEngine.Object.Instantiate(prefab.transform.Find(entry[2]).gameObject);
            var paths=new List<string>();
            try
            {
                for(int state=0;state<2;state++)
                {
                    var values=new MaterialPropertyBlock();
                    values.SetFloat(entry[3],float.Parse(entry[4+state],System.Globalization.CultureInfo.InvariantCulture));
                    foreach(Renderer renderer in instance.GetComponentsInChildren<Renderer>()) renderer.SetPropertyBlock(values);
                    string path="art/RecursiveIndustry/World/previews/control-"+entry[0]+"-"+state+".png";
                    Render(instance,Path.Combine(publicRoot,path)); paths.Add(path);
                }
                if(File.ReadAllBytes(Path.Combine(publicRoot,paths[0])).SequenceEqual(File.ReadAllBytes(Path.Combine(publicRoot,paths[1]))))
                    throw new InvalidOperationException("Native shader control produced identical pixels: "+entry[0]);
                result.Add(new ControlRecord {key=entry[0],property=entry[3],previews=paths.ToArray(),distinct_pixels=true});
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        return result.ToArray();
    }

    private static AssemblyRecord[] RenderAssemblies(AssetBundle[] bundles,List<ModelRecord> models)
    {
        string[][] groups={
            new[]{"general_tank","autonomous_hauler","hauler_tank"},
            new[]{"general_flatbed","autonomous_hauler","hauler_flatbed","frontier_program"},
            new[]{"general_dump","autonomous_hauler","hauler_dump"},
            new[]{"heavy_tank","autonomous_tank_hauler","heavy_tank"},
            new[]{"heavy_dump","autonomous_dump_hauler","heavy_dump"},
            new[]{"amphibious_tank","autonomous_amphibious_hauler","amphibious_tank"},
            new[]{"amphibious_flatbed","autonomous_amphibious_hauler","amphibious_flatbed","companion_provisions"},
            new[]{"amphibious_dump","autonomous_amphibious_hauler","amphibious_dump"},
            new[]{"steam_train","autonomous_steam_locomotive_ii","autonomous_steam_tender_ii"},
            new[]{"nuclear_train","autonomous_nuclear_locomotive_cab","autonomous_nuclear_locomotive_reactor","autonomous_nuclear_locomotive_condenser"},
        };
        var result=new List<AssemblyRecord>();
        Catalog catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(publicRoot,"data/world-art.json")));
        foreach(string[] group in groups)
        {
            var root=new GameObject(group[0]);
            bool stateChecked=false;
            try
            {
                float trailing=0;
                for(int index=1;index<group.Length;index++)
                {
                    ModelRecord model=models.Single(model=>model.key==group[index]);
                    GameObject prefab=bundles.Select(bundle=>bundle.LoadAsset<GameObject>(model.asset_path)).FirstOrDefault(value=>value!=null);
                    GameObject part=UnityEngine.Object.Instantiate(prefab,root.transform,false);
                    if(model.category=="train")
                    {
                        float length=model.bounds_size[0];
                        if(index>1) part.transform.localPosition=new Vector3(-trailing-length/2,0,0);
                        trailing+=index==1?length/2:length;
                    }
                    if(model.category=="cargo")
                    {
                        part.transform.localPosition=new Vector3(-1.2f,group[0].StartsWith("amphibious",StringComparison.Ordinal)?3.55f:1.55f,0);
                        part.transform.localScale=Vector3.one*0.7f;
                    }
                    if(model.category=="attachment" && model.family=="dump")
                    {
                        Animator animator=part.GetComponent<Animator>(); animator.Rebind(); animator.Play("Down",0,1); animator.Update(0);
                        Transform bed=part.transform.Find("bed"); Quaternion lowered=bed.localRotation;
                        animator.Play("Up",0,0.95f); animator.Update(0);
                        if(Quaternion.Angle(lowered,bed.localRotation)<20) throw new InvalidOperationException("Composed dump body does not tilt: "+group[0]);
                        animator.Play("Down",0,1); animator.Update(0);
                        Transform rough=part.transform.Find("bed/PileRough"); if(rough!=null) rough.gameObject.SetActive(false);
                        stateChecked=true;
                    }
                    if(model.category=="attachment")
                    {
                        Spec chassis=catalog.models.Single(spec=>spec.key==group[1]);
                        float cabRear=chassis.origin_x+chassis.width*(0.33f-0.23f/2);
                        if(BoundsOf(part).max.x>cabRear-0.03f)
                            throw new InvalidOperationException("Load geometry intersects its cabin: "+group[0]);
                        if(group[1].StartsWith("autonomous_dump_hauler",StringComparison.Ordinal) || group[1].StartsWith("autonomous_tank_hauler",StringComparison.Ordinal))
                        {
                            Bounds load=BoundsOf(part);
                            if(load.min.y<3.7f) throw new InvalidOperationException("Heavy load does not clear tire tops: "+group[0]);
                        }
                    }
                }
                string preview="art/RecursiveIndustry/World/previews/assembly-"+group[0]+".png";
                int pixels=Render(root,Path.Combine(publicRoot,preview));
                result.Add(new AssemblyRecord {key=group[0],models=group.Skip(1).ToArray(),preview_path=preview,nonblank_pixels=pixels,sampled_state_changes=stateChecked});
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        return result.ToArray();
    }
}
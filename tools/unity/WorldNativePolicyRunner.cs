using System;
using System.Reflection;
using UnityEngine;

public static class WorldNativePolicyRunner
{
    public static void Run()
    {
        string path=Environment.GetEnvironmentVariable("RI_WORLD_POLICY_ASSEMBLY");
        if(string.IsNullOrEmpty(path)) throw new InvalidOperationException("RI_WORLD_POLICY_ASSEMBLY is required");
        Assembly assembly=Assembly.LoadFrom(path);
        Type fixture=assembly.GetType("Fixture",true);
        try
        {
            int result=(int)fixture.GetMethod("Run",BindingFlags.Static|BindingFlags.Public).Invoke(null,null);
            if(result!=0) throw new InvalidOperationException("Native graphics policy returned "+result);
            int count=(int)fixture.GetField("count",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            Debug.Log("RI_WORLD_NATIVE_POLICY_COMPLETE checks="+count);
        }
        catch(TargetInvocationException error)
        {
            throw error.InnerException??error;
        }
    }
}
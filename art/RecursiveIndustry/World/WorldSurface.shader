Shader "RecursiveIndustry/WorldSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _AccentColor ("Accent", Color) = (0.2,0.65,0.62,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normals", 2D) = "bump" {}
        _MetallicGlossMap ("Metallic smoothness", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.3
        _Glossiness ("Smoothness", Range(0,1)) = 0.35
        _EmissionMap ("Emission mask", 2D) = "white" {}
        _EmissionColor ("Emission", Color) = (0.15,0.26,0.25,1)
        _EmissionIntensity ("Emission intensity", Float) = 1
        _EmissionStrength ("Native vehicle lights", Float) = 1
        _TexOffset ("Native track offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing
        struct Input { float2 uv_MainTex; float2 uv_BumpMap; float4 color:COLOR; };
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        sampler2D _EmissionMap;
        fixed4 _Color;
        fixed4 _AccentColor;
        fixed4 _EmissionColor;
        half _EmissionIntensity;
        half _EmissionStrength;
        float _TexOffset;
        half _Metallic;
        half _Glossiness;
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 paint=tex2D(_MainTex,input.uv_MainTex+float2(_TexOffset,0));
            fixed4 surface=tex2D(_MetallicGlossMap,input.uv_MainTex);
            output.Albedo=paint.rgb*input.color.rgb*_Color.rgb;
            output.Normal=UnpackNormal(tex2D(_BumpMap,input.uv_BumpMap));
            output.Metallic=_Metallic*surface.r;
            output.Smoothness=_Glossiness*surface.a;
            output.Emission=_EmissionColor.rgb*_EmissionIntensity*_EmissionStrength*input.color.a*tex2D(_EmissionMap,input.uv_MainTex).rgb;
            output.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
Shader "RecursiveIndustry/WorldGlass"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normals", 2D) = "bump" {}
        _MetallicGlossMap ("Metallic smoothness", 2D) = "white" {}
        _EmissionMap ("Emission", 2D) = "black" {}
        _EmissionColor ("Emission color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0
        struct Input { float2 uv_MainTex; };
        sampler2D _MainTex;
        fixed4 _Color;
        void surf(Input input,inout SurfaceOutputStandard output)
        {
            fixed4 color=tex2D(_MainTex,input.uv_MainTex)*_Color;
            output.Albedo=color.rgb;
            output.Smoothness=0.7;
            output.Alpha=color.a;
        }
        ENDCG
    }
    Fallback "Transparent/Diffuse"
}
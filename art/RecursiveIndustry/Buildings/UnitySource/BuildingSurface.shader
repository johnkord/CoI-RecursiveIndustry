Shader "RecursiveIndustry/BuildingSurface"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.25
        _Glossiness ("Smoothness", Range(0, 1)) = 0.3
        _EmissionColor ("Native emission", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma shader_feature _EMISSION
        struct Input { float4 color : COLOR; };
        fixed4 _Color;
        half _Metallic;
        half _Glossiness;
        fixed4 _EmissionColor;
        void surf (Input input, inout SurfaceOutputStandard output)
        {
            output.Albedo = _Color.rgb * input.color.rgb;
            output.Metallic = _Metallic;
            output.Smoothness = _Glossiness;
            #ifdef _EMISSION
            output.Emission = _EmissionColor.rgb * input.color.rgb;
            #endif
            output.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
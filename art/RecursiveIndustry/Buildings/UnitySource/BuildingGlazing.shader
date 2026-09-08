Shader "RecursiveIndustry/BuildingGlazing"
{
    Properties
    {
        _Color ("Glass tint", Color) = (0.45, 0.65, 0.67, 0.16)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.65
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0
        struct Input { float4 color : COLOR; };
        fixed4 _Color;
        half _Glossiness;
        void surf (Input input, inout SurfaceOutputStandard output)
        {
            output.Albedo = _Color.rgb;
            output.Smoothness = _Glossiness;
            output.Alpha = _Color.a;
        }
        ENDCG
    }
    Fallback "Transparent/Diffuse"
}
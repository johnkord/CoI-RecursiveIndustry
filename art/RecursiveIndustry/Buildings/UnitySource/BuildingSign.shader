Shader "RecursiveIndustry/BuildingSign"
{
    Properties
    {
        _IconTex ("Product", 2D) = "black" {}
        _IconScale ("Scale", Float) = 1
        _IconAlpha ("Opacity", Range(0, 1)) = 1
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _IconTex;
            float _IconScale;
            float _IconAlpha;
            fixed4 _Color;
            struct VertexInput { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct VertexOutput { float4 position:SV_POSITION; float2 uv:TEXCOORD0; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }
            fixed4 frag(VertexOutput input):SV_Target
            {
                float scale = max(abs(_IconScale), 0.1);
                float2 uv = (input.uv - 0.5) / scale + 0.5;
                if (_IconScale < 0) uv.x = 1 - uv.x;
                fixed4 icon = tex2D(_IconTex, uv);
                float inside = step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1);
                return fixed4(lerp(fixed3(0.065,0.075,0.075), icon.rgb*_Color.rgb, icon.a*_IconAlpha*inside),1);
            }
            ENDCG
        }
    }
}
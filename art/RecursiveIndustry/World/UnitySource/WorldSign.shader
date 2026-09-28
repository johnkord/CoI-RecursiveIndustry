Shader "RecursiveIndustry/WorldSign"
{
    Properties
    {
        _IconTex ("Native product icon", 2D) = "black" {}
        _IconScale ("Native icon scale", Float) = 1
        _IconAlpha ("Native icon opacity", Float) = 1
        _LocoNumber ("Native locomotive number", Float) = 0
        _NumberMode ("Locomotive sign", Float) = 0
        _MainTex ("Native train sign", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AccentColor ("Accent", Color) = (0.2,0.6,0.6,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct App { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            sampler2D _IconTex;
            sampler2D _MainTex;
            float _IconScale;
            float _IconAlpha;
            float _LocoNumber;
            float _NumberMode;
            fixed4 _Color;
            Output vert(App input) { Output output; output.vertex=UnityObjectToClipPos(input.vertex); output.uv=input.uv; return output; }
            float bar(float2 position,float2 center,float2 halfSize)
            {
                float2 delta=abs(position-center);
                return step(delta.x,halfSize.x)*step(delta.y,halfSize.y);
            }
            float numeral(float2 position,float digit)
            {
                float result=0;
                if(digit!=1 && digit!=4) result+=bar(position,float2(0.5,0.88),float2(0.28,0.055));
                if(digit!=5 && digit!=6) result+=bar(position,float2(0.80,0.70),float2(0.055,0.18));
                if(digit!=2) result+=bar(position,float2(0.80,0.30),float2(0.055,0.18));
                if(digit!=1 && digit!=4 && digit!=7) result+=bar(position,float2(0.5,0.12),float2(0.28,0.055));
                if(digit==0 || digit==2 || digit==6 || digit==8) result+=bar(position,float2(0.20,0.30),float2(0.055,0.18));
                if(digit!=1 && digit!=2 && digit!=3 && digit!=7) result+=bar(position,float2(0.20,0.70),float2(0.055,0.18));
                if(digit!=0 && digit!=1 && digit!=7) result+=bar(position,float2(0.5,0.50),float2(0.28,0.055));
                return saturate(result);
            }
            fixed4 frag(Output input):SV_Target
            {
                if(_NumberMode>0.5)
                {
                    if(_LocoNumber<=0) return fixed4(0.08,0.14,0.15,1);
                    float column=min(5,floor(input.uv.x*6));
                    float digit=fmod(floor(max(0,_LocoNumber)/pow(10,5-column)),10);
                    float mask=numeral(float2(frac(input.uv.x*6),input.uv.y),digit);
                    return fixed4(lerp(fixed3(0.05,0.08,0.08),fixed3(0.85,0.94,0.86),mask),1);
                }
                float2 uv=(input.uv-0.5)/max(abs(_IconScale),0.1)+0.5;
                fixed4 icon=tex2D(_IconTex,uv);
                fixed4 panel=tex2D(_MainTex,input.uv)*_Color;
                return fixed4(lerp(panel.rgb,icon.rgb,icon.a*_IconAlpha),1);
            }
            ENDCG
        }
    }
}
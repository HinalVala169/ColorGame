Shader "Custom/RevealMaskTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Reveal ("Reveal Amount", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

           

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Reveal;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                half4 baseColor = tex2D(_MainTex, i.uv);
                half4 maskColor = tex2D(_MaskTex, i.uv);
                half mask = maskColor.a; 
                half revealFactor = saturate((mask - _Reveal) / (1.0 - _Reveal));
                half4 resultColor = lerp(baseColor, maskColor, revealFactor);
                return resultColor;
            }
            ENDCG
        }
    }
}

Shader "Custom/RegionFillShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {} // Line art texture
        _ColorTex ("Coloring Texture", 2D) = "white" {} // Coloring texture
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

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
            sampler2D _ColorTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                fixed4 colorOverlay = tex2D(_ColorTex, i.uv);
                colorOverlay.a = colorOverlay.a * mainColor.a; // Preserve line art transparency
                return lerp(mainColor, colorOverlay, colorOverlay.a);
            }
            ENDCG
        }
    }
}

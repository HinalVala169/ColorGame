Shader "Custom/RevealMaskWithNumbers"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}         // Base texture
        _MaskNumTex ("Mask Texture", 2D) = "white" {}      // Mask texture
        _NumberTex ("Number Texture", 2D) = "white" {}     // Number texture
        _RegionNumber ("Region Number", Float) = 0         // Region being filled
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

            sampler2D _MainTex;       // Base texture
            sampler2D _MaskNumTex;    // Mask texture
            sampler2D _NumberTex;     // Number texture
            float _RegionNumber;      // Region being filled

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // Pass UV coordinates
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Sample textures
                half4 baseColor = tex2D(_MainTex, i.uv);     // Base texture
                half4 maskColor = tex2D(_MaskNumTex, i.uv);  // Mask texture
                half4 numberColor = tex2D(_NumberTex, i.uv); // Number texture

                // Initialize final color as the base color
                half4 finalColor = baseColor;

                // Reveal mask texture based on filled region
                if (baseColor.a > 0.5) // Assume regions filled have alpha > 0.5
                {
                    finalColor = maskColor; // Show mask in filled areas
                }

                // Overlay number texture (always visible)
                finalColor = lerp(finalColor, numberColor, numberColor.a);

                return finalColor;
            }
            ENDCG
        }
    }
}

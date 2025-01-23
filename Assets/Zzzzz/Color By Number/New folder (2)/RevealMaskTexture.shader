Shader "Custom/RevealMaskTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MainTex ("MaskNumber Texture", 2D) = "white" {}
        _Region1Color ("Region 1 Color", Color) = (1, 0, 0, 1) // Red
        _Region2Color ("Region 2 Color", Color) = (0, 1, 0, 1) // Green
        _Region3Color ("Region 3 Color", Color) = (0, 0, 1, 1) // Blue
        _Region4Color ("Region 4 Color", Color) = (1, 1, 0, 1) // Yellow
        _Region5Color ("Region 5 Color", Color) = (1, 0, 1, 1) // Magenta
        _Region6Color ("Region 6 Color", Color) = (0, 1, 1, 1) // Cyan
        _Region7Color ("Region 7 Color", Color) = (0.5, 0.5, 0.5, 1) // Gray
        _RevealAndMask ("Reveal and Mask", Range(0, 1)) = 0.0 // Single slider to control both reveal and mask visibility
        _RegionNumber ("Region Number", Float) = 0
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

            // Define properties
            sampler2D _MainTex;
            sampler2D _MaskNumTex;
            float4 _Region1Color;
            float4 _Region2Color;
            float4 _Region3Color;
            float4 _Region4Color;
            float4 _Region5Color;
            float4 _Region6Color;
            float4 _Region7Color;
            float _RevealAndMask;
            float _RegionNumber;
            float4 _MainTex_ST;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 baseColor = tex2D(_MainTex, i.uv);

                half4 regionColor = baseColor; // Default to base color

                // Check the region number to apply specific colors (based on UVs or defined areas in your texture)
                if (_RegionNumber == 1 && i.uv.x > 0.0 && i.uv.x < 0.33 && i.uv.y > 0.0 && i.uv.y < 0.33)
                {
                    regionColor = _Region1Color;
                }
                else if (_RegionNumber == 2 && i.uv.x > 0.33 && i.uv.x < 0.66 && i.uv.y > 0.0 && i.uv.y < 0.33)
                {
                    regionColor = _Region2Color;
                }
                else if (_RegionNumber == 3 && i.uv.x > 0.66 && i.uv.x < 1.0 && i.uv.y > 0.0 && i.uv.y < 0.33)
                {
                    regionColor = _Region3Color;
                }
                else if (_RegionNumber == 4 && i.uv.x > 0.0 && i.uv.x < 0.33 && i.uv.y > 0.33 && i.uv.y < 0.66)
                {
                    regionColor = _Region4Color;
                }
                else if (_RegionNumber == 5 && i.uv.x > 0.33 && i.uv.x < 0.66 && i.uv.y > 0.33 && i.uv.y < 0.66)
                {
                    regionColor = _Region5Color;
                }
                else if (_RegionNumber == 6 && i.uv.x > 0.66 && i.uv.x < 1.0 && i.uv.y > 0.33 && i.uv.y < 0.66)
                {
                    regionColor = _Region6Color;
                }
                else if (_RegionNumber == 7 && i.uv.x > 0.0 && i.uv.x < 1.0 && i.uv.y > 0.66 && i.uv.y < 1.0)
                {
                    regionColor = _Region7Color;
                }

                // Blend the region color based on the reveal slider
                half4 finalColor = lerp(baseColor, regionColor, _RevealAndMask);

                return finalColor;
            }
            ENDCG
        }
    }
}

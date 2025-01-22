Shader "Custom/RevealMaskTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Region1Color ("Region 1 Color", Color) = (1, 0, 0, 1) // Red
        _Region2Color ("Region 2 Color", Color) = (0, 1, 0, 1) // Green
        _Region3Color ("Region 3 Color", Color) = (0, 0, 1, 1) // Blue
        _Region4Color ("Region 4 Color", Color) = (1, 1, 0, 1) // Yellow
        _Region5Color ("Region 5 Color", Color) = (1, 0, 1, 1) // Magenta
        _Region6Color ("Region 6 Color", Color) = (0, 1, 1, 1) // Cyan
        _Region7Color ("Region 7 Color", Color) = (0.5, 0.5, 0.5, 1) // Gray
        _RevealAndMask ("Reveal Mask Texture", Range(0, 1)) = 0.0 // Single slider to control both reveal and mask visibility
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
            sampler2D _MaskTex;
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
                half4 maskColor = tex2D(_MaskTex, i.uv);

                half4 regionColor = baseColor; // Default to base color

                // Check the region number to apply specific colors
                if (_RegionNumber == 1 && maskColor.r > 0.8 && maskColor.g < 0.2 && maskColor.b < 0.2)
                {
                    regionColor = _Region1Color;
                }
                else if (_RegionNumber == 2 && maskColor.r < 0.2 && maskColor.g > 0.8 && maskColor.b < 0.2)
                {
                    regionColor = _Region2Color;
                }
                else if (_RegionNumber == 3 && maskColor.r < 0.2 && maskColor.g < 0.2 && maskColor.b > 0.8)
                {
                    regionColor = _Region3Color;
                }
                else if (_RegionNumber == 4 && maskColor.r > 0.8 && maskColor.g > 0.8 && maskColor.b < 0.2)
                {
                    regionColor = _Region4Color;
                }
                else if (_RegionNumber == 5 && maskColor.r > 0.8 && maskColor.g < 0.2 && maskColor.b > 0.8)
                {
                    regionColor = _Region5Color;
                }
                else if (_RegionNumber == 6 && maskColor.r < 0.2 && maskColor.g > 0.8 && maskColor.b > 0.8)
                {
                    regionColor = _Region6Color;
                }
                else if (_RegionNumber == 7 && maskColor.r > 0.4 && maskColor.g > 0.4 && maskColor.b > 0.4 && maskColor.r < 0.6)
                {
                    regionColor = _Region7Color;
                }

                // Apply the effect based on the Reveal and Mask slider
                half4 finalColor;

                // If _RevealAndMask is 1, show the mask texture and apply the reveal effect
                if (_RevealAndMask > 0.5)
                {
                    finalColor = lerp(baseColor, maskColor, _RevealAndMask); // Blend the mask texture over the base
                }
                else
                {
                    // Else just apply the reveal based on _RevealAndMask
                    finalColor = lerp(baseColor, regionColor, _RevealAndMask); // Blend the region color based on reveal
                }

                // Return the final color
                return finalColor;
            }
            ENDCG
        }
    }
}

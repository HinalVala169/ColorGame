Shader "Custom/RegionFillShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {} // Line art texture
        _ColorTex ("Coloring Texture", 2D) = "white" {} // Coloring texture (from ColorFillScript)
        _TargetColor ("Target Color", Color) = (1, 1,1, 1) // The color to fill (passed from script)
        _Tolerance ("Tolerance", Range(0, 1)) = 0.1 // Color matching tolerance
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100

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

            // Texture samplers
            sampler2D _MainTex;
            sampler2D _ColorTex;

            // Color and tolerance
            fixed4 _TargetColor;
            float _Tolerance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the base texture (line art)
                fixed4 mainColor = tex2D(_MainTex, i.uv);

                // Sample the color texture (mask or filled regions)
                fixed4 colorOverlay = tex2D(_ColorTex, i.uv);

                // Compare the base texture color with the target color, respecting tolerance
                if (distance(mainColor.rgb, _TargetColor.rgb) < _Tolerance)
                {
                    // If the pixel color is within tolerance, fill it with the target color
                    colorOverlay.rgb = lerp(colorOverlay.rgb, _TargetColor.rgb, colorOverlay.a);
                }

                // Preserve line art transparency (alpha) while filling with color
                colorOverlay.a = max(colorOverlay.a, mainColor.a);

                // Return the final color (blending the base texture and color fill)
                return lerp(mainColor, colorOverlay, colorOverlay.a);
            }
            ENDCG
        }
    }
}

Shader "Custom/RevealMaskTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}    
        _NumberTex ("Number Texture", 2D) = "white" {}    
        _Highlight ("Highlight Texture", 2D) = "white" {}       
        _MaskTex ("Mask Texture", 2D) = "white" {}      
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

            sampler2D _MainTex;    
            sampler2D _NumberTex;   
            sampler2D _Highlight;
            sampler2D _MaskTex;    
            float _RegionNumber;      // The region number to highlight

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
                o.uv = v.uv; 
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Sample textures
                half4 baseColor = tex2D(_MainTex, i.uv);     // Base texture
                half4 numberColor = tex2D(_NumberTex, i.uv); // Number texture
                half4 highlightColor = tex2D(_Highlight, i.uv);  
                half4 maskColor = tex2D(_MaskTex, i.uv);  // Mask texture

                // Get the region number from the Number Texture
                float regionValue = tex2D(_NumberTex, i.uv).r * 255;  // Get region ID from the grayscale value of NumberTex

                // Initialize final color as the base texture
                half4 finalColor = baseColor;

                // Show highlight if the current region matches the selected region number and it's not filled
                if (regionValue == _RegionNumber && baseColor.a < 0.5)
                {
                    finalColor = highlightColor;  // Show highlight texture
                }

                // Show the mask color if the region is filled
                if (baseColor.a > 0.5)
                {
                    finalColor = maskColor;
                    numberColor.a = 0.0;
                }

                // Overlay number texture on top, respecting alpha
                finalColor = lerp(finalColor, numberColor, numberColor.a);

                return finalColor;
            }
            ENDCG
        }
    }
}

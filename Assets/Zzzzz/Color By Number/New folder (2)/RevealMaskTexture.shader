Shader "Custom/RevealMaskTexture"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}    
        _NumberTex ("Number Texture", 2D) = "white" {}      
        _MaskTex ("Mask Texture", 2D) = "white" {}      
          // Number texture
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
            sampler2D _MaskTex;    
            float _RegionNumber;     

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
                half4 maskColor = tex2D(_MaskTex, i.uv);  // Mask texture
                

                // Determine if the region is filled
                bool isFilled = baseColor.a > 0.5; // Consider alpha > 0.5 as filled

                // Initialize final color as the base texture
                half4 finalColor = baseColor;

                if (isFilled)
                {
                    // If the region is filled, show the mask texture
                    finalColor = maskColor;

                    // Hide the corresponding area of the number texture
                    numberColor.a = 0.0;
                }

                // Overlay number texture on top (respecting alpha)
                finalColor = lerp(finalColor, numberColor, numberColor.a);

                return finalColor;
            }
            ENDCG
        }
    }
}

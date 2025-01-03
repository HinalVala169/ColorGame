Shader "UnityCoder/CanvasAlphaAndMask"
{
    Properties
    {
        _MainTex("Particle Texture", 2D) = "white" {}

        _MaskTex("Mask (RGBA)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.4
        _BlurAmount("Blur Amount", Range(0, 1)) = 0.6
    }

        Category
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

            BindChannels
            {
                Bind "Color", color
                Bind "Vertex", vertex
                Bind "TexCoord", texcoord
            }

            SubShader
            {
                Pass
                {
                    SetTexture[_MainTex]
                    {
                        combine texture * primary
                    }

                    SetTexture[_MaskTex]
                    {
                        combine texture lerp(texture) previous, previous + texture
                    }
                }

                Tags
                {
                    "Queue" = "Transparent"
                    "RenderType" = "Transparent"
                    "PreviewType" = "Plane"
                    "CanUseSpriteAtlas" = "True"
                }

                LOD 300
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma surface surf ToonRamp vertex:vert alpha alphatest:_Cutoff addshadow
                #pragma lighting ToonRamp

                sampler2D _MaskTex;
                float _BlurAmount;
                float4 _MaskTex_TexelSize;

                // for hard double-sided proximity lighting
                inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
                {
                    half4 c;
                    c.rgb = s.Albedo * _LightColor0.rgb * sqrt(atten);
                    c.a = s.Alpha;
                    return c;
                }

                struct Input
                {
                    float2 uv_MaskTex : TEXCOORD0;
                    fixed4 color;
                };

                void vert(inout appdata_full v, out Input o)
                {
                    UNITY_INITIALIZE_OUTPUT(Input, o);
                    o.color = v.color;
                }

                void surf(Input IN, inout SurfaceOutput o)
                {
                    half4 original = tex2D(_MaskTex, IN.uv_MaskTex);
                    half4 finalOutput = tex2D(_MaskTex, IN.uv_MaskTex);

                    float amount = _BlurAmount;
                    float2 up = float2(0.0, _MaskTex_TexelSize.y) * amount;
                    float2 right = float2(_MaskTex_TexelSize.x, 0.0) * amount;

                    for (int i = 0; i < 3; i++)
                    {
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex + up);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex - up);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex + right);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex - right);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex + right + up);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex - right + up);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex - right - up);
                        finalOutput += tex2D(_MaskTex, IN.uv_MaskTex + right - up);

                        amount += amount;
                        up = float2(0.0, _MaskTex_TexelSize.y) * amount;
                        right = float2(_MaskTex_TexelSize.x, 0.0) * amount;
                    }

                    finalOutput = (finalOutput) / 25;

                    fixed3 finalOutput2 = lerp((0,0,0,0), finalOutput.rgb, finalOutput.a);
                    finalOutput2 = lerp(finalOutput2, finalOutput.rgb, original.a);
                    finalOutput2 *= IN.color;

                    o.Alpha = (finalOutput.a * IN.color.a * 1.8);

                    o.Albedo = finalOutput2;

                }

                ENDCG


            }
        }
}
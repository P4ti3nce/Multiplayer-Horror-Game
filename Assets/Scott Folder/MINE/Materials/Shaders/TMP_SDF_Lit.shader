Shader "TextMeshPro/URP/SDF Lit"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 1)) = 0.1
        _OutlineSoftness ("Outline Softness", Range(0, 1)) = 0.05
        _GradientScale ("Gradient Scale", Float) = 10.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 posWS       : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FaceColor;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;
            float _GradientScale;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float3 ApplyLighting(float3 normalWS, float3 posWS, float3 baseColor)
            {
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lit = baseColor * (max(0, dot(normalWS, -lightDir)) * mainLight.color.rgb);

                // Additional lights
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < lightCount; ++i)
                {
                    Light light = GetAdditionalLight(i, posWS);
                    float3 lightDirAdd = normalize(light.direction);
                    float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                    float ndotl = max(0, dot(normalWS, lightDirAdd));
                    lit += baseColor * ndotl * light.color.rgb * attenuation;
                }

                //return lit + baseColor * 0.1; // Ambient boost
				return lit;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float sdf = tex2D(_MainTex, IN.uv).a;

                float center = 0.5;
                float halfOutline = _OutlineWidth * 0.5;
                float softness = max(_OutlineSoftness / _GradientScale, 1e-5);

                float outlineStart = center - halfOutline;
                float outlineEnd   = center + halfOutline;

                float outline = smoothstep(outlineStart - softness, outlineStart + softness, sdf) -
                                smoothstep(outlineEnd - softness, outlineEnd + softness, sdf);

                float face = smoothstep(outlineEnd - softness, outlineEnd + softness, sdf);

                float alpha = saturate(outline + face);
                float3 color = lerp(_OutlineColor.rgb, _FaceColor.rgb, face);

                float3 litColor = ApplyLighting(normalize(IN.normalWS), IN.posWS, color);

                return float4(litColor, alpha * _FaceColor.a);
            }
            ENDHLSL
        }
    }
}

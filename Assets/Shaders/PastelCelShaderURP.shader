Shader "Custom/PastelCelShaderURP"
{
    Properties
    {
        [MainTexture] _BaseMap   ("Base Texture", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)
        _RampTex      ("Lighting Ramp (1D)", 2D) = "gray" {}
        _BrightBoost  ("Pastel Brightness Boost", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }
        Cull Back
        ZWrite On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            // Main-light shadow variants
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures & samplers
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_RampTex); SAMPLER(sampler_RampTex);

            // SRP Batcher-friendly per-material constants
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _BrightBoost;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 shadowCoord: TEXCOORD3;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // World & clip positions
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);

                // World-space normal
                OUT.normalWS = NormalizeNormalPerVertex(TransformObjectToWorldNormal(IN.normalOS));

                // UVs
                OUT.uv = IN.uv;

                // Shadow coord for main light
                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.shadowCoord = GetShadowCoord(vp);

                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                // Base color
                float3 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
                float3 baseCol = baseTex * _BaseColor.rgb;

                // Normal (world)
                float3 N = normalize(IN.normalWS);

                // Main light with shadowing
                Light mainLight = GetMainLight(IN.shadowCoord);

                // Classic N·L (ensure light points from light to surface -> use -direction)
                float NdotL = dot(N, -normalize(mainLight.direction));

                // Half-Lambert to keep it soft 0..1
                float nl01 = saturate(0.5 * NdotL + 0.5);

                // Sample 1D ramp (V can be anything constant; use 0.5)
                float rampV = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(nl01, 0.5)).r;

                // Pastel floor
                rampV = max(rampV, _BrightBoost);

                // Attenuation (distance=1 for directional; include shadows)
                float atten = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                // Final (matte)
                float3 lit = baseCol * rampV * mainLight.color * atten;

                return float4(lit, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

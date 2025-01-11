
Shader "Skybox/Dual Cubemap"
{
    Properties
    {
        _Tint1("Tint Color 1", Color) = (.5, .5, .5, .5)
        _Tint2("Tint Color 2", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure1("Exposure 1", Range(0, 8)) = 1.0
        [Gamma] _Exposure2("Exposure 2", Range(0, 8)) = 1.0
        _Rotation1("Rotation1", Range(0, 360)) = 0
        _Rotation2("Rotation2", Range(0, 360)) = 0
        [NoScaleOffset] _Cubemap1("Cubemap 1", CUBE) = "white" {}
        [NoScaleOffset] _Cubemap2("Cubemap 2", CUBE) = "white" {}
        _Blend("Blend", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            samplerCUBE _Cubemap1;
            samplerCUBE _Cubemap2;

            half4 _Tint1;
            half4 _Tint2;
            half _Exposure1;
            half _Exposure2;
            float _Rotation1;
            float _Rotation2;
            float _Blend;

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Rotate texture coordinates
                float3 rotatedCoords1 = RotateAroundYInDegrees(i.texcoord, _Rotation1);
                float3 rotatedCoords2 = RotateAroundYInDegrees(i.texcoord, _Rotation2);

                // Sample cubemaps
                half4 tex1 = texCUBE(_Cubemap1, rotatedCoords1);
                half4 tex2 = texCUBE(_Cubemap2, rotatedCoords2);

                // Blend colors
                half3 blendedColor = lerp(tex1.rgb, tex2.rgb, _Blend);

                // Apply tint and exposure
                blendedColor *= lerp(_Tint1.rgb * _Exposure1, _Tint2.rgb * _Exposure2, _Blend);

                return half4(blendedColor, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

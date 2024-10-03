Shader "Custom/Mv_VoxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _EdgeThickness ("Edge Thickness", Range(0, 0.1)) = 0.01
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _ColorTint;
            float _EdgeThickness;
            float4 _EdgeColor;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float3 normalWS     : NORMAL;
                float3 viewDirWS    : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _ColorTint;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 finalColor = texColor * IN.color;
                 
                float3 normalWS = normalize(IN.normalWS);
                float3 lightDir = GetMainLight().direction;
                float NdotL = dot(normalWS, lightDir);
                float lighting = saturate(NdotL * 0.5 + 0.5);

                float3 viewDirWS = normalize(IN.viewDirWS);
                float edgeFactor = 1 - saturate(dot(viewDirWS, normalWS));
                float edge = edgeFactor > (1 - _EdgeThickness) ? 1 : 0;

                finalColor.rgb = lerp(finalColor.rgb * lighting, _EdgeColor.rgb, edge);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
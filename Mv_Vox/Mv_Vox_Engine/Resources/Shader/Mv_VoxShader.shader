Shader "Custom/Mv_VoxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _EdgeThickness ("Edge Thickness", Range(0, 0.1)) = 0.01
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl" 

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _ColorTint;
            float _EdgeThickness;
            float4 _EdgeColor;
            float _Smoothness;
            float _Metallic;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL; 
                float3 viewDirWS : TEXCOORD2; 
                float2 uv : TEXCOORD3; 
                float4 color : COLOR; 
                float4 shadowCoord : TEXCOORD4;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _ColorTint;

                // Передаем координаты теней
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 vertexColor = texColor * IN.color;
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.normalWS);
                float3 lightDirWS = normalize(mainLight.direction);
                float NdotL = max(dot(normalWS, lightDirWS), 0);
                half3 shadowColor = (0.5 * (1 - NdotL)) + 0.5;
                float shadowFactor = MainLightRealtimeShadow(IN.shadowCoord);

                if (NdotL > 0)
                {
                    half4 finalColor = vertexColor;
                    finalColor.rgb *= NdotL * mainLight.color.rgb;
                    finalColor.rgb *= (shadowFactor * 0.5) + 0.5;
                    return saturate(finalColor);
                }
                else
                {
                    half4 finalColor = vertexColor;
                    finalColor.rgb *= shadowColor;
                    return saturate(finalColor);
                }
            }
            ENDHLSL
        } 
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}

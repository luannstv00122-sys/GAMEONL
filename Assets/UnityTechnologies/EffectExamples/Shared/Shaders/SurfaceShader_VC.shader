Shader "Custom/SurfaceShader_VC"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Normal]
        _Normal ("Normal Map", 2D) = "bump" {}

        _NormalStrength ("Normal Strength", Range(0,2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 300

        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UniversalForward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            // Lighting
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma multi_compile _ _SHADOWS_SOFT

            // Fog
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Normal);
            SAMPLER(sampler_Normal);

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;
                float4 _MainTex_ST;
                float4 _Normal_ST;
                float _NormalStrength;

            CBUFFER_END


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;

                float2 uv         : TEXCOORD0;

                float4 color      : COLOR;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float2 uv         : TEXCOORD0;

                float3 positionWS : TEXCOORD1;

                float3 normalWS   : TEXCOORD2;

                float4 tangentWS  : TEXCOORD3;

                float4 color      : COLOR;

                float fogFactor   : TEXCOORD4;
            };


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(
                        IN.normalOS,
                        IN.tangentOS
                    );

                OUT.positionCS = positionInputs.positionCS;

                OUT.positionWS = positionInputs.positionWS;

                OUT.normalWS = normalInputs.normalWS;

                OUT.tangentWS = float4(
                    normalInputs.tangentWS,
                    IN.tangentOS.w
                );

                OUT.uv = TRANSFORM_TEX(
                    IN.uv,
                    _MainTex
                );

                OUT.color = IN.color;

                OUT.fogFactor = ComputeFogFactor(
                    positionInputs.positionCS.z
                );

                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                // ----------------------------------------
                // ALBEDO
                // ----------------------------------------

                half4 albedoTexture =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        IN.uv
                    );

                half4 finalColor =
                    albedoTexture * _Color;

                // Particle System Vertex Color
                finalColor.rgb *= IN.color.rgb;

                finalColor.a *= IN.color.a;


                // ----------------------------------------
                // NORMAL MAP
                // ----------------------------------------

                half4 normalTexture =
                    SAMPLE_TEXTURE2D(
                        _Normal,
                        sampler_Normal,
                        IN.uv
                    );

                half3 normalTS =
                    UnpackNormalScale(
                        normalTexture,
                        _NormalStrength
                    );


                // ----------------------------------------
                // TANGENT SPACE → WORLD SPACE
                // ----------------------------------------

                half3 normalWS =
                    normalize(IN.normalWS);

                half3 tangentWS =
                    normalize(IN.tangentWS.xyz);

                half3 bitangentWS =
                    normalize(
                        cross(
                            normalWS,
                            tangentWS
                        )
                        * IN.tangentWS.w
                    );

                half3x3 tangentToWorld =
                    half3x3(
                        tangentWS,
                        bitangentWS,
                        normalWS
                    );

                normalWS =
                    normalize(
                        mul(
                            normalTS,
                            tangentToWorld
                        )
                    );


                // ----------------------------------------
                // INPUT DATA
                // ----------------------------------------

                InputData inputData = (InputData)0;

                inputData.positionWS =
                    IN.positionWS;

                inputData.normalWS =
                    normalWS;

                inputData.viewDirectionWS =
                    SafeNormalize(
                        GetWorldSpaceViewDir(
                            IN.positionWS
                        )
                    );

                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(
                        IN.positionCS
                    );

                inputData.fogCoord =
                    IN.fogFactor;


                // ----------------------------------------
                // SURFACE DATA
                // ----------------------------------------

                SurfaceData surfaceData =
                    (SurfaceData)0;

                surfaceData.albedo =
                    finalColor.rgb;

                surfaceData.metallic =
                    0;

                surfaceData.specular =
                    half3(0, 0, 0);

                surfaceData.smoothness =
                    0;

                surfaceData.normalTS =
                    normalTS;

                surfaceData.emission =
                    half3(0, 0, 0);

                surfaceData.occlusion =
                    1;

                surfaceData.alpha =
                    finalColor.a;

                surfaceData.clearCoatMask =
                    0;

                surfaceData.clearCoatSmoothness =
                    0;


                // ----------------------------------------
                // URP PBR LIGHTING
                // ----------------------------------------

                half4 color =
                    UniversalFragmentPBR(
                        inputData,
                        surfaceData
                    );


                // ----------------------------------------
                // FOG
                // ----------------------------------------

                color.rgb =
                    MixFog(
                        color.rgb,
                        inputData.fogCoord
                    );

                return color;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
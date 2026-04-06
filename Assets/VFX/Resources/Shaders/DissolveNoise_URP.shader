Shader "Custom/Particles/DissolveNoise_URP"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _TextureNoise("Texture Noise", 2D) = "white" {}
        _Dissolvenoise("Dissolve noise", 2D) = "white" {}

        _NoisespeedXYEmissonZPowerW("Noise speed XY / Emisson Z / Power W", Vector) = (0.5,0,2,1)
        _DissolvespeedXY("Dissolve speed XY", Vector) = (0,0,0,0)

        _Maincolor("Main color", Color) = (0.7609469,0.8547776,0.9433962,1)
        _Noisecolor("Noise color", Color) = (0.2470588,0.3012382,0.3607843,1)
        _Dissolvecolor("Dissolve color", Color) = (1,1,1,1)

        [Toggle]_Usetexturecolor("Use texture color", Float) = 0
        [Toggle]_Usetexturedissolve("Use texture dissolve", Float) = 0

        _Opacity("Opacity", Range(0, 1)) = 1

        [Toggle]_Usedepth ("Use depth?", Float) = 0
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // Optional but fine for particles
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_TextureNoise);   SAMPLER(sampler_TextureNoise);
            TEXTURE2D(_Dissolvenoise);  SAMPLER(sampler_Dissolvenoise);

            float4 _MainTex_ST;
            float4 _TextureNoise_ST;
            float4 _Dissolvenoise_ST;

            float4 _NoisespeedXYEmissonZPowerW;
            float4 _DissolvespeedXY;

            float4 _Maincolor;
            float4 _Noisecolor;
            float4 _Dissolvecolor;

            float _Usetexturecolor;
            float _Usetexturedissolve;

            float _Opacity;
            float _Usedepth;
            float _InvFade;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;

                // IMPORTANT: Particle System is providing TEXCOORD0 as a float4:
                // xy = UV, zw = UV2 (because you added "UV2 (TEXCOORD0.zw)")
                float4 uv0        : TEXCOORD0;

                // Custom1.xyzw is TEXCOORD1.xyzw
                float4 custom1    : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float4 uv0        : TEXCOORD0;
                float4 custom1    : TEXCOORD1;

                // For soft particles
                float2 screenUV   : TEXCOORD2;
                float  eyeDepth   : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            static float2 ComputeScreenUV(float4 positionCS)
            {
                // Convert clip space -> normalized screen UV (0..1)
                float2 uv = positionCS.xy / max(positionCS.w, 1e-5);
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(positionWS);

                o.screenUV = ComputeScreenUV(o.positionCS);

                // Eye depth: positive forward distance in view space
                float3 positionVS = TransformWorldToView(positionWS);
                o.eyeDepth = -positionVS.z;

                o.color = v.color;
                o.uv0 = v.uv0;
                o.custom1 = v.custom1;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Soft particles depth fade (optional)
                half alphaMul = 1.0h;
                if (_Usedepth > 0.5)
                {
                    // Sample scene depth (raw) and convert to eye depth
                    float rawDepth = SampleSceneDepth(i.screenUV);
                    float sceneEye = LinearEyeDepth(rawDepth, _ZBufferParams);

                    float fade = saturate(_InvFade * (sceneEye - i.eyeDepth));
                    alphaMul *= (half)fade;
                }

                // Unpack values like the original shader
                float Emission = _NoisespeedXYEmissonZPowerW.z;
                float2 noiseSpeed = _NoisespeedXYEmissonZPowerW.xy;
                float  noisePower = _NoisespeedXYEmissonZPowerW.w;

                float2 dissolveSpeed = _DissolvespeedXY.xy;

                // This matches the original: W120 = uv1.z (Custom1.z)
                float W120 = i.custom1.z;

                // uv0 is float4:
                // i.uv0.xy = UV
                // i.uv0.zw = UV2 (packed)
                float2 uv = i.uv0.xy;

                // MainTex sample
                float2 uvMain = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain);
                float mainTexR = mainTex.r;

                // TextureNoise panner (matches your math closely)
                float2 uvTexNoise = uv * _TextureNoise_ST.xy + _TextureNoise_ST.zw;
                float2 pannerNoise = (_Time.y * noiseSpeed) + (W120 + float2(0.2, 0.4) + uvTexNoise);

                half4 texNoise = SAMPLE_TEXTURE2D(_TextureNoise, sampler_TextureNoise, pannerNoise);

                // clamp( pow(texNoise, power) * power, 0..1 )
                half4 noisePow = pow(texNoise, noisePower.xxxx);
                half4 noiseMask = saturate(noisePow * noisePower);

                // lerp(MainColor, NoiseColor, noiseMask)
                half4 baseCol = lerp(_Maincolor, _Noisecolor, noiseMask);

                // Dissolve noise panner
                float2 uvDiss = (i.custom1.xy * _Dissolvenoise_ST.xy) + _Dissolvenoise_ST.zw;
                float2 pannerDiss = (_Time.y * dissolveSpeed) + (uvDiss + W120);
                half4 dissTex = SAMPLE_TEXTURE2D(_Dissolvenoise, sampler_Dissolvenoise, pannerDiss);

                // In original:
                // temp_output_88_0 = step( lerp(diss.r, diss.r * mainTexR, _Usetexturedissolve), uv0_TextureNoise.z )
                // where uv0_TextureNoise.z = i.uv0.z (because TEXCOORD0.z comes from UV2.x)
                float dissValue = lerp(dissTex.r, dissTex.r * mainTexR, _Usetexturedissolve);
                float stepMask = step(dissValue, i.uv0.z);

                // temp_output_93_0 = baseCol * (1 - stepMask)
                half4 insideCol = baseCol * (1.0h - (half)stepMask);

                // Edge band shaping (ported as-is)
                float a = (1.0 - i.uv0.z);
                float remapA = (-0.65 + (a - 0.0) * (0.65 - -0.65) / (1.0 - 0.0));
                float edgeBand = (-4.0 + ((remapA + dissValue) - 0.0) * (7.0 - -4.0) / (1.0 - 0.0)) * 3.0;
                float edgeClamp = saturate(edgeBand);

                // Lerp between inside color and dissolve color on edge * stepMask
                half4 texColorOn = insideCol * mainTex;
                half4 baseToggle = lerp(insideCol, texColorOn, _Usetexturecolor);

                half4 dissColorOn = lerp(_Dissolvecolor, _Dissolvecolor * mainTex, _Usetexturecolor);

                half4 finalCol = lerp(baseToggle, dissColorOn, (half)(edgeClamp * stepMask));

                // Alpha shaping (ported as-is)
                float remapW = (-0.65 + (i.uv0.w - 0.0) * (0.65 - -0.65) / (1.0 - 0.0));
                float alphaBand = (-15.0 + ((dissValue + remapW) - 0.0) * (15.0 - -15.0) / (1.0 - 0.0));
                float alphaClamp = saturate(alphaBand);

                // Output: rgb = Emission * finalCol * vertexColor, alpha = vertexAlpha * mainTex.a * alphaClamp * Opacity
                half4 vc = i.color;
                half3 rgb = (half3)(Emission * finalCol.rgb) * vc.rgb;

                half aOut = vc.a * mainTex.a * (half)alphaClamp * (half)_Opacity;
                aOut *= alphaMul;

                return half4(rgb, aOut);
            }
            ENDHLSL
        }
    }
}
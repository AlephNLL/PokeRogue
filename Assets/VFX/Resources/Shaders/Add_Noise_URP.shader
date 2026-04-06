Shader "Custom/Particles/Add_Noise_URP"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,0.503)
        _Emission ("Emission", Float) = 2
        _MainTexUspeed ("MainTex U speed", Float) = 0
        _MainTexVspeed ("MainTex V speed", Float) = 0
        _Noise ("Noise", 2D) = "white" {}
        _NoiseUspeed ("Noise U speed", Float) = 0
        _NoiseVspeed ("Noise V speed", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP core
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            float4 _Noise_ST;

            float4 _TintColor;
            float  _Emission;
            float  _MainTexUspeed;
            float  _MainTexVspeed;
            float  _NoiseUspeed;
            float  _NoiseVspeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float t = _Time.y;

                float2 uvMain  = i.uv + float2(_MainTexUspeed, _MainTexVspeed) * t;
                float2 uvNoise = i.uv + float2(_NoiseUspeed,  _NoiseVspeed)  * t;

                float2 uvMainST  = uvMain  * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 uvNoiseST = uvNoise * _Noise_ST.xy   + _Noise_ST.zw;

                half4 mainTex  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMainST);
                half4 noiseTex = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uvNoiseST);

                half alphaScale = mainTex.a * noiseTex.a * i.color.a * _TintColor.a;

                half3 emissive =
                    (mainTex.rgb * noiseTex.rgb * i.color.rgb * _TintColor.rgb * _Emission) * alphaScale;

                // Additive output, alpha isn't used much; keep 1 like legacy shader
                return half4(emissive, 1);
            }
            ENDHLSL
        }
    }
}
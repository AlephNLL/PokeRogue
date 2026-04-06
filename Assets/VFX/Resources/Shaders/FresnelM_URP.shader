Shader "Custom/FresnelM_URP"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Borderfresnel ("Border fresnel", Range(0, 10)) = 1
        _Borderfresnelcolor ("Border fresnel color", Color) = (1,1,1,1)
        _Noise ("Noise", 2D) = "white" {}
        _Fresnelstrench ("Fresnel strench", Float) = 2
        _NoiseUspeed ("Noise U speed", Float) = 0.3
        _NoiseVspeed ("Noise V speed", Float) = 0.2
        _Cutoff ("Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            Cull Off
            ZWrite On
            // Cutout materials are effectively opaque in depth; keep ZWrite on.

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;
            float  _Borderfresnel;
            float4 _Borderfresnelcolor;
            float  _Fresnelstrench;
            float  _NoiseUspeed;
            float  _NoiseVspeed;
            float  _Cutoff;

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
            float4 _Noise_ST;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 uv0        : TEXCOORD0; // keep float4 because you use uv0.z
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 uv0         : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float4 color       : COLOR;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS  = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.normalWS    = TransformObjectToWorldNormal(v.normalOS);
                o.uv0         = v.uv0;
                o.color       = v.color;
                return o;
            }

            half4 frag (Varyings i, half facing : VFACE) : SV_Target
            {
                // Match your faceSign logic
                half faceSign = (facing >= 0) ? 1.0h : -1.0h;

                float3 n = normalize(i.normalWS) * faceSign;
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));

                // Scroll noise using uv0.xy
                float t = _Time.y;
                float2 uvNoise = i.uv0.xy + t * float2(_NoiseUspeed, _NoiseVspeed);
                float2 uvNoiseST = uvNoise * _Noise_ST.xy + _Noise_ST.zw;

                half4 noiseTex = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uvNoiseST);

                // Luma like your dot(rgb, 0.3/0.59/0.11) multiplied by uv0.z (your i.uv0.b)
                half noiseLuma = dot(noiseTex.rgb, half3(0.3h, 0.59h, 0.11h));
                half mask = noiseLuma * (half)i.uv0.z;

                // Your shader: clip(mask - 0.5);
                clip(mask - (half)_Cutoff);

                // Fresnel term: pow(1 - max(0, dot(n, viewDir)), Borderfresnel)
                half ndv = saturate(dot(n, v));
                half fres = pow(1.0h - ndv, (half)_Borderfresnel);

                half3 rim = _Borderfresnelcolor.rgb * ((half)_Fresnelstrench * fres) * noiseTex.rgb * i.color.rgb;

                // Your shader adds base color + saturate(rim)
                half3 emissive = _Color.rgb + saturate(rim);

                return half4(emissive, 1);
            }
            ENDHLSL
        }

        // Optional: if you need correct shadows from the cutout, add a ShadowCaster pass.
        // Many VFX-ish cutouts don’t need it, but “opaque cutout meshes” often do.
    }
}
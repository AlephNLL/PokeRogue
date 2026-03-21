Shader "ErbGameArt/Particles/Add_Noise" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,0.503)
        _Emission ("Emission", Float ) = 2
        _MainTexUspeed ("MainTex U speed", Float ) = 0
        _MainTexVspeed ("MainTex V speed", Float ) = 0
        _Noise ("Noise", 2D) = "white" {}
        _NoiseUspeed ("Noise U speed", Float ) = 0
        _NoiseVspeed ("Noise V speed", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
			Cull off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _Emission;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _NoiseUspeed;
            uniform float _NoiseVspeed;
            uniform float _MainTexUspeed;
            uniform float _MainTexVspeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 nnn = ((float2(_MainTexUspeed,_MainTexVspeed)*_Time.g)+i.uv0);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(nnn, _MainTex));
                float2 sss = ((float2(_NoiseUspeed,_NoiseVspeed)*_Time.g)+i.uv0);
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(sss, _Noise));
                float3 emissive = ((_MainTex_var.rgb*_Noise_var.rgb*i.vertexColor.rgb*_TintColor.rgb*_Emission)*_MainTex_var.a*_Noise_var.a*i.vertexColor.a*_TintColor.a);
                fixed4 finalRGBA = fixed4(emissive,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}

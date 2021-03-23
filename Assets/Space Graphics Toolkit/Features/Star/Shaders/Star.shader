Shader "Space Graphics Toolkit/Star"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Brightness("Brightness", Float) = 1.0
		_MainWarp("Noise Warp", Float) = 0.005
		[NoScaleOffset]_MainTex("Texture (RGB)", 2D) = "white" {}

		[Header(RIM)]
		_RimColor("	Color", Color) = (1,1,1,1)
		_RimPower("	Power", Float) = 2.0

		[Header(NOISE)]
		_NoiseColor("	Channels", Color) = (1,1,1,1)
		_NoiseStrength("	Strength", Float) = 0.1
		_NoiseTile("	Tile", Float) = 1.0
		_NoiseSpeed("	Speed", Float) = 10.0
		_NoiseOctaves("	Octaves", Range(1,6)) = 6

		[Header(FLOW)]
		_FlowStrength("	Strength", Float) = 1.0
		[NoScaleOffset]_FlowTex("	Texture (A)", 2D) = "black" {}

		[Header(SUNSPOTS)]
		_SunspotsColor("	Color", Color) = (1,1,1,1)
		_SunspotsStrength("	Strength", Float) = 1.0
		_SunspotsWarp("	Noise Warp", Float) = 0.005
		[NoScaleOffset]_SunspotsTex("	Texture (A)", 2D) = "gray" {}

		[Header(OTHER)]
		[NoScaleOffset]_NoiseTex("	Noise Texture (RG)", 3D) = "black" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define NOISE_OCTAVES 6
			#include "UnityCG.cginc"

			float3    _Color;
			float     _Brightness;
			sampler2D _MainTex;
			float     _MainWarp;

			float3 _RimColor;
			float  _RimPower;

			sampler3D _NoiseTex;
			float4    _NoiseTex_TexelSize;
			float3    _NoiseColor;
			float     _NoiseStrength;
			float     _NoiseTile;
			float     _NoiseSpeed;
			int       _NoiseOctaves;

			sampler2D _FlowTex;
			float     _FlowStrength;

			sampler2D _SunspotsTex;
			float3    _SunspotsColor;
			float     _SunspotsWarp;
			float     _SunspotsStrength;

			float Noise4D(float4 p)
			{
				const float3 noiseStep = float3(23, 29, 31);

				float4 i     = floor(p);
				float4 f     = smoothstep(0.0, 1.0, frac(p));
				float3 pixel = i.xyz + f.xyz + i.w * noiseStep;
				float3 grad  = p / _NoiseTex_TexelSize.w;
				float4 noise = tex3Dgrad(_NoiseTex, (pixel + 0.5) / _NoiseTex_TexelSize.w, ddx(grad), ddy(grad));

				return lerp(noise.x, noise.y, f.w);
			}

			struct a2v
			{
				float4 vertex    : POSITION;
				float4 normal    : NORMAL;
				float2 texcoord0 : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex      : SV_POSITION;
				float2 texcoord0   : TEXCOORD0;
				float3 localPos    : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 worldRefl   : TEXCOORD3;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out v2f o)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex      = UnityObjectToClipPos(i.vertex);
				o.texcoord0   = i.texcoord0;
				o.localPos    = i.vertex.xyz;
				o.worldNormal = mul((float3x3)unity_ObjectToWorld, i.normal);
				o.worldRefl   = mul(unity_ObjectToWorld, i.vertex).xyz - _WorldSpaceCameraPos;
			}

			void Frag(v2f i, out f2g o)
			{
				// Normalize vectors before use
				i.worldNormal = normalize(i.worldNormal);
				i.worldRefl   = normalize(i.worldRefl);

				// Fbm noise
				float  str   = 1.0f;
				float  off   = tex2D(_FlowTex, i.texcoord0.xy).a * _FlowStrength;
				float4 pos   = float4(i.localPos * _NoiseTile, off);
				float  noise = 0.0f;

				for (int j = 0; j < _NoiseOctaves; j++)
				{
					pos.w += _Time.x * _NoiseSpeed;
					noise += (Noise4D(pos) - 0.5f) * str;
					str   /= 2.0f;
					pos   *= 2.0f;
				}

				// Make the base color the main tex * rim gradient
				float nfDot = abs(dot(i.worldNormal, i.worldRefl));
				float rim = 1.0f - pow(1.0f - nfDot, _RimPower);

				float2 mt_uv = i.texcoord0.xy; mt_uv.x += noise * _MainWarp;
				o.color.rgb = lerp(_RimColor, _Color, rim) * tex2D(_MainTex, mt_uv) * _Brightness;
				o.color.a = 1.0f;

				// Mix in noise
				o.color.rgb += _NoiseColor * _NoiseStrength * noise;

				// Sunspots
				float2 ss_uv = i.texcoord0.xy; ss_uv.x += noise * _SunspotsWarp;
				o.color.rgb += (tex2D(_SunspotsTex, ss_uv).a - 0.5f) * _SunspotsColor * _SunspotsStrength;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
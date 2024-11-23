// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "N5KK7/Sprite w. Rounded Edge"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Enum(Off,0,Front,1,Back,2)]_CullMode("Cull Mode", Float) = 2
		[Enum(OFF,0,ON,1)]_ZWrite("Z Write", Float) = 1
		_Color("Color", Color) = (1,1,1,0)
		_MainTex("Sprite Texture", 2D) = "white" {}
		_RimSize("Rim Size", Float) = 1
		_RimRadius("Rim Radius", Range( 0 , 1)) = 0
		_Zoom("Zoom", Float) = 1
		[KeywordEnum(SpriteTextureRGB,SpriteTextureAlpha,GlowMap)] _GlowSource("Glow Source", Float) = 0
		_GlowMap("GlowMap", 2D) = "white" {}
		_Glow("Glow", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+0" "PreviewType"="Plane" }
		Cull [_CullMode]
		ZWrite [_ZWrite]
		Lighting Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _GLOWSOURCE_MainTexRGB _GLOWSOURCE_MainTexALPHA _GLOWSOURCE_GLOWMAP
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _ZWrite;
		uniform float _CullMode;
		uniform sampler2D _MainTex;
		uniform float _Zoom;
		uniform float4 _MainTex_ST;
		uniform float4 _Color;
		uniform sampler2D _GlowMap;
		uniform float4 _GlowMap_ST;
		uniform float _Glow;
		uniform float _RimSize;
		uniform float _RimRadius;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 temp_output_25_0 = ( ( i.uv_texcoord * _Zoom ) + ( ( 1.0 - _Zoom ) * 0.5 ) );
			float4 tex2DNode10 = tex2D( _MainTex, (temp_output_25_0*_MainTex_ST.xy + _MainTex_ST.zw) );
			float4 temp_output_12_0 = ( tex2DNode10 * tex2DNode10.a * _Color );
			o.Emission = temp_output_12_0.rgb;
			float4 temp_cast_1 = (tex2DNode10.a).xxxx;
			#if defined(_GLOWSOURCE_MainTexRGB)
				float4 staticSwitch52 = temp_output_12_0;
			#elif defined(_GLOWSOURCE_MainTexALPHA)
				float4 staticSwitch52 = temp_cast_1;
			#elif defined(_GLOWSOURCE_GLOWMAP)
				float4 staticSwitch52 = tex2D( _GlowMap, (temp_output_25_0*_GlowMap_ST.xy + _GlowMap_ST.zw) );
			#else
				float4 staticSwitch52 = temp_output_12_0;
			#endif
			o.Alpha = ( staticSwitch52 * _Glow ).r;
			float temp_output_2_0_g4 = _RimSize;
			float temp_output_3_0_g4 = _RimSize;
			float2 appendResult21_g4 = (float2(temp_output_2_0_g4 , temp_output_3_0_g4));
			float Radius25_g4 = max( min( min( abs( ( _RimRadius * 2 ) ) , abs( temp_output_2_0_g4 ) ) , abs( temp_output_3_0_g4 ) ) , 1E-05 );
			float2 temp_cast_3 = (0.0).xx;
			float temp_output_30_0_g4 = ( length( max( ( ( abs( (i.uv_texcoord*2.0 + -1.0) ) - appendResult21_g4 ) + Radius25_g4 ) , temp_cast_3 ) ) / Radius25_g4 );
			clip( saturate( ( ( 1.0 - temp_output_30_0_g4 ) / fwidth( temp_output_30_0_g4 ) ) ) - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}

//CHKSM=7143F9E3A8DED86E5919BF22FF22590540DA55A3
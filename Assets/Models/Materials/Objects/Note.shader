// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MicroMapper/Note"
{
    Properties
    {
        _NoteColor("Note Color", Color) = (1,0,0,0)
        [Toggle]_Transparent("Transparent", Float) = 0
        [Toggle]_Unlit("Unlit", Float) = 0
        _ScreenspaceTexture("Screenspace Texture", 2D) = "white" {}
        _ScreenspaceIntensity("Screenspace Intensity", Range(0, 10)) = 1
        [HideInInspector]_maskclip("mask clip", Float) = 0.5
        [HideInInspector] __dirty( "", Int ) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true" }
        Cull Back
        Blend One [_Transparent]

        ColorMask RGB
        CGINCLUDE
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        struct Input
        {
            float4 screenPos;
        };

        uniform float4 _NoteColor;
        uniform sampler2D _ScreenspaceTexture;
        uniform float _Transparent;
        uniform float _ScreenspaceIntensity;
        uniform float _maskclip;
        uniform float _Unlit;

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float4 temp_output_1_0 = _NoteColor;
            o.Albedo = temp_output_1_0.rgb;
            float4 ase_screenPos = float4(i.screenPos.xyz, i.screenPos.w + 0.00000000001);
            float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            float4 tex2DNode7 = tex2D(_ScreenspaceTexture, ase_screenPosNorm.xy);

            if (_Unlit == 1.0)
            {
                o.Emission = (_NoteColor * _ScreenspaceIntensity).rgb;
            }
            else
            {
                o.Emission = ((_NoteColor * tex2DNode7 * _Transparent) * _ScreenspaceIntensity).rgb;
            }

            float ifLocalVar28 = 0;
            if (_Transparent == 1.0)
                ifLocalVar28 = 0.0;
            else if (_Transparent < 1.0)
                ifLocalVar28 = 1.0;
            o.Alpha = ifLocalVar28;

            float4 temp_cast_3 = (1.0).xxxx;
            float4 ifLocalVar31 = 0;
            if (_Transparent == 1.0)
                ifLocalVar31 = tex2DNode7;
            else if (_Transparent < 1.0)
                ifLocalVar31 = temp_cast_3;
            clip(ifLocalVar31.r - _maskclip);
        }

        ENDCG
        CGPROGRAM
        #pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

        ENDCG
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = worldPos;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            half4 frag(v2f IN
            #if !defined(CAN_SKIP_VPOS)
            , UNITY_VPOS_TYPE vpos : VPOS
            #endif
            ) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Input surfIN;
                UNITY_INITIALIZE_OUTPUT(Input, surfIN);
                float3 worldPos = IN.worldPos;
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                surfIN.screenPos = IN.screenPos;
                SurfaceOutputStandard o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o)
                surf(surfIN, o);
                #if defined(CAN_SKIP_VPOS)
                float2 vpos = IN.pos;
                #endif
                half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy * 0.25, o.Alpha * 0.9375)).a;
                clip(alphaRef - 0.01);
                SHADOW_CASTER_FRAGMENT(IN)
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
    CustomEditor "ASEMaterialInspector"
}

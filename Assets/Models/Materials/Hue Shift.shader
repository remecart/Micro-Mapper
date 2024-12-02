Shader "Custom/HueShift"
{
    
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0.001,0.999)) = 0.5
    _HueShift ("Shift Hue", Color) = (0,0,0,0)
}
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100

    Lighting Off

    Pass {  
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _HueShift;
            fixed _Cutoff;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                //Shift the color here
                col.rgb = lerp(col.rgb, col.gbr, _HueShift.r); //90 degrees
                col.rgb = lerp(col.rgb, col.gbr, _HueShift.g); //180 degrees
                col.rgb = lerp(col.rgb, col.bgr, _HueShift.b); //270 degrees


                clip(col.a - _Cutoff);
                UNITY_APPLY_FOG(i.fogCoord, col);

                return lerp(
                    col,
                    fixed4(0, 4, 2, 1),
                    step(col.a, _Cutoff)
                );
            }
        ENDCG
    }
}
}

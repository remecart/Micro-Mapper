Shader "Custom/TransparentWireframeShader"
{
    Properties
    {
        _WireColor ("Wireframe Color", Color) = (1, 1, 1, 1)
        _Thickness ("Wireframe Thickness", Range(0.01, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            uniform float4 _WireColor;
            uniform float _Thickness;

            v2g vert(appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            [maxvertexcount(6)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                for (int i = 0; i < 3; i++)
                {
                    g2f o;
                    o.pos = input[i].pos;

                    float3 worldPos0 = input[0].worldPos;
                    float3 worldPos1 = input[1].worldPos;
                    float3 worldPos2 = input[2].worldPos;

                    float edge1 = length(worldPos1 - worldPos0);
                    float edge2 = length(worldPos2 - worldPos0);
                    float edgeFactor = min(edge1, edge2) * _Thickness;

                    o.color = float4(_WireColor.rgb, edgeFactor);
                    triStream.Append(o);
                }
            }

            float4 frag(g2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
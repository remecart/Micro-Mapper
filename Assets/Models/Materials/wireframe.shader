Shader "SPR/CubeWireframe"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _WireframeColor("Wireframe Color", color) = (1.0, 1.0, 1.0, 1.0)
        _WireframeAliasing("Wireframe aliasing", float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            // Cull front faces to allow the wireframe to be seen from the back
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream)
            {
                float edgeLengthX = length(IN[1].vertex - IN[2].vertex);
                float edgeLengthY = length(IN[0].vertex - IN[2].vertex);
                float edgeLengthZ = length(IN[0].vertex - IN[1].vertex);
                float3 modifier = float3(0.0, 0.0, 0.0);

                if ((edgeLengthX > edgeLengthY) && (edgeLengthX > edgeLengthZ)) {
                    modifier = float3(1.0, 0.0, 0.0);
                }
                else if ((edgeLengthY > edgeLengthX) && (edgeLengthY > edgeLengthZ)) {
                    modifier = float3(0.0, 1.0, 0.0);
                }
                else if ((edgeLengthZ > edgeLengthX) && (edgeLengthZ > edgeLengthY)) {
                    modifier = float3(0.0, 0.0, 1.0);
                }

                g2f o;
                o.pos = UnityObjectToClipPos(IN[0].vertex);
                o.barycentric = float3(1.0, 0.0, 0.0) + modifier;
                triStream.Append(o);

                o.pos = UnityObjectToClipPos(IN[1].vertex);
                o.barycentric = float3(0.0, 1.0, 0.0) + modifier;
                triStream.Append(o);

                o.pos = UnityObjectToClipPos(IN[2].vertex);
                o.barycentric = float3(0.0, 0.0, 1.0) + modifier;
                triStream.Append(o);
            }

            fixed4 _WireframeColor;
            float _WireframeAliasing;

            fixed4 frag(g2f i) : SV_Target
            {
                // Calculate the unit width based on triangle size.
                float3 unitWidth = fwidth(i.barycentric);
                // Alias the line a bit.
                float3 aliased = smoothstep(float3(0.0, 0.0, 0.0), unitWidth * _WireframeAliasing, i.barycentric);
                // Use the coordinate closest to the edge.
                float alpha = 1 - min(aliased.x, min(aliased.y, aliased.z));
                
                // Only render the wireframe by ensuring transparency for the rest
                if (alpha < 0.01)
                    discard;

                return fixed4(_WireframeColor.rgb, alpha);  // Set wireframe color and alpha
            }
            ENDCG
        }

        Pass
        {
            // Cull back faces to allow the wireframe to be seen from the front
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream)
            {
                float edgeLengthX = length(IN[1].vertex - IN[2].vertex);
                float edgeLengthY = length(IN[0].vertex - IN[2].vertex);
                float edgeLengthZ = length(IN[0].vertex - IN[1].vertex);
                float3 modifier = float3(0.0, 0.0, 0.0);

                if ((edgeLengthX > edgeLengthY) && (edgeLengthX > edgeLengthZ)) {
                    modifier = float3(1.0, 0.0, 0.0);
                }
                else if ((edgeLengthY > edgeLengthX) && (edgeLengthY > edgeLengthZ)) {
                    modifier = float3(0.0, 1.0, 0.0);
                }
                else if ((edgeLengthZ > edgeLengthX) && (edgeLengthZ > edgeLengthY)) {
                    modifier = float3(0.0, 0.0, 1.0);
                }

                g2f o;
                o.pos = UnityObjectToClipPos(IN[0].vertex);
                o.barycentric = float3(1.0, 0.0, 0.0) + modifier;
                triStream.Append(o);

                o.pos = UnityObjectToClipPos(IN[1].vertex);
                o.barycentric = float3(0.0, 1.0, 0.0) + modifier;
                triStream.Append(o);

                o.pos = UnityObjectToClipPos(IN[2].vertex);
                o.barycentric = float3(0.0, 0.0, 1.0) + modifier;
                triStream.Append(o);
            }

            fixed4 _WireframeColor;
            float _WireframeAliasing;

            fixed4 frag(g2f i) : SV_Target
            {
                float3 unitWidth = fwidth(i.barycentric);
                float3 aliased = smoothstep(float3(0.0, 0.0, 0.0), unitWidth * _WireframeAliasing, i.barycentric);
                float alpha = 1 - min(aliased.x, min(aliased.y, aliased.z));
                
                // Only render the wireframe by ensuring transparency for the rest
                if (alpha < 0.01)
                    discard;

                return fixed4(_WireframeColor.rgb, alpha);  // Set wireframe color and alpha
            }
            ENDCG
        }
    }
}

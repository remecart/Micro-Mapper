Shader "Custom/WaveformShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveformColor ("Waveform Color", Color) = (1, 1, 1, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _SamplePoints ("Sample Points", 2D) = "white" {}
        _Scale ("Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SamplePoints;
            float4 _WaveformColor;
            float4 _BackgroundColor;
            float _Scale;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Calculate the distance from a point to a line segment
            float PointToLineSegmentDistance(float2 p1, float2 p2, float2 a)
            {
                float2 ap1 = p1 - a;
                float2 ap2 = p2 - a;
                float2 p1p2 = p2 - p1;

                if (dot(p1p2, -ap1) < 0 || dot(-p1p2, -ap2) < 0)
                {
                    return min(length(ap1), length(ap2));
                }

                if (length(p1p2) < 1e-12)
                {
                    return length(ap1);
                }

                return abs(ap1.x * ap2.y - ap2.x * ap1.y) / length(p1p2);
            }

            // Fragment shader to render the waveform
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * _Scale;

                // Get the number of sample points
                int sampleCount = 1024; // Assume a fixed number for simplicity

                // Get the distance to the nearest waveform segment
                float minDistance = 1.0;
                for (int j = 0; j < sampleCount - 1; j++)
                {
                    float2 p1 = tex2D(_SamplePoints, float2(j / (float)sampleCount, 0.0)).xy;
                    float2 p2 = tex2D(_SamplePoints, float2((j + 1) / (float)sampleCount, 0.0)).xy;
                    float distance = PointToLineSegmentDistance(p1, p2, uv);
                    minDistance = min(minDistance, distance);
                }

                // Map distance to color intensity
                float intensity = saturate(1.0 - minDistance * 10.0);
                fixed4 color = lerp(_BackgroundColor, _WaveformColor, intensity);

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

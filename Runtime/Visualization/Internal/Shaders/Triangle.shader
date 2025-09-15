Shader "Visualization/Triangle"
{
    Properties
    {
        _Color("Fill Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _TrianglePointA("Triangle Point A", Vector) = (0,0,0,0)
        _TrianglePointB("Triangle Point B", Vector) = (1,0,0,0)
        _TrianglePointC("Triangle Point C", Vector) = (0.5,1,0,0)
        _RoundedEdges("Rounded Edges", Float) = 0
        _RoundRadius("Round Radius", Float) = 0.1
        _OutlineWidth("Outline Width", Float) = 0.05
        _UsePixelOutline("Use Pixel-based Outline", Float) = 0
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _Color;
            float4 _OutlineColor;
            float3 _TrianglePointA;
            float3 _TrianglePointB;
            float3 _TrianglePointC;
            float _RoundedEdges;
            float _RoundRadius;
            float _OutlineWidth;
            float _UsePixelOutline;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            // Signed distance to triangle
            float sdTriangle(float2 p, float2 a, float2 b, float2 c)
            {
                float2 e0 = b - a;
                float2 e1 = c - b;
                float2 e2 = a - c;
                
                float2 v0 = p - a;
                float2 v1 = p - b;
                float2 v2 = p - c;
                
                float2 pq0 = v0 - e0 * saturate(dot(v0, e0) / dot(e0, e0));
                float2 pq1 = v1 - e1 * saturate(dot(v1, e1) / dot(e1, e1));
                float2 pq2 = v2 - e2 * saturate(dot(v2, e2) / dot(e2, e2));
                
                float s = sign(e0.x * e2.y - e0.y * e2.x);
                float2 d = min(min(float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                                   float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                                   float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
                
                return -sqrt(d.x) * sign(d.y);
            }

            float4 frag (v2f i) : SV_Target
            {
                // Convert triangle points to screen/quad space
                float2 a = _TrianglePointA.xy;
                float2 b = _TrianglePointB.xy;
                float2 c = _TrianglePointC.xy;
                
                // Find bounding box
                float2 minBounds = min(min(a, b), c);
                float2 maxBounds = max(max(a, b), c);
                
                // Calculate the maximum expansion we might need
                float maxExpansion = _RoundRadius + _OutlineWidth;
                
                minBounds -= maxExpansion;
                maxBounds += maxExpansion;
                
                // Map UV to world position
                float2 worldPos2D = lerp(minBounds, maxBounds, i.uv);
                
                // Calculate base SDF
                float dist = sdTriangle(worldPos2D, a, b, c);
                
                // Apply rounding if enabled
                if (_RoundedEdges > 0.5)
                {
                    dist = dist - _RoundRadius;
                }
                
                // Calculate actual outline width
                float outlineWidth = _OutlineWidth;
                if (_UsePixelOutline > 0.5)
                {
                    // Get pixel size in world units by taking derivative of world position
                    float2 dWorlddx = ddx(worldPos2D);
                    float2 dWorlddy = ddy(worldPos2D);
                    float pixelSize = length(float2(length(dWorlddx), length(dWorlddy))) * 0.5;
                    outlineWidth = _OutlineWidth * pixelSize;
                }
                
                // Calculate antialiasing width (based on how fast the distance field changes)
                float aa = fwidth(dist) * 0.5;
                
                // SDF regions:
                // dist < 0: inside the shape
                // dist = 0: edge of the shape
                // dist > 0: outside the shape
                
                float4 col = float4(0, 0, 0, 0);
                
                if (outlineWidth > 0.001)
                {
                    // Fill region: everything inside the shape
                    float fillAlpha = smoothstep(aa, -aa, dist);
                    
                    // Outline region: from edge (0) to outline width
                    float outlineAlpha = smoothstep(outlineWidth + aa, outlineWidth - aa, dist) * 
                                        smoothstep(-aa, aa, dist);
                    
                    // Composite: outline on top of fill
                    col = _Color * fillAlpha;
                    col = lerp(col, _OutlineColor, outlineAlpha);
                }
                else
                {
                    // No outline, just render the fill with antialiasing
                    float alpha = smoothstep(aa, -aa, dist);
                    col = float4(_Color.rgb, _Color.a * alpha);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
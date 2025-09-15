Shader "Visualization/Rectangle"
{
    Properties
    {
        _Color("Fill Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _RectCenter("Rectangle Center", Vector) = (0.5,0.5,0,0)
        _RectSize("Rectangle Size", Vector) = (0.8,0.6,0,0)
        _RoundedCorners("Rounded Corners", Float) = 0
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
            float2 _RectCenter;
            float2 _RectSize;
            float _RoundedCorners;
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

            // Signed distance to box/rectangle
            float sdBox(float2 p, float2 center, float2 halfSize)
            {
                float2 d = abs(p - center) - halfSize;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            // Rounded box SDF
            float sdRoundedBox(float2 p, float2 center, float2 halfSize, float r)
            {
                float2 d = abs(p - center) - halfSize + r;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Rectangle parameters
                float2 center = _RectCenter.xy;
                float2 halfSize = _RectSize.xy * 0.5;
                
                // Calculate bounding box with padding for outline and rounding
                float maxExpansion = _RoundRadius + _OutlineWidth;
                float2 minBounds = center - halfSize - maxExpansion;
                float2 maxBounds = center + halfSize + maxExpansion;
                
                // Map UV to world position
                float2 worldPos2D = lerp(float2(0, 0), float2(1, 1), i.uv);
                
                // Calculate SDF
                float dist;
                if (_RoundedCorners > 0.5)
                {
                    dist = sdRoundedBox(worldPos2D, center, halfSize, _RoundRadius);
                }
                else
                {
                    dist = sdBox(worldPos2D, center, halfSize);
                }
                
                // Calculate actual outline width
                float outlineWidth = _OutlineWidth;
                if (_UsePixelOutline > 0.5)
                {
                    // Get pixel size in world units
                    float2 dWorlddx = ddx(worldPos2D);
                    float2 dWorlddy = ddy(worldPos2D);
                    float pixelSize = length(float2(length(dWorlddx), length(dWorlddy))) * 0.5;
                    outlineWidth = _OutlineWidth * pixelSize;
                }
                
                // Antialiasing width
                float aa = fwidth(dist) * 0.5;
                
                float4 col = float4(0, 0, 0, 0);
                
                if (outlineWidth > 0.001)
                {
                    // Fill alpha
                    float fillAlpha = smoothstep(aa, -aa, dist);
                    
                    // Outline alpha (between edge and outline width)
                    float outlineAlpha = smoothstep(outlineWidth + aa, outlineWidth - aa, dist) * 
                                        smoothstep(-aa, aa, dist);
                    
                    // Composite
                    col = _Color * fillAlpha;
                    col = lerp(col, _OutlineColor, outlineAlpha);
                }
                else
                {
                    // No outline
                    float alpha = smoothstep(aa, -aa, dist);
                    col = float4(_Color.rgb, _Color.a * alpha);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
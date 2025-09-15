Shader "Visualization/Circle"
{
    Properties
    {
        _Color("Fill Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _Center("Center", Vector) = (0.5,0.5,0,0)
        _Radius("Radius", Float) = 0.4
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
                float maxExpansion : TEXCOORD2; // Add this to pass maxExpansion to fragment shader
            };

            float4 _Color;
            float4 _OutlineColor;
            float2 _Center;
            float _Radius;
            float _OutlineWidth;
            float _UsePixelOutline;
            float _ObjectScale; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                
                // Calculate maxExpansion in vertex shader
                float maxExpansion = _Radius;
                if (_UsePixelOutline > 0.5)
                {
                    // Transform a unit vector from object to clip space to estimate scale
                    float4 origin = UnityObjectToClipPos(float4(0, 0, 0, 1));
                    float4 unit = UnityObjectToClipPos(float4(1, 0, 0, 1));
                    
                    // Get the clip space distance for 1 object space unit
                    float2 clipDist = (unit.xy/unit.w - origin.xy/origin.w);
                    
                    // Convert to pixel distance (clip space is -1 to 1, so multiply by screen size/2)
                    float pixelsPerObjectUnit = length(clipDist) * _ScreenParams.y * 0.5;
                    
                    // Calculate object space size of N pixels
                    float pixelSizeInObjectSpace = _OutlineWidth / pixelsPerObjectUnit;
                    
                    maxExpansion += pixelSizeInObjectSpace;
                }
                else
                {
                    maxExpansion += _OutlineWidth;
                }
                
                // Store maxExpansion in interpolator to use in fragment shader
                o.maxExpansion = maxExpansion; // Add this to v2f struct
                
                return o;
            }

            // Signed distance to circle
            float sdCircle(float2 p, float2 center, float radius)
            {
                return length(p - center) - radius;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Calculate bounding box for the circle
                float maxExpansion = i.maxExpansion; // Use interpolated maxExpansion


                float2 minBounds = _Center - maxExpansion;
                float2 maxBounds = _Center + maxExpansion;
                
                // Map UV to world position
                float2 worldPos2D = lerp(minBounds, maxBounds, i.uv);
                
                // Calculate SDF
                float dist = sdCircle(worldPos2D, _Center, _Radius);
                
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
                
                // Calculate antialiasing width
                float aa = fwidth(dist) * 0.5;
                
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
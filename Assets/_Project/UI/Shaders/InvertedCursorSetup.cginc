#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
#if UNITY_VERSION >= 202020
    float4 uv1 : TEXCOORD1;
#else
    float2 uv1 : TEXCOORD1;
#endif
    float2 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv : TEXCOORD0;
#if UNITY_VERSION >= 202020
    float4 uv1 : TEXCOORD1;
#else
    float2 uv1 : TEXCOORD1;
#endif
    uint2 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    float4 worldPosition : TEXCOORD4;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO
};

#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

#include "UnityUI.cginc"

float4 _ClipRect;
sampler2D _MaskTex;

v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.worldPosition = v.vertex;
    o.uv = v.uv;
    o.uv1 = v.uv1;
    o.uv2 = asuint(v.uv2);
    o.uv3 = v.uv3;
    o.color = v.color;
    return o;
}

inline fixed4 mixAlpha(fixed4 mainTexColor, float2 uv, fixed4 color, float4 world, float sdfAlpha)
{
    fixed4 col = mainTexColor * color;
    col.a = min(col.a, sdfAlpha);
    col.a *= tex2D(_MaskTex, uv).r;

#ifdef UNITY_UI_CLIP_RECT
    col.a *= UnityGet2DClipping(world.xy, _ClipRect);
#endif

#ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001);
#endif

    return col;
}

float2 decode2(uint value)
{
    uint aInt = value & 0x0000ffff;
    uint bInt = (value & 0xffff0000) >> 16;

    return float2((float)aInt, bInt) / 0x0000ffff;
}

float sdRoundedBox(in float2 p, in float2 s, in float4 r)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    float2 q = abs(p) - s + r.x;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

float AntialiasedCutoff(float distance, float amount)
{
    if (amount <= 0)
        return step(distance, 0);

    return smoothstep(amount, -amount, distance + amount);
}

float RoundedBox(float2 samplePosition, float2 size, float4 radius, float falloff)
{
    float2 samplePositionTranslated = (samplePosition - .5) * size;
    float distToRect = sdRoundedBox(samplePositionTranslated, size * .5, radius * .5 * min(size.x, size.y));
    return AntialiasedCutoff(distToRect, falloff);
}

float ShadowRoundedBox(float2 samplePosition, float2 size, float4 radius, float falloff)
{
    float2 samplePositionTranslated = (samplePosition - .5) * size;
    float distToRect = sdRoundedBox(samplePositionTranslated, size * .5, radius * .5 * min(size.x, size.y));
    if (falloff <= 0)
        return step(distToRect, 0);

    return smoothstep(0, -falloff, distToRect);
}

float RoundedBoxBorder(float2 samplePosition, float2 size, float4 radius, float falloff, float distance)
{
    float2 samplePositionTranslated = (samplePosition - .5) * size;
    float distToRect = abs(sdRoundedBox(samplePositionTranslated, size * .5, radius * .5 * min(size.x, size.y)) + distance) - distance;
    return AntialiasedCutoff(distToRect, falloff);
}

float DynamicRoundedBox(float2 samplePosition, float2 size, float4 radius, float falloff, float distance)
{
    const float normalizedBorder = distance / 0.25 / min(size.x, size.y);
    return normalizedBorder > 0.999
        ? RoundedBox(samplePosition, size, radius, falloff)
        : RoundedBoxBorder(samplePosition, size, radius, falloff, distance);
}

float BorderSideVisibility(float2 samplePosition, float2 size, float borderDistance, float sideMask, float hasSideData)
{
    if (hasSideData < 0.5 || sideMask > 14.5)
        return 1;

    if (sideMask < 0.5)
        return 0;

    float leftEnabled = fmod(floor(sideMask), 2.0);
    float rightEnabled = fmod(floor(sideMask / 2.0), 2.0);
    float topEnabled = fmod(floor(sideMask / 4.0), 2.0);
    float bottomEnabled = fmod(floor(sideMask / 8.0), 2.0);

    float2 position = (samplePosition - 0.5) * size;
    float2 distanceToEdge = size * 0.5 - abs(position);
    float verticalSide = step(distanceToEdge.x, distanceToEdge.y);
    float selectedVerticalSide = position.x < 0 ? leftEnabled : rightEnabled;
    float selectedHorizontalSide = position.y < 0 ? bottomEnabled : topEnabled;
    float nearestSideVisibility = lerp(selectedHorizontalSide, selectedVerticalSide, verticalSide);

    float borderWidth = borderDistance * 2.0;
    float horizontalBandVisibility = selectedHorizontalSide * step(distanceToEdge.y, borderWidth);
    float verticalBandVisibility = selectedVerticalSide * step(distanceToEdge.x, borderWidth);

    return max(nearestSideVisibility, max(horizontalBandVisibility, verticalBandVisibility));
}

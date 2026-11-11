Shader "HarvestFame/Pickup Paint"
{
    Properties
    {
        _MainTex ("Original color atlas", 2D) = "white" {}
        _Paint ("Body paint", Color) = (1, 0.69, 0.18, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Paint;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            fixed4 color = tex2D(_MainTex, i.uv_MainTex);
            half chroma = color.r - max(color.g, color.b);
            half saturation = chroma / max(color.r, 0.03);
            half redPaint = smoothstep(0.055, 0.14, chroma)
                * smoothstep(0.27, 0.51, saturation)
                * smoothstep(0.13, 0.30, color.r);
            half variation = 0.33 + color.r * 0.83;
            o.Albedo = lerp(color.rgb, _Paint.rgb * variation, redPaint);
            o.Smoothness = 0.24;
            o.Alpha = color.a;
        }
        ENDCG
    }

    Fallback "Diffuse"
}

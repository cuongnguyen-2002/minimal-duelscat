Shader "MinimalDuetCats/VignetteShader"
{
    Properties
    {
        [Header(Vignette Settings)]
        [Space]
        _Progress ("Mở (0=đen, 1=mở)", Range(0, 1)) = 0
        _Smoothness ("Mượt cạnh", Range(0, 1)) = 0.15
        [HDR] _Color ("Màu Vignette", Color) = (0, 0, 0, 1)

        [Header(Stretch)]
        [Space]
        [Toggle] _StretchX ("Dãn theo X", Float) = 0
        [Toggle] _StretchY ("Dãn theo Y", Float) = 0
        _Stretch ("Độ dãn", Range(0, 5)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Cull Off
        ZWrite On
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Vignette"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma shader_feature_local _STRETCHX_ON
            #pragma shader_feature_local _STRETCHY_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half _Progress;
            half _Smoothness;
            half _Stretch;
            half4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half aspect = _ScreenParams.x / _ScreenParams.y;
                half2 uv = i.uv - 0.5h;
                uv.x *= aspect;

                half stretchK = 1.0h + _Stretch * 2.0h;
                half2 suv = uv;

                #if defined(_STRETCHX_ON) && defined(_STRETCHY_ON)
                    suv *= stretchK;
                #elif defined(_STRETCHX_ON)
                    suv.x *= stretchK;
                #elif defined(_STRETCHY_ON)
                    suv.y *= stretchK;
                #endif

                half dist = length(suv);

                half halfW = aspect * 0.5h;
                half halfH = 0.5h;

                #if defined(_STRETCHX_ON) && defined(_STRETCHY_ON)
                    halfW *= stretchK;
                    halfH *= stretchK;
                #elif defined(_STRETCHX_ON)
                    halfW *= stretchK;
                #elif defined(_STRETCHY_ON)
                    halfH *= stretchK;
                #endif

                half maxDist = sqrt(halfW * halfW + halfH * halfH);
                half ndist = dist / maxDist;

                half radius = _Progress;
                half edgeWidth = max(_Smoothness * 0.15h, 0.001h);

                half alpha = smoothstep(radius - edgeWidth, radius + edgeWidth, ndist);

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }

    Fallback "Unlit/Transparent"
}

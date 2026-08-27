Shader "VoidZen/PixelRipple"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorBase ("Base Color", Color) = (0.04, 0.0, 0.08, 1)
        _ColorRipple ("Ripple Color", Color) = (0.16, 0.1, 0.29, 1)
        _ColorEdge ("Edge Highlight Color", Color) = (0.7, 0.5, 1.0, 1)
        _PixelGrid ("Pixel Grid Size", Float) = 120
        _Frequency ("Ripple Frequency", Float) = 30
        _Speed ("Ripple Speed", Float) = 1.2
        _MaxAlpha ("Max Ripple Intensity", Range(0,1)) = 0.3
        _EdgeWidth ("Edge Width", Range(0.01, 0.3)) = 0.08
        _EdgeIntensity ("Edge Intensity", Range(0,1)) = 0.8
        _AspectRatio ("Aspect Ratio Correction", Float) = 1.777
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _ColorBase;
            float4 _ColorRipple;
            float4 _ColorEdge;
            float _PixelGrid;
            float _Frequency;
            float _Speed;
            float _MaxAlpha;
            float _EdgeWidth;
            float _EdgeIntensity;
            float _AspectRatio;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // 1. 相對中心座標，修正長寬比
                float2 centered = (IN.uv - 0.5) * float2(1, _AspectRatio);

                // 2. 像素化量化
                float2 pixelated = floor(centered * _PixelGrid) / _PixelGrid;

                // 3. 到中心的距離
                float dist = length(pixelated);

                // 4. 波動計算（-1 ~ 1）
                float wave = sin(dist * _Frequency - _Time.y * _Speed);

                // 5. 基礎漣漪強度
                float intensity = (wave * 0.5 + 0.5) * _MaxAlpha;
                float3 rippleColor = lerp(_ColorBase.rgb, _ColorRipple.rgb, intensity);

                // 6. 邊框偵測：當 wave 接近波峰（接近 1）時觸發亮邊
                //    用 abs(wave - 1) 算「離波峰多近」，越接近 0 代表越在邊上
                float edgeDist = abs(wave - 1.0);
                float edgeMask = 1.0 - smoothstep(0.0, _EdgeWidth, edgeDist);

                // 7. 把邊框色疊加上去（用 max 而非 lerp，確保邊框夠銳利、不被吃掉）
                float3 finalColor = lerp(rippleColor, _ColorEdge.rgb, edgeMask * _EdgeIntensity);

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}
Shader "Custom/CRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Curvature ("Curvature", Range(0, 10)) = 3.0
        _ScanlineIntensity ("Scanline", Range(0, 1)) = 0.1
        _Vignette ("Vignette", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float _Curvature;
            float _ScanlineIntensity;
            float _Vignette;

            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = CurveUV(i.uv);
                
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0,0,0,1);

                fixed4 col = tex2D(_MainTex, uv);
                
                // 掃描線
                float scanline = sin(uv.y * 800.0) * _ScanlineIntensity;
                col.rgb -= scanline;
                
                // Vignette
                float2 vig = uv * (1.0 - uv);
                float vigVal = pow(vig.x * vig.y * 15.0, _Vignette);
                col.rgb *= vigVal;
                
                return col;
            }
            ENDCG
        }
    }
}
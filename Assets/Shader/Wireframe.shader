Shader "Custom/Wireframe" {
    Properties {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _WireColor ("Wire Color", Color) = (0,0,0,1)
        _WireWidth ("Wire Width", Range(0, 0.5)) = 0.05
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _MainColor;
            fixed4 _WireColor;
            float _WireWidth;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // 必须确保模型有UV坐标
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0; // 重心坐标
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.bary = float3(v.uv.x, v.uv.y, 1 - v.uv.x - v.uv.y);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 deltas = fwidth(i.bary);
                float3 thickness = deltas * _WireWidth;
                float3 edgeFactor = smoothstep(float3(0, 0, 0), thickness, i.bary);
                float minEdge = min(min(edgeFactor.x, edgeFactor.y), edgeFactor.z);
                return lerp(_WireColor, _MainColor, minEdge);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

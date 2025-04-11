Shader "Custom/ColorBlindFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FilterType ("Filter Type", Int) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _FilterType;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Color blindness simulation matrices
                float3x3 protanopia = float3x3(
                    0.567, 0.433, 0.000,
                    0.558, 0.442, 0.000,
                    0.000, 0.242, 0.758
                );
                
                float3x3 deuteranopia = float3x3(
                    0.625, 0.375, 0.000,
                    0.700, 0.300, 0.000,
                    0.000, 0.300, 0.700
                );
                
                float3x3 tritanopia = float3x3(
                    0.950, 0.050, 0.000,
                    0.000, 0.433, 0.567,
                    0.000, 0.475, 0.525
                );
                
                float3 rgb = float3(col.r, col.g, col.b);
                float3 result;
                
                switch (_FilterType)
                {
                    case 1: // Protanopia
                        result = mul(protanopia, rgb);
                        break;
                    case 2: // Deuteranopia
                        result = mul(deuteranopia, rgb);
                        break;
                    case 3: // Tritanopia
                        result = mul(tritanopia, rgb);
                        break;
                    default:
                        result = rgb;
                        break;
                }
                
                return fixed4(result, col.a);
            }
            ENDCG
        }
    }
} 
Shader "YuGiOh/MillenniumItem/GlowEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            float4 _Color;
            float _EmissionIntensity;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float pulse = (sin(_Time.y * _PulseSpeed) + 1) * 0.5 * _PulseAmount;
                col.rgb *= _EmissionIntensity * (1 + pulse);
                return col;
            }
            ENDCG
        }
    }
}

Shader "YuGiOh/MillenniumItem/OutlineEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0.1
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _PulseSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                float3 normalOffset = v.normal * _OutlineWidth * (sin(_Time.y * _PulseSpeed) + 1) * 0.5;
                float4 pos = UnityObjectToClipPos(v.vertex + normalOffset);
                o.vertex = pos;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);
                float rim = 1.0 - saturate(dot(normal, viewDir));
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, rim * _OutlineColor.a);
                return col;
            }
            ENDCG
        }
    }
}

Shader "YuGiOh/MillenniumItem/ShieldEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShieldColor ("Shield Color", Color) = (0.5,0.8,1,0.5)
        _ShieldPulse ("Shield Pulse", Range(0, 1)) = 0.5
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShieldColor;
            float _ShieldPulse;
            float _RimPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);
                float rim = 1.0 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                col.rgb = lerp(col.rgb, _ShieldColor.rgb, rim * _ShieldPulse);
                col.a = lerp(col.a, _ShieldColor.a, rim * _ShieldPulse);
                return col;
            }
            ENDCG
        }
    }
}

Shader "YuGiOh/MillenniumItem/FutureVisionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VisionColor ("Vision Color", Color) = (0,0.5,1,0.5)
        _VisionAlpha ("Vision Alpha", Range(0, 1)) = 0.5
        _TimeDistortion ("Time Distortion", Range(-1, 1)) = 0
        _DistortionSpeed ("Distortion Speed", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            float4 _VisionColor;
            float _VisionAlpha;
            float _TimeDistortion;
            float _DistortionSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 distortedUV = i.uv;
                distortedUV.x += sin(i.uv.y * 10 + _Time.y * _DistortionSpeed) * _TimeDistortion * 0.1;
                distortedUV.y += cos(i.uv.x * 10 + _Time.y * _DistortionSpeed) * _TimeDistortion * 0.1;
                
                fixed4 col = tex2D(_MainTex, distortedUV);
                col.rgb = lerp(col.rgb, _VisionColor.rgb, _VisionAlpha);
                col.a = lerp(col.a, _VisionColor.a, _VisionAlpha);
                return col;
            }
            ENDCG
        }
    }
}

Shader "YuGiOh/MillenniumItem/PowerLevelEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PowerColor ("Power Color", Color) = (1,0,0,1)
        _PowerLevel ("Power Level", Range(0, 1)) = 0.5
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            float4 _PowerColor;
            float _PowerLevel;
            float _PulseSpeed;

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
                float pulse = (sin(_Time.y * _PulseSpeed) + 1) * 0.5;
                float powerIndicator = step(i.uv.y, _PowerLevel);
                col.rgb = lerp(col.rgb, _PowerColor.rgb, powerIndicator * (0.7 + pulse * 0.3));
                col.a = lerp(col.a, _PowerColor.a, powerIndicator);
                return col;
            }
            ENDCG
        }
    }
}

Shader "YuGiOh/MillenniumItem/MindControlEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ControlColor ("Control Color", Color) = (0.5,0,1,0.7)
        _ControlPulse ("Control Pulse", Range(0, 1)) = 0.5
        _NoiseScale ("Noise Scale", Range(1, 50)) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            float4 _ControlColor;
            float _ControlPulse;
            float _NoiseScale;

            // Simple noise function
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

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
                float noiseValue = noise(i.uv * _NoiseScale + _Time.y);
                float controlEffect = noiseValue * _ControlPulse;
                col.rgb = lerp(col.rgb, _ControlColor.rgb, controlEffect);
                col.a = lerp(col.a, _ControlColor.a, controlEffect * 0.5);
                return col;
            }
            ENDCG
        }
    }
} 
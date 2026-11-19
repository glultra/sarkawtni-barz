// Seamwalk: the sky of one place, a dome that fades from the horizon colour up to the zenith colour.
Shader "Seamwalk/Sky"
{
    Properties
    {
        _Horizon ("Horizon", Color) = (0.8, 0.9, 1, 1)
        _Zenith ("Zenith", Color) = (0.3, 0.6, 1, 1)
        _Glow ("Sun glow", Color) = (1, 0.95, 0.8, 1)
        _SunDir ("Sun direction", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+400" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Horizon;
                float4 _Zenith;
                float4 _Glow;
                float4 _SunDir;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 dir : TEXCOORD0; };
            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.dir = i.positionOS.xyz;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float t = saturate(d.y * 1.6);
                half3 c = lerp(_Horizon.rgb, _Zenith.rgb, pow(t, 0.7));
                float s = saturate(dot(d, normalize(_SunDir.xyz)));
                c += _Glow.rgb * (pow(s, 24) * 0.35 + pow(s, 900) * 6);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}

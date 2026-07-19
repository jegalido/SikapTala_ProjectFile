Shader "Custom/ScreenBlur"
{
    Properties
    {
        _BlurSize ("Blur Size", Range(0, 10)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ScreenBlurPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _BlurSize;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texel = _BlitTexture_TexelSize.xy * _BlurSize;

                half4 col = half4(0, 0, 0, 0);

                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2(-1, -1)) * 0.077847;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 0, -1)) * 0.123317;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 1, -1)) * 0.077847;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2(-1,  0)) * 0.123317;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 0,  0)) * 0.195346;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 1,  0)) * 0.123317;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2(-1,  1)) * 0.077847;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 0,  1)) * 0.123317;
                col += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texel * float2( 1,  1)) * 0.077847;

                return col;
            }
            ENDHLSL
        }
    }
}
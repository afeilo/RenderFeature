Shader "Unlit/BlendShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SplitTexture ("SplitTexture", 2D) = "white" {}
        _Splat1 ("Splat1", 2D) = "white" {}
        _Splat2 ("Splat2", 2D) = "white" {}
        _Splat3 ("Splat3", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SplitTexture;
            sampler2D _Splat1;
            sampler2D _Splat2;
            sampler2D _Splat3;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 control = tex2D(_SplitTexture, i.uv);
                fixed4 col1 = tex2D(_Splat1, i.uv);
                fixed4 col2 = tex2D(_Splat2, i.uv);
                fixed4 col3 = tex2D(_Splat3, i.uv);
                fixed4 mixedDiffuse = 0.0h;
                mixedDiffuse = lerp(col1, col2, control.r);
                mixedDiffuse = lerp(mixedDiffuse, col3, control.g);
                return mixedDiffuse;
            }
            ENDCG
        }
    }
}

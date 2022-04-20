Shader "Unlit/BlendShader2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SplitTexture ("SplitTexture", 2D) = "white" {}
        _Splat ("Splat", 2D) = "white" {}
        _Tiling ("Tiling", int) = 1
        _SplitTiling ("SplitTiling", int) = 1
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
            sampler2D _Splat;
            float4 _Splat_TexelSize;
            float4 _MainTex_ST;
            int _Tiling;
            int _SplitTiling;
            float MipLevel( float2 uv )
			{	
				float2 dx=ddx(uv);
	            float2 dy=ddy(uv);
	            // texSize.xy为纹理tex的纹素大小
	            // texSize.xy=1.0/float2(texWidth,texHeight)
	            // Vector4(1 / width, 1 / height, width, height)
                float2 px = _Splat_TexelSize.z * dx;
                float2 py = _Splat_TexelSize.w * dy;
                float lod = floor(0.5 + log2(max(dot(px, px), dot(py, py))));
                return lod;
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
                fixed4 control = tex2D(_SplitTexture, i.uv * _SplitTiling);
                float2 hdim = _Splat_TexelSize.zw * 0.5;
                float2 offset = _Splat_TexelSize.xy * 0.5f;
                float2 uv = i.uv * _Tiling * hdim;
                float ox = _Splat_TexelSize.x * control.r;
                float oy = _Splat_TexelSize.y * control.g;
                offset += float2(ox, oy);
                float2 uv3 = floor(uv) / hdim;
                fixed4 col = tex2D(_Splat, uv3 + offset);
                return col;
            }
            ENDCG
        }
    }
}

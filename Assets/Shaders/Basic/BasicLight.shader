// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BasicLight" {
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fwdadd
			#pragma multi_compile_fwdbase 
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(0,1)
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return LIGHT_ATTENUATION(i);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

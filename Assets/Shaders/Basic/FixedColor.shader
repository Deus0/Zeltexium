// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader ".Zeltex/Fixed Unlit"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			fixed4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f	// vertex 2
			{
				float4 pos : SV_POSITION;
			};

			v2f Vert(appdata IN)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(IN.vertex);
				return OUT;
			}

			fixed4 Frag(v2f IN) : COLOR
			{
				return _Color;
			}
			ENDCG
		}
	}
}
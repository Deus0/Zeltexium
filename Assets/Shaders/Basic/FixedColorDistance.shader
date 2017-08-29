// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader ".Zeltex/Fixed Distance Check"
{
	Properties
	{
		_CheckDistance("CheckDistance", Float) = 25
		//_T("Transparency", Range(0,1)) = 1
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex Vert
			#pragma fragment Frag       

			float _CheckDistance;
			float4 VertexPosition;

			struct MyVertData
			{
				float4 vertex : POSITION;
			};

			struct v2f	// vertex 2	// cant rename v2f...
			{
				float4 pos : SV_POSITION;
				float4 worldSpacePosition : TEXCOORD0;
			};

			v2f Vert(MyVertData MyInput)
			{
				v2f MyOutput;
				MyOutput.pos = UnityObjectToClipPos(MyInput.vertex);
				//VertexPosition = mul(_WorldSpaceCameraPos, MyInput.vertex);
				MyOutput.worldSpacePosition = mul(unity_ObjectToWorld, MyInput.vertex);
				return MyOutput;
			}

			fixed4 Frag(v2f MyInput) : COLOR
			{
				//return fixed4(1, 0.25f, 0.25f, 1);
				float4 pixelWorldSpacePosition = MyInput.worldSpacePosition;
				float MyDistance = distance(_WorldSpaceCameraPos, pixelWorldSpacePosition);
				if (MyDistance < _CheckDistance)
					return fixed4(0, 0.85f, 0.85f, 1);
				else
					return fixed4(1, 0.25f, 0.25f, 1);
			}
			ENDCG
		}
	}
}
//float MyDistance = distance(_WorldSpaceCameraPos, mul(_Object2World, MyInput.vertex));
//float MyDistance = length(ObjSpaceViewDir(MyInput.vertex));
//float4 WorldPos = mul(unity_ObjectToWorld, MyInput);
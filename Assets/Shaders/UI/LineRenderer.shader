// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ZeltexGUI/LineRenderer"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_ColorMask("Color Mask", Float) = 15
		_OverrideColor("Color Override", Float) = 1
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Overlay"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

			Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Off
			//Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
		{
			CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord  : TEXCOORD0;
		};

		fixed4 _Color;
		fixed4 _TextureSampleAdd; //Added for font color support
		fixed4 _OverrideColor;

		v2f vert(appdata_t IN)
		{
			v2f Output;
			Output.vertex = IN.vertex;
			Output.texcoord = IN.texcoord;
			Output.vertex = UnityObjectToClipPos(IN.vertex);
		#ifdef UNITY_HALF_TEXEL_OFFSET
			//Output.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1,1);
		#endif
			Output.color = IN.color * _Color;
			return Output;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f IN) : SV_Target
		{
			half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;  //Added for font color support
			//if (_OverrideColor > 0)
			//{
				color.r = IN.color.r;
				color.g = IN.color.g;
				color.b = IN.color.b;
				color.a = IN.color.a;
				//color.a = IN.color.a;
			//}
			//clip(color.a - 0.01);
			return color;
		}
		ENDCG
		}
	}
}
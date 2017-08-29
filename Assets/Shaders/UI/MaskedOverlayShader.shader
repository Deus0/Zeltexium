// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ZeltexGUI/MaskOverlay"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Overlay+1"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			/*
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"*/
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		
		ColorMask[_ColorMask]
		Lighting Off
		ZTest Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct MainVertexData
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct ShaderVertexData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			fixed4 _TextureSampleAdd; //Added for font color support
			sampler2D _MainTex;

			ShaderVertexData vert(MainVertexData Input)
			{
				ShaderVertexData Output;
				Output.vertex = UnityObjectToClipPos(Input.vertex);
				#ifdef UNITY_HALF_TEXEL_OFFSET
					OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1,1);
				#endif
				Output.texcoord = Input.texcoord;
				Output.color = Input.color * _Color;
				return Output;
			}

			fixed4 frag(ShaderVertexData IN) : SV_Target
			{
				half4 PixelColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				PixelColor.r = IN.color.r;
				PixelColor.g = IN.color.g;
				PixelColor.b = IN.color.b;
				//PixelColor.a = IN.color.a;
				clip(PixelColor.a - 0.01);
				PixelColor.a = 1.00;
				return PixelColor;
			}
			ENDCG
		}
	}
}
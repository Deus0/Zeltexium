// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FadeShader" {
	SubShader{
		Pass{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct vertexInput {
		float4 vertex : POSITION;
		float4 texcoord0 : TEXCOORD0;
	};

	struct fragmentInput {
		float4 position : SV_POSITION;
		float4 texcoord0 : TEXCOORD0;
	};

	fragmentInput vert(vertexInput i) {
		fragmentInput o;
		o.position = UnityObjectToClipPos(i.vertex);
		o.texcoord0 = i.texcoord0;
		return o;
	}

	float4 frag(fragmentInput i) : COLOR{
		float4 color;
	float x = i.texcoord0.x;
	float y = i.texcoord0.y;
	float b = 0.0;
	x *= 4;
	y *= 4;
	if (x > 2) {
		x = 4 - x;
	}
	if (y > 2) {
		y = 4 - y;
	}
	if (x > y) {
		b = y;
	}
	else if (y > x) {
		b = x;
	}
	else {
		b = x / 4;
	}
	color = float4(0.0,0.0,0.0,b);
	return (color);
	}
		ENDCG
	}
	}
		Fallback "Unlit"
}


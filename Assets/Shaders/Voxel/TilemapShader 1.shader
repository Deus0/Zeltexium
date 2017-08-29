Shader "TileMapShader"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		//_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white"
		_TileMap("TileMap (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0         
		_Margin("Margin", Range(0,1)) = 0.05
	}
		SubShader{
		//Tags{ "RenderType" = "Opaque" }
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
//#pragma surface surf Standard fullforwardshadows     
#pragma surface surf Standard alpha

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _TileMap;

	struct Input
	{
		float2 uv_MainTex;
		float4 color : COLOR;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color;
		o.Alpha = _Color.a;
		//o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;
		/*int Multiple = 100;
		float2 MyUVs2 = IN.uv_MainTex;
		int u = (int) (MyUVs2.x*Multiple);
		MyUVs2.x = ((float)u / (float)Multiple);
		int v = (int)(MyUVs2.y * Multiple);
		MyUVs2.y = ((float)v / (float)Multiple);*/
		//o.Albedo = tex2D(_TileMap, MyUVs2) * 32;
		/*
		int PositionX = index / 64;
		float _GutterSize = 0;
		float _AtlasSize = 128;
		// Albedo comes from a texture tinted by color
		int TilesetDim = 8;
		int xpos = index % TilesetDim;
		int ypos = index / TilesetDim;
		float2 uv = float2(xpos, ypos) / TilesetDim;
		float xoffset = frac(IN.uv_MainTex.x * TilesetDim) / TilesetDim;
		float yoffset = frac(IN.uv_MainTex.y * TilesetDim) / TilesetDim;
		uv += float2(xoffset, yoffset) + _GutterSize / _AtlasSize;
		o.Albedo = tex2D(_MainTex, uv);	// IN.uv_MainTex
		o.Albedo *= IN.color;*/

		// Clamping to colours!
		/*int TileIndex = tex2D(_TileMap, IN.uv_MainTex).x * 255;// *_MaxIndex;
		int TileResolution = 8; // number of tiles
		float PixelLength = 0.25 / (float)128;
		float2 MyUVs = IN.uv_MainTex;
		float TileSize = 1 / ((float)TileResolution);
		int PosX = (TileIndex % (TileResolution - 1));
		int PosY = (TileIndex / (TileResolution - 1));
		float MinU = PosX * TileSize + PixelLength;
		float MaxU = PosX * TileSize + TileSize - PixelLength;
		//MyUVs.x = clamp(MyUVs.x, MinU, MaxU);
		float MinV = PosY * TileSize + PixelLength;
		float MaxV = PosY * TileSize + TileSize - PixelLength;*/
		//MyUVs.y = clamp(MyUVs.y, MinV, MaxV);
		//if (TileIndex == 2)
		{
			//o.Albedo = float3(1, 1, 1);
			//o.Albedo = tex2D(_TileMap, IN.uv_MainTex);
		}
		/* * _Color;
		o.Albedo = c.rgb;
		// Metallic and smoothness come from slider variables
		o.Alpha = c.a;*/
	}
	ENDCG
	}
		FallBack "Diffuse"
}

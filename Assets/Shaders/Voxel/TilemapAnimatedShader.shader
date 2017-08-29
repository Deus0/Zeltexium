Shader "Tilemap/TileMapAnimatedShader"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_TileMap("TileMap (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_TilePositionAddition("TilePositionAddition", Range(1,16)) = 1
		//_TilePosition("TilePosition", Range(1,8)) = 1
		_Speed("Speed", Range(1,8)) = 1
			_TileCount("TileCount", Range(1,16)) = 16
			_IsFade("Fade", Range(0,1)) = 1
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows

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
		//float _TilePosition;
		float _Speed;
		float _TileCount;
		float _TilePositionAddition;
		float _IsFade;
		float TilePosition = 1;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			// increase tile position
			float Direction = 1;
			if (TilePosition > _TileCount)
			{
				Direction = -1;
				TilePosition = _TileCount;
			}
			else if (TilePosition < 1)
			{
				Direction = 1;
				TilePosition = 1;
			}
			TilePosition = _TilePositionAddition + (_Time * _Speed * 10 * Direction);
		//float PixelLength = 0.5 / (float)128;
		float TilesCount = _TileCount;
		float TileSize = 1 / TilesCount;
		// first tile
		float2 uvs = IN.uv_MainTex;
		uvs.x = (((int)(TilePosition) - 1) * TileSize) + uvs.x / TilesCount;
		fixed4 MyColor = tex2D(_MainTex, uvs);
		// second tile
		if (_IsFade == 1) 
		{
			float2 uvs2 = IN.uv_MainTex;
			uvs2.x = (((int)(TilePosition)) * TileSize) + uvs2.x / TilesCount;
			fixed4 MyColor2 = tex2D(_MainTex, uvs2);
			float FadeDifference = ((int)TilePosition + 1) - TilePosition;
			o.Albedo = (FadeDifference * MyColor + (1 - FadeDifference) * MyColor2) * _Color* IN.color;
		}
		else 
		{
			o.Albedo = MyColor * _Color;// *IN.color;
			o.Alpha = 1;
		}
	}
	ENDCG
	}
		FallBack "Diffuse"
}

//float HalfwayPoint = ((_TilePosition - 0.5) * TileSize);
//uvs.x = floor((uvs.x - PixelLength) * 128) / 128;	// - PixelLength
/*if (uvs.x > HalfwayPoint)
{
uvs.x -= PixelLength;
}
else
{
if (MyTilePosition > 1)
{
uvs.x += PixelLength;
}
}*/
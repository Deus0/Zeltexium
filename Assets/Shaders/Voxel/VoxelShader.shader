Shader "Zeltex/VoxelShader"
{
	// First our editor variables
	// https://docs.unity3d.com/Manual/SL-Properties.html
	Properties
	{
		ColorTint("Color", Color) = (1,1,1,1)
		_MainTex("Diffuse Texture", 2D) = ""
		NormalsTex("Normal Texture", 2D) = ""
		Brightness("Brightness", Float) = 1
		_Smoothness("Smoothness", Float) = 0.0
		_Metallic("Metallic", Float) = 0
		Desaturate("Desaturate", Int) = 0
		IsTextures("IsTextures", Int) = 1
	}
	// Next our subshader
	SubShader
	{
		// Set our shader tags
		// Passes use tags to tell how and when they expect to be rendered to the rendering engine
		// https://docs.unity3d.com/Manual/SL-PassTags.html
		Tags
		{
			"RenderType" = "Opaque"
			//"RenderType" = "Transparent"
			//"Queue" = "Transparent"
		}
		LOD 200	// what this do?


		CGPROGRAM
		#pragma surface surf Standard// fullforwardshadows	//alpha
		//#pragma target 3.0	// Use shader model 3.0 target, to get nicer looking lighting

		// Re-Declare our editor variables here
		fixed4 ColorTint;
		sampler2D _MainTex;
		sampler2D NormalsTex;
		int Desaturate;
		float Brightness;
		int IsTextures;
		//sampler2D _TileMap;
		half _Smoothness;
		half _Metallic;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			fixed3 BaseColor = tex2D(_MainTex, IN.uv_MainTex);
			if (IsTextures == 0) 
			{
				BaseColor = fixed3(1, 1, 1);
			}
			//o.Alpha = tex2D(MainTex, IN.uv).a * ColorTint.a * IN.color.a;
			o.Alpha = ColorTint.a;
			if (Desaturate >= 1)
			{
				BaseColor *= IN.color;
				float Blended = (BaseColor.r + BaseColor.g + BaseColor.b) / 3;
				fixed3 BlendedColor = fixed3(Blended, Blended, Blended);
				//BaseColor *=  * 0.5;
				/*if (Blended > 0.1)
				{
					ColorTint *= 0.5f;
				}*/
				o.Albedo = (BlendedColor * ColorTint) * Brightness;
			} 
			else
			{
				o.Albedo = BaseColor * IN.color * Brightness;
			}
			o.Albedo.r *= ColorTint.r;
			o.Albedo.g *= ColorTint.g;
			o.Albedo.b *= ColorTint.b;
		}
		ENDCG
	}
	//FallBack "Zeltex/BasicVoxelShader"	// a more basic version
}

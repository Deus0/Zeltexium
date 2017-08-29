Shader ".Zeltex/Colored Diffuse" 
{
    Properties
	{
    }
     
    SubShader 
	{
		//_MainTex("Color (RGB) Alpha (A)", 2D) = "white"
		//Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 150
     
		CGPROGRAM
		#pragma vertex Vert
		#pragma surface Surf Lambert
     
		sampler2D _MainTex;
     
		struct Input 
		{
			float2 uv_MainTex;
			float3 vertColor;
		};
     
		void Vert(inout appdata_full v, out Input MyInput)
		{
			UNITY_INITIALIZE_OUTPUT(Input, MyInput);
			MyInput.vertColor = v.color;
		}

		void Surf(Input MyInput, inout SurfaceOutput Output)
		{
			Output.Albedo = MyInput.vertColor;	// just show our vertex color
		}
		ENDCG
    }
}

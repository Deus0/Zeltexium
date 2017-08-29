Shader ".Zeltex/Colored Diffuse" 
{
    Properties
	{
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
     
    SubShader 
	{
        Tags { "RenderType"="Fade" }
        LOD 150
     
		CGPROGRAM
		#pragma vertex Vert
		//#pragma fragment Frag
		#pragma surface Surf Lambert
     
		sampler2D _MainTex;
		fixed4 _Color;
     
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
			fixed4 MyColor = tex2D(_MainTex, MyInput.uv_MainTex) * _Color;
			Output.Albedo = MyColor.rgb * MyInput.vertColor;
			Output.Albedo = float3(1 - Output.Albedo.r, 1 - Output.Albedo.g, 1 - Output.Albedo.b);
			Output.Alpha = 0.1f;
		}
		ENDCG
    }
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader ".Zeltex/VoxelShader"
{

	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_FogColor("Fog Color", Color) = (1,1,1,1)
		CheckDistance("CheckDistance", Range(0,8)) = 1.5
		FadeRate("FadeRate", Range(0, 1)) = 0.1

		NoiseAmplitude("NoiseAmplitude", Range(0,2)) = 0.25
		NoiseSpeed("NoiseSpeed", Range(0,2)) = 0.25

		//IsNoiseColor("IsNoiseColor", Range(0,1)) = 1
		NoiseColor("NoiseColor", Color) = (1,1,1,1)
		NoiseColorAmplitude("NoiseColorAmplitude", Range(0,1)) = 0.2
		Transparency("Transparency", Range(0,1)) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Lighting On

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "ZeltexFunctions.cginc"
			#include "AutoLight.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdbase

			// defines a vertex function
			#pragma vertex Vert
			// defines a fragment function
			#pragma fragment Frag        

			//#pragma multi_compile_fog    

			sampler2D _MainTex;
			float CheckDistance;
			float FadeRate;
			float NoiseAmplitude;
			float NoiseSpeed;

			float IsNoiseColor;
			fixed4 NoiseColor;
			float NoiseColorAmplitude;
			fixed4 _Color;
			fixed4 _FogColor;
			float3 directionalLightColor;

			struct MyVertData
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;            
				float4 color : COLOR;
				//float4 normal : Normal;
 
			};

			struct v2f	// vertex 2	// cant rename v2f...
			{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				float4 worldSpacePosition : TANGENT;
				float4 color : COLOR;
				float3 ambientAndBackLightColor : TEXCOORD1;
				//half fog;
                //float3 vNormal : TEXCOORD2;
				//float3 lightDir : TEXCOORD1;
				//LIGHTING_COORDS(3, 4)
				// in v2f struct;
				LIGHTING_COORDS(0, 1) // replace 0 and 1 with the next available TEXCOORDs in your shader, don't put a semicolon at the end of this line.
			};

			v2f Vert(MyVertData MyInput)
			{
				v2f MyOutput;
				MyOutput.pos = UnityObjectToClipPos(MyInput.vertex);
				MyOutput.texcoord = MyInput.texcoord;
				MyOutput.color = MyInput.color;

				MyOutput.worldSpacePosition = mul(unity_ObjectToWorld, MyInput.vertex);

				GetShadeVertexLights(v.vertex, v.normal, o.keyLightColor, o.ambientAndBackLightColor);
				TRANSFER_VERTEX_TO_FRAGMENT(MyOutput); // Calculates shadow and light attenuation and passes it to the frag shader.
				//MyOutput.pos.y += noise(_Time.g * MyOutput.worldSpacePosition.x*NoiseSpeed) + noise(_Time.g * MyOutput.worldSpacePosition.z*NoiseSpeed) - 1;
				return MyOutput;
			}

			fixed4 Frag(v2f MyInput) : COLOR
			{
				fixed4 MyColor = tex2D(_MainTex, MyInput.texcoord);	// get normal texture colours

				// Fade
				//MyColor = FadeColor(MyColor, FadeRate, CheckDistance, _WorldSpaceCameraPos, MyInput.worldSpacePosition);

					// Add Random Noise Colour to shader
					if (NoiseColorAmplitude != 0)
					{
						MyColor.r += noise(MyInput.worldSpacePosition.x)	* NoiseColor.r * NoiseColorAmplitude;
						MyColor.g += noise(MyInput.worldSpacePosition.y)	* NoiseColor.g * NoiseColorAmplitude;
						MyColor.b += noise(MyInput.worldSpacePosition.z)	* NoiseColor.b * NoiseColorAmplitude;
						//MyColor = ClampColor(MyColor);
					}

					/*if (NoiseAmplitude != 0)
					{
						float Darkness = (MyColor.r + MyColor.g + MyColor.b) / 3;
						float MyNoise = NoiseAmplitude * noise(_Time.g * NoiseSpeed * Darkness);	// + MyDistance	// MyInput.worldSpacePosition.x + MyInput.worldSpacePosition.y +
					}*/

					MyColor = ClampColor(MyColor);
					MyColor *= MyInput.color;	// multiply by vertex colour
					//MyColor *= _Color;			// tint the colour

											//in frag shader;
				float atten = LIGHT_ATTENUATION(MyInput); // This is a float for your shadow/attenuation value, multiply your lighting value by this to get shadows. Replace i with whatever you've defined your input struct to be called (e.g. frag(v2f [b]i[/b]) : COLOR { ... ).
				//MyColor *= atten;

				float3 color = (MyInput.keyLightColor * atten + MyInput.ambientAndBackLightColor);
				return half4(color, 1.0f);
				//return MyColor;
			}

			/*
			MyInput.lightDir = normalize(MyInput.lightDir);

			MyInput.vNormal = normalize(MyInput.vNormal);float atten = LIGHT_ATTENUATION(MyInput);

			float3 color;

			float NdotL = saturate(dot(MyInput.vNormal, MyInput.lightDir));

			color = UNITY_LIGHTMODEL_AMBIENT.rgb * 2;

			color += MyInput.color;

			color += _LightColor0.rgb * NdotL * (atten * 2);

			return half4(color, 1.0f);*/
			//}
			ENDCG
		}
	}
}
//float MyDistance = distance(_WorldSpaceCameraPos, mul(_Object2World, MyInput.vertex));
//float MyDistance = length(ObjSpaceViewDir(MyInput.vertex));
//float4 WorldPos = mul(unity_ObjectToWorld, MyInput);
					//float r = clamp(MyFadedColor.r - FadeRate*DistanceAway, 0.0, 1.0);
					//float g = clamp(MyFadedColor.g - FadeRate*DistanceAway, 0.0, 1.0);
					//float b = clamp(MyFadedColor.b - FadeRate*DistanceAway, 0.0, 1.0);
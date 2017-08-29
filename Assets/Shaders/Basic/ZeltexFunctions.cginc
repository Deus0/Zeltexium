// Functions for my sick shaders

float3 GetLightColor(int _index, float3 _viewpos, float3 _viewN)
{
	float3 toLight = unity_LightPosition[_index].xyz - _viewpos.xyz * unity_LightPosition[_index].w;
	float diff = max(0, dot(_viewN, normalize(toLight)));
	return unity_LightColor[_index].rgb * diff;
}

void GetShadeVertexLights(float4 vertex, float3 normal, out float3 mainLightColor, out float3 lightColor)
{
	float3 viewpos = mul(UNITY_MATRIX_MV, vertex).xyz;
	float3 viewN = mul((float3x3)UNITY_MATRIX_IT_MV, normal);
	mainLightColor = GetLightColor(0, viewpos, viewN);
	lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz * 2 + GetLightColor(1, viewpos, viewN);
}

fixed4 ClampColor(fixed4 InputColor)
{
	return fixed4(
		clamp(InputColor.r, 0, 1),
		clamp(InputColor.g, 0, 1),
		clamp(InputColor.b, 0, 1),
		clamp(InputColor.a, 0, 1));
}

fixed4 FadeColor(fixed4 MyColor, float FadeRate, float FadeOffset, float4 CameraPosition, float4 WorldPosition)
{
	float MyDistance = distance(CameraPosition, WorldPosition);
	if (FadeRate != 0)
	{
		// Fade into the distance
		float DistanceAway = MyDistance - FadeOffset;
		if (DistanceAway < 0)
		{
			DistanceAway = 0;	// cllamp at 0
		}
		float FadeValue = (DistanceAway) * (FadeRate);
		if (FadeValue < 1)
			FadeValue = 1;	// minimum fade is 1
		// clamp color
		MyColor = ClampColor(MyColor);
	}
	return MyColor;
}

float hash(float n)
{
	return frac(sin(n)*43758.5453);
}
			float noise(float3 x)
			{
				// The noise function returns a value in the range -1.0f -> 1.0f

				float3 p = floor(x);
				float3 f = frac(x);

				f = f*f*(3.0 - 2.0*f);
				float n = p.x + p.y*57.0 + 113.0*p.z;

				return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
					lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
					lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
						lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
			}
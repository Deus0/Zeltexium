// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "FX/Mirror Reflection" {
Properties {
    _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
    _ReflectionTex ("Environment Reflection", 2D) = "" {}
}  

// -----------------------------------------------------------
// ARB vertex program

Subshader {
    Pass {
    
CGPROGRAM
// profiles arbfp1
// vertex vert
// fragmentoption ARB_precision_hint_fastest 
// fragmentoption ARB_fog_exp2

#include "UnityCG.cginc"

struct v2f {
    float4 pos : SV_POSITION;
    float4 ref : TEXCOORD0;
};

v2f vert(appdata_base v)
{
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    
    // calculate the reflection vector
    float4x4 mat= float4x4 (
        .5, 0, 0,.5,
         0,.5, 0,.5,
         0, 0,.5,.5,
         0, 0, 0, 1
    );    
    o.ref = mul (mat, o.pos);
    
    return o;
}
ENDCG

        SetTexture [_ReflectionTex] { constantColor[_ReflectColor] combine texture * constant }
    }
}

// -----------------------------------------------------------
//  Fallback to non-reflective for older cards or Unity non-Pros

Subshader {
    Pass {
        SetTexture [_MainTex] { constantColor[_ReflectColor] combine constant }
    }
}

}
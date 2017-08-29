Shader "Masked/Mask" {

	SubShader{
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Geometry+10" }

		Stencil
	{
		Ref 0
		Comp Always
		Pass Zero
		ReadMask 255
		WriteMask 255
	}
		// Don't draw in the RGBA channels; just the depth buffer
		ColorMask 0
		ZWrite On

		// Do nothing specific in the pass:

		Pass{}
	}
}
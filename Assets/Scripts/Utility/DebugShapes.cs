using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Util
{
    public static class DebugShapes
    {
	    public static void DrawCube(Vector3 Position, Vector3 Size, Color MyColor)
        {
		    DrawCube (Position, Size, MyColor, false);
	    }

	    public static void DrawCube(Vector3 Position, Vector3 Size, Color MyColor, bool IsDepth)
        {
		    List<Vector3> CubeLines = new List<Vector3> ();
		    CubeLines.Add (new Vector3(-Size.x, -Size.y, -Size.z));
		    CubeLines.Add (new Vector3(Size.x, -Size.y, -Size.z));

		    CubeLines.Add (new Vector3(Size.x, -Size.y, -Size.z));
		    CubeLines.Add (new Vector3(Size.x, -Size.y, Size.z));
		
		    CubeLines.Add (new Vector3(Size.x, -Size.y, Size.z));
		    CubeLines.Add (new Vector3(-Size.x, -Size.y, Size.z));
		
		    CubeLines.Add (new Vector3(-Size.x, -Size.y, Size.z));
		    CubeLines.Add (new Vector3(-Size.x, -Size.y, -Size.z));

		    CubeLines.Add (new Vector3(-Size.x, Size.y, -Size.z));
		    CubeLines.Add (new Vector3(Size.x,  Size.y, -Size.z));
		    CubeLines.Add (new Vector3(Size.x,  Size.y, -Size.z));
		    CubeLines.Add (new Vector3(Size.x,  Size.y, Size.z));
		    CubeLines.Add (new Vector3(Size.x,  Size.y, Size.z));
		    CubeLines.Add (new Vector3(-Size.x,  Size.y, Size.z));
		    CubeLines.Add (new Vector3(-Size.x,  Size.y, Size.z));
		    CubeLines.Add (new Vector3(-Size.x,  Size.y, -Size.z));

		    CubeLines.Add (new Vector3(-Size.x, -Size.y, -Size.z));
		    CubeLines.Add (new Vector3(-Size.x,  Size.y, -Size.z));
		    CubeLines.Add ( new Vector3(Size.x, -Size.y, -Size.z));
		    CubeLines.Add ( new Vector3(Size.x,  Size.y, -Size.z));
		    CubeLines.Add ( new Vector3(Size.x, -Size.y, Size.z));
		    CubeLines.Add ( new Vector3(Size.x,  Size.y, Size.z));
		    CubeLines.Add ( new Vector3(-Size.x, -Size.y, Size.z));
		    CubeLines.Add ( new Vector3(-Size.x,  Size.y, Size.z));


		    GL.PushMatrix ();
		    Material MyMaterial = new Material (Shader.Find("Transparent/Diffuse"));
		    // reset matrix
		    //GL.LoadIdentity ();
		    Matrix4x4 MyMatrix = Matrix4x4.TRS (Position, Quaternion.identity, new Vector3(1,1,1));
		    GL.MultMatrix (MyMatrix);
		    //GL.MultMatrix (Position.localToWorldMatrix);
		    //GL.matr
		    MyMaterial.SetPass (0);
		    // Draw lines
		    GL.Begin (GL.LINES);
		    for (int i = 0; i < CubeLines.Count; i += 2)
		    {
			    GL.Color (MyColor);
			    GL.Vertex3 (CubeLines[i].x, CubeLines[i].y, CubeLines[i].z);
			    GL.Vertex3 (CubeLines[i+1].x, CubeLines[i+1].y, CubeLines[i+1].z);
		    }
		    GL.End ();
		    GL.PopMatrix ();
	    }

	    public static void DebugDrawCube(Vector3 Position, Vector3 Size, Color MyColor, float Duration, bool IsDepth)
        {
		    Debug.DrawLine(Position + new Vector3(-Size.x, -Size.y, -Size.z), 	Position + new Vector3(Size.x, -Size.y, -Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x, -Size.y, -Size.z), 	Position + new Vector3(Size.x, -Size.y, Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x, -Size.y, Size.z), 	Position + new Vector3(-Size.x, -Size.y, Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(-Size.x, -Size.y, Size.z), 	Position + new Vector3(-Size.x, -Size.y, -Size.z), MyColor, Duration,IsDepth);
		
		
		    Debug.DrawLine(Position + new Vector3(-Size.x, Size.y, -Size.z), 	Position + new Vector3(Size.x,  Size.y, -Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x,  Size.y, -Size.z), 	Position + new Vector3(Size.x,  Size.y, Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x,  Size.y, Size.z), 	Position + new Vector3(-Size.x,  Size.y, Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(-Size.x,  Size.y, Size.z), 	Position + new Vector3(-Size.x,  Size.y, -Size.z), MyColor, Duration,IsDepth);
		
		
		    Debug.DrawLine(Position + new Vector3(-Size.x, -Size.y, -Size.z), 	Position + new Vector3(-Size.x,  Size.y, -Size.z) , MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x, -Size.y, -Size.z), 	Position + new Vector3(Size.x,  Size.y, -Size.z) , MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(Size.x, -Size.y, Size.z), 	Position + new Vector3(Size.x,  Size.y, Size.z), MyColor, Duration,IsDepth);
		    Debug.DrawLine(Position + new Vector3(-Size.x, -Size.y, Size.z),  	Position + new Vector3(-Size.x,  Size.y, Size.z), MyColor, Duration,IsDepth);
	    }

	    public static void DrawSquare(Vector3 Position, Vector3 Size, Color MyColor)
        {
		    Debug.DrawLine(Position + new Vector3(0, 0, 0), 	Position + new Vector3(Size.x, 0, 0), MyColor);
		    Debug.DrawLine(Position + new Vector3(Size.x, 0, 0), 	Position + new Vector3(Size.x, 0, Size.z), MyColor);
		    Debug.DrawLine(Position + new Vector3(Size.x, 0, Size.z), 	Position + new Vector3(0, 0, Size.z), MyColor);
		    Debug.DrawLine(Position + new Vector3(0, 0, Size.z), 	Position + new Vector3(0, 0, 0), MyColor);
	    }
    }
}
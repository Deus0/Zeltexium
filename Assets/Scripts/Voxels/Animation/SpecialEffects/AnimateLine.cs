using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities 
{
	public class AnimateLine : MonoBehaviour 
	{
		protected GameObject Target;
		public Vector3 TargetOffset;
		//public GameObject MyLineObject;
		protected List<LineRenderer> MyLines = new List<LineRenderer>();
		protected List<Vector3> MyFromPositions = new List<Vector3>();
		protected List<Vector3> MyTargetPositions = new List<Vector3>();
		public Material MyLineMaterial;
        private float LineSize = 0.01f;
		private int VertsCount = 30;
		private float LineSinAmplitudeX = 0.2f;
		private float LineSinAmplitudeZ = 0.1f;
		private float Speed = 3f;
		
		void Start ()
		{
			MyLines.Add (CreateNewLine ());
		}
		// Update is called once per frame
		void Update () 
		{
			MyTargetPositions.Clear ();
			MyFromPositions.Clear ();
			MyFromPositions.Add (transform.position);
			MyTargetPositions.Add(Target.transform.position+TargetOffset);
			UpdateLinePositionsTotal ();
		}
		public void SetTarget(GameObject NewTarget) 
		{
			if (NewTarget != null)
				Target = NewTarget;
		}

		public void SetVisibity(bool NewVisible) 
		{
			for (int i = 0; i < MyLines.Count; i++) 
			{
				MyLines [i].gameObject.SetActive (NewVisible);
			}
		}

		public LineRenderer CreateNewLine()
		{
			GameObject MyLineObject = new GameObject ();
            MyLineObject.transform.SetParent(transform, false);
            MyLineObject.transform.position = transform.position;
            MyLineObject.name = "ItemInspectLine " + MyLines.Count;
			LineRenderer MyLine = MyLineObject.AddComponent<LineRenderer> ();	//LineRenderer ();
			MyLine.material = MyLineMaterial;
            MyLine.SetWidth(LineSize, LineSize);
            // This causes  IsFinite(outDistanceForSort)
            //MyLine.SetVertexCount (30);
            //MyLine.transform.SetParent (transform.GetChild(0));	// so the first object in the list holds the lines!
			return MyLine;
		}

		public void UpdateLinePositionsTotal() 
		{
			for (int i = 0; i < MyTargetPositions.Count; i++) {
				UpdateLinePositions (MyLines[i], MyFromPositions[i], MyTargetPositions[i]);
			}
		}

		public void UpdateLinePositions(LineRenderer MyLine, Vector3 FromPosition, Vector3 TargetPosition) 
		{
			Vector3 MyDirectionX = (TargetPosition-FromPosition).normalized;
			MyDirectionX = new Vector3 (MyDirectionX.y, MyDirectionX.x, MyDirectionX.z);
			Vector3 MyDirectionZ = (TargetPosition-FromPosition).normalized;
			MyDirectionZ = new Vector3 (MyDirectionZ.x, MyDirectionZ.z, MyDirectionZ.y);

			Vector3 MyDirection = LineSinAmplitudeX*MyDirectionX*Mathf.Sin (Speed*Time.time)
				+LineSinAmplitudeZ*MyDirectionZ*Mathf.Cos(Speed*Time.time/0.7f);

			Vector3[] points = new Vector3[] {
                FromPosition,
				FromPosition +(TargetPosition-FromPosition)/2f+MyDirection*0.5f,
				TargetPosition};

			points = Curver.MakeSmoothCurve(points, 3.0f);
			MyLine.SetVertexCount (points.Length);
			for (int i = 0; i < points.Length; i++)
            {
				MyLine.SetPosition(i, points[i]);
			}
		}
	}

	public class Curver : MonoBehaviour
	{
		//arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
		public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve,float smoothness){
			List<Vector3> points;
			List<Vector3> curvedPoints;
			int pointsLength = 0;
			int curvedLength = 0;
			
			if(smoothness < 1.0f) smoothness = 1.0f;
			
			pointsLength = arrayToCurve.Length;
			
			curvedLength = (pointsLength*Mathf.RoundToInt(smoothness))-1;
			curvedPoints = new List<Vector3>(curvedLength);
			
			float t = 0.0f;
			for(int pointInTimeOnCurve = 0;pointInTimeOnCurve < curvedLength+1;pointInTimeOnCurve++){
				t = Mathf.InverseLerp(0,curvedLength,pointInTimeOnCurve);
				
				points = new List<Vector3>(arrayToCurve);
				
				for(int j = pointsLength-1; j > 0; j--){
					for (int i = 0; i < j; i++){
						points[i] = (1-t)*points[i] + t*points[i+1];
					}
				}
				
				curvedPoints.Add(points[0]);
			}
			
			return(curvedPoints.ToArray());
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Bugs:
//	Sometimes triangles doesnt properly cut

// check out the assets stores packages on destroying meshes
namespace Zeltex.AnimationUtilities
{

	// for the slicing i will be using raw triangle data
	// when they get added to the mesh, i will generate the indicies then
	class MyTriangle 
	{
		Vector3 Vertex1;
		Vector3 Vertex2;
		Vector3 Vertex3;
	}
	// abstract out the slicing part
	// of turning 1 triangle into either 1,2 or 3
	// returns 1 triangle if plane doesnt slice it
	// returns 2 if the plane goes through, intersecting only one point, it therefor intersects just one line, slicing it perfectly in half
	// returns 3 triangles if the plane goes through 2 lines, slicing the triangle into 3

	// test1: Rip one half off using plane intersection points
	// test2: fill in whole using triangulation
	// test3: Instantiate a new mesh + GameObject using all points on the otherside of the plane

	[ExecuteInEditMode]
	public class MeshSlicer : MonoBehaviour
    {
		[Header("Debug")]
		public bool IsDebugMode = false;
		public bool IsDebugGui = false;
		public KeyCode SliceKey;
		public bool IsSlice = false;
		public KeyCode DebugVerticiesKey;
		public KeyCode RotateXKey;
		public KeyCode RotateYKey;
		public KeyCode RotateZKey;
		public float RotateSpeed = 30f;
		
		[Header("Options")]
		private GameObject MyDebugPlane;	// used to get normal and position of plane
		private Plane MySlicePlane;			// the plane itself used to find intersection points in triangles
		public Vector3 MyPlanePosition = new Vector3 (0,0,0);	// the normal for a plane
		public Vector3 MyPlaneNormal = new Vector3 (0,1f,0);	// the normal for a plane
		// the edge loop that gets hit
		public List<Vector3> SlicedPoints;
		private bool IsDebugSlicePoints = false;

		void OnGUI()
        {
			if (IsDebugMode && IsDebugGui)
            {
				GUILayout.Label ("Total Pieces: " + 0);
				GUILayout.Label ("SlicedPoints: " + SlicedPoints.Count);
				GUILayout.Label ("Plane Position: " + MyPlanePosition.ToString ());
				GUILayout.Label ("Plane normal: " + MyPlaneNormal.ToString () + ":" + MyPlaneNormal.magnitude);
				GUILayout.Label ("Plane normal: " + MySlicePlane.normal.ToString () + ":" + MySlicePlane.normal.magnitude);

			}
		}
		// Debug stuff
		// Debug lines for the slicing
		// an option to create new meshes from slices
		// make the original custom mesh debug the mesh points
		void Start()
        {
			if (transform.childCount == 2)
            {
				MyDebugPlane = transform.GetChild(1).gameObject;
				//Debug.LogError("NOrmalTHing:" + MyDebugPlane.transform.forward);
				UpdatePlane();
			}
		}

		public void UpdatePlane()
        {
			MyPlaneNormal = transform.InverseTransformVector(MyDebugPlane.transform.forward);
			// round it down to 2 decimal places
			/*MyPlaneNormal = new Vector3 (
				Mathf.RoundToInt(MyPlaneNormal.x*100)/100f,
				Mathf.RoundToInt(MyPlaneNormal.y*100)/100f,
				Mathf.RoundToInt(MyPlaneNormal.z*100)/100f
			);*/
			Debug.LogError ("Plane normal: " + MyPlaneNormal.ToString () + ":" + MyPlaneNormal.magnitude);

			MyPlanePosition = (MyDebugPlane.transform.localPosition);
			MyPlanePosition.x *= transform.localScale.x;
			MyPlanePosition.y *= transform.localScale.y;
			MyPlanePosition.z *= transform.localScale.z;
			MySlicePlane = new Plane(MyPlaneNormal, MyPlanePosition);
		}
		//public int whichone;
		// Update is called once per frame
		void Update () {
			if ((IsDebugMode && Input.GetKeyDown(SliceKey)) || IsSlice) 
			{
				IsSlice = false;
				IsDebugSlicePoints = false;
				UpdatePlane();
				Slice();
			}
			if (Input.GetKeyDown (DebugVerticiesKey)) {
				IsDebugSlicePoints = true;
				UpdatePlane();
				Slice();
				//transform.GetChild(0).GetComponent<RenderPoints> ().UpdatePoints (gameObject.GetComponent<MeshFilter> ().sharedMesh.vertices);
				transform.GetChild(0).GetComponent<RenderPoints> ().UpdatePoints (SlicedPoints.ToArray());
				//Debug.DrawLine(transform.TransformPoint(SlicedPoints[whichone]),
				//               transform.TransformPoint(SlicedPoints[whichone+1]),
				//               Color.red);
			}

			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey (RotateXKey)) 
			{
				MyDebugPlane.transform.Rotate(MyDebugPlane.transform.InverseTransformDirection(-new Vector3(RotateSpeed*Time.deltaTime,0,0)));
			} else if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (RotateXKey)) {
				MyDebugPlane.transform.Rotate(MyDebugPlane.transform.InverseTransformDirection(new Vector3(RotateSpeed*Time.deltaTime,0,0)));
			}
			if (Input.GetKey (RotateYKey)) 
			{
				MyDebugPlane.transform.Rotate(MyDebugPlane.transform.InverseTransformDirection(new Vector3(0,RotateSpeed*Time.deltaTime,0)));
			}
			if (Input.GetKey (RotateZKey)) 
			{
				MyDebugPlane.transform.Rotate(MyDebugPlane.transform.InverseTransformDirection(new Vector3(0,0,RotateSpeed*Time.deltaTime)));
			}
		} 
		// game object stuff
		private void UpdateMeshes(List<Vector3> PositionsLeft, List<int> IndiciesLeft, List<Vector3> PositionsRight, List<int> IndiciesRight) {
			// make changes here
			Mesh MyMesh = new Mesh ();
			MyMesh.vertices = PositionsLeft.ToArray();
			MyMesh.triangles = IndiciesLeft.ToArray();
			MyMesh.RecalculateNormals();
			gameObject.GetComponent<MeshFilter>().sharedMesh = MyMesh;
			gameObject.GetComponent<MeshCollider>().sharedMesh = null;
			gameObject.GetComponent<MeshCollider>().sharedMesh = MyMesh;
			// scrapped piece (theoritically i should calculate the biggest piece or piece attached to a bone structure
			Mesh MyMesh2 = new Mesh();
			MyMesh2.vertices = PositionsRight.ToArray();
			MyMesh2.triangles = IndiciesRight.ToArray();
			MyMesh2.RecalculateNormals();
			CreateShard(MyMesh2);
		}
		public void CreateShard(Mesh MyMesh2) {
			GameObject NewObject = new GameObject();
			NewObject.name = name + "'s shard";
			NewObject.transform.position = transform.position;
			NewObject.transform.localScale = transform.localScale;
			NewObject.AddComponent<MeshFilter>().sharedMesh = MyMesh2;
			NewObject.AddComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
			MeshCollider MyCollider = NewObject.AddComponent<MeshCollider>();
			MyCollider.sharedMesh = MyMesh2;
			MyCollider.convex = true;
			Rigidbody MyRigid = NewObject.AddComponent<Rigidbody>();
			MyRigid.isKinematic = gameObject.GetComponent<Rigidbody> ().isKinematic;
		}

		/*public Vector3 GetTriangleFaceNormal(Vector3 P1, Vector3 P2, Vector3 P3) {
			//center = ((P1 + P2 + P3) / 3)
			return Vector3.Cross(P2 - P1, P2 - P3).normalized;
		}*/

		// after slice, grab all the triangles that have lines intersecting the plane
		// fill in the points with triangles

		public void Slice() 
		{
			bool DidSliceMesh = false;
			Mesh MyMesh = gameObject.GetComponent<MeshFilter> ().sharedMesh;
			if (MyMesh) {

				List<Vector3> PositionsLeft = new List<Vector3>();
				List<int> IndiciesLeft = new List<int>();
				List<Vector3> PositionsRight = new List<Vector3>();
				List<int> IndiciesRight = new List<int>();

				SlicedPoints = new List<Vector3>(); // just used here

				int[] OriginalIndicies = MyMesh.GetIndices(0);
				Vector3[] OriginalVerticies = MyMesh.vertices;
				// Add all the indicies that intersect with the plane
				for (int i = 0; i < OriginalIndicies.Length; i += 3) {	// for all the meshes triangles
					Vector3 Vertex1 = OriginalVerticies[OriginalIndicies[i]];
					Vector3 Vertex2 = OriginalVerticies[OriginalIndicies[i+1]];
					Vector3 Vertex3 = OriginalVerticies[OriginalIndicies[i+2]];
					// left side
					if (MySlicePlane.GetSide(Vertex1) && MySlicePlane.GetSide(Vertex2) && MySlicePlane.GetSide(Vertex3)) 
					{	// if all 3 verticies are on the same side
						PositionsLeft.Add (Vertex1);
						IndiciesLeft.Add (PositionsLeft.Count-1);
						PositionsLeft.Add (Vertex2);
						IndiciesLeft.Add (PositionsLeft.Count-1);
						PositionsLeft.Add (Vertex3);
						IndiciesLeft.Add (PositionsLeft.Count-1);
					}
					// right side
					else if (!MySlicePlane.GetSide(Vertex1) && !MySlicePlane.GetSide(Vertex2) && !MySlicePlane.GetSide(Vertex3))
					{ // if they are not on same side, they are intersected by the plane
						// for now just remove these
						PositionsRight.Add (Vertex1);
						IndiciesRight.Add (PositionsRight.Count-1);
						PositionsRight.Add (Vertex2);
						IndiciesRight.Add (PositionsRight.Count-1);
						PositionsRight.Add (Vertex3);
						IndiciesRight.Add (PositionsRight.Count-1);
					}
					// has been sliced
					else 
					{
						DidSliceMesh = true;
						bool[] DidSlice = CheckVertexForPlaneIntersect(Vertex1, Vertex2, Vertex3,
						                                               PositionsLeft, PositionsRight);	// the one thats false is the odd one out
						if (!(!DidSlice[0] && !DidSlice[1] && !DidSlice[2])) // if didnt fail to intersect 2 lines
						{
							ConvertStuff(DidSlice, Vertex1, Vertex2, Vertex3,
							             PositionsLeft, IndiciesLeft, PositionsRight, IndiciesRight);
						} else {

						}
					}
				}
				//RemoveDuplicates(SlicedPoints);
				if (SlicedPoints.Count >= 3) 
				{
					for (int i = 0; i < SlicedPoints.Count-3; i += 2) 
					{
						IndiciesLeft.Add (IndexOf(PositionsLeft, SlicedPoints[i]));
						IndiciesLeft.Add (IndexOf(PositionsLeft, SlicedPoints[i+1]));
						IndiciesLeft.Add (IndexOf(PositionsLeft, SlicedPoints[i+3]));
					}
				}
				if (DidSliceMesh && !IsDebugSlicePoints) {
					UpdateMeshes(PositionsLeft, IndiciesLeft, PositionsRight, IndiciesRight);
					Debug.LogError("Sliced mesh: " + PositionsLeft.Count + " : " + PositionsRight.Count);
				}
			}
		}

		public void RemoveDuplicates(List<Vector3> Verticies) {
			for (int i = Verticies.Count - 2; i >= 0; i--) {
				for (int j = i+1; j < Verticies.Count; j++) {
					if (Verticies[i] == Verticies[j]) {
						Verticies.RemoveAt(j);
					}
				}
			}
		}

		public int IndexOf(List<Vector3> Verticies, Vector3 SearchVertex) {
			for (int i = 0; i < Verticies.Count; i++)
			if (SearchVertex == Verticies [i])
				return i;
			return -1;
		}

		// returns a list of triangles on either side of the plane
		// indicies are 0 to 4, for vertex 1 to 5
		// returns 9 indicies, for 3 triangles
		public bool AddIntersectPoint(Vector3 Vertex1, Vector3 Vertex2) {
			float Distance;
			Ray MyRay1 = new Ray(Vertex1,	//(Vertex1+Vertex2)/2f,
			                     (Vertex2-Vertex1).normalized);
			if (MySlicePlane.Raycast(MyRay1, out Distance)) 
			{
				Vector3 RayPoint = MyRay1.GetPoint(Distance);
				Vector3 MidPoint = (Vertex2+Vertex1)/2f;	// mid point of line
				float DistanceFromMid = Vector3.Distance(Vertex1, Vertex2);
				if (Vector3.Distance(MidPoint, RayPoint) <= DistanceFromMid/2f) {
					SlicedPoints.Add (RayPoint);
					//bSlice[0] = true;
					return true;
				}
			}
			return false;
		}

		bool[] CheckVertexForPlaneIntersect(Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3, List<Vector3> PositionsLeft, List<Vector3> PositionsRight) 
			// for any triangle, the plane should slice through only 2 lines
		{
			bool[] bSlice = new bool[]{false, false, false};
			List<int> NewIndicies = new List<int> ();
			NewIndicies.Add (0);
			NewIndicies.Add (1);
			NewIndicies.Add (2);
			bSlice [0] = AddIntersectPoint (Vertex1, Vertex2);
			bSlice [1] = AddIntersectPoint (Vertex2, Vertex3);
			bSlice [2] = AddIntersectPoint (Vertex3, Vertex1);

			if (SlicedPoints.Count < 2) {
				Debug.LogError("Problem slicing a certain triangle!");
				return new bool[]{false,false,false};	// failed to intersect 2 lines..
			} 
			// if intersects with one line and goes through the others?
			//else if (SlicedPoints.Count == 1) {
				//PositionsLeft.Add (SlicedPoints [SlicedPoints.Count - 1]);
				//PositionsRight.Add (SlicedPoints [SlicedPoints.Count - 1]);
			//} 
			else {
				PositionsLeft.Add (SlicedPoints [SlicedPoints.Count - 2]);
				PositionsLeft.Add (SlicedPoints [SlicedPoints.Count - 1]);
				PositionsRight.Add (SlicedPoints [SlicedPoints.Count - 2]);
				PositionsRight.Add (SlicedPoints [SlicedPoints.Count - 1]);
			}
			return bSlice;
		}

		// Create a 3 triangles from 5 verticies
		// Where the line A-B was not intersected
		public void AddTriangleStuff(Vector3 VertexSingle,
		                             Vector3 VertexA, Vector3 VertexB, 
		                             bool IsReversed,  
		                             List<Vector3> PositionsLeft, List<int> IndiciesLeft,
		                             List<Vector3> PositionsRight, List<int> IndiciesRight) 
		{
			int IndexLeftD = PositionsLeft.Count-2;
			int IndexLeftE = PositionsLeft.Count-1;
			int IndexRightD = PositionsRight.Count-2;
			int IndexRightE = PositionsRight.Count-1;
			if (IsReversed) {
				IndexLeftE = PositionsLeft.Count-2;
				IndexLeftD = PositionsLeft.Count-1;
				IndexRightE = PositionsRight.Count-2;
				IndexRightD = PositionsRight.Count-1;
			}
			// which side is this on?
			if (!MySlicePlane.GetSide(VertexSingle)) {// right side has 1, left side has 2-3
				// verticies
				PositionsRight.Add (VertexSingle);
				int IndexRightC = PositionsRight.Count-1;

				PositionsLeft.Add (VertexA);
				int IndexRightA = PositionsLeft.Count-1;
				PositionsLeft.Add (VertexB);
				int IndexRightB = PositionsLeft.Count-1;
				
				// now indicies
				IndiciesRight.Add(IndexRightD);
				IndiciesRight.Add(IndexRightE);
				IndiciesRight.Add(IndexRightC);
				
				IndiciesLeft.Add (IndexLeftD);
				IndiciesLeft.Add(IndexRightB);
				IndiciesLeft.Add(IndexLeftE);
				IndiciesLeft.Add (IndexLeftD);
				IndiciesLeft.Add(IndexRightA);
				IndiciesLeft.Add(IndexRightB);
			} else {
				// left side verticies
				PositionsLeft.Add (VertexSingle);
				int IndexRightC = PositionsLeft.Count-1;

				PositionsRight.Add (VertexA);
				int IndexRightA = PositionsRight.Count-1;
				PositionsRight.Add (VertexB);
				int IndexRightB = PositionsRight.Count-1;
				
				// now indicies
				IndiciesLeft.Add (IndexLeftD);
				IndiciesLeft.Add(IndexLeftE);
				IndiciesLeft.Add(IndexRightC);
				
				IndiciesRight.Add (IndexRightD);
				IndiciesRight.Add(IndexRightB);
				IndiciesRight.Add(IndexRightE);
				IndiciesRight.Add (IndexRightD);
				IndiciesRight.Add(IndexRightA);
				IndiciesRight.Add(IndexRightB);
			}
		}
		// now turn the triangle into 3 triangles
		public void ConvertStuff(bool[] DidSlice, Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3, List<Vector3> PositionsLeft, List<int> IndiciesLeft, List<Vector3> PositionsRight, List<int> IndiciesRight) {
			int IndexLeft4 = PositionsLeft.Count-2;
			int IndexLeft5 = PositionsLeft.Count-1;
			int IndexRight4 = PositionsRight.Count-2;
			int IndexRight5 = PositionsRight.Count-1;
			if (!DidSlice[1]) // line 2-3 is not cut
			{
				AddTriangleStuff(Vertex1,
				                 Vertex2, Vertex3,
				                 false,
				                 PositionsLeft, IndiciesLeft, 
				                 PositionsRight, IndiciesRight);
			} 
			else if (!DidSlice[0]) // line 1-2 is not cut
			{
				AddTriangleStuff(Vertex3, 
				                 Vertex1, Vertex2,
				                 true,
				                 PositionsLeft, IndiciesLeft, 
				                 PositionsRight, IndiciesRight);
			} 
			else if (!DidSlice[2]) // line 3-1 is not cut - one with 2 is seperate
			{
				AddTriangleStuff(Vertex2, 
				                 Vertex3, Vertex1,
				                 true,
				                 PositionsLeft, IndiciesLeft, 
				                 PositionsRight, IndiciesRight);
			}
		}

		Vector3 GetRotatedNormal(Vector3 MyNormal) {
			if (MyNormal.normalized == Vector3.forward)
				return Vector3.Cross (MyNormal, Vector3.forward).normalized;
			else
				return Vector3.Cross (MyNormal, Vector3.up).normalized;
		}
		void DebugDrawPlane(Vector3 position, Vector3 normal, Color PlaneColor) 
		{
			float DebugPlaneSize = 0.6f;
			Vector3 v3 = normal*DebugPlaneSize;
			if (normal.normalized != Vector3.forward)
				v3 = Vector3.Cross(normal, Vector3.forward).normalized*DebugPlaneSize;
			else
				v3 = Vector3.Cross(normal, Vector3.up).normalized*DebugPlaneSize;

			Vector3 Up = v3;
			Vector3 Right = GetRotatedNormal (v3);

			var corner1 = position + Right + Up;
			var corner3 = position - Right - Up;
			
			var corner0 = position + Up - Right;
			var corner2 = position - Up + Right;
			
			Debug.DrawLine(corner0, corner2, PlaneColor);
			Debug.DrawLine(corner1, corner3, PlaneColor);
			Debug.DrawLine(corner0, corner1, PlaneColor);
			Debug.DrawLine(corner1, corner2, PlaneColor);
			Debug.DrawLine(corner2, corner3, PlaneColor);
			Debug.DrawLine(corner3, corner0, PlaneColor);
			Debug.DrawRay(position, normal, Color.red);
		}
	}
}
using UnityEngine;
using System.Collections;


namespace Zeltex.AnimationUtilities
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PointCloud : RenderPoints
    {
	    public float Range = 10f;	// range of the spawning of points
	    int SpawnedPoints = 60000;
	
	    // Use this for initialization
	    void Start () {
		    MyPoints.Clear ();
		    for (int i = 0; i < SpawnedPoints; i++) {
			    Vector3 NewPosition = new Vector3 (Random.Range (-Range, Range), Random.Range (-Range, Range), Random.Range (-Range, Range));
			    //NewPosition.x = Mathf.Clamp(NewPosition.x, -Range, Range);
			    MyPoints.Add (NewPosition);
			    MyColors.Add (new Color32 ((byte)Random.Range (0.0f, 1.0f),
			                               (byte)Random.Range (0.0f, 1.0f),
			                               (byte)Random.Range (0.0f, 1.0f),
			                               (byte)1.0f));
		    }
		    CreateMesh();
	    }
    }
}
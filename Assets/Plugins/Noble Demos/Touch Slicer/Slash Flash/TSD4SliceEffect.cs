using UnityEngine;
using System.Collections;

public class TSD4SliceEffect : MonoBehaviour
{
	public MeshFilter filter;
	
	public AnimationCurve brightnessCurve;
	
	public float duration = 0.5f;
	
	private Mesh mesh;
	
	private float startedAt;
	private float dieAt;
	
	private Color[] colorState;
	
	private static bool hasEverWarnedAboutNRE = false;
	
	// Use this for initialization
	void Start ()
	{
		if(filter == null)
		{
			if(hasEverWarnedAboutNRE == false)
			{
				hasEverWarnedAboutNRE = true;
				Debug.LogWarning("SliceEffect script got is missing a mesh filter. It will not be able to do a color shift.");
			}
		}
		else
		{
			colorState = new Color[ filter.sharedMesh.vertexCount ];
		
			mesh = filter.sharedMesh;
		}
		
		startedAt = Time.time;
		dieAt = startedAt + duration;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(dieAt < Time.time)
		{
			GameObject.Destroy(gameObject);
		}
		else if(mesh != null)
		{
			float t = (Time.time - startedAt) / duration;
			float b = brightnessCurve.Evaluate(t);
			
			Color c = Color.white;
			
			c.r = b;
			c.g = b;
			c.b = b;
			
			for(int i = 0; i < colorState.Length; i++)
			{
				colorState[i] = c;
			}
			
			mesh.colors = colorState;
		}
	}
}

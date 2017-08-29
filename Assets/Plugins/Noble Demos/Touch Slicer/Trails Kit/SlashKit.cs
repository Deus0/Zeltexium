using UnityEngine;
using System.Collections.Generic;

public enum SlashFadeMode {
	Square, Linear
}

public class SlashKit : MonoBehaviour
{
	private Mesh mesh;
	
	private List<Vector3> record = new List<Vector3>();
	private List<float> timeRecord = new List<float>();
	
	private Vector2 viewportSize, inputScaleFactor;
	
	public float width = 0.03f;
	public float maxAge = 0.1f;
	public float minimumGap = 0.06f;
	
	public SlashFadeMode fadeMode = SlashFadeMode.Square;
	
	private bool recording = false;
	
	public Material slashEffect;
	
	public int touchID = -1;
	
	// Use this for initialization
	void Start ()
	{
		Input.multiTouchEnabled = true;
		
		mesh = new Mesh();
		
		gameObject.AddComponent<MeshFilter>();
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		gameObject.AddComponent<MeshRenderer>();
		
		this.GetComponent<Renderer>().sharedMaterial = slashEffect;
		
		viewportSize = new Vector2(transform.parent.GetComponent<Camera>().aspect, 1f) * 2f;
		
		inputScaleFactor = new Vector2(viewportSize.x / Screen.width, viewportSize.y / Screen.height);	
	}
	
	// Update is called once per frame
	void Update ()
	{
		bool inhibit = Mathf.Approximately(Time.timeScale, 0);
		
		recording = false;
		
		Vector3 v = Vector3.zero;
		
		if(touchID == -1)
		{
			recording = Input.GetButton("Fire1") && Input.touchCount == 0;
			v = Input.mousePosition;
		}
		else
		{
			//Find the relevant touch.
			foreach(Touch t in Input.touches)
			{
				if(t.fingerId == touchID)
				{
					recording = true;
					v = t.position;
				}
			}
		}
		
		recording &= !inhibit;
		
		if(recording)
		{
			v.x *= inputScaleFactor.x;
			v.y *= inputScaleFactor.y;
			v.x -= viewportSize.x / 2f;
			v.y -= viewportSize.y / 2f;
			
			int priorIndex = record.Count - 1;
			bool isFarEnough = true;
			
			if(record.Count > 0)
			{
				Vector3 prior = record[ priorIndex ];
				
				float gap = (v - prior).magnitude;
				
				isFarEnough = gap > minimumGap;
			}
			
			if(isFarEnough)
			{
				record.Add(v);
				timeRecord.Add(Time.time);
			}
			else
			{
				record[priorIndex] = v;
			}
		}
		
		List<Vector3> vertices = new List<Vector3>();
		List<Color> color = new List<Color>();
		List<Vector2> UVs = new List<Vector2>();
		List<int> indices = new List<int>();
		
		if(record.Count > 0)
		{
			bool flip = false;
			
			int stoppedAt = -1;
			
			for(int i = record.Count - 1; i >= 0; i--)
			{
				Vector3 vertex = record[i];
				
				float age = Time.time - timeRecord[i];
				float fractionalAge = age / maxAge;
				
				if(fractionalAge > 1f)
				{
					//Just pretend we're done. If we set 0 to zero, later code will interpret us as at the end, and the loop will bail.
					fractionalAge = 1f;
					stoppedAt = i;
					i = 0;
				}
				
				if(fadeMode == SlashFadeMode.Square)
					fractionalAge *= fractionalAge;
				
				float opacity = 1f - fractionalAge;
				float localWidth = width * opacity;
				
				Color c = new Color(1f, 1f, 1f, opacity);
				
				if(i == 0)
				{
					//Last vertex
					indices.Add(vertices.Count);
					vertices.Add(vertex);
					UVs.Add(new Vector2(0.5f, 0f));
					color.Add(c);
				}
				else
				{
					//We have another vertex after
					Vector3 next = record[i - 1];
					
					Vector3 delta = vertex - next;
					
					Vector3 perpOne = new Vector3(-delta.y, delta.x).normalized * localWidth;
					Vector3 perpTwo = new Vector3(delta.y, -delta.x).normalized * localWidth;
					
					vertex.z = 10f;
					
					if(i == record.Count - 1)
					{
						indices.Add(vertices.Count);
						indices.Add(vertices.Count + 1);
						
						Vector3 lead = delta.normalized * localWidth;
						
						vertices.Add(vertex + perpOne + lead);
						vertices.Add(vertex + perpTwo + lead);
						
						UVs.Add(new Vector2(0f, 1f));
						UVs.Add(new Vector2(1f, 1f));
						
						color.Add(c);
						color.Add(c);						
					}
					
					Vector3 vertexOne = vertex + perpOne;
					Vector3 vertexTwo = vertex + perpTwo;
					
					indices.Add(vertices.Count);
					indices.Add(vertices.Count + 1);
					
					vertices.Add(vertexOne);
					vertices.Add(vertexTwo);
					
					UVs.Add(new Vector2(0f, 0.5f));
					UVs.Add(new Vector2(1f, 0.5f));
					
					color.Add(c);
					color.Add(c);
					
					flip = !flip;
				}
				
				if(stoppedAt != -1)
				{
					record.RemoveRange(0, stoppedAt + 1);
					timeRecord.RemoveRange(0, stoppedAt + 1);
				}
			}
			
			mesh.Clear();
			
			mesh.vertices = vertices.ToArray();
			mesh.uv = UVs.ToArray();
			mesh.colors = color.ToArray();
			
			mesh.SetTriangles( SlashKit.destripifyIndices( indices.ToArray() ), 0);
		}	
	}
		
	//We're going to convert our triangle strip indices to triangle indices here
	//because Unity no longer offers this service.
		
	private static int[] destripifyIndices(int[] indices)
	{
		int triangleCount = Mathf.Max( indices.Length - 2, 0 );
		
		int[] result = new int[triangleCount * 3];
		
		for(int i = 0; i < triangleCount; i++)
		{
			bool reverse = i % 2 == 1;
			
			int j = i * 3;
			
			if(reverse)
			{
				result[j] = i;
				result[j + 1] = i + 2;
				result[j + 2] = i + 1;
			}
			else
			{
				result[j] = i;
				result[j + 1] = i + 1;
				result[j + 2] = i + 2;
			}
		}
		
		return result;
	}
}



























































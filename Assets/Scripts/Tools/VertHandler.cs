#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace ZeltexTools
{
    //[AddComponentMenu("Mesh/Vert Handler")]
    //[ExecuteInEditMode]
    public class VertHandler : MonoBehaviour 
    {
	
	    public bool _destroy;
	
	    private Mesh mesh;
	    private Vector3[] verts;
	    private Vector3 vertPos;
	    private GameObject[] handles;
	
	    //private const string TAG_HANDLE = "VertHandle";
	
	    void OnEnable()
	    {
		    mesh = GetComponent<MeshFilter>().mesh;
		    verts = mesh.vertices;
		    foreach(Vector3 vert in verts)
		    {
			    vertPos = transform.TransformPoint(vert);
			    GameObject handle = new GameObject();
			    //         handle.hideFlags = HideFlags.DontSave;
			    handle.transform.position = vertPos;
			    handle.transform.parent = transform;
			    //handle.tag = TAG_HANDLE;
			    handle.AddComponent<VertHandleGizmo>()._parent = this;
			
		    }
	    }
	
	    void OnDisable()
	    {
		    //GameObject[] handles = GameObject.FindGameObjectsWithTag(TAG_HANDLE);
		    //foreach(GameObject handle in handles)
		    for (int i = transform.childCount-1; i >= 0 ; i--)
		    {
			    DestroyImmediate(transform.GetChild(i).gameObject);    
		    }
	    }
	
	    void Update()
        {
		    if(_destroy)
            {
			    _destroy = false;
			    DestroyImmediate(this);
			    return;
		    }
		
		    //handles = GameObject.FindGameObjectsWithTag (TAG_HANDLE);
		
		    for(int i = 0; i < transform.childCount; i++)
            {
			    verts[i] = transform.GetChild(i).localPosition;   
		    }
		
		    mesh.vertices = verts;
		    mesh.RecalculateBounds();
		    mesh.RecalculateNormals();
		
		
	    }
	
    }

    //[ExecuteInEditMode]
    public class VertHandleGizmo : MonoBehaviour 
    {
	
	    private static float CURRENT_SIZE = 0.1f;
	
	    public float _size = CURRENT_SIZE;
	    public VertHandler _parent;
	    public bool _destroy;
	
	    private float _lastKnownSize = CURRENT_SIZE;
	
	    void Update()
        {
		    // Change the size if the user requests it
		    if(_lastKnownSize != _size)
            {
			    _lastKnownSize = _size;
			    CURRENT_SIZE = _size;
		    }
		
		    // Ensure the rest of the gizmos know the size has changed...
		    if(CURRENT_SIZE != _lastKnownSize) {
			    _lastKnownSize = CURRENT_SIZE;
			    _size = _lastKnownSize;
		    }
		
		    if(_destroy)
			    DestroyImmediate(_parent);
	    }
	
	    void OnDrawGizmos()
        {
		    Gizmos.color = Color.red;
		    Gizmos.DrawCube(transform.position, Vector3.one * CURRENT_SIZE);
	    }
	
    }
}
#endif
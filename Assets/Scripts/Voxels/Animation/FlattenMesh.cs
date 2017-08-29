using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities
{
    /// <summary>
    /// Flattens a mesh at a certain speed
    /// </summary>
    public class FlattenMesh : MonoBehaviour
    {
	    private Mesh MyMesh;
	    private List<Vector3> MyOriginalVerts = new List<Vector3>();

	    float[] TimeStart = new float[3];
	    bool[] IsAnimating = new bool[3];
	    bool[] Direction = new bool[3];
	    public float AnimationSpeed = 3f;
	    public Vector3 TargetSize = new Vector3(0.1f,0.1f,0.1f);
	    public bool IsAnimateVerticies;
	    private Vector3 OriginalScale;
	    private Vector3 OriginalPosition;
	    public AudioClip MyJumpOnSound;
	    Vector3 OriginalBounds;

	    // oscillation
	    public bool[] IsOscilateScale =  new bool[]{false, false, false};
	    public Vector3 SinAmplitude = new Vector3(1,1,1);
	    public float RandomWavePhase = 0f;

	    // Use this for initialization
	    void Start () {

	    }
	    void Awake() {
		    if (gameObject.GetComponent<MeshFilter> ())
			    MyMesh = gameObject.GetComponent<MeshFilter> ().mesh;
		    if (gameObject.GetComponent<SkinnedMeshRenderer> ())
			    MyMesh = gameObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh;
		    gameObject.AddComponent<AudioSource> ();
		    OriginalScale = transform.localScale;	//MyMesh.bounds.size.y;
		    OriginalBounds = MyMesh.bounds.size;
		    OriginalPosition = transform.position;
		    //for (int i = 0; i <= 2; i++)
		    //	TimeStart[i] = Time.time;

		    for (int i = 0; i < MyMesh.vertices.Length; i++) {
			    MyOriginalVerts.Add (MyMesh.vertices[i]);
		    }
		    RandomWavePhase = Random.Range (0f, 9999f);
	    }
	    public void ToggleAnimation(int Type) {
		    Direction[Type] = !Direction[Type];
		    BeginAnimation (Type);
	    }

	    // Update is called once per frame
	    void Update () {
		    if (GetComponent<Rigidbody> ().isKinematic)
			    transform.position = OriginalPosition;
		    for (int i = 0; i <= 2; i++) {
			    //Debug.LogError("IsAnimating" + i + " : " + IsAnimating[i].ToString());
			    if (IsAnimating[i]) {
				    if (IsAnimateVerticies) {
					    AnimateVerticies(i);
				    } else {
					    AnimateScale(i);
				    }
			    }
			    if (IsOscilateScale[i]) {
				    OscilateScale(i);
			    }	
		    }
		    ReAdjustPositionFromScale ();
	    }
	
	    public bool IsGrowX = false;
	    public bool IsGrowY = false;
	    public bool IsGrowZ = false;
	    public bool IsSpecialX = false;	// used for z path
	    public bool IsSpecialZ = false;	// used for z path
	    public void BeginAnimation(int Type) {
		    if ((Type == 0 && IsGrowY) || (Type == 1 && IsGrowX) || (Type == 2 && IsGrowZ)) {
			    //Debug.LogError ("Starting of animation: " + Type);
			    TimeStart[Type] = Time.time;
			    IsAnimating[Type] = true;
			    Direction[Type] = false;
			    gameObject.GetComponent<AudioSource> ().PlayOneShot(MyJumpOnSound);
		    }
	    }
	    public void ContinueAnimation(int Type) {
		    if (!Direction[Type]) {	// if has not animated yet
			    if (!IsAnimating[Type]) {
				    //Debug.LogError (gameObject.name + " - taking hit: " + Time.time);
				    BeginAnimation(Type);
			    }
		    }
	    }


	    public void OscilateScale(int Type) {
		    //float SinAmplitude = 1f;
		    float MyWaveAddition;
		
		    switch(Type) {
			    case(0):
				    MyWaveAddition = SinAmplitude.y+SinAmplitude.y*Mathf.Sin (Time.time*AnimationSpeed+RandomWavePhase);
				    transform.localScale = new Vector3 (transform.localScale.x, OriginalScale.y + MyWaveAddition, transform.localScale.z);
				    if (GetComponent<Rigidbody> ().isKinematic)
					    transform.position += new Vector3(0, MyWaveAddition/2f,0);
				    break;
			
			    case(1):
				    MyWaveAddition = SinAmplitude.x+SinAmplitude.x*Mathf.Sin (Time.time*AnimationSpeed+RandomWavePhase);
				    transform.localScale = new Vector3 (OriginalScale.x + MyWaveAddition, transform.localScale.y, transform.localScale.z);
				    if (GetComponent<Rigidbody> ().isKinematic)
					    transform.position += new Vector3(MyWaveAddition/2f,0,0);
				
			    break;
			
			    case(2):
			
			    MyWaveAddition = SinAmplitude.z+SinAmplitude.z*Mathf.Sin (Time.time*AnimationSpeed+RandomWavePhase);
			    transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, OriginalScale.z + MyWaveAddition);
				    if (GetComponent<Rigidbody> ().isKinematic)
					    transform.position += new Vector3(0,0,MyWaveAddition/2f);
				    break;
		    }
	    }

	    public float GetTimeValue(int Type) 
	    {
		    return (1 / AnimationSpeed) * (Time.time - TimeStart[Type]);
	    }

	    public float GetNewY(int Type, float OriginalY, float TempY, float TimeValue) {
		    if (!Direction[Type])
			    return Mathf.Lerp(OriginalY, TempY,TimeValue);
		    else
			    return Mathf.Lerp(TempY, OriginalY,TimeValue);
	    }

	    public void AnimateScale(int Type) {
		    float TimeValue = GetTimeValue (Type);
		    float NewY;
		    switch(Type) {
		    case(0):
			    NewY = GetNewY(Type, OriginalScale.y, TargetSize.y, TimeValue);
			    transform.localScale = new Vector3 (transform.localScale.x, NewY, transform.localScale.z);
			    break;
		    case(1):
			    NewY = GetNewY(Type, OriginalScale.x, TargetSize.x, TimeValue);
			    transform.localScale = new Vector3 (NewY, transform.localScale.y, transform.localScale.z);
			    break;
		    case(2):
			    NewY = GetNewY(Type, OriginalScale.z, TargetSize.z, TimeValue);
			    transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, NewY);
			    break;
		    }
	    }
	    public void ReAdjustPositionFromScale() {
		    if (GetComponent<Rigidbody> ().isKinematic)
		    {
			    Vector3 NewPosition = OriginalPosition;
			    if (IsAnimating[0])
				    NewPosition += new Vector3(0,Mathf.Lerp(0f, TargetSize.y/2f-OriginalScale.y/2f, GetTimeValue (0)),0);
			    if (IsAnimating[1])
				    NewPosition +=  new Vector3(Mathf.Lerp(0f, TargetSize.x/2f-OriginalScale.x/2f,GetTimeValue (1)),0,0);
			    if (IsAnimating[2])
				    NewPosition += new Vector3(0f,0f,Mathf.Lerp(0f, TargetSize.z/2f-OriginalScale.z/2f,GetTimeValue (2)));
			    transform.position = NewPosition;
		    }
	    }

	    public void AnimateVerticies(int Type) {
		    Vector3[] vertices = MyMesh.vertices;
		    float BoundsY = OriginalScale.y;
		    float TimeValue = GetTimeValue (Type);
		    for (int i = 0; i < MyMesh.vertices.Length; i++) {
			    float TempY = TargetSize.y;
			    if (MyOriginalVerts[i].y >= TargetSize.y)
				    TempY = TargetSize.y;
			    else
				    TempY = 0f;
			
			    TempY = (TargetSize.y/(BoundsY*OriginalScale.y))*MyOriginalVerts[i].y;	// percentage times original y value
			    float NewY = GetNewY(Type, MyOriginalVerts[i].y, TempY, TimeValue);
			    vertices[i] = new Vector3(MyMesh.vertices[i].x, NewY, MyMesh.vertices[i].z);//+ new Vector3(0,-Mathf.Lerp(0f, BoundsY,TimeValue),0);
			    //if (GetComponent<Rigidbody> ().isKinematic)
			    //	vertices[i] = vertices[i] + new Vector3 (0, MyMesh.bounds.size.y, 0);	// new Vector3(0,Mathf.Lerp(0f, (OriginalBoundsY/OriginalScaleY)/2f,TimeValue),0);
			    //vertices[i] = new Vector3(MyMesh.vertices[i].x+Mathf.Sin(Time.time), MyMesh.vertices[i].y, MyMesh.vertices[i].z+Mathf.Sin(Time.time));
			    MyMesh.vertices = vertices;
		    }
		    if (GetComponent<Rigidbody> ().isKinematic)
			    transform.position = OriginalPosition + new Vector3 (0, MyMesh.bounds.size.y/2f, 0);

		    if (gameObject.GetComponent<MeshCollider>()) {
			    gameObject.GetComponent<MeshCollider>().sharedMesh = null;
			    gameObject.GetComponent<MeshCollider>().sharedMesh = MyMesh;
		    }
	    }
	    public void TakeHit(Collider MyCollision, Vector3 MyCollisionNormal) {
		    //Debug.LogError("Taking hit at " + Time.time + " : " + MyCollisionNormal.ToString());
		    if (MyCollisionNormal == new Vector3 (0, -1, 0)) {
			    ContinueAnimation(0);
			    if (IsSpecialZ)
				    ContinueAnimation(2);
			    if (IsSpecialX)
				    ContinueAnimation(1);
		    }
		    else if (MyCollisionNormal == new Vector3 (1, 0, 0) || MyCollisionNormal == new Vector3 (-1, 0, 0)) {
			    ContinueAnimation(1);
		    }
		    else if (MyCollisionNormal == new Vector3 (0, 0, 1) || MyCollisionNormal == new Vector3 (0, 0, -1)) {
			    ContinueAnimation(2);
		    }
	    }
    }
}
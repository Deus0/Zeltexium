using UnityEngine;
using System.Collections;

namespace Zeltex.AI
{
    public class ArtificialGravity : MonoBehaviour
    {
	    public Vector3 GravityForce;
	    private Rigidbody MyRigid;
	    //public bool IsAutoBalance = false;
		
	    // Use this for initialization
	    void Start () {
		    MyRigid = gameObject.GetComponent<Rigidbody> ();
	    }
	    void Update() {
		    if (Input.GetKeyDown (KeyCode.U)) {
			    if (gameObject.GetComponent<RectTransform> ()){
				    GravityForce = -GravityForce;
				    transform.localScale = new Vector3 (transform.localScale.x, -transform.localScale.y, transform.localScale.z);
			    }
		    }
	    }
	    // Update is called once per frame
	    void FixedUpdate () {
		    if (MyRigid)
			    MyRigid.AddForce (GravityForce);
		    //if (IsAutoBalance) {
		    //	MyRigid.AddRelativeTorque(GravityForce.normalized);
		    //}
	    }
    }
}
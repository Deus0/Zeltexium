using UnityEngine;
using System.Collections;

namespace Zeltex.Physics
{
    /// <summary>
    /// Gives something gravity
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Gravity : MonoBehaviour
    {
        public Vector3 GravityForce = new Vector3(0, -1, 0);
	    private Rigidbody MyRigid;
		
	    // Use this for initialization
	    void Awake()
        {
		    MyRigid = gameObject.GetComponent<Rigidbody>();
	    }

	    /*void Update()
        {
		    if (Input.GetKeyDown (KeyCode.U))
            {
			    if (gameObject.GetComponent<RectTransform> ())
                {
				    GravityForce = -GravityForce;
				    transform.localScale = new Vector3 (transform.localScale.x, -transform.localScale.y, transform.localScale.z);
			    }
		    }
	    }*/

	    // Update is called once per frame
	    void FixedUpdate ()
        {
		    if (MyRigid)
            {
                MyRigid.AddForce(GravityForce);
            }
	    }
    }
}
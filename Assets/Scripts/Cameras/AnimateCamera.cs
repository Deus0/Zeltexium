using UnityEngine;
using System.Collections;

namespace Zeltex.WorldUtilities
{

    public class AnimateCamera : MonoBehaviour
    {
	    public float TimeAnimate = 5f;
	    public float FieldOfView1 = 60;
	    public float FieldOfView2 = 90;
	    public float AdditionY1 = 0.2f;
	    public float AdditionY2 = 0.8f;
	    float TimeBegan = 0f;
	    // Use this for initialization
	    void Start () {
		    TimeBegan = Time.time;
	    }
	
	    // Update is called once per frame
	    void Update () {
		    float TimeSince = Time.time - TimeBegan;
		    if (TimeSince <= TimeAnimate) {
			    float TimePercent = TimeSince / TimeAnimate;
			    gameObject.GetComponent<Camera> ().fieldOfView = Mathf.Lerp (FieldOfView1, FieldOfView2, TimePercent);
			    float NewY = Mathf.Lerp (AdditionY1, AdditionY2, TimePercent);
			    transform.localPosition = new Vector3 (transform.localPosition.x, NewY, transform.localPosition.z);
			    //Debug.LogError ("New Y: " + NewY);
		    }
	    }
    }
}

using UnityEngine;
using System.Collections;

namespace Zeltex.WorldUtilities
{
    public class OpenDoor : MonoBehaviour
    {
	    public GameObject MyRaycastObject;

	    // Use this for initialization
	    void Awake ()
        {
		    if (MyRaycastObject == null)
			    MyRaycastObject = gameObject;
	    }
	
	    // Update is called once per frame
	    void Update ()
        {
		    if (Input.GetMouseButtonDown (0))
            {
			    Debug.Log("Attempting to open door!");
			    RaycastHit MyHit;
			    if (Physics.Raycast (MyRaycastObject.transform.position, MyRaycastObject.transform.forward, out MyHit))
                {
				    Door MyDoor = MyHit.collider.gameObject.GetComponent<Door>();
				    if (MyDoor) {
					    Debug.Log("Toggling door!");
					    MyDoor.ToggleDoor();
				    }
			    }
		    }
        }
    }
}

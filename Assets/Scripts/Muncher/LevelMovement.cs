using UnityEngine;

namespace MuncherSystem
{
    public class LevelMovement : MonoBehaviour
    {
	    public GameObject MyFollowObject;
	    public float InitialWait = 3f;
	    private float StartTime = 0f;
	    public float AnimateTime = 30f;
	    public Vector3 BeginPosition;
	    public Vector3 EndPosition;
	    private RectTransform MyRect;
	    public float AnimationSpeed = 4f;

	    // Use this for initialization
	    void Start ()
        {
		    MyRect = gameObject.GetComponent<RectTransform> ();
		    StartTime = Time.time;
		    BeginPosition = MyRect.anchoredPosition3D;
	    }
	
	    // Update is called once per frame
	    void Update()
        {
		    if (MyFollowObject == null)
            {
			    if (Time.time > StartTime + InitialWait)
                {
				    float TimeThingy = (Time.time - StartTime - InitialWait) / AnimateTime;
				    MyRect.anchoredPosition3D = Vector3.Lerp (BeginPosition, EndPosition, TimeThingy);
			    }
		    }
            else
            {
			    float DifferenceX = MyFollowObject.GetComponent<RectTransform>().anchoredPosition3D.x;
			    float DifferenceY = MyFollowObject.GetComponent<RectTransform>().anchoredPosition3D.y;
			    Vector3 LevelTargetPosition = MyRect.anchoredPosition3D + new Vector3(-DifferenceX,-DifferenceY,0);
			    MyRect.anchoredPosition3D = Vector3.Lerp (MyRect.anchoredPosition3D, LevelTargetPosition, Time.deltaTime*AnimationSpeed);
			    Vector3 PlayerTargetPosition = MyFollowObject.GetComponent<RectTransform>().anchoredPosition3D  + new Vector3(-DifferenceX,-DifferenceY,0);
			    MyFollowObject.GetComponent<RectTransform>().anchoredPosition3D = 
				    Vector3.Lerp (MyFollowObject.GetComponent<RectTransform>().anchoredPosition3D , PlayerTargetPosition, Time.deltaTime*AnimationSpeed);
		    }
	    }
    }
}
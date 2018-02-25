using UnityEngine;
using System.Collections;

namespace MuncherSystem
{
    public class MinigameGuiHandler : MonoBehaviour
    {
	    public UnityEngine.Events.UnityEvent OnBegin = null;
	    public UnityEngine.Events.UnityEvent OnExit = null;

	    // Update is called once per frame
	    void Update ()
        {
		    if (Input.GetKeyDown(KeyCode.Escape))
            {
			    //Debug.LogError("Pressing Esc");
			    if (OnExit != null)
                {
				    OnExit.Invoke();
			    }
		    }
	    }
    }
}
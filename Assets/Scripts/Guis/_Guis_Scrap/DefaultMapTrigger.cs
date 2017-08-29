using UnityEngine;
using System.Collections;

namespace Zeltex.Guis
{
    /// <summary>
    /// Enables the upload button upon a certain key pressed
    /// Need to make this only work with a console command
    /// </summary>
    public class DefaultMapTrigger : MonoBehaviour
    {
        public KeyCode MyTrigger = KeyCode.L;
        public GameObject MyUploadButton;

        // Update is called once per frame
        void Update ()
        {
	        if (Input.GetKeyDown(MyTrigger))
            {
                MyUploadButton.SetActive(true);
            }
	    }
    }
}
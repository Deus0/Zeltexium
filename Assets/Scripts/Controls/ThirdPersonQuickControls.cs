using UnityEngine;
using Zeltex.Characters;
using Zeltex.Game;

namespace Zeltex
{
    /// <summary>
    /// Really basic script to change camera position
    /// </summary>
    public class ThirdPersonQuickControls : MonoBehaviour
    {
        #region Variables
        public float ZoomDistance = -0.5f;
        public KeyCode ThirdPersonKey = KeyCode.V;
        float OriginalZ;
        Vector3 DifferenceVector;
        bool IsThirdPerson = false;
        #endregion

        #region Mono
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(ThirdPersonKey) && transform.parent != null)
            {
                IsThirdPerson = !IsThirdPerson;
                Debug.Log("On Key Press [" + Time.realtimeSinceStartup + "] Setting Camera ThirdPerson in " + name + " to " + IsThirdPerson);
                GetComponent<Player>().SetHeadMesh(IsThirdPerson);
                if (IsThirdPerson)
                {
                    transform.localPosition = new Vector3(0, 0, ZoomDistance);
                }
                else
                {
                    transform.localPosition = new Vector3(0, 0, 0);
                }
            }
        }
        #endregion
    }
}
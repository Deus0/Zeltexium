using UnityEngine;

namespace DerelictComputer
{
    public class EscToQuit : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}

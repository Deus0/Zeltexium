using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// Managers player controllers
    /// </summary>
    public class PlayerManager : ManagerBase<PlayerManager>
    {
        /// <summary>
        /// Sets the new controller for a player manager
        /// </summary>
        public void SetController(string ControllerName)
        {
            Debug.Log("Setting controller to " + ControllerName);
            Possess[] MyControllers = Camera.main.gameObject.GetComponents<Possess>();
            for (int i = 0; i < MyControllers.Length; i++)
            {
                MyControllers[i].enabled = false;
            }

            if (ControllerName == "First Person")
            {
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i] is Player)
                    {
                        MyControllers[i].enabled = true;
                        break;
                    }
                }
            }
            else if (ControllerName == "Third Person")
            {

            }
            else if (ControllerName == "Top Down")
            {
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i] is StrategistController)
                    {
                        MyControllers[i].enabled = true;
                        break;
                    }
                }
            }
            else if (ControllerName == "Strategist")
            {
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i] is StrategistController)
                    {
                        MyControllers[i].enabled = true;
                        break;
                    }
                }
            }
        }
    }

}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Guis
{
    /// <summary>
    /// Util functions for GUis
    /// </summary>
    public class GUIUtil
    {

        public static bool IsInputFieldFocused()
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return (obj != null && obj.GetComponent<InputField>() != null);
        }
    }

}
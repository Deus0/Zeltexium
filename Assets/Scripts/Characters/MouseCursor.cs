using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex
{
    /// <summary>
    /// Sets the mouse cursor texture and lock state
    /// Different states:
    ///     - No tool selected
    /// Texture Painter tools
    ///     - Brush
    ///     - Erase
    ///     - Fill
    ///     - Line
    ///     
    /// </summary>
    public class MouseCursor : ManagerBase<MouseCursor>
    {
        #region Variables
        //private bool IsVisible = true;
        //private bool IsLocked = false;
        [Header("Mouse Textures")]
        public List<Texture2D> MyCursors;
        string MouseType = "DefaultMouse";
        #endregion

        #region Mono
        void Start()
        {
            //Debug.Log("StartUp [" + Time.realtimeSinceStartup + "] Setting Cursor Texture in " + name);
            if (MyCursors.Count > 0)
            {
                Cursor.SetCursor(MyCursors[0], new Vector2(0, 0), CursorMode.ForceSoftware);
            }
        }
        #endregion

        #region MouseSettings
        public void SetMouseIcon(string IconType)
        {
            if (IconType != MouseType)
            {
                for (int i = 0; i < MyCursors.Count; i++)
                {
                    if (MyCursors[i].name == IconType)
                    {
                        Cursor.SetCursor(MyCursors[i], new Vector2(0, 0), CursorMode.ForceSoftware);
                        break;
                    }
                }
                MouseType = IconType;
            }
        }
        #endregion
    }
}
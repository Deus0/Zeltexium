using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// Manages cheating in Zeltex
    /// </summary>
    public class CheatsManager : ManagerBase<CheatsManager>
    {
        public bool IsCheatsEnabled = true;
        public KeyCode DamageCheatKey = KeyCode.F3;

    }

}
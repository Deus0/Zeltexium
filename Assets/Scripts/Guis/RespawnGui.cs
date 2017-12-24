using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex
{
    public class RespawnGui : MonoBehaviour
    {
        public Text CountingDownText;

        public IEnumerator CountDown(System.Action OnFinish = null, int Counter = 5)
        {
            Debug.Log("Respawning Gui set to " + Counter);
            string TextDots = "";
            while (Counter > 0)
            {
                TextDots += ".";
                yield return new WaitForSeconds(1f);
                Counter--;
                if (TextDots.Length > 6)
                {
                    TextDots = "";
                }
                CountingDownText.text = " Respawning in " + "[" + Counter + "] " + TextDots;
            }
            if (OnFinish != null)
            {
                OnFinish.Invoke();
            }
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Zeltex.Guis
{
    public class LoadingGui : MonoBehaviour
    {
        public TextMeshProUGUI HeaderText;
        public UnityEngine.UI.Text PercentText;
        public bool IsTurnOffOnStart = true;
        [HideInInspector]
        public ZelGui MyZel;

        protected void Awake()
        {
            MyZel = GetComponent<ZelGui>();
            if (IsTurnOffOnStart)
            {
                MyZel.TurnOff();
            }
        }

        public void SetPercentage(float NewPercent)
        {
            MyZel = GetComponent<ZelGui>();
            MyZel.TurnOn();
            PercentText.text = Mathf.FloorToInt(NewPercent * 100f) + "%";
        }

        public void SetText(string NewText)
        {
            HeaderText.text = NewText;
        }

        public void TurnOn(string NewText)
        {
            MyZel = GetComponent<ZelGui>();
            SetText(NewText);
            MyZel.TurnOn();
        }

        public void TurnOff() 
        {
            MyZel.TurnOff();
        }
    }
}
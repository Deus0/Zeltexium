using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Guis
{
    public class LoadingGui : ManagerBase<LoadingGui>
    {
        public UnityEngine.UI.Text PercentText;
        [HideInInspector]
        public ZelGui MyZel;
        private float Percent = 0;

        protected override void Awake()
        {
            base.Awake();
            MyZel = GetComponent<ZelGui>();
            MyZel.TurnOff();
        }

        public void SetPercentage(float NewPercent)
        {
            Percent = NewPercent;
            PercentText.text = Mathf.RoundToInt(NewPercent * 100f) + "%";
        }
    }
}
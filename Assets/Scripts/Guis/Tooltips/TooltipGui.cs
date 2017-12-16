using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis
{
    public class TooltipGui : ManagerBase<TooltipGui>
    {
        public Text NameLabel;
        public Text DescriptionLabel;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void SetTexts(string NameText, string DescText)
        {
            NameLabel.text = NameText;
            DescriptionLabel.text = DescText;
        }
    }

}
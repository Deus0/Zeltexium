using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Zeltex.Guis
{
    public class TooltipHandle : MonoBehaviour,
                                IPointerEnterHandler,
                                IPointerExitHandler
    {
        private TooltipGui MyTooltipGui;
        public string TooltipNameLabel = "Play";
        public string TooltipDescriptionLabel = "Play through the game. You will fight many foes and make many allies.";

        private void Start()
        {
            MyTooltipGui = TooltipGui.Get();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MyTooltipGui.gameObject.SetActive(true);
            MyTooltipGui.SetTexts(TooltipNameLabel, TooltipDescriptionLabel);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MyTooltipGui.gameObject.SetActive(false);
        }
    }
}
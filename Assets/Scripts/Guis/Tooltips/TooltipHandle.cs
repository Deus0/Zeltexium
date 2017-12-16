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
        private bool HasEntered;
        public string TooltipNameLabel = "Play";
        public string TooltipDescriptionLabel = "Play through the game. You will fight many foes and make many allies.";

        private void Start()
        {
            MyTooltipGui = TooltipGui.Get();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HasEntered = true;
            MyTooltipGui.gameObject.SetActive(true);
            MyTooltipGui.SetTexts(TooltipNameLabel, TooltipDescriptionLabel);
        }

        private void Update()
        {
            if (HasEntered)
            {

            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void OnDestroy()
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            if (HasEntered)
            {
                HasEntered = false;
                MyTooltipGui.gameObject.SetActive(false);
            }
        }
    }
}
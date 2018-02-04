using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zeltex.Quests;

namespace Zeltex.Guis
{
    public class QuestBeginGui : MonoBehaviour
    {
        private Quest TargetQuest;
        [SerializeField]
        private Text QuestNameText;
        [SerializeField]
        private Text QuestDescriptionText;
        [SerializeField]
        private Text QuestRewardText;

        public void SetQuest(Quest MyQuest)
        {
            TargetQuest = MyQuest;
            FillGui();
        }

        private void FillGui()
        {
            QuestNameText.text = TargetQuest.GetName();
            QuestDescriptionText.text = TargetQuest.GetDescriptionText();
            QuestRewardText.text = "";
        }
    }

}
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

        public void Initialize(Quest MyQuest)
        {
            TargetQuest = MyQuest;
            QuestNameText.text = MyQuest.GetName();
            QuestDescriptionText.text = MyQuest.GetDescriptionText();
            QuestRewardText.text = "";
        }
    }

}
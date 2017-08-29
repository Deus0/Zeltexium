using UnityEngine;
using UnityEngine.UI;
using Zeltex.Characters;
using Zeltex.Combat;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Handles the stats gui for characters
    /// </summary>
	public class StatGuiHandler : GuiList
    {
		public Character MyCharacter;
        public bool IsEquipmentStats;

        public void OnNewGuiStats()
        {
            Clear();
           // CharacterStats MyCharacterStats = MyCharacter.GetComponent<CharacterStats>();
            if (MyCharacter.GetStats() != null)
            {
                if (!IsEquipmentStats)
                {
                    for (int i = 0; i < MyCharacter.GetStats().GetSize(); i++)
                    {
                        AddStat(MyCharacter.GetStats().GetStat(i));
                    }
                }
                else
                {
                    for (int i = 0; i < MyCharacter.GetStats().EquipmentStats.GetSize(); i++)
                    {
                        AddStat(MyCharacter.GetStats().EquipmentStats.GetStat(i));
                    }
                }
            }
        }
        private void AddStat(Stat NewStat)
        {
            GuiListElementData MyData = new GuiListElementData();
            MyData.LabelText = NewStat.GetToolTipName();
            MyData.DescriptionText = NewStat.GetToolTipText();
            Add(NewStat.GuiString(), MyData);
        }

        public void UpdateGuiStats()
        {
            if (MyCharacter.GetStats().GetSize() != MyGuis.Count)
            {
                OnNewGuiStats();
                return;
            }
            if (MyCharacter.GetStats() != null)
            {
                for (int i = 0; i < MyCharacter.GetStats().GetSize(); i++)
                {
                    UpdateStat(
                        MyCharacter.GetStats().GetStat(i),
                        i
                    );
                }
            }
        }
        /// <summary>
        /// Converts the stat data to a gui object
        /// </summary>
        private void UpdateStat(Stat NewStat, int GuiIndex)
        {
            GameObject MyGuiCell = MyGuis[GuiIndex].gameObject;
            GuiListElementData MyData = MyGuiCell.GetComponent<GuiListElement>().MyGuiListElementData;
            MyData.LabelText = NewStat.GetToolTipName();
            MyData.DescriptionText = NewStat.GetToolTipText();
            MyGuiCell.GetComponent<GuiListElement>().MyGuiListElementData = MyData;
            MyGuiCell.transform.Find("CellLabel").gameObject.GetComponent<Text>().text = NewStat.GuiString();
            if (NewStat.GetTexture() != null)
            {
                MyGuiCell.transform.Find("Texture").gameObject.GetComponent<RawImage>().texture = NewStat.GetTexture();
            }
        }

    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Characters;
using Zeltex.Quests;

/*
	Handles just the gui's of quests
		
*/

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Handles the gui for the quests
    /// </summary>
	public class QuestLogGuiHandler : GuiList
    {
		public Character MyCharacter;
		public Color32 MyQuestColour = new Color (0, 22, 66);
		public Color32 MyQuestCompleteColour = new Color (0, 55, 155);
		public Color32 MyHandedInQuestColour = new Color (0, 55, 155);
		public bool IsCompletedOnly = false;
		public bool IsNonCompletedOnly = false;
		
		public void ResetFilters()
        {
			IsCompletedOnly = false;
			IsNonCompletedOnly = false;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}

		public void FilterNonCompletedOnly()
        {
			IsCompletedOnly = false;
			IsNonCompletedOnly = true;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}
		public void FilterCompletedOnly()
        {
			IsCompletedOnly = true;
			IsNonCompletedOnly = false;
			UpdateQuestGuis ();
			CheckQuestCompletitions ();
		}

		public void UpdateQuestGuis()
        {
            QuestLog MyQuestLog = MyCharacter.GetQuestLog();
            //Debug.LogError("Updating Quests! " + MyQuestLog.GetSize());
			//Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			if (MyQuestLog != null)
            {
				Clear ();
				for (int i = 0; i < MyQuestLog.GetSize(); i++)
                {
                    Quest MyQuest = MyQuestLog.Get(i);

                    if (IsRenderQuest (MyQuest))
                    {
						GuiListElementData MyData = new GuiListElementData ();
						MyData.LabelText = MyQuest.GetLabelText();
						MyData.DescriptionText = MyQuest.GetDescriptionText ();
                        Add(MyData.LabelText, MyData);
					}
				}
				CheckQuestCompletitions ();	// always keep colours up to date!
			}
		}

		public bool IsRenderQuest(Quest MyQuest)
        {
			if ((!IsCompletedOnly && !IsNonCompletedOnly) || 
			    (IsCompletedOnly && MyQuest.HasCompleted()) || 
			    (IsNonCompletedOnly && !MyQuest.HasCompleted())) 
				return true;
			return false;
		}

		public void CheckQuestCompletitions()
        {
			QuestLog MyQuestLog = MyCharacter.GetQuestLog();
			if (MyQuestLog != null)
            {
				int j = 0;
				for (int i = 0; i < MyQuestLog.GetSize(); i++)
                {
                    Quest MyQuest = MyQuestLog.Get(i);

                    if (IsRenderQuest(MyQuest))
					{
						if (MyQuest.IsHandedIn)
                        {
							MyGuis [j].GetComponent<RawImage> ().color = MyHandedInQuestColour;
						} 
						else if (MyQuest.HasCompleted())
                        {
							MyGuis [j].GetComponent<RawImage> ().color = MyQuestColour;
						}
                        else
                        {
							MyGuis [j].GetComponent<RawImage> ().color = MyQuestCompleteColour;
						}
						j++;
					}
				}
			}
		}
	}
}
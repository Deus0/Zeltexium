using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Used to display tutorial tips to the user
    /// </summary>
    public class TutorialHandler : MonoBehaviour 
    {
	    private int SelectedTip = 0;
        private List<string> MyTutorialTips = new List<string>();
	    public Text MyTutorialText;

        public void Awake()
        {
            SelectedTip = -1;
            Next();
        }
        public void GenerateTutorialTips()
        {
            // input tips
            MyTutorialTips.Add("Welcome to Zeltex\n" +
                "   Adventure to your hearts content\n" +
                "   Strategize to defeat your enemies\n" +
                "   Survive until you are the last one");
            MyTutorialTips.Add(
                "Keys\n" +
                "   Right Click in FreeRoam to Move the Camera\n" +
                "   WASD to Move the Character\n" +
                "   Mouse Movement for Camera Rotation[FirstPerson]\n" +
                "   Mouse Left Click to activate an items ability\n" +
                "   C to open the main menu\n" +
                "   E to use an action on a Voxel or Character\n" +
                "   Keys 1 to 5 to Switch Items\n" +
                "   F to turn on Fly mode\n" +
                "   If Flying, E or Q to fly up and down\n"
            );
            // Character GUIS
            MyTutorialTips.Add(
                "Character Guis\n" +
                "   Skillbar - Quick Slot Items\n" +
                "   Label - Name and Stats\n" +
                "   Inventory - Character Items\n" +
                "   QuestLog - Log of Quests\n" +
                "   Stats - Stats\n" +
                "   Clock - Time of day and date\n" +
                "   Log - A log of recent events\n"
            );
            // Match Mode
            /*MyTutorialTips.Add(
                "Match Mode\n" +
                "   Editor Tools\n" +
                "   Limited Map\n" +
                "   Temporary Data\n" +
                "   Various rules determine the match\n"
            );*/
            // Game Modes
            MyTutorialTips.Add(
                "Game Rules\n" +
                "   - Time Limit\n" +
                "   - Kill Count\n" +
                "   - Lives Limit\n" +
                "   - Base Kill\n" +
                "   - Capture Points\n" +
                "   - Party\n" +
                "   - Bases\n" +
                "   - Bosses\n" +
                "   - Minion Spawns\n"
            );
            // Adventure Mode
            MyTutorialTips.Add(
                "[Future] Adventure Mode\n" +
                "   - Seeds Generate Content\n" +
                "   - Disabled Editor Tools\n" +
                "   - Endless Terrain\n" +
                "   - Towns, Cities and Roads\n" +
                "   - Dungeon Instances\n" +
                "   - Cities are built around dungeon entrances\n"
            );
            // Tools
            // Meta Dat
            MyTutorialTips.Add(
                "There are many Tools you can use to Edit Data\n" +
                "   Voxels\n" +
                "   Items\n" +
                "   Stats\n" +
                "   Recipes\n" +
                "   Spells\n" +
                "   Quests\n" +
                "   Dialogue\n" +
                "   Classes\n" +
                "   Levels\n"
            );
            // Purely for Art
            MyTutorialTips.Add(
                "There are many Tools that let you create Art\n" +
                "   Textures\n" +
                "   Polygonal Models\n" +
                "   Voxel Models\n" +
                "   Skeletons\n" +
                "   animations\n" +
                "   Sounds\n"
            );
            // About tips
            MyTutorialTips.Add(
                "Created by Deus\n" +
                "   - Inspired by Minecraft, Zelda and the Many RPG novels of the last decade\n" +
                "   - Version 0.1\n" +
                "   - Been in development since 2015\n" +
                "   - Any feedback, please send to marz.tierney@gmail.com\n"

            );
	    }

        public void Previous()
        {
            SelectedTip--;
            OnSwitchedTip();
        }
        public void Next() 
	    {
	        SelectedTip++;
            OnSwitchedTip();
        }
        void OnSwitchedTip()
        {
            if (MyTutorialText)
            {
                if (MyTutorialTips.Count == 0)
                {
                    GenerateTutorialTips();
                }
                if (SelectedTip >= MyTutorialTips.Count)
                    SelectedTip = 0;
                if (SelectedTip < 0)
                    SelectedTip = MyTutorialTips.Count-1;
                //SelectedTip = Mathf.Clamp(SelectedTip, 0, MyTutorialTips.Count - 1);
                MyTutorialText.text = MyTutorialTips[SelectedTip];
            }
        }

	    public void SaveTutorialTips() 
	    {

	    }

	    public void LoadTutorialTips() 
	    {

	    }
    }
}
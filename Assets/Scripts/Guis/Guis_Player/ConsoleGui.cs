using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Items;
using Zeltex.Util;
using Zeltex.Guis.Maker;
using Zeltex;

namespace Zeltex.Guis.Players
{
    // Example Commands
    // /give [charactername] [itemname] [itemquantity]
    // /opengui [guiname] (textureeditor, mapeditor, blockmeta, etc)
    /// <summary>
    /// Used for in game commands.
    /// </summary>
    public class ConsoleGui : MonoBehaviour
    {
        #region Variables
        public InputField MyInput;
        public Text PreviousLinesText;
        public int MaxLines = 150;
        public int Height = 10;
        [Header("References")]
        public CharacterManager MyCharacterManager;
        public ItemManager MyItemManager;
        public FPSDebugger MyFPSDebugger;
        public MapMaker MyMapMaker;
        public RectTransform MyContent;
        public ZelGui MyTexturePainter;
        public ZelGui MyVoxelPainter;
        public ZelGui MyPolygonPainter;
        public ZelGui MySkeletonPainter;
        static ConsoleGui ThisConsole;
        List<string> MyLines = new List<string>();
        #endregion

        void Start()
        {
            //Debug.LogError("Creating Console Gui");
            ThisConsole = this;
            MyFPSDebugger = FPSDebugger.Get();
        }
        public static ConsoleGui Get()
        {
            return ThisConsole;
        }

        public void OnInput()
        {
            string NewLine = MyInput.text;
            if (NewLine.Length == 0)
                return;
            if (NewLine[NewLine.Length - 1] == '\n')
            {
                NewLine = NewLine.Remove(NewLine.Length - 1); // remove enter symbol
                AddLine(NewLine);
            }
        }
        void NewCommand(string MyCommand)
        {
            // example /give player Texture_0
            string[] CommandParts = MyCommand.Split(' ');
            if (CommandParts.Length == 1)
            {
                if (CommandParts[0] == "/help")
                {
                    AddLine("Commands:");
                    AddLine("   /give player [itemname] ~[itemquantity]");
                    AddLine("   /[enable/disable] [fps, fog, noise]");  //, MapDebug
                    AddLine("   /[list] [items, stats, quests, models, voxels, skeletons]");
                    AddLine("   /[toggle] [texture, voxel, polygon, skeleton]");
                    AddLine("   /[toggle] [texturepainter, voxelpainter, polygonpainter, skeletonpainter]");
                    return;
                }
            }
            if (CommandParts.Length == 3 || CommandParts.Length == 4)
            {
                if (CommandParts[0] == "/give")
                {
                    if (CommandParts[1] == "player")
                    {
                        GameObject MyPlayer = Camera.main.gameObject;
                        if (MyPlayer != null)
                        {
                            //MyItemManager.GatherItemsFromScene();
                            string ItemName = CommandParts[2];
                            Item MyItem = Zeltex.DataManager.Get().GetElement("ItemMeta", ItemName) as Item;
                            if (MyItem != null)
                            {
                                int Quantity = 1;
                                if (CommandParts.Length == 4)
                                {
                                    Quantity = int.Parse(CommandParts[3]);
                                }
                                MyPlayer.GetComponent<Zeltex.Player>().GetCharacter().GetBackpackItems().Add(MyItem, Quantity);
                                AddLine("Command executed successfully.");
                                return;
                            }
                            else
                            {
                                AddLine("Item [" + CommandParts[2] + "] not found.");
                                return;
                            }
                        }
                        else
                        {
                            AddLine("No Player loaded.");
                            return;
                        }
                    }
                }
            }
            else if (CommandParts.Length == 2)
            {
                if (CommandParts[0] == "/enable" || CommandParts[0] == "/disable" || CommandParts[0] == "/toggle")
                {
                    if (CommandParts[1] == "fps")
                    {
                        MyFPSDebugger.enabled = !MyFPSDebugger.enabled;
                        AddLine("Toggling FPS [" + MyFPSDebugger.enabled + "]");
                        return;
                    }
                    if (CommandParts[1] == "fog")
                    {
                        RenderSettings.fog = !RenderSettings.fog;
                        AddLine("Toggling Fog [" + RenderSettings.fog + "]");
                        return;
                    }
                    if (CommandParts[1] == "noise")
                    {
                        /*Camera.main.GetComponent<UnityStandardAssets.ImageEffects.NoiseAndGrain>().enabled = 
                            !Camera.main.GetComponent<UnityStandardAssets.ImageEffects.NoiseAndGrain>().enabled;
                        AddLine("Toggling Noise [" + Camera.main.GetComponent<UnityStandardAssets.ImageEffects.NoiseAndGrain>().enabled + "]");*/
                        return;
                    }
                    if (CommandParts[1] == "spawner")
                    {
                        bool NewState = ZoneManager.Get().ToggleSpawnersEnabled();
                        AddLine("Toggling Spawner [" + NewState + "]");
                        return;
                    }
                    /*else if (CommandParts[1] == "MapDebug")
                    {
                        MyMapMaker.DebugGui = !MyMapMaker.DebugGui;
                        AddLine("Toggling MapDebug [" + MyMapMaker.DebugGui + "]");
                        return;
                    }*/
                }
                else if (CommandParts[0] == "/list")
                {
                    if (CommandParts[1] == "items")
                    {
                        /*ItemMaker MyItemMaker = ItemMaker.Get();
                        for (int i = 0; i < MyItemMaker.MyInventory.MyItems.Count; i++)
                        {
                            AddLine((i + 1) + "\tItem [" + MyItemMaker.MyInventory.MyItems[i].Name + "]");
                        }*/
                        return;
                    }
                }
                else if (CommandParts[0] == "/toggle")
                {
                    if (CommandParts[1] == "texture")
                    {
                        MyMapMaker.MyTextureManager.GetComponent<ZelGui>().Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "skeleton")
                    {
                        MyMapMaker.MySkeletonManager.GetComponent<ZelGui>().Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "voxel")
                    {
                        MyMapMaker.MyModelMaker.GetComponent<ZelGui>().Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "polygon")
                    {
                        MyMapMaker.MyPolygonMaker.GetComponent<ZelGui>().Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "texturepainter")
                    {
                        MyTexturePainter.Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "voxelpainter")
                    {
                        MyVoxelPainter.Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "polygonpainter")
                    {
                        MyPolygonPainter.Toggle();
                        return;
                    }
                    else if (CommandParts[1] == "skeletonpainter")
                    {
                        MySkeletonPainter.Toggle();
                        return;
                    }
                }
            }
            AddLine("Command Unrecognized.");
        }

        public void Clear()
        {
            PreviousLinesText.text = "\n\n\n\n\n\n\n\n";
            MyContent.offsetMax = new Vector2(MyContent.offsetMax.x, 0);
        }

        public void AddLine(string NewLine)
        {
            if (NewLine.Length != 0)
            {
                //Debug.LogError("Adding new line: " + NewLine);
                MyLines.Add(NewLine);
                if (MyLines.Count >= MaxLines)
                {
                    MyLines.RemoveAt(0);
                }
                PreviousLinesText.text = FileUtil.ConvertToSingle(MyLines);
                MyInput.text = "";
                if (NewLine[0] == '/')
                {
                    NewCommand(NewLine);
                }
            }
            else
            {
                //Debug.LogError("Could not add line: " + NewLine);
            }
            //if (PreviousLinesText.text != "")
            //    PreviousLinesText.text += "\n";
            //MyContent.
            /*string[] MyLinesArray = PreviousLinesText.text.Split('\n');
            if (MyLinesArray.Length <= 8)
            {
                MyContent.offsetMax = new Vector2(MyContent.offsetMax.x, 0);
            }
            else
            {
                MyContent.offsetMax = new Vector2(MyContent.offsetMax.x, (MyLinesArray.Length - 7) * Height);
            }
            for (int i = MyLinesArray.Length - 1; i >= 0; i--)
            {
                if (MyLinesArray[i].Length != 0)
                {
                    MyLines.Add(MyLinesArray[i]);
                    if (MyLines.Count == MaxLines)
                    {
                        break;  // end adding lines
                    }
                }
            }
            if (MyLines.Count >= MaxLines)
            {
                string NewText = "";
                for (int i = MyLines.Count - 1; i >= 0; i--)
                {
                    string MyLine = MyLines[i];
                    // remake these lines into text
                    if (MyLine.Length != 0)
                    {
                        NewText += MyLine;
                        NewText += '\n';
                    }
                    else
                    {
                        Debug.LogError("Dayyem");
                    }
                }
                PreviousLinesText.text = NewText;
            }
            MyInput.text = "";
            if (NewLine[0] == '/')
            {
                NewCommand(NewLine);
            }*/
        }
    }
}
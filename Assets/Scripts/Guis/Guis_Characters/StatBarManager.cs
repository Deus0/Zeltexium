using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Util;
using Zeltex.Combat;
using Zeltex.Characters;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Shows some gui bars for the character stats.
    /// To Do: Add effects gui at bottom of it
    /// todo:
    ///     Add class above Malzar[Mage]
    /// </summary>
	public class StatBarManager : MonoBehaviour 
	{
        #region Variables
        [Header("Actions")]
        public EditorAction ActionRefreshStats;

        [Header("Data")]
        [Tooltip("A prefab that will be used to spawn stat bars")]
		public GameObject StatBarPrefab;
        [SerializeField]
        private List<string> MyStats = new List<string> ();
        [Tooltip("The Margin Between bars")]
        public float MarginY = 5f;
		// spawned bars
		private List<GameObject> SpawnedBars = new List<GameObject>();
        private List<Color32> MyColours = new List<Color32>();
        // gets linked to tooltips
        private GameObject MyTooltipGui;
        public GameObject NameBar;
        //public GameObject NameBarPrefab;

        private static string BarAddOnText = " Bar";
        #endregion

        private void Start()
        {
            CharacterGuiHandle MyCharacterGuiHandle = GetComponent<CharacterGuiHandle>();
            if (MyCharacterGuiHandle && MyCharacterGuiHandle.GetCharacter())
            {
                GetComponent<CharacterGuiHandle>().GetCharacter().GetGuis().UpdateLabel(gameObject);
            }
            RefreshStats();
        }

        private void Update()
        {
            if (ActionRefreshStats.IsTriggered())
            {
                //OnUpdateStats();
                RefreshStats();
            }
        }

        #region GettersAndSetters
        public void SetTooltip(GameObject MyTooltip_)
        {
            MyTooltipGui = MyTooltip_;
        }
        public void SetTarget(GameObject MyTarget_)
        {
            //TargetCharacter = MyTarget_;
            OnNewStats();
        }

		private bool IsInStats(string StatName) 
		{
			for (int i = 0; i < MyStats.Count; i++)
			{
				if (StatName == MyStats[i]) 
				{
					return true;
				}
			}
			return false;
        }
        private float GetRectWidth()
        {
            return StatBarPrefab.GetComponent<RectTransform>().GetSize().x;
        }
        private float GetRectHeight()
        {
            return (StatBarPrefab.GetComponent<RectTransform>().GetSize().y + MarginY);
        }
        private float GetTotalRectSize()
        {
            return (transform.childCount - 1) * GetRectHeight();
        }
        #endregion

        #region Events

        /// <summary>
        /// Refresh the stats gui
        /// </summary>
        public void RefreshStats()
        {
            OnNewStats();
        }

        /// <summary>
        /// on update stats - called on things like regen etc
        /// </summary>
        public void OnUpdateStats()
        {
            Character MyCharacter = gameObject.GetComponent<CharacterGuiHandle>().GetCharacter();
            if (MyCharacter != null && MyCharacter.GetStats() != null)
			{
                //Debug.LogError("Updating stats: " + MyStats.Count);
				for (int i = 0; i < MyStats.Count; i++) 
				{
					Stat MyStat = MyCharacter.GetStats().GetStat(MyStats[i]);
                    if (MyStat != null)
                    {
                        UpdateStatBar(MyStat);
                    }
                    else
                    {
                        //Debug.LogError("Could not find stat " + MyStats[i] + " inside OnUpdateStats");
                    }
				}
			}
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateStatBar(Stat MyStat)
        {
            for (int i = 0; i < SpawnedBars.Count; i++)
            {
                string MyName = MyStat.Name + BarAddOnText;
                if (MyName == SpawnedBars[i].name)
                {
                    UpdateStatBar(SpawnedBars[i], MyStat);
                    break;
                }
            }
        }

        /// <summary>
        /// Update a stat bar
        /// </summary>
        private void UpdateStatBar(GameObject MyStatBar, Stat MyStat)
        {
            if (MyStat != null && MyStatBar != null)
            {
                Text StatBar = MyStatBar.transform.Find("StatText").GetComponent<Text>();
                RectTransform BarState = MyStatBar.transform.Find("BarState").GetComponent<RectTransform>();
                // make the state bar, the normal bar*percentage of stat
                if (MyStat.GetStatType() == StatType.State)
                {
                    StatBar.text = MyStat.Name + " [" +
                        Mathf.RoundToInt(MyStat.GetState()) + "/" + Mathf.RoundToInt(MyStat.GetMaxState()) + "]";
                }
                else if (MyStat.GetStatType() == StatType.Base)
                {
                    StatBar.text = MyStat.Name + " [" + Mathf.RoundToInt(MyStat.GetValue()) + "]";
                }
                float NewWidth = 0;
                if (MyStat.GetStatType() == StatType.State)
                {
                    NewWidth = GetRectWidth() * MyStat.GetPercentage();
                }
                else if (MyStat.GetStatType() == StatType.Base)
                {
                    NewWidth = GetRectWidth();
                }
                BarState.SetWidth(NewWidth);
                BarState.anchoredPosition = new Vector2(NewWidth / 2f, BarState.anchoredPosition.y);
            }
        }

        /// <summary>
        /// Clear the stats
        /// </summary>
        private void Clear()
        {
            //Destroy(NameBar);
            for (int i = 0; i < SpawnedBars.Count; i++)
            {
                if (SpawnedBars[i])
                {
                    Destroy(SpawnedBars[i]);   // otherwise they are still in the transform.childCount thingo!
                }
            }
            SpawnedBars.Clear();
            //NameBar = null;
        }
        #endregion

        #region Data

        private void DefaultStats()
        {
            if (MyStats.Count == 0)
            {
                Debug.Log("DefaultStats " + MyStats.Count);
                MyColours.Add(Color.gray);  // gray for level
                MyStats.Add("Level");
                MyColours.Add(new Color32(152, 30, 30, 255));   // red for health
                MyStats.Add("Health");
                MyColours.Add(new Color32(23, 221, 197, 255));  // blue for mana
                MyStats.Add("Mana");
                MyColours.Add(new Color32(217, 236, 33, 255));  // yellow for energy
                MyStats.Add("Energy");
                //MyColours.Add(new Color32(65, 8, 77, 255));     // purple for combo
            }
            if (MyStats.Count != MyColours.Count)
            {
                MyColours.Add(Color.gray);  // gray for level
            }
            if (MyStats.Count != MyColours.Count)
            {
                MyColours.Add(new Color32(152, 30, 30, 255));   // red for health
            }
            if (MyStats.Count != MyColours.Count)
            {
                MyColours.Add(new Color32(23, 221, 197, 255));  // blue for mana
            }
            if (MyStats.Count != MyColours.Count)
            {
                MyColours.Add(new Color32(217, 236, 33, 255));  // yellow for energy
            }
        }

        /// <summary>
        /// generate the star bars here, mainly when a new state stat is added
        /// called if the stats change - later on it should take in what state is changed / added removed, and only update depending on that
        /// </summary>
        public void OnNewStats()
        {
            DefaultStats();
            Clear();
            CharacterGuiHandle MyCharacterGuiHandle = gameObject.GetComponent<CharacterGuiHandle>();
            if (MyCharacterGuiHandle == null)
            {
                Debug.LogError("MyCharacterGuiHandle is null inside: " + name);
                return;
            }
            Character MyCharacter = MyCharacterGuiHandle.GetCharacter();
            if (MyCharacter != null)
            {
                //Debug.LogError("Created bar for " + TargetCharacter.name);
                AlterNameBar();
                if (NameBar)
                {
                    NameBar.transform.GetChild(0).gameObject.GetComponent<Text>().text = MyCharacter.name;
                    for (int i = 0; i < MyStats.Count; i++)
                    {
                        Stat MyStat = MyCharacter.GetStats().GetStat(MyStats[i]);
                        if (MyStat != null)
                        {
                            GameObject MyBar = CreateStatBar(MyStat);
                            if (MyBar)
                            {
                                SpawnedBars.Add(MyBar);
                            }
                        }
                        else
                        {
                            //Debug.LogError("Inside: " + name + " - Problem in Stat Bar Manager. [" + MyStats[i] + "] was not found in stats of character: " + MyCharacterStats.name);
                        }
                    }
                    OnUpdateStats();
                }
            }
        }

        public void AlterNameBar()
        {
            Character MyCharacter = gameObject.GetComponent<CharacterGuiHandle>().GetCharacter();
            if (MyCharacter)    //NameBarPrefab && 
            {
                GameObject NewBar = NameBar.gameObject;// (GameObject)Instantiate(NameBarPrefab, transform.position, Quaternion.identity);
                NewBar.name = MyCharacter.name + BarAddOnText;
                //NewBar.transform.SetParent(transform, false);
                NewBar.transform.localPosition = GetBarPosition(0);
                Color32 MyColor = Color.gray;
                //MyColor = Generators.TextureGenerator.DarkenColor(MyColor);
                NewBar.GetComponent<RawImage>().color = Generators.TextureGenerator.DarkenColor(MyColor);
                NewBar.GetComponent<RawImage>().color = MyColor;
                // Invert Colour for stat bar text
                NewBar.transform.GetChild(0).GetComponent<Text>().color =
                    new Color32(
                        (byte)(ColorCorrection(MyColor.r)),
                        (byte)(ColorCorrection(MyColor.g)),
                        (byte)(ColorCorrection(MyColor.b)),
                        255);
                //return NewBar;
            }
           // else
            //{
            //    return null;
            //}
        }
        /// <summary>
        /// Creates a new stat bar
        /// </summary>
        private GameObject CreateStatBar(Stat MyStat)
        {
            if (StatBarPrefab != null)
            {
                //  create new bar object
                GameObject NewBar = (GameObject)Instantiate(StatBarPrefab, transform.position, Quaternion.identity);
                NewBar.SetActive(true);
                NewBar.name = MyStat.Name + " Bar";
                NewBar.transform.SetParent(transform, false);
                NewBar.transform.localPosition = GetBarPosition(SpawnedBars.Count + 1);
                // set tooltips
                GuiListElementData MyGuiListElementData = new GuiListElementData();
                MyGuiListElementData.LabelText = MyStat.GetToolTipName();
                MyGuiListElementData.DescriptionText = MyStat.GetToolTipText();
                NewBar.GetComponent<GuiListElement>().MyGuiListElementData = MyGuiListElementData;
                NewBar.GetComponent<GuiListElement>().SetTooltip(MyTooltipGui);

                int ColorIndex = SpawnedBars.Count;
                if (ColorIndex < MyColours.Count)
                {
                    Color32 MyColor = MyColours[ColorIndex];
                    NewBar.GetComponent<RawImage>().color = Generators.TextureGenerator.DarkenColor(MyColor);
                    NewBar.transform.Find("BarState").GetComponent<RawImage>().color = MyColor;
                    // Invert Colour for stat bar text
                    NewBar.transform.Find("StatText").GetComponent<Text>().color = 
                        new Color32(
                            (byte)(ColorCorrection(MyColor.r)),
                            (byte)(ColorCorrection(MyColor.g)),
                            (byte)(ColorCorrection(MyColor.b)),
                            255);
                }
                return NewBar;
            }
            else
            {
                Debug.LogError("Inside " + name + ", StatBarPrefab is null.");
                return null;
            }
        }

        int ColorCorrection(int OriginalColor)
        {
            int  MyColor = 255 - OriginalColor;
            // if new colour is too close to original (mainly for numbers in the middle)
            if (MyColor >= OriginalColor - 10 && MyColor <= OriginalColor + 10)
            {
                MyColor += 128;
                if (MyColor >= 255)
                {
                    MyColor -= 255;
                }
            }
            return MyColor;
        }
        #endregion
        
        #region Positioning

        /// <summary>
        /// Get the position of a bar depending on its index in the list
        /// </summary>
        private Vector3 GetBarPosition(int BarsCount)
        {
            //BarsCount--;    // if count 1, position index is 0
            float TotalHeight = GetComponent<RectTransform>().GetHeight();
            Vector3 NewPosition = new Vector3();
            float BarHeight = StatBarPrefab.GetComponent<RectTransform>().GetHeight();

            Transform Header = transform.Find("Header");
            if (Header != null)
            {
                NewPosition = Header.GetComponent<RectTransform>().anchoredPosition
                                        - (new Vector2(0, Header.GetComponent<RectTransform>().GetHeight() / 2f));
                NewPosition.y -= BarsCount * BarHeight;   // Modify children count for original stuff
            }
            NewPosition.y = TotalHeight / 2f + (0.5f - BarsCount) * BarHeight;
            //Debug.LogError("For bar " + BarsCount + " got position: " + NewPosition.y + " with height: " + BarHeight);
            return NewPosition;
        }
        #endregion
    }
}

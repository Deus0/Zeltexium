using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Util;

namespace Zeltex.Guis
{
    /// <summary>
    /// The parent class of all gui lists.
    /// </summary>
	public class GuiList : MonoBehaviour
    {
        #region Variables
		[SerializeField]
        protected List<RectTransform> MyGuis = new List<RectTransform>();	// spawned guis
		[Header("References")]
		[Tooltip("Prefab of the gui Cell")]
		public GameObject GuiCellPrefab;
        [Tooltip("Parent of the spawned elements.")]
        public GameObject MyList;
       // [Tooltip("Tool tip gui, used to show descriptions of highlighted elements.")]
        //public GameObject MyTooltipGui;
        public Scrollbar MyScrollbar;

        [Header("Options")]
        [Tooltip("Updates List at beginning of runtime")]
		public bool IsUpdateOnStart = false;
		[Tooltip("Initial margin of the cell positions")]
		public Vector2 GridMargin = new Vector2(25,25);
		[Tooltip("Distance between each cell")]
		public Vector2 CellMargin = new Vector2(25,25);
		[Tooltip("Keep at 0 if you want no limits.")]
		public Vector2 LimitGrid = new Vector2(0,0);
        [Tooltip("Can cells be selected")]
		public bool IsSelectable = false;
        [SerializeField]
        private int MaxCells = 0;
        [Tooltip("stretches the cell to the horizontal of the rect area")]
        [SerializeField]
        private bool IsStretchHorizontal;
        private float ScrollBeginTime;
        private float ScrollBeginPositionY;
        private float LastScrollPositionY;
        //[Tooltip("Time it takes to scroll from one position to the next")]
        private float ScrollAnimationTime = 0.5f;
        private Vector2 ScrollPosition = new Vector2(0, 0);
        [SerializeField, HideInInspector]
        private Vector2 MaxGrid;    // visible amount of cells showing
        [SerializeField, HideInInspector]
        private Vector2 CellSize;	// this depends on prefab
        [SerializeField, HideInInspector]
        private List<Vector2> ScrollBeginPositions = new List<Vector2>();

        protected int SelectedIndex = -1;
		protected string SelectedName = "";
        private GuiListElement SelectedCell;

        [Header("Events")]
        public MyEventInt OnActivateEvent = new MyEventInt(); // when the confirm button is clicked, and an item is selected
        [SerializeField]
        private bool IsActiveOnClick = true;
        [SerializeField]
        private bool isDeselectOnActivate = false;
        #endregion

        #region Mono
        void Start()
        {
            if (MyList == null)
            {
                MyList = gameObject;
            }
            if (IsUpdateOnStart)
            {
                RefreshList();
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            AnimateCells();
        }
        #endregion

        #region Setters

        /// <summary>
        /// Sets the tooltip for the entire list
        /// </summary>
        /*public void SetTooltip(GameObject NewTooltipGui)
        {
            MyTooltipGui = NewTooltipGui;
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i].GetComponent<GuiListElement>())
                {
                    MyGuis[i].GetComponent<GuiListElement>().SetTooltip(NewTooltipGui);
                }
            }
        }*/
        #endregion

        #region Events

        /// <summary>
        /// Invoked when an element is activated
        /// </summary>
		public void OnActivate()
		{
			//Debug.Log ("Activating: " + SelectedName);
			if (SelectedName != "")
            {
                OnActivateEvent.Invoke(SelectedIndex);
            }
		}
        #endregion

        #region List

        /// <summary>
        /// Destroys all the guis
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    MyGuis[i].gameObject.Die();
                }
            }
            MyGuis.Clear();
            if (MyScrollbar)
            {
                MyScrollbar.value = 0;
                MyScrollbar.size = 1f;
            }
            DeSelect();
        }

        /// <summary>
        /// Does guis contain one by this name
        /// </summary>
		public bool Contains(string name)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i].name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Destroys just one gui
        /// </summary>
		public void RemoveAt(int Index)
        {
            if (Index >= 0 && Index < MyGuis.Count)
            {
				if (SelectedIndex == Index)
				{ 
					// deselect
					SelectedIndex = -1;
					SelectedName = "";
				}
                Destroy(MyGuis[Index].gameObject);
                MyGuis.RemoveAt(Index);
				// reset tooltip indexes
				for (int i = Index; i < MyGuis.Count; i++)
				{
					MyGuis[i].GetComponent<GuiListElement>().MyGuiListElementData.Index = i;
				}
                RefreshPositions();
                RefreshScrollbar();
            }
        }

        /// <summary>
        /// Rename an element in the list
        /// </summary>
        public void Rename(string MyGuiName, string NewGuiName)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i].name == MyGuiName)
                {
                    NameGui(MyGuis[i].gameObject, NewGuiName);
                    return;
                }
            }
        }

        /// <summary>
        /// Rename
        /// </summary>
        public void Remove(string MyGuiName)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i].name == MyGuiName)
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes the selected element
        /// </summary>
        public void RemoveSelected()
        {
            RemoveAt(SelectedIndex);
        }

        /// <summary>
        /// Gets max grid
        /// </summary>
		float GetMaxGrid(float GridSize, float GridMargin, float CellSize, float CellMargin)
        {
            float MaxGridSize = (GridSize - GridMargin * 2f);    // the space in between the margins - margins on both sides
            float TempGridX = MaxGridSize;
            MaxGridSize = 0;
            bool IsFinished = false;
            while (!IsFinished)
            {
                if (MaxGridSize == 0)
                {
                    TempGridX -= CellSize;
                }
                else
                {
                    TempGridX -= CellSize + CellMargin;
                }
                if (TempGridX < 0)
                {
                    IsFinished = true;
                }
                else
                {
                    MaxGridSize++;
                }
            }
            return MaxGridSize;
        }

        /// <summary>
        /// Updates the overall grid size, depending on the cell sizes.
        /// </summary>
        private void UpdateSize()
        {
            if (CellSize.x == 0 || CellSize.y == 0)
            {
                if (MyList == null)
                {
                    MyList = gameObject;
                }
                if (GuiCellPrefab != null && MyList)
                {
                    RectTransform MyGuiPrefabRect = GuiCellPrefab.GetComponent<RectTransform>(); //MyGuis[0].GetComponent<RectTransform>();// 
                    if (MyGuiPrefabRect)
                    {
                        CellSize = MyGuiPrefabRect.GetSize();
                    }
                    if (IsStretchHorizontal)
                    {
                        CellSize.x = MyList.GetComponent<RectTransform>().GetWidth();
                    }
                    // calculate the size of the grid - MaxGrid
                    Vector2 GridSize = MyList.GetComponent<RectTransform>().GetSize();    // raw rect size
                    MaxGrid.x = GetMaxGrid(GridSize.x, GridMargin.x, CellSize.x, CellMargin.x);
                    MaxGrid.y = GetMaxGrid(GridSize.y, GridMargin.y, CellSize.y, CellMargin.y);
                    if (MaxGrid.x < 1)
                    {
                        MaxGrid.x = 1;
                    }
                    if (MaxGrid.y < 1)
                    {
                        MaxGrid.y = 1;
                    }
                    if (LimitGrid.x != 0 && MaxGrid.x > LimitGrid.x)
                    {
                        MaxGrid.x = LimitGrid.x;
                    }
                    if (LimitGrid.y != 0 && MaxGrid.y > LimitGrid.y)
                    {
                        MaxGrid.y = LimitGrid.y;
                    }
                }
            }
        }

        /// <summary>
        /// Renames a gui
        /// </summary>
        private void NameGui(GameObject MyCell, string MyName)
        {
            GuiListElement MyListElement = MyCell.GetComponent<GuiListElement>();
            if (MyListElement)
            {
                MyListElement.Rename(MyName);
            }
        }

        /// <summary>
        /// Creates a guilist out of a list of strings
        /// </summary>
        public void AddRange(List<string> Data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                Add(Data[i]);
            }
        }

        /// <summary>
        /// Adds a gui element with a string label, and no tooltip
        /// </summary>
        public GameObject Add(string GuiLabel)
        {
            GuiListElementData NewTooltip = new GuiListElementData();
            NewTooltip.LabelText = GuiLabel;
            return Add(GuiLabel, NewTooltip);
        }

        /// <summary>
        /// Adds a gui at the last index
        /// </summary>
		public GameObject Add(string GuiLabel, GuiListElementData MyGuiListElementData)
        {
            return Add(GuiLabel, MyGuiListElementData, -1);
        }

        /// <summary>
        /// Returns a cell by its name, or null
        /// </summary>
        public GameObject GetCell(string CellName)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i] && MyGuis[i].name == CellName)
                {
                    return MyGuis[i].gameObject;
                }
            }
            return null;
        }

        public GameObject GetCell(int CellIndex)
        {
            if (CellIndex >= 0 && CellIndex < MyGuis.Count && MyGuis[CellIndex])
            {
                return MyGuis[CellIndex].gameObject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a newly created gui element.
        /// </summary>
        public GameObject Add(string GuiLabel, GuiListElementData MyGuiListElementData, int Index)
        {
            if (GuiCellPrefab && (MaxCells == 0 ||  MyGuis.Count < MaxCells))
            {
                // Spawn the gui cell
                GameObject NewGuiCell = (GameObject)Instantiate(GuiCellPrefab,
                                                              new Vector3(0, 0, 0),
                                                              Quaternion.identity);
                NameGui(NewGuiCell, GuiLabel);
                GuiListElement MyGuiListElement = NewGuiCell.GetComponent<GuiListElement>();
                if (MyGuiListElement == null)
                {
                    MyGuiListElement = NewGuiCell.AddComponent<GuiListElement>();
                }
                RectTransform ListRect = MyList.GetComponent<RectTransform>();
                RectTransform CellRect = NewGuiCell.GetComponent<RectTransform>();
                MyGuiListElement.MyGuiList = this;
                //MyGuiListElement.SetTooltip(MyTooltipGui);
                MyGuiListElementData.OnSelectEventInt.RemoveAllListeners();
                MyGuiListElementData.OnSelectEventInt.AddEvent(Select);
                if (Index == -1)
                {
                    MyGuiListElementData.Index = MyGuis.Count;
                }
                else
                {
                    MyGuiListElementData.Index = Index;
                }
                MyGuiListElement.MyGuiListElementData = MyGuiListElementData;
                if (IsStretchHorizontal)
                {
                    CellRect.SetWidth(ListRect.GetWidth());
                }
                UpdateSize();   // initiate size
                NewGuiCell.transform.SetParent(MyList.transform, false);
                //Debug.LogError ("Assigning new position: " + GetCellPosition (MyGuis.Count).ToString ());
                if (Index == -1)
                {
                    MyGuis.Add(CellRect);
                    CellRect.anchoredPosition = GetCellPosition(MyGuis.Count - 1);
                }
                else
                {
                    MyGuis.Insert(Index, CellRect);
                    CellRect.anchoredPosition = GetCellPosition(Index);
                    for (int i = Index + 1; i < MyGuis.Count; i++)
                    {
                        MyGuis[i].GetComponent<RectTransform>().anchoredPosition = GetCellPosition(i);
                    }
                }
                // modify scroll bar size
                RefreshScrollbar();
                NewGuiCell.SetActive(true);
                MyGuiListElement.Initialize();
                RefreshPositions();
                return NewGuiCell;
            }
            else
            {
                return null;
            }
        }
        private void RefreshScrollbar()
        {
            if (MyScrollbar != null)
            {
                if ((MyGuis.Count / MaxGrid.x) < MaxGrid.y)
                {
                    MyScrollbar.interactable = false;
                    MyScrollbar.size = 1;
                }
                else
                {
                    MyScrollbar.interactable = true;
                    float MaxScrollPositions = ((MyGuis.Count / MaxGrid.x) - MaxGrid.y);    // say this is 1
                    MyScrollbar.size = 1 / (1 + MaxScrollPositions);  // get the amount of possible vertical positions
                    MyScrollbar.size = MaxGrid.y / MyGuis.Count;
                    if (MyScrollbar.size > 1)
                    {
                        MyScrollbar.size = 1;
                    }
                    // say this has one more position, and its divided by 2, so 0.5, i would rather it be like 0.9
                }
            }
        }
        #endregion

        #region ListSelecetion

        /// <summary>
        /// gets the size of the list
        /// </summary>
        public int GetSize()
        {
            return MyGuis.Count;
        }
        /// <summary>
        /// Returns the selected index
        /// </summary>
        public int GetSelected()
        {
            return SelectedIndex;
        }
        /// <summary>
        /// Returns the selected gui string.
        /// </summary>
        public string GetSelectedName()
        {
            if (SelectedIndex >= 0 && SelectedIndex < MyGuis.Count)
            {
                return MyGuis[SelectedIndex].name;
            }
            else
            {
                Debug.LogError("No Name Selected and trying to get name.");
                return "";
            }
		}

		/// <summary>
		/// DeSelects anything selected
		/// </summary>
		public void DeSelect()
		{
			Select(-1);
        }

        /// <summary>
        /// Selects a gui with the name
        /// </summary>
        public void Select(string GuiName)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i].name == GuiName)
                {
                    Select(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Responsive to gui clicking, -1 is used for deselection
        /// </summary>
        public virtual void Select(int NewSelectedIndex)
        {
            if (NewSelectedIndex != SelectedIndex)
            {
                if (NewSelectedIndex >= -1 && NewSelectedIndex < MyGuis.Count)
                {
					//Debug.Log("Selected " + CellID + " at " + Time.time);
					SelectedIndex = NewSelectedIndex; // activate event!
                    if (IsSelectable && SelectedIndex != -1)
                    {
                        SelectedName = MyGuis[SelectedIndex].name;
                    }
                    else
                    {
                        SelectedName = "";
                    }

					if (IsSelectable)
					{
                        // deselect previous one
                        if (SelectedCell)
                        {
                            SelectedCell.OnDeSelected();
                        }
                        // Set new SelectedCell reference
                        if (SelectedIndex == -1)
                        {
                            SelectedCell = null;
                        }
                        else
                        {
                            SelectedCell = MyGuis[NewSelectedIndex].GetComponent<GuiListElement>();
                            SelectedCell.OnSelected();
                        }
					}
					if (SelectedIndex != -1 && IsActiveOnClick)
					{
						OnActivate();
                        if (isDeselectOnActivate)
                        {
                            DeSelect();
                        }
					}
				}
            }
		}

        /// <summary>
        /// Is any gui selected.
        /// </summary>
        protected bool IsGuiSelected()
        {
            return (SelectedIndex != -1);
        }
        #endregion

        #region ListPositioning
        /// <summary>
        /// Returns a position of a cell depending on an index
        /// </summary>
        public virtual Vector3 GetCellPosition(int Index)
        {
            Vector3 CellPosition = new Vector3();
            int FlooredMaxGridX = Mathf.FloorToInt(MaxGrid.x);
            if (FlooredMaxGridX == 0)
            {
                FlooredMaxGridX = 1;
            }
            int PositionX = Index % FlooredMaxGridX;
            int PositionY = Index / FlooredMaxGridX;
            float PositionY2 = PositionY - (ScrollPosition.y);    //Mathf.FloorToInt
            CellPosition = new Vector3(GridMargin.x + (CellSize.x / 2f) + (PositionX) * (CellSize.x + CellMargin.x),
                                        -(GridMargin.y + (CellSize.y / 2f) + (PositionY2) * (CellSize.y + CellMargin.y)),
                                        0);
            GameObject MyCell = GetCell(Index);
            if (MyCell)
            {
                RectTransform CellRect = MyCell.GetComponent<RectTransform>();
                CellPosition -= new Vector3(CellRect.GetWidth() * CellRect.anchorMin.x, 0, 0);  //CellRect.GetHeight() * CellRect.anchorMin.y
            }
            return CellPosition;
        }

        /// <summary>
        /// Main Animation part, for when i change scroll position
        /// </summary>
        private void AnimateCells()
        {
            if (ScrollBeginTime != -1)
            {	// finished animating
                float ScaledAnimationTime = ScrollAnimationTime * Mathf.Abs(ScrollBeginPositionY - LastScrollPositionY);   // distance
                if (Time.time - ScrollBeginTime < ScaledAnimationTime)
                {
                    float LerpTime = (Time.time - ScrollBeginTime) / ScaledAnimationTime;
                    for (int i = 0; i < MyGuis.Count; i++)
                    {
                        if (MyGuis[i] != null)
                        {
                            if (i < ScrollBeginPositions.Count)
                            {
                                MyGuis[i].anchoredPosition = Vector2.Lerp(ScrollBeginPositions[i], GetCellPosition(i), LerpTime);
                            }
                        }
                        else
                        {
                            Debug.LogError("Problem: Null Gui in list: " + name);
                        }
                    }
                }
                else
                {
                    // end animation
                    for (int i = 0; i < MyGuis.Count; i++)
                    {
                        MyGuis[i].anchoredPosition = GetCellPosition(i);
                    }
                    ScrollBeginTime = -1;
                    ScrollBeginPositionY = ScrollPosition.y;
                    LastScrollPositionY = ScrollPosition.y;

                }
            }
        }

        /// <summary>
        /// Scrolls in the bool direction
        /// </summary>
        public void Scroll(bool Direction)
        {
            if (Direction)
            {
                int Max = Mathf.FloorToInt(ScrollPosition.y * MaxGrid.x + MaxGrid.y * MaxGrid.x);
                //MyGuis.Count/ListSizeX-ListSizeY*ListSizeX
                if (Max < MyGuis.Count - 1) // 5 is the size that it can hold
                    ScrollPosition.y++;
            }
            else
            {
                if (ScrollPosition.y > 0)
                    ScrollPosition.y--;
            }
            RefreshPositions();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshPositions()
        {
            ScrollBeginTime = Time.time;
            ScrollBeginPositions.Clear();
            for (int i = 0; i < MyGuis.Count; i++)
            {
                ScrollBeginPositions.Add(MyGuis[i].GetComponent<RectTransform>().anchoredPosition);
            }
        }
        /// <summary>
        /// Repositions all the cells depending on the new scroll position.
        /// int NewScrollPosition = Mathf.CeilToInt(Percentage * MaxScrollPositionY);
        ///     example 5 x 1 + 2 * 1 is less then our max guis
        /// </summary>
        public void ScrollBar(float Percentage)
        {
            ScrollBeginTime = Time.time;
            ScrollBeginPositionY = ScrollPosition.y;
            ScrollBeginPositions.Clear();   // positions of all our guis
            for (int i = 0; i < MyGuis.Count; i++)
            {
                ScrollBeginPositions.Add(MyGuis[i].GetComponent<RectTransform>().anchoredPosition);
            }
            float CellsDisplayed = (MaxGrid.y) * (MaxGrid.x);
            float MaxScrollPositionY = (MyGuis.Count) - CellsDisplayed;
            if (MaxScrollPositionY > MyGuis.Count - 1)
                MaxScrollPositionY = MyGuis.Count - 1;
            if (MaxScrollPositionY < 0)
                MaxScrollPositionY = 0;
            //Debug.LogError("MaxScrollPosY = " + MaxScrollPositionY + " - And cells displayed: " + CellsDisplayed);
            ScrollPosition.y = Percentage * MaxScrollPositionY;
        }
        #endregion

        #region Virtuals
        /// <summary>
        /// Activated when game object is enabled.
        /// </summary>
		virtual protected void OnEnable()
        {

        }
        /// <summary>
        /// Stops and starts the refreshing.
        /// </summary>
        public void ForceRefresh()
        {
            StopRefresh();
            StartRefresh();
        }
        /// <summary>
        /// Begins the coroutine for list refreshing. 
        /// Sometimes the refreshing will be ongoing.
        /// </summary>
        virtual public void StartRefresh()
        {

        }
        /// <summary>
        /// Stops the coroutine for list refreshing
        /// </summary>
        virtual public void StopRefresh()
        {

        }
        /// <summary>
        /// Instant list refreshing
        /// </summary>
        virtual public void RefreshList()
        {

        }
        #endregion

        #region Utility
        /// <summary>
        /// Debugging for the positioning variables. Incase I want to add more features and accidently break it.
        /// </summary>
        public List<string> GetDebugList()
        {
            List<string> MyData = new List<string>();
            MyData.Add(name + " has " + MyGuis.Count + " Guis.");
            MyData.Add("MaxGrid: " + MaxGrid.ToString());
            MyData.Add("CellSize: " + CellSize.ToString());
            MyData.Add("GridMargin: " + GridMargin.ToString());
            MyData.Add("CellMargin: " + CellMargin.ToString());
            return MyData;
        }
        #endregion
    }
}
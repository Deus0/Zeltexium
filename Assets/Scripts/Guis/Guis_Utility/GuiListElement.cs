using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Zeltex.Util;

namespace Zeltex.Guis
{

    /// <summary>
    /// this class is added onto every gui cell object
    /// Displays tool tips when mouse is over
    /// ToDo:
    ///     - Lerp Colours depending on state - similar to the button class
    /// </summary>
    public class GuiListElement : 	MonoBehaviour, 
								IPointerEnterHandler, 
								IPointerExitHandler, 
								IPointerClickHandler
	{
        #region Variables
        // references
        [HideInInspector]
        public GuiList MyGuiList;                           // reference to the list it is in
        protected RawImage MyImage;
        protected Text HeaderText;
        private Button MyButton;
        // Data
        public GuiListElementData MyGuiListElementData;     // the meta data for the element
        // events
        [Header("Events")]
        public UnityEvent OnClick = new UnityEvent();
        public EventGuiListElement OnClickListElement = new EventGuiListElement();
        // States
        protected bool IsMouseFollow;
        // Tooltip
        protected GameObject MyTooltipGui;                     // reference to the tooltip it is linked to
        protected Text TooltipLabelText;
        protected Text TooltipDescriptionText;
        //protected RectTransform TooltipRect;
        [Header("Color")]
        public ColorBlock MyColorBlock;
        #endregion

        // THe update function is used for mouse following
        #region Mono
        private TooltipHandle MyHandle;
        private void Awake()
        {
            MyHandle = GetComponent<TooltipHandle>();
            if (MyHandle == null)
            {
                MyHandle = gameObject.AddComponent<TooltipHandle>();
            }
            MyHandle.enabled = false;
        }

        public void Initialize()
        {
            if (transform.childCount > 0)
            {
                HeaderText = transform.GetChild(0).gameObject.GetComponent<Text>();
                if (HeaderText)
                {
                    HeaderText.text = gameObject.name;
                }
            }
            if (IsColorBlank(MyColorBlock.normalColor))
            {
                MyColorBlock.normalColor = new Color(0.7f, 0.7f, 0.7f);
            }
            if (IsColorBlank(MyColorBlock.highlightedColor))
            {
                MyColorBlock.highlightedColor = new Color(1f, 1f, 1f);
            }
            if (IsColorBlank(MyColorBlock.pressedColor))
            {
                MyColorBlock.pressedColor = new Color(0.7f, 1f, 1f);
            }
            MyButton = GetComponent<Button>();
            MyImage = GetComponent<RawImage>();
            if (MyButton)
            {
                MyButton.onClick.AddEvent(OnButtonClick);
            }
            else
            {
                if (MyImage)
                {
                    MyImage.color = MyColorBlock.normalColor;
                }
            }
        }

        private bool IsColorBlank(Color MyColor)
        {
            return (MyColor.r == 0 && MyColor.g == 0 && MyColor.b == 0 && MyColor.a == 0);
        }

        /*void Update()
        {
            MouseFollow();
        }*/

        void OnDisable()
        {
            if (MyTooltipGui)
            {
                MyTooltipGui.GetComponent<ZelGui>().TurnOff();
            }
        }
        #endregion

        #region ZelGui
        // UUsed for menu gui
        private bool IsFlipped;
		private Color32 OffColor = new Color32(55, 125, 76, 255);
		private Color32 OnColor = new Color32(66, 175, 77, 255);

        public void SetColors(Color32 OffColor_, Color32 OnColor_)
        {
            OffColor = OffColor_;
            OnColor = OnColor_;
			SetToggleState(IsFlipped);
        }

        public void OnToggledOn()
        {
			SetToggleState(true);
        }
        public void OnToggledOff()
        {
			SetToggleState(false);
        }
        public void SetToggleState(bool NewState)
        {
            IsFlipped = NewState;
            if (NewState == false)
            {
                GetComponent<RawImage>().color = OffColor;
            }
            else
            {
                GetComponent<RawImage>().color = OnColor;
            }
        }
        #endregion

        #region Setters
        
        public void Rename(string NewName)
        {
            // name it using the tooltips label text
            name = NewName;
            // also update the label of the cell with its name - (child gameobject with text component)
            if (HeaderText)
            {
                HeaderText.text = NewName;
            }
        }

        /// <summary>
        /// Link up the tooltip to the gui list element
        /// </summary>
        /* public void SetTooltip(GameObject NewTooltipGui)
         {
             MyTooltipGui = NewTooltipGui;
             if (MyTooltipGui != null)
             {
                 TooltipLabelText = MyTooltipGui.transform.Find("LabelText").GetComponent<Text>();
                 TooltipDescriptionText = MyTooltipGui.transform.Find("DescriptionText").GetComponent<Text>();
                 TooltipRect = MyTooltipGui.GetComponent<RectTransform>();
             }
             else
             {
                 TooltipLabelText = null;
                 TooltipDescriptionText = null;
                 TooltipRect = null;
             }
         }*/

        public void SetData(GuiListElementData NewData)
        {
            MyGuiListElementData = NewData;
            OnUpdatedTooltip();
        }

        private void OnUpdatedTooltip()
        {
            MyHandle.TooltipNameLabel = MyGuiListElementData.LabelText;
            MyHandle.TooltipDescriptionLabel = MyGuiListElementData.DescriptionText;
            MyHandle.enabled = MyHandle.TooltipNameLabel != "";
        }
        /// <summary>
        /// Main function used to set tool tip texts.
        /// </summary>
        public void SetData(string LabelText, string DescriptionText)
        {
            MyGuiListElementData = new GuiListElementData();
            MyGuiListElementData.LabelText = LabelText;
            MyGuiListElementData.DescriptionText = DescriptionText;
            OnUpdatedTooltip();
        }
        #endregion

        #region Events
        /// <summary>
        /// Add an event to on click
        /// </summary>
        /*public void AddListener(UnityAction MyEvent)
        {
            OnClick.AddEvent(MyEvent);
        }*/
		
        /// <summary>
        /// When user clicks on the element
        /// </summary>
		public void OnPointerClick(PointerEventData eventData) 
		{
            if (MyButton == null)
            {
                OnButtonClick();
            }
        }
        void OnButtonClick()
        {
            if (MyGuiListElementData.OnSelectEventInt != null)
            {
                MyGuiListElementData.OnSelectEventInt.Invoke(MyGuiListElementData.Index);
            }
            if (MyGuiListElementData.OnSelectEventString != null)
            {
                MyGuiListElementData.OnSelectEventString.Invoke(MyGuiListElementData.LabelText);
            }
            /*if (IsFlipColor)
            {
                SetFlippedState(!IsFlipped);
            }*/
            OnClick.Invoke();
            OnClickListElement.Invoke(name, this);
        }

        /// <summary>
        /// When pointer enters the element
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (MyGuiListElementData.IsToolTip)
            {
                if (MyTooltipGui)
                {
                    IsMouseFollow = true;
                    //Debug.Log ("Pointering entering a gui element: " + name);
                    MyTooltipGui.GetComponent<ZelGui>().TurnOn();
                    RectTransform MyRect = MyTooltipGui.GetComponent<RectTransform>();
                    if (TooltipLabelText)
                    {
                        TooltipLabelText.text = MyGuiListElementData.LabelText;
                    }
                    if (TooltipDescriptionText)
                    {
                        TooltipDescriptionText.text = MyGuiListElementData.DescriptionText;
                    }
                }
            }
            OnHighlighted();
        }

        /// <summary>
        /// When pointer leaves the element
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (MyTooltipGui)
            {
                IsMouseFollow = false;
                MyTooltipGui.GetComponent<ZelGui>().TurnOff();
            }
            OnDeHighlighted();
        }
        #endregion

        #region Selection
        private bool IsSelected;
        private bool IsHighlighted;

        public void OnSelected()
        {
            if (!IsSelected)
            {
                IsSelected = true;
                if (MyImage)
                {
                    if (IsHighlighted == false)
                    {
                        MyImage.color = MyColorBlock.pressedColor;
                    }
                    else
                    {
                        MyImage.color = new Color(
                            (MyColorBlock.highlightedColor.r + MyColorBlock.pressedColor.r) / 2f,
                            (MyColorBlock.highlightedColor.g + MyColorBlock.pressedColor.g) / 2f,
                            (MyColorBlock.highlightedColor.b + MyColorBlock.pressedColor.b) / 2f);
                    }
                }
            }
        }

        public void OnDeSelected()
        {
            if (IsSelected)
            {
                IsSelected = false;
                if (MyImage)
                {
                    if (IsHighlighted)
                    {
                        MyImage.color = MyColorBlock.highlightedColor;
                    }
                    else
                    {
                        MyImage.color = MyColorBlock.normalColor;
                    }
                }
            }
        }

        public void OnHighlighted()
        {
            if (!IsHighlighted)
            {
                IsHighlighted = true;
                if (MyImage)
                {
                    if (!IsSelected)
                    {
                        MyImage.color = MyColorBlock.highlightedColor;
                    }
                    else
                    {
                        MyImage.color = new Color(
                            (MyColorBlock.highlightedColor.r + MyColorBlock.pressedColor.r) / 2f,
                            (MyColorBlock.highlightedColor.g + MyColorBlock.pressedColor.g) / 2f,
                            (MyColorBlock.highlightedColor.b + MyColorBlock.pressedColor.b) / 2f);
                    }
                }
            }
        }

        public void OnDeHighlighted()
        {
            if (IsHighlighted)
            {
                IsHighlighted = false;
                if (MyImage)
                {
                    if (!IsSelected)
                    {
                        MyImage.color = MyColorBlock.normalColor;
                    }
                    else
                    {
                        MyImage.color = MyColorBlock.pressedColor;
                    }
                }
            }
        }
        #endregion

        #region Utility

        /// <summary>
        /// Makes the tooltip follow the mouse
        /// Accounts for going out of the screen's size
        /// </summary>
        /*private void MouseFollow()
        {
            if (IsMouseFollow)
            {
                if (MyTooltipGui)
                {
                    Vector2 MySize = TooltipRect.GetSize();
                    Vector2 ScaleMousePosition = Input.mousePosition;
                    float Buffer = 32;// 64;
                    bool IsTop = (ScaleMousePosition.y < MySize.y - 2.5f * Buffer);
                    bool IsRight = (ScaleMousePosition.x > Screen.width - MySize.x + 2.5f * Buffer);
                    //Debug.LogError("IsTop: " + IsTop + ": " + ScaleMousePosition.y);
                    if (IsRight)
                    {
                        ScaleMousePosition.x -= MySize.x / 2f;
                        ScaleMousePosition.x += Buffer;
                    }
                    else
                    {
                        ScaleMousePosition.x += MySize.x / 2f;
                        ScaleMousePosition.x -= Buffer;
                    }
                    if (IsTop)
                    {
                        ScaleMousePosition.y += MySize.y / 2f;
                        ScaleMousePosition.y -= Buffer;
                    }
                    else
                    {
                        ScaleMousePosition.y -= MySize.y / 2f;
                        ScaleMousePosition.y += Buffer;
                    }
                    //rescale
                    ScaleMousePosition = new Vector2(1920f * (ScaleMousePosition.x / Screen.width), 1080f * (ScaleMousePosition.y / Screen.height));
                    ScaleMousePosition.x -= 1920f / 2f;
                    ScaleMousePosition.y -= 1080 / 2f;
                    MyTooltipGui.GetComponent<Orbitor>().SetScreenPosition(ScaleMousePosition);
                }
                else
                {
                    Debug.LogError(name + " is trying to follow but with no linked tooltip object.");
                }
            }
        }*/
        #endregion
    }
}

using UnityEngine;
using Zeltex.Util;
using Zeltex.Items;


namespace Zeltex.Guis
{
    /// <summary>
    /// Data used for tool tips
    /// </summary>
    [System.Serializable]
    public class GuiListElementData
    {
        [HideInInspector]
        public int Index;
        [HideInInspector]
        public string LabelText;
        [HideInInspector]
        public string DescriptionText;
        [HideInInspector]
        public bool IsToolTip = true;
        [HideInInspector]
        public EventString OnSelectEventString = new EventString();
        [HideInInspector]
        public MyEventInt OnSelectEventInt = new MyEventInt();
        [HideInInspector]
        public Item MyItem;
        //public int InventoryIndex = -1;

        public GuiListElementData()
        {
            LabelText = "-";
            DescriptionText = "";
        }
    }
}
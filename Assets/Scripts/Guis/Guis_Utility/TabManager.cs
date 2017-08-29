using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Zeltex.Guis
{
    /// <summary>
    /// a quick solution for tabs
    /// </summary>
    public class TabManager : MonoBehaviour
    {
        [Tooltip("If -1, it won't enable a tab on start")]
        public int TabStart = -1;
        public List<GameObject> MyTabs;
        public List<GameObject> MyTabButtons;
        public Color32 NormalColor = new Color32(152, 249, 157, 255);
        public Color32 MySelectedColor = new Color32(225, 156, 255, 255);

        void Start()
        {
            if (MyTabs.Count > TabStart && TabStart >= 0)
            {
                EnableTab(MyTabs[TabStart]);
            }
        }

        public void EnableTab(string TabName)
        {
            for (int i = 0; i < MyTabs.Count; i ++)
            {
                if (MyTabs[i].name == TabName)
                {
                    EnableTab(MyTabs[i]);
                    break;
                }
            }
        }

        public void EnableTab(GameObject MyTab)
        {
            DisableTabs();
            MyTab.SetActive(true);
            int Myindex = 0;
            for (int i = 0; i < MyTabs.Count; i++)
            {
                if (MyTabs[i] && MyTab == MyTabs[i])
                {
                    Myindex = i;
                }
            }
            if (Myindex < MyTabButtons.Count)
            {
                MyTabButtons[Myindex].GetComponent<RawImage>().color = MySelectedColor;
            }
        }

        private void DisableTabs()
        {
            for (int i = 0; i < MyTabs.Count; i++)
            {
                if (MyTabs[i])
                {
                    MyTabs[i].SetActive(false);
                    if (i < MyTabButtons.Count)
                    {
                        MyTabButtons[i].GetComponent<RawImage>().color = NormalColor;
                    }
                }
                /*
                ColorBlock MyColorBlock = MyTabButtons[i].GetComponent<Button>().GetComponent<Button>().colors;
                MyColorBlock.normalColor = Color.white;
                MyTabButtons[i].GetComponent<Button>().GetComponent<Button>().colors = MyColorBlock;*/
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Small extensions for elements instead of strings with datamanager
    /// </summary>
    public class ElementMakerGui : MakerGui
    {
        #region DataManager

        /// <summary>
        /// Save the folder!
        /// </summary>
        public override void SaveAll()
        {
            DataManager.Get().SaveElements(DataManagerFolder);
        }

        /// <summary>
        /// When an index is removed
        /// </summary>
        protected override void RemovedData(int Index)
        {
            DataManager.Get().RemoveElement(DataManagerFolder, Index);
        }

        /// <summary>
        /// Get Size
        /// </summary>
        public override int GetSize()
        {
            return DataManager.Get().GetSizeElements(DataManagerFolder);
        }

        public Element GetSelectedElement()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex());
        }

        public override string GetSelected()
        {
            if (GetSelectedElement() != null)
            {
                return GetSelectedElement().Name;
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region UI


        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            //base.UseInput(MyButton);
            if (MyButton.name == "SaveButton")
            {
                DataManager.Get().SaveElements(DataManagerFolder);
            }
        }
        #endregion

        #region SaveManagement

        /// <summary>
        /// 
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();
            for (int i = 0; i < DataManager.Get().GetSizeElements(DataManagerFolder); i++)
            {
                Element MyElement = DataManager.Get().GetElement(DataManagerFolder, i);
                if (MyElement != null)
                {
                    MyElement.ModifiedEvent.RemoveListener(OnModifiedElement);
                    MyElement.ModifiedEvent.AddEvent(OnModifiedElement);
                    MyElement.SavedEvent.RemoveListener(OnSavedElement);
                    MyElement.SavedEvent.AddEvent(OnSavedElement);
                }
                else
                {
                    Debug.LogWarning("Element " + i + " is null.");
                }
            }
        }

        /// <summary>
        /// When modified an element maker element
        /// </summary>
        public void OnModifiedElement(Element MyElement)
        {
            Debug.Log(Time.time + ": [" + MyElement.Name + "] has been modified.");
            int MyIndex = DataManager.Get().GetElementIndex(DataManagerFolder, MyElement);
            GuiList MyList = GetList("FilesList");
            if (MyList)
            {
                GameObject MyCell = GetList("FilesList").GetCell(MyIndex);
                Material MyMaterial = MyCell.GetComponent<RawImage>().material;
                Vector2 CellSize = MyCell.GetComponent<RectTransform>().GetSize();
                if (MyCell.transform.Find("ModifiedIcon") == null)
                {
                    Vector2 ModifiedIconSize = new Vector2(20, 20);
                    GameObject ModifiedIconObject = new GameObject();
                    ModifiedIconObject.name = "ModifiedIcon";
                    ModifiedIconObject.transform.SetParent(MyCell.transform);
                    ModifiedIconObject.transform.localPosition = Vector3.zero;
                    ModifiedIconObject.transform.localRotation = Quaternion.identity;
                    ModifiedIconObject.transform.localScale = new Vector3(1, 1, 1);
                    RectTransform MyRect = ModifiedIconObject.AddComponent<RectTransform>();
                    MyRect.anchoredPosition = new Vector2(CellSize.x / 2f - ModifiedIconSize.x, 0);
                    MyRect.SetSize(ModifiedIconSize);
                    RawImage MyImage = ModifiedIconObject.AddComponent<RawImage>();
                    MyImage.material = MyMaterial;
                    MyImage.color = new Color(1f, 0, 0, 0.85f);
                }
            }
            //MyCell.GetComponent<RawImage>().color = Color.red;
        }

        /// <summary>
        /// When saved an element maker element
        /// </summary>
        public void OnSavedElement(Element MyElement)
        {
            Debug.Log(Time.time + ": [" + MyElement.Name + "] has been saved.");
            int MyIndex = DataManager.Get().GetElementIndex(DataManagerFolder, MyElement);
            GuiList MyList = GetList("FilesList");
            if (MyList)
            {
                GameObject MyCell = GetList("FilesList").GetCell(MyIndex);
                //MyCell.GetComponent<RawImage>().color = Color.white;
                Transform ModifiedIconTransform = MyCell.transform.Find("ModifiedIcon");
                if (ModifiedIconTransform != null)
                {
                    ModifiedIconTransform.gameObject.Die();
                }
            }
            else
            {
                Debug.LogError("Files list doesn't exist in maker " + name);
            }
        }
        #endregion
    }
}
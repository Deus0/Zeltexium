using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis
{

    /// <summary>
    /// This is added to each guilist element that is linked to a file
    /// </summary>
    public class GuiListElementFile : GuiListElement
    {
        [Header("File")]
        [SerializeField]
        private GameObject NeedsToSaveIcon;
        private GameObject SpawnedNeedsToSaveIcon;
        [SerializeField]
        private Material IconMaterial;
        
        public void SetModified(bool NewState)
        {
            if (NewState)
            {
                OnModified(null);
            }
            else
            {
                OnSaved(null);
            }
        }

        /// <summary>
        /// On initial loading
        /// </summary>
        public void CheckElement(Element MyElement)
        {
            if (MyElement.CanSave())
            {
                OnModified(MyElement);
            }
            else
            {
                OnSaved(MyElement);
            }
        }

        public void OnModified(Element ModifiedElement)
        {
            if (ModifiedElement != null)
            {
                string DataFolder = ModifiedElement.GetFolder();
                if (DataFolder == "")
                {
                    Debug.LogError(ModifiedElement.Name + " Has not yet got it's element folder assigned.");
                }
            }
            //else
            {
                //int MyIndex = DataManager.Get().GetElementIndex(DataFolder, ModifiedElement);
                //GameObject MyCell = gameObject;// GetList("FilesList").GetCell(MyIndex);
                if (gameObject.transform.Find("ModifiedIcon") == null)
                {
                    SpawnModifiedIcon();
                }
            }
        }

        public void OnSaved(Element SavedElement)
        {
            if (SpawnedNeedsToSaveIcon != null)
            {
                SpawnedNeedsToSaveIcon.Die();
            }
        }

        private void SpawnModifiedIcon()
        {
            if (SpawnedNeedsToSaveIcon == null)
            {
                Vector2 CellSize = gameObject.GetComponent<RectTransform>().GetSize();
                Material MyMaterial;
                Vector2 ModifiedIconSize = new Vector2(20, 20);
                SpawnedNeedsToSaveIcon = new GameObject();
                SpawnedNeedsToSaveIcon.name = "ModifiedIcon";
                SpawnedNeedsToSaveIcon.transform.SetParent(transform);
                SpawnedNeedsToSaveIcon.transform.localPosition = Vector3.zero;
                SpawnedNeedsToSaveIcon.transform.localRotation = Quaternion.identity;
                SpawnedNeedsToSaveIcon.transform.localScale = new Vector3(1, 1, 1);
                RectTransform MyRect = SpawnedNeedsToSaveIcon.AddComponent<RectTransform>();
                MyRect.anchoredPosition = new Vector2(CellSize.x / 2f - ModifiedIconSize.x, 0);
                MyRect.SetSize(ModifiedIconSize);
                RawImage MyImage = SpawnedNeedsToSaveIcon.AddComponent<RawImage>();
                if (IconMaterial)
                {
                    MyMaterial = IconMaterial;
                }
                else
                {
                    MyMaterial = gameObject.GetComponent<RawImage>().material;
                }
                MyImage.material = MyMaterial;
                MyImage.color = new Color(1f, 0, 0, 0.85f);
            }
        }
	}
}

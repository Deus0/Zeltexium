using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zeltex.Util;

namespace Zeltex.Guis
{
    /// <summary>
    /// The main class to cycle through data files.
    /// </summary>
    public class IndexController : MonoBehaviour
    {
        [HideInInspector]
        public int MaxSelected;
        public int SelectedIndex = -1;
        private int OldSelectedIndex = -1;
        [HideInInspector]
        public MyEventInt OnIndexUpdated;
        [HideInInspector]
        public MyEventInt OnRemove;
        [HideInInspector]
        public MyEventInt OnAdd;
        [HideInInspector]
        public UnityEvent OnListEmpty;
        [Header("References")]
        public Text MyIndexLabel;
        public Button MyNextButton;
        public Button MyBackButton;
        public Button MyAddButton;
        public Button MyRemoveButton;

        public void OnBegin()
        {
            OnUpdatedIndex();
        }
        public void OnEnd()
        {
            SelectedIndex = 0;
            OldSelectedIndex = -1;
        }
        /*void OnGUI()
        {
            if (DebugGui)
            {
                GUILayout.Label("Selected: " + SelectedIndex + " out of " + (MaxSelected - 1));
                GUILayout.Label("OldSelectedIndex: " + OldSelectedIndex);
            }
        }*/
        public void Disable()
        {
            MyBackButton.interactable = false;
            MyNextButton.interactable = false;
            MyAddButton.interactable = false;
            MyRemoveButton.interactable = false;
            MyIndexLabel.text = "[=]";
        }
        public void Enable()
        {
            MyBackButton.interactable = (SelectedIndex != 0);
            MyNextButton.interactable = (SelectedIndex != (MaxSelected - 1));
            MyAddButton.interactable = true;
            MyRemoveButton.interactable = (MaxSelected >= 1);
            MyIndexLabel.text = (SelectedIndex + 1).ToString();
        }

        #region GettersAndSetters

        /// <summary>
        /// Set the maximum value that the index controller can go
        /// </summary>
        public void SetMaxSelected(int NewMax)
        {
            MaxSelected = NewMax;
            OnUpdatedMaxSelected();
        }

        public int GetSelectedIndex()
        {
            return SelectedIndex;
        }
        public int GetOldIndex()
        {
            return OldSelectedIndex;
        }
        /// <summary>
        /// Sets Old Index to -1
        /// </summary>
        public void RemovedOldIndex()
        {
            OldSelectedIndex = -1;
        }
        #endregion

        // UI Functions
        #region UI
        /// <summary>
        /// For when things like loading new!
        /// </summary>
        public void ForceSelect(int NewIndex)
        {
            SelectedIndex = NewIndex;
            OldSelectedIndex = SelectedIndex - 1;
            OnUpdatedIndex();
        }
        public void SelectIndex(int NewIndex)
        {
            SelectedIndex = NewIndex;
            OnUpdatedIndex();
        }
        public void Next()
        {
            SelectedIndex++;
            OnUpdatedIndex();
        }
        public void Back()
        {
            SelectedIndex--;
            OnUpdatedIndex();
        }
        /// <summary>
        /// When selected index changes
        /// </summary>
        private void OnUpdatedIndex()
        {
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, MaxSelected - 1);
            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
            }
            //Debug.Log("New: " + SelectedIndex + " and old: " + OldSelectedIndex + " -Max: " + MaxSelected);
            if (SelectedIndex != OldSelectedIndex)
            {
                if (MaxSelected != 0)
                {
                    MyIndexLabel.text = (SelectedIndex + 1).ToString();
                    MyBackButton.interactable = (SelectedIndex != 0);
                    MyNextButton.interactable = (SelectedIndex != (MaxSelected - 1));
                    OnIndexUpdated.Invoke(SelectedIndex);
                }
                OldSelectedIndex = SelectedIndex;
            }
        }
        /// <summary>
        /// When the item list changes size
        /// </summary>
        void OnUpdatedMaxSelected()
        {
            MyRemoveButton.interactable = (MaxSelected != 0);
            if (MaxSelected == 0)
            {
                //Debug.LogError("List is empty.");
                MyIndexLabel.text = "[=]";
                MyBackButton.interactable = false;
                MyNextButton.interactable = false;
                OnListEmpty.Invoke();
            }
            else
            {
                //Debug.LogError("List is non empty: " + MaxSelected);
                MyNextButton.interactable = (SelectedIndex != (MaxSelected - 1));
            }
            //OnUpdatedIndex();   // don't need to do this as in maker gui, i set a new index
        }
        #endregion

        /// <summary>
        /// Called when opening gui
        /// </summary>
        public void ResetIndex()
        {
            SelectedIndex = 0;
            OnUpdatedIndex();
        }
        public void Remove()
        {
            if (MaxSelected != 0)
            {
                MaxSelected--;
                OnUpdatedMaxSelected();
                OldSelectedIndex = SelectedIndex - 2;   // make sure it is updated
                //ClampIndex();
                OnRemove.Invoke(SelectedIndex);    // updates the list with connected class
                //OnUpdatedIndex();                   // as list might shrink
            }
        }

        public void Add()
        {
            if (MaxSelected == 0)
            {
                OldSelectedIndex = -1;
            }
            SelectedIndex = MaxSelected;
            MaxSelected++;
            OnAdd.Invoke(SelectedIndex);    // updates the list with connected class
            OnUpdatedMaxSelected(); // wait til thing is added
        }
    }
}
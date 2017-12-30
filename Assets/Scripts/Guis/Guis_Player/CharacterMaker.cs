using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zeltex.Characters;
using Zeltex.Skeletons;
using Zeltex.Util;
using Zeltex.Guis.Maker;

namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Character Maker Handles the spawning of a character and a gui
    /// </summary>
    public class CharacterMaker : MonoBehaviour
    {
        #region Variables
        [Header("Guis")]
        public ClassMaker MyClassMaker;
        public SkeletonMaker MySkeletonManager;
        [Header("UI")]
        public CharacterViewer MyCharacterViewer;
        public Text MyClassLabel;
        public Text MyRaceLabel;
        public Button ConfirmButton;
        public InputField NameInput;
        private int SelectedIndex = 0;
        private int PreviousIndex;
        private int SelectedRaceIndex;
        private int PreviousRaceIndex;
        #endregion

        #region Input
        public void UpdateName(InputField MyInput)
        {
            MyCharacterViewer.GetSpawn().GetComponent<Character>().UpdateName(NameInput.text);
            NameInput.text = MyCharacterViewer.GetSpawn().name;
        }
        #endregion

        #region ZelGui
        /// <summary>
        /// When the ZelGui is turned on
        /// </summary>
        public void OnBegin()
        {
            PreviousIndex = -1; // force update
            SelectedIndex = 0;
            OnUpdatedClass();
            PreviousRaceIndex = -1;
            SelectedRaceIndex = 0;
            OnUpdatedRace();
            NameInput.text = Zeltex.NameGenerator.GenerateVoxelName();
            MyCharacterViewer.GetSpawn().GetComponent<Character>().UpdateName(NameInput.text);
        }

        public void OnCancel()
        {
            MyCharacterViewer.OnCancel();
        }

        public void OnConfirm()
        {
            MyCharacterViewer.OnConfirm();
        }
        #endregion

        #region ClassController
        public void NextClass()
        {
            SelectedIndex++;
            OnUpdatedClass();
        }

        public void PreviousClass()
        {
            SelectedIndex--;
            OnUpdatedClass();
        }
        /// <summary>
        /// Called when class index is updated
        /// </summary>
        void OnUpdatedClass()
        {
            /*SelectedIndex = Mathf.Clamp(SelectedIndex, 0, MyClassMaker.MyNames.Count - 1);
            if (SelectedIndex == -1)
            {
                SelectedRaceIndex = 0;
            }
            if (PreviousIndex != SelectedIndex)
            {
                Debug.Log("Updated race to " + SelectedIndex + " of " + MyClassMaker.MyNames.Count + " from " + PreviousIndex);
                if (MyClassMaker.MyNames.Count > 0)
                {
                    MyClassLabel.text = MyClassMaker.MyNames[SelectedIndex];
                    //string FilePath = MyClassMaker.GetFullFileName(SelectedIndex);
                    if (MyCharacterViewer.GetSpawn() != null)
                    {
                        //MyCharacterViewer.GetSpawnedObject().GetComponent<CharacterSaver>().Clear();
                        List<string> MyScript = FileUtil.ConvertToList(MyClassMaker.MyData[SelectedIndex]);
                        MyCharacterViewer.GetSpawn().GetComponent<Character>().RunScript(MyScript);
                    }
                }
                else
                {
                    MyClassLabel.text = "No Classes";
                }
                // update the character with the data
                PreviousIndex = SelectedIndex;
            }*/
        }
        #endregion

        #region RaceController
        // race picking
        public void NextRace()
        {
            SelectedRaceIndex++;
            OnUpdatedRace();
        }

        public void PreviousRace()
        {
            SelectedRaceIndex--;
            OnUpdatedRace();
        }

        public void OnUpdatedRace()
        {
            /*int MyMax = MySkeletonManager.MyNames.Count;
            SelectedRaceIndex = Mathf.Clamp(SelectedRaceIndex, 0, MyMax - 1);
            if (SelectedRaceIndex == -1)
            {
                SelectedRaceIndex = 0;
            }
            if (PreviousRaceIndex != SelectedRaceIndex)
            {
                Debug.Log("Updated race to " + SelectedRaceIndex + " of " + MyMax + " from " + PreviousRaceIndex);
                if (MyMax > 0)
                {
                    MyRaceLabel.text = MySkeletonManager.MyNames[SelectedRaceIndex];
                    StopCoroutine(ChangeRace());
                    StartCoroutine(ChangeRace());
                }
                else
                {
                    MyRaceLabel.text = "No Races";
                }
                PreviousRaceIndex = SelectedRaceIndex;
            }*/
        }
       /* private IEnumerator ChangeRace()
        {
            ConfirmButton.interactable = false;
            GameObject MyCharacter = MyCharacterViewer.GetSpawn();
            if (MyCharacter != null)
            {
                Skeleton MySkeleton = MyCharacter.transform.Find("Body").GetComponent<Skeleton>();
                yield return MySkeleton.RunScriptRoutine((MySkeletonManager.GetData(SelectedRaceIndex)));
            }
            ConfirmButton.interactable = true;
        }*/
        #endregion
    }
}


// Used by summoner spell
/*public static void UpdateCharacterWithScript(Transform MyCharacter, string ClassName)
{
    ClassMaker MyClassMaker = ClassMaker.Get();
    MyCharacter.GetComponent<CharacterSaver>().RunScript(MyClassMaker.GetData(ClassName));  // run class maker script on network
}
public static void UpdateCharacterSkeleton(Transform MyCharacter, string SkeletonName)
{
    SkeletonManager MySkeletonManager = SkeletonManager.Get();
    Skeleton MySkeleton = MyCharacter.FindChild("Body").GetComponent<Skeleton>();
    MySkeleton.RunScript(MySkeletonManager.GetData(SkeletonName));
}*/

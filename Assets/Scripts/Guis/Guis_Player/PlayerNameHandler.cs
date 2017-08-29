using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.AnimationUtilities;

/// <summary>
/// Anything that is local and saved just for the player
/// Including:
///     - Player Name
///     - Graphics Options
///     - Sound Options
///     - Player Gui Options
/// </summary>
namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Saves the player name in 'PlayerSettings.txt' file
    /// </summary>
    public class PlayerNameHandler : MonoBehaviour
    {
        public GameObject WhoAreYouTab;
        public GameObject WelcomeBackTab;
        //public SpeechAnimator MyWelcomeText;
        public InputField PlayerNameLabel;
        private string MyFileLocation;

        public Material MyCaretMaterial;

        // Load the player name here
        void Start ()
        {
            WhoAreYouTab.SetActive(true);
            WelcomeBackTab.SetActive(false);
            LoadName();
        }
        bool IsLoadingName = false;
        private void LoadName()
        {
            MyFileLocation = Application.persistentDataPath + "/PlayerSettings.txt";  //FileUtil.GetWorldFolderPath() + 
            Debug.Log("Loading Player settings from [" + MyFileLocation + "]");
            if (FileUtil.DoesFileExist(MyFileLocation))
            {
                StreamReader MyStreamReader = new StreamReader(MyFileLocation);
                string MyFileText = MyStreamReader.ReadToEnd();
                if (MyFileText != "")
                {
                    WelcomeBackTab.SetActive(true);
                    WhoAreYouTab.SetActive(false);
                    //string MyText = File.ReadAllText(MyFileLocation);
                    IsLoadingName = true;
                    PlayerNameLabel.text = MyFileText;
                    IsLoadingName = false;
                }
                MyStreamReader.Close();
                if (MyFileText != "")
                {
                    UpdatePlayerName(MyFileText);
                }
            }
            else
            {
                Debug.Log("File not found.");
                WhoAreYouTab.SetActive(true);
            }
            StartCoroutine(ChangeInputMaterial());
        }
        /// <summary>
        /// UI function
        /// </summary>
        public void UpdatePlayerName(string NewName)
        {
            if (IsLoadingName == false)
            {
            Debug.Log("Saving Player settings from [" + MyFileLocation + "]");
            //PhotonNetwork.player.name = NewName;
            StreamWriter MyStreamWriter = new StreamWriter(MyFileLocation);
            MyStreamWriter.WriteLine(NewName);
            //File.WriteAllText(MyFileLocation, NewName);
            MyStreamWriter.Close();
            }
        }
        IEnumerator ChangeInputMaterial()
        {
            // wait a frame such that the caret GO gets created.
            yield return new WaitForSeconds(0.5f);  // wait one frame

            if (PlayerNameLabel)
            {
                // Find the child by name. This usually isnt good but is the easiest way for the time being.
                //InputField[] MyInputs = FindObjectsOfType(typeof(InputField)) as InputField[];
                List<InputField> MyInputs = GetAllObjectsInScene();
                foreach (InputField MyInput in MyInputs)
                {
                    if (MyInput)
                    {
                        //Debug.LogError(" Input: " + MyInput.name);
                        Transform MyCaret = MyInput.transform.GetChild(0);
                        if (MyCaret)
                            MyCaret.GetComponent<CanvasRenderer>().SetMaterial(MyCaretMaterial, Texture2D.whiteTexture);
                    }
                }
            }
        }

        List<InputField> GetAllObjectsInScene()
        {
            List<InputField> objectsInScene = new List<InputField>();

            foreach (InputField go in Resources.FindObjectsOfTypeAll(typeof(InputField)) as InputField[])
            {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;
    //#if UNITY_EDITOR
                //if (!UnityEditor.EditorUtility.IsPersistent(go.transform.root.gameObject))
                //    continue;
    //#endif
                objectsInScene.Add(go);
            }
            return objectsInScene;
        }
    }
}
/*Transform MyCaret = PlayerNameLabel.transform.GetChild(0);  
//FindChild(PlayerNameLabel.transform.name + " Input Caret");
if (MyCaret)
    MyCaret.GetComponent<CanvasRenderer>().SetMaterial(MyCaretMaterial, Texture2D.whiteTexture);
    */

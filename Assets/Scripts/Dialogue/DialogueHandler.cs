using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.AnimationUtilities;
using Zeltex.Characters;

namespace Zeltex.Dialogue
{
    /// <summary>
    /// Handles conversation between 2 characters
    /// Communicates with the gui and the dialogue tree
    /// </summary>
    public class DialogueHandler : MonoBehaviour
    {
        [Header("Debug")]
        public bool DebugGui;
        [Header("References")]
        public Button ConfirmButton;
        [Tooltip("Link to a gameObject with a Text Component. If blank, this script won't work.")]
        public Text MyDialogueText;			// used to display the text in the chat - and toggle it on/off
        public Text MyReplyText;			// used to display the text in the chat - and toggle it on/off
        public List<Button> MyButtons;      // buttons to use for options
        [Header("Data")]
        public DialogueTree MyTree = new DialogueTree();    // stores all the data
        private bool IsAnimating;
        private bool IsFirst = true;
        public Character MyCharacter;
        public Character OtherCharacter;
        public UnityEvent OnEndTalk;

        void OnGUI()
        {
            if (DebugGui)
            {
                //MyTree.OnGUI();
                GUILayout.Label("Index: " + MyTree.GetIndex());
                GUILayout.Label("IsAnimating? " + IsAnimating);
            }
        }
        void Start()
        {
            MyTree.Reset();
        }
        public void OnConfirm()
        {
            OnConfirm(0);
        }
        public void OnConfirm(int OptionsIndex)
        {
            if (IsAnimating == false)
            {
                IsAnimating = true;
                if (IsFirst)
                {
                    IsFirst = false;
                }
                else
                {
                    MyTree.ProgressDialogue(OptionsIndex, MyCharacter, OtherCharacter);  // advanced to next dialogue line
                }

                DialogueData CurrentDialogue = MyTree.GetCurrentDialogue();
                if (CurrentDialogue != null)
                {
                    string Speaker = CurrentDialogue.GetSpeaker();
                    string NewLine = CurrentDialogue.GetSpeechLine();
                    Text MyText;
                    if (Speaker == DialogueGlobals.SpeakerName2)
                    {
                        MyText = MyReplyText;
                    }
                    else// if (Speaker == "Character")
                    {
                        MyText = MyDialogueText;
                        MyDialogueText.text = "";
                        MyReplyText.text = "";
                    }
                    UpdateText(CurrentDialogue, MyText, NewLine);
                }
                else
                {
                    // end talk
                    //Debug.LogError("Ending Talk.");
                    EndTalk();
                }
            }
        }
        public void EndTalk()
        {
            //Debug.Log("Ending talk!");
            MyDialogueText.text = "~";
            MyReplyText.text = "~";
            IsFirst = true;
            IsAnimating = false;
            MyTree.Reset();
            MyCharacter.OnEndTalkEvent.Invoke();
            OtherCharacter.OnEndTalkEvent.Invoke();
            //OnEndTalk.Invoke();
            //OtherCharacter.GetComponent<DialogueHandler>().OnEndTalk.Invoke();
        }
        // Animating The Text!
        public void UpdateText(DialogueData CurrentDialogue, Text MyText, string NewLine)
        {
            HideButtons();  // Hide Options Buttons
            SpeechAnimator MySpeechAnimator = MyText.gameObject.GetComponent<SpeechAnimator>();
            MySpeechAnimator.OnEnd.RemoveAllEvents();
            if (CurrentDialogue.HasOptions())
            {
                bool IsAuto = (CurrentDialogue.SpeechIndex == 0);
                if (IsAuto == false)
                {
                    AddAnimationListener(MyText, 0);
                }
                else
                {
                    AddAnimationListener(MyText, 1);
                }
                if (CurrentDialogue.GetSpeaker() == DialogueGlobals.SpeakerName2)
                {
                    NewLine = "";
                    for (int i = 0; i < CurrentDialogue.GetOptionsCount(); i++)
                    {
                        NewLine += (i + 1) + ": " + CurrentDialogue.GetSpeechLine(CurrentDialogue.SpeechIndex + i) + '\n';
                    }
                    CurrentDialogue.SpeechIndex += CurrentDialogue.GetOptionsCount() - 1;  // increase this!
                    ButtonsToShowCount = CurrentDialogue.GetOptionsCount();
                    MySpeechAnimator.OnEnd.AddEvent(ShowButtons);
                }
            }
            else
            {
                ButtonsToShowCount = 0;
                AddAnimationListener(MyText, 2);
            }
            MyText.gameObject.GetComponent<SpeechAnimator>().NewLine(NewLine);
        }

        private int ButtonsToShowCount = 0;
        private void ShowButtons()
        {
            ShowButtons(ButtonsToShowCount);
        }

        public void AddAnimationListener(Text MyText, int AddType)
        {
            SpeechAnimator MySpeechAnimator = MyText.gameObject.GetComponent<SpeechAnimator>();
            MySpeechAnimator.OnEnd.RemoveAllEvents();
            if (AddType == 0)
            {
                MySpeechAnimator.OnEnd.AddEvent(OnEndAnimation);
            }
            else if (AddType == 1)
            {
                MySpeechAnimator.OnEnd.AddEvent(OnEndAnimationAuto);
            }
            else if (AddType == 2)
            {
                MySpeechAnimator.OnEnd.AddEvent(OnEndAnimationSingle);
            }
        }

        public void OnEndAnimation()
        {
            IsAnimating = false;
        }
        public void OnEndAnimationSingle()
        {
            IsAnimating = false;
            ConfirmButton.gameObject.SetActive(true);
        }

        public void OnEndAnimationAuto()
        {
            IsAnimating = false;
            OnConfirm();
        }

        // Buttons / Options
        void HideButtons()
        {
            for (int i = 0; i < MyButtons.Count; i++)
            {
                MyButtons[i].gameObject.SetActive(false);
            }
            ConfirmButton.gameObject.SetActive(false);
        }
        public void ShowButtons(int ButtonsCount)
        {
            if (ButtonsCount > MyButtons.Count)
                ButtonsCount = MyButtons.Count;
            for (int i = 0; i < ButtonsCount; i++)
            {
                MyButtons[i].gameObject.SetActive(true);
            }
            ConfirmButton.gameObject.SetActive(false);
        }

        public void TestDialogueTree()
        {
            MyTree.Clear();
            // First test out the speech line list
            DialogueData NewData = new DialogueData("Beginning", "Line 1");
            NewData.AddSpeechLine("Character", "Hello There Pal.");// My name is Jefferson El Numerous. I am the greatest Wizard to live.");
            NewData.AddSpeechLine("Player", "....So?");
            NewData.AddSpeechLine("Character", "DIEEE MORTAL!");
            MyTree.Add(NewData);
            // Next test out a condition for options
            DialogueData NewData2 = new DialogueData("Line 1", "Line 2");
            NewData2.AddSpeechLine("Character", "So How many People have you exploded in your life?");
            NewData2.AddSpeechLine("Player", "[1] Ummm... \t[2] ALOT, DIE VERMINS!");
            NewData2.AddOptions(new List<string>() { "Line 2", "Line 3" });
            MyTree.Add(NewData2);
            // to options
            DialogueData NewData3 = new DialogueData("Line 2", "Exit");
            NewData3.AddSpeechLine("Character", "Well now, thats just rude..");
            NewData3.SetDefault("Exit");
            MyTree.Add(NewData3);

            DialogueData NewData4 = new DialogueData("Line 3", "Exit");
            NewData4.AddSpeechLine("Character", "Quite an impressive feat!");
            MyTree.Add(NewData4);


            DialogueData NewData5 = new DialogueData("Exit", "End");
            NewData5.AddSpeechLine("Character", "Now now, shoe away sonny jimmy ma boi.");
            MyTree.Add(NewData5);
        }

        public void UpdateDialogueText(Text MyText)
        {
            MyDialogueText = MyText;
        }
    }
}

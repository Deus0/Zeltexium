using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Guis;
using Zeltex.Quests;
using UnityEngine.Events;
using Zeltex.AnimationUtilities;
using Zeltex.Util;
/* Speech Handler - Attach to bot
 * Link it to the Speech Gameobject(with Text on it)
	Once initialized, a certain player will start the talking
	MainCharacter -> Lotus etc
	(the idea is that characters will initialize their own dialogue with anyone)
	One of the Speech Handlers are used as the client, and the other is the server

	Depends on scripts:
		Character
		SpeechAnimator	(GuiUtilities)
		DialogueData	(Data Format)
		SpeechFileReader(Static Reader)
		List			(Unity)
*/
// today:
//		Remove all quest info from this
//		Remove the gui creation stuff to put in its own script

/*namespace Zeltex.Dialogue 
{
	
	public class SpeechHandler : MonoBehaviour 
	{
		public bool IsDebugMode = false;
		public KeyCode SkipSpeechButton;
		[Header("Handlers")]
		public UnityEvent OnBeginTalk = new UnityEvent();
		public MyEvent3 OnBeginTalkTo = new MyEvent3();
		public MyEvent OnTalkTo = new MyEvent();
		public UnityEvent OnEndTalk = new UnityEvent();
		[Header("Data")]
		[SerializeField] public DialogueTree MyTree = new DialogueTree();

		[Header("References")]
		//[System.NonSerialized] 
		[Tooltip("Link to a gameObject with a Text Component. If blank, this script won't work.")]
		public Text MyDialogueText;			// used to display the text in the chat - and toggle it on/off
		//private GuiSystem.GuiManager MyCharacter;	// set in /awake - attached to same gameobject as this class
		private GameObject MyCharacter2;
		private SpeechAnimator MySpeechAnimator;
		
		[Header("States")]
		//[SerializeField] private bool HasLoaded = false;		// used to check if the file has loaded - io
		private bool IsOpen = false;							// whether the dialogue is currently opened or not
		private bool IsLineNotAnimating = true;		// true if line is not animating
		// int TalkerIndex = 0 etc	// 0 is main where the data is read from
		private bool IsFirstTalker = false;		// used to understand what speech handler is dictating the conversation - one initiator and one reciever
		private bool IsOptions = false;	// used primarily in secondary speaker, to check if its options
		//private bool bIsTalking = false;	// now use mycharacter2
		bool IsReversingDialogue = false;	// whether or not the dialogue is being reversed

		[Header("Sounds")]
		private AudioSource MySource;
		public AudioClip BeginTalkSound;
		public AudioClip NextLineSound;
		public AudioClip EndTalkSound;

		void Start()
		{
			MySource = GetComponent<AudioSource> ();
			if (MySource == null)
				MySource = gameObject.AddComponent<AudioSource> ();
		}
        // Update only runs when its active
        void Update() 
		{
			if (IsFirstTalker && IsDebugMode) 
			{
				if (Input.GetKeyDown(SkipSpeechButton))
                    {
					Debug.Log("Hitting Spacebar: " + Time.time);

					OnHitSpace ();
				}
			}
		}

		void OnGUI() 
		{
            if (!IsDebugMode)
                return;

            GUILayout.Label(name + "'s DialogueTree Index: " + (MyTree.GetIndex() + 1) + " of " + MyTree.GetSize());
            if (MyCharacter2 != null)
            {
				SpeechHandler MySpeech2 = GetMainSpeaker();
				DialogueData CurrentLine = GetMainSpeaker().GetCurrentDialogue();
				int PositionY = 0;
				GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20), name + " is talking to " + MyCharacter2.name);
				//GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20),"Player Can Press Space: " + CanHitSpace().ToString());
				
				GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20), "DialogueTree Index: " + (MySpeech2.MyTree.GetIndex()+1)+ " of " + MySpeech2.MyTree.GetSize());
				if (CurrentLine != null)
                {
					GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20),"SpeechIndex: " + (CurrentLine.SpeechIndex+1) + " of " + CurrentLine.SpeechLines.Count);
					
					for (int i = 0; i < CurrentLine.SpeechLines.Count; i++)
						GUI.Label (new Rect (0, (++PositionY) * 20f, 1000, 20),"\t Speech: " + CurrentLine.SpeechLines[i].GetLabelText());
				}
                else
                {
                    GUI.Label(new Rect(0, (++PositionY) * 20f, 1000, 20), "Current Speech Line is null");
                }
			}
            else
            {
                GUILayout.Label(name + " is talking to himself..");
                GUILayout.Label("Tree Size: " + MyTree.GetSize());
                for (int i = 0; i < MyTree.GetSize(); i++)
                {
                    DialogueData MyDialogueData = MyTree.Get(i);
                    GUILayout.Label("DialogueData [" + i + "] " + MyDialogueData.Name);
                    for (int j = 0; j < MyDialogueData.SpeechLines.Count; j++)
                    {
                        SpeechLine MyLine = MyDialogueData.SpeechLines[j];
                        GUILayout.Label("[" + MyLine.Speaker + "] " + MyLine.Speech);
                    }
                }
            }
        }

        // returns true if the dialogue tree has any lines
        public bool HasSpeech()
        {
            return (!MyTree.IsEmpty());
        }

        // Scripts
        public List<string> GetScriptList()
        {
            return MyTree.GetScriptList();
        }
        public void RunScriptList(List<string> MyData)
        {
            MyTree.RunScriptList(MyData);
        }
        public void AddDialogue(DialogueData NewDialogue)
        {
            MyTree.Add(NewDialogue);
        }
        // empties the data
        public void Clear()
        {
			MyTree.Clear ();
		}
		
		// same variable for both
		public bool CanHitSpace()
        {
			return GetMainSpeaker().IsLineNotAnimating;	
		}

		public QuestLog GetMainQuestLog()
        {
			return gameObject.GetComponent<QuestLog>();
		}
		public QuestLog GetSecondaryQuestLog()
        {
			if (MyCharacter2 == null) {
				//Debug.LogError("Why is this happening..!");
				return null;
			}
			return MyCharacter2.gameObject.GetComponent<QuestLog>();
		}
		public void SetMainTalker()
        {
			IsFirstTalker = true;
		}
		public void SetSecondaryTalker()
        {
			IsFirstTalker = false;
		}
		public void SetCharacter2(GameObject MyCharacter2_)
        {
			MyCharacter2 = MyCharacter2_;
		}
		public int GetSize()
        {
			return MyTree.GetSize();
		}

		public bool IsTalking() 
		{
			return (MyCharacter2 != null);
		}

		// Use this for initialization
		void Awake ()
        {
			MyTree.OnEndTree.AddEvent(delegate{
				ExitChat();
			});
		}

		// example: the player starts the conversation, second character is the npc
		public void Begin(GameObject Character2_) 
		{
			SetSecondaryTalker();		// only call this in main speaker
			SetCharacter2(Character2_);
			MyCharacter2.GetComponent<SpeechHandler>().SetCharacter2 (gameObject);

			ToggleSpeech (true, false);
			SetMainTalker();

			// gui stuff
			IsLineNotAnimating = false;
			//bIsTalking = true;
			IsOpen = true;
			if (MyDialogueText)
				MyDialogueText.gameObject.GetComponent<Text> ().text = "";
			else
            {
				Debug.LogError ("SpeechHandler has no Dialogue Gui at " + name);
                return;
            }

            if (MyDialogueText.gameObject.GetComponent<Text>())
                MyDialogueText.gameObject.GetComponent<Text>().text = "";
            else
            {
                Debug.LogError("MyDialogueText is null in SpeechHandler at " + name);
                return;
            }
            // Dialogue Tree Stuff
            ResetDialogueTree ();
			// Sound
            if (BeginTalkSound)
			    MySource.PlayOneShot(BeginTalkSound);
			// Events
			OnBeginTalk.Invoke ();
			OnTalkTo.Invoke (MyCharacter2.gameObject);
			OnBeginTalkTo.Invoke (MyCharacter2.gameObject, "Talked To");
			MyCharacter2.GetComponent<SpeechHandler>().OnBeginTalk.Invoke ();
			MyCharacter2.GetComponent<SpeechHandler>().OnBeginTalkTo.Invoke (gameObject, "Talked To");
			MyCharacter2.GetComponent<SpeechHandler>().OnTalkTo.Invoke (gameObject);
		}

		public void ResetDialogueTree()
        {
			MyTree.Reset (GetMainQuestLog(), GetSecondaryQuestLog());
			UpdateSpeech ();
		}
		// just need to check if the user has pressed the close button or the convo has shifted
		public void ExitOnClose()
		{
			if (!IsReversingDialogue) 
			{
				ExitChat();
			}
		}
		// ends the chat
		public void ExitChat() 
		{
			if (MyCharacter2 != null) // if conversation has already started
			{
				MyTree.End();
				Debug.Log ("ExitChat(): " + name + " is Ending talk between: " + name + " and " + MyCharacter2.name + " at " + Time.time);
				IsLineNotAnimating = false;
				IsOpen = false;
				MyCharacter2.GetComponent<SpeechHandler>().MyTree.ChattedCount++;
				// DialogueTree stuff
				MyDialogueText.text = "";
				// Gui Managers
				MyCharacter2.GetComponent<SpeechHandler>().Reset();
				Reset ();
				// Gui Buttons
				DestroyOldButtons();
				// Sound
                if (EndTalkSound)
				    MySource.PlayOneShot(EndTalkSound);
				// Events
				if (OnEndTalk != null)
					OnEndTalk.Invoke ();
			} else {
				Debug.LogError("Failure to exit chat in " + name);
			}
		}
		public void Reset() 
		{
			MyCharacter2 = null;
            if (gameObject.GetComponent<SpeechInitiator>())
            {
                gameObject.GetComponent<SpeechInitiator>().EndTalk();
            }
        }

        // DialogueTree Stuff
        public void HandInQuest() 
		{	// make sure this is added
			if (MyCharacter2) 
			{
				if (GetSecondaryQuestLog() == null)
					Debug.LogError("Could not hand in quest as already exited chat!");
				else
					GetSecondaryQuestLog().HandInQuest(GetCurrentDialogue().InputString, GetMainQuestLog());
			}
		}
	
		public DialogueData GetCurrentDialogue() 
		{
			return MyTree.GetCurrentDialogue ();
		}
		
	// Activates on the person that is doing the stuff

		
		public bool IsSingleLine()
        {
			if (IsFirstTalker)
				return (!GetCurrentDialogue ().HasOptions ());
			else
				return !IsOptions;
		}

		
		public void OnHitSpace()
        {
			OnHitSpace (0);
		}
		public SpeechHandler GetMainSpeaker() {
			if (IsFirstTalker)
				return this;
			else
				return MyCharacter2.GetComponent<SpeechHandler> ();
		}
		// only activates the next line if it is the main talker
		public void OnHitSpace(int OptionsIndex)
		{
			Debug.Log ("OnHitSpace: " + GetMainSpeaker ().name  + " : with option : " + OptionsIndex);
			GetMainSpeaker ().NextLineDo (OptionsIndex);
		}
		public void NextLineDo(int OptionsIndex) 
		{
			if (MyCharacter2 == null) {
				Debug.LogError("In NextLineDo, no secondary talker. Cannot continue talk!");
				return;
			}
			Debug.Log (name + " Activating Next Line.");
			//if (bIsTalking) // if has no exited the chat
			{
				MyTree.NextLine(OptionsIndex, GetMainQuestLog(), GetSecondaryQuestLog());
				
				if (MyCharacter2 != null)
                {
					UpdateSpeech ();
                    if (NextLineSound)
					    MySource.PlayOneShot(NextLineSound);
				}
			}
		}
		// this either starts the speaking on the player or the npc - depending on the speaker variable
		private void UpdateSpeech() 
		{
			DialogueData CurrentDialogue = GetCurrentDialogue ();
			if (CurrentDialogue != null)
            {
				string Speaker = CurrentDialogue.GetSpeaker ();
				if (Speaker == "Player")
                {
					string NewLine = CurrentDialogue.GetSpeechLine ();
					MyCharacter2.GetComponent<SpeechHandler>().NewReverseLine (NewLine, CurrentDialogue.HasOptions ());	// NewLineSpeaker("SpeakerName" etc - for n speakers
				}
                else if (Speaker == "Character")
                {   // else if Character
					string NewLine = CurrentDialogue.GetSpeechLine ();
					NewNormalLine (NewLine);
				}
			}
            else
            {
				//Debug.LogError("Current Dialogue Line is null..");
                GetComponent<SpeechInitiator>().EndTalk();
			}
		}

	// GUI Stuff

		// when main talker starts talking
		public void NewNormalLine(string NewLine) 
		{
			IsReversingDialogue = false;
			if (MyDialogueText == null || MyDialogueText.gameObject.GetComponent<SpeechAnimator> () == null) {
				Debug.LogError("MyDialogueText is null in " + name + " : " + (MyDialogueText == null) + " : " + (MyDialogueText.gameObject.GetComponent<SpeechAnimator> () == null));
				return;
			}
			DestroyOldButtons ();
			MyCharacter2.GetComponent<SpeechHandler>().DestroyOldButtons ();
			ToggleSpeech(true, false);
			MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().NewLine (NewLine);
			AddAnimationListener ();
			IsLineNotAnimating = false;
		}
		// called from the second characters speech handler
		public void NewReverseLine(string NewLine, bool IsOptions_) 
		{
			IsReversingDialogue = true;
			DestroyOldButtons ();
			MyCharacter2.GetComponent<SpeechHandler>().DestroyOldButtons ();
			//Debug.LogError ("New Reverse line at " + name);
			ToggleSpeech(true, false);
			MyDialogueText.GetComponent<SpeechAnimator> ().NewLine (NewLine);
			AddAnimationListener();
			MyCharacter2.GetComponent<SpeechHandler>().IsLineNotAnimating = false;
			IsOptions = IsOptions_;	// will options be generated
		}

		public void ToggleSpeech(bool IsSpeech) 
		{
			ToggleSpeech (IsSpeech, IsSpeech);
		}

		public void ToggleSpeech(bool IsSpeech, bool IsSpeech2) 
		{
			ToggleSpeechTarget (gameObject, IsSpeech);
			ToggleSpeechTarget (MyCharacter2, IsSpeech2);
		}

		public void ToggleSpeechTarget(GameObject MyCharacterTarget, bool IsOpenNew) 
		{
			if (MyCharacter2) 
			{
				MyCharacterTarget.GetComponent<SpeechHandler>().IsOpen = IsOpenNew;
                if (IsOpenNew)
                    MyCharacterTarget.GetComponent<GuiManager>().GetZelGui("Dialogue").TurnOn();
                else
                    MyCharacterTarget.GetComponent<GuiManager>().GetZelGui("Dialogue").TurnOff();
            }
		}

		// this way the handler knowns when the text has finished animation - stops skipping to next line
		
		public void ResetAnimationListener() 
		{
			MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().OnEnd = new UnityEvent ();
		}
		public void AddAnimationListener() 
		{
			ResetAnimationListener ();
			MyDialogueText.gameObject.GetComponent<SpeechAnimator> ().OnEnd.AddEvent(OnEndAnimation);
		}

		// called at the end of animating text
		private void OnEndAnimation() {
			//Debug.LogError ("Finished animating text.");
			if (IsFirstTalker)
				UpdateResponseGui (GetCurrentDialogue ());
			else
				MyCharacter2.GetComponent<SpeechHandler>().UpdateResponseGui (MyCharacter2.GetComponent<SpeechHandler>().GetCurrentDialogue ());
		}

		// need to put this in a new class
		// Create Gui class - input will be a rect in world position
		// 		- creates gui windows around it
		// handles the various setups for responses
		private void UpdateResponseGui(DialogueData NewDialogue) 
		{
			DestroyOldButtons ();

			if (!NewDialogue.HasOptions()) 
			{
				//MyDialogueText.gameObject.transform.FindChild("NextButton").gameObject.SetActive(true);
				//Debug.Log ("Activating Next button at " + Time.time);
				//	IsLineNotAnimating = true;	// only do this when the next button is active

				MySpeechButtons = SpeechGui.CreateAnswerButtons(1, transform, (transform.position-MyCharacter2.transform.position).normalized);
				int OptionsIndex = 0;
				MySpeechButtons[OptionsIndex].GetComponent<Button>().onClick.AddListener (delegate {
						OnHitSpace (OptionsIndex);
				});
			}
			else
			{
				IsLineNotAnimating = false;
				//MyCharacter2.GetComponentIsLineNotAnimating = false;	// only do this when the next button is active
				Debug.Log ("Activating Options at " + Time.time);
				ToggleSpeechTarget(MyCharacter2, true);
				MyCharacter2.GetComponent<SpeechHandler>().NewOptionsText (NewDialogue.GetAllSpeech("Player", true));
				MySpeechButtons = SpeechGui.CreateAnswerButtons(NewDialogue.GetOptionsCount (), transform, (transform.position-MyCharacter2.transform.position).normalized);
				for (int i = 0; i < MySpeechButtons.Count; i++) {
					int OptionsIndex = i;
					MySpeechButtons[i].GetComponent<Button>().onClick.AddListener (delegate {
						OnHitSpace (OptionsIndex);
					});
				}
				NewDialogue.End();
			}
		}
		private void NewOptionsText(string NewLine) 
		{
			ResetAnimationListener ();	// can't press space for options
			MyDialogueText.GetComponent<SpeechAnimator> ().NewLine (NewLine);
			MyCharacter2.GetComponent<SpeechHandler> ().IsLineNotAnimating = false;	// so we can't press spacebar
		}
		List<GameObject> MySpeechButtons = new List<GameObject>();
		private void DestroyOldButtons() {
			for (int i = MySpeechButtons.Count-1; i >= 0; i--) 
			{
				Destroy (MySpeechButtons[i]);
				MySpeechButtons.RemoveAt(i);
			}
		}
	}
}*/
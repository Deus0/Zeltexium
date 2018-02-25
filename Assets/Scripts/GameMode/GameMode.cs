using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using Zeltex.Characters;
using System.Collections.Generic;
using Zeltex;

// used by various spawning things - GameObject.Find(ManagerName); etc
namespace Zeltex.Game
{
    /// <summary>
    /// Managers the game mode
    /// </summary>
    public class GameMode : MonoBehaviour
    {
        #region Variables
        private static GameMode instance;
        public static bool IsPlaying;
        [Header("Options")]
	    public bool DebugMode;
        public bool IsStrategist;
        // Timed match
        [Header("Timed Mode")]
	    public bool IsTimeLimit = false;
	    public float MinutesTimeLimit = 5f;	 // limit in minutes
	    public float SecondsTimeLimit = 5f;	 // limit in seconds
	    // Kill a number of times
	    [Header("Slaughter Mode")]
	    public bool IsKillLimit = false;
	    public int KillsMax = 10;
	    // Destroy a number of blocks
	    [Header("Mining Mode")]
	    public bool IsBlockDestroyLimit = false;
	    public int BlocksDestroyMax = 100;
	    public int BlockType = 1;
	    // temp
	    [Header("Temp")]
	    public float TimeStarted = 0;
	    public float TimeGameCount = 0;
	    //public int KillsCount = 0;
	    public int BlocksDestroyCount = 0;
	    [Header("Events")]
	    public UnityEvent OnStartGame;
	    public UnityEvent OnEndGame;
        //public LevelMaker MyLevelMaker;
        [Header("UI")]
        public Text MyWinnerLabel;
        public Toggle StrategistToggle;
        #endregion

        public static GameMode Get()
        {
            return instance;
        }

        #region Mono
        void Start()
        {
            instance = this;
            enabled = false;    // disabled while game not active
            SetStrategistInternal(IsStrategist);    // set on startup
        }
        void Update() 
	    {
		    CheckTimeLimit ();
	    }
        #endregion

        #region GameMode 

        public void StartGame()
        {
            TimeStarted = Time.realtimeSinceStartup;
            //Debug.Log("Starting Game Match: " + TimeStarted);
            //Time.timeScale = 1;
            OnStartGame.Invoke();
            enabled = true;
            IsPlaying = true;
        }

        public void EndGame()
        {
            TimeGameCount = Time.realtimeSinceStartup - TimeStarted;
            Debug.Log("Ended Game Match: " + TimeGameCount);
            //Time.timeScale = 0;
            UpdateGui();
            OnEndGame.Invoke();
            enabled = false;
            IsPlaying = false;
        }
        #endregion

        public void CheckTimeLimit() 
	    {
		    TimeGameCount = Time.realtimeSinceStartup - TimeStarted;
		    if (IsTimeLimit) 
		    {
			    if (TimeGameCount >= SecondsTimeLimit) 
			    {
				    EndGame ();
			    }
		    }
	    }

	    public void CheckKillCondition(int KillsCount)
	    {
		    if (IsKillLimit)
		    {
			    if (KillsCount >= KillsMax)
			    {
				    EndGame ();
			    }
		    }
	    }

	    private void UpdateGui() 
	    {
			Character MyWinner = null;
		    int HighScore = 0;
		    //for (int i = 0; i < PhotonNetwork.playerList.Length; i++) 
		    List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
            for (int i = MyCharacters.Count-1; i >= 0; i--)
            {
                if (MyCharacters[i] == null || MyCharacters[i].GetComponent<Character>() == null)
                {
                    MyCharacters.RemoveAt(i);
                }
            }
		    for (int i = 0; i < MyCharacters.Count; i++) 
		    {
			    //if (PhotonNetwork.playerList [i].GetScore () > HighScore)
			    int ThisScore = MyCharacters[i].GetComponent<Character>().GetScore();
			    if (ThisScore  > HighScore)
			    {
				    HighScore = ThisScore;
				    MyWinner = MyCharacters[i];
			    }
		    }
		    // also check for ties?!
		    if (MyWinnerLabel != null)
		    {
			    if (MyWinner != null)
				    MyWinnerLabel.text = "[" + MyWinner.name + "] has won with a kill count of [" + HighScore + "]"
					    + "\nMatch went for [" + TimeGameCount + "] seconds";
			    else
				    MyWinnerLabel.text = "No one was killed.";
		    }
	    }

	    public void OnCharacterKill(string KillingCharacter, string KilledCharacter)
	    {

	    }

	    public void OnBlockDestroy(int DestroyedBlockType) 
	    {
		    if (BlockType == DestroyedBlockType) 
		    {
			    BlocksDestroyCount++;
			    if (IsBlockDestroyLimit && BlocksDestroyCount >= BlocksDestroyMax) 
			    {
				    EndGame ();
			    }
		    }
	    }

        #region UI
        // UI Stuff
        public void SetIsTimeLimit(bool NewIsTimeLimit)
	    {
		    IsTimeLimit = NewIsTimeLimit;
	    }
	    public void SetSecondsTimeLimit(string NewSecondsTimeLimit)
	    {
		    SecondsTimeLimit = float.Parse(NewSecondsTimeLimit);
	    }
	    public void SetIsKillLimit(bool NewIsKillLimit)
	    {
		    IsKillLimit = NewIsKillLimit;
	    }
	    public void SetKillsMax(string NewKillsMax)
	    {
		    KillsMax = int.Parse(NewKillsMax);
	    }
        #endregion

        public void SetStrategistInternal(bool IsStrategist_)
        {
            if (StrategistToggle)
            {
                StrategistToggle.isOn = IsStrategist_;
            }
            SetStrategist(IsStrategist_);
        }
        public void SetStrategist(bool IsStrategist_)
        {
            IsStrategist = IsStrategist_;
            if (Camera.main)
            {
                Player MyPlayer = Camera.main.gameObject.GetComponent<Player>();
                if (MyPlayer)
                {
                    MyPlayer.enabled = !IsStrategist;
                }
                StrategistController MyStrategistController = Camera.main.gameObject.GetComponent<StrategistController>();
                if (MyStrategistController)
                {
                    MyStrategistController.enabled = IsStrategist;
                }
            }
            else
            {
                Debug.LogError("Camera main is null!");
            }
        }
    }

}
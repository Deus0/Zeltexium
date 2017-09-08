using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MusicMaker
{
    /// <summary>
    /// A data struct for each column of notes
    /// </summary>
    [System.Serializable]
    class PianoColumn
    {
        public int ColumnIndex;
        public List<Toggle> PianoNotes = new List<Toggle>();

        public PianoColumn(int NewIndex)
        {
            ColumnIndex = NewIndex;
        }
        public void Add(Toggle NewToggle)
        {
            PianoNotes.Add(NewToggle);
        }
        public bool IsAnyZelGuiOn()
        {
            for (int i = 0; i < PianoNotes.Count; i++)
            {
                if (PianoNotes[i].isOn)
                {
                    PianoNotes[i].GetComponent<AudioSource>().Play();
                    return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// a basic piano script
    /// </summary>
    public class Piano : MonoBehaviour
    {
        [Header("Referennces")]
        public GameObject KeyPrefab;
        public AudioClip PianoSound;
        public GameObject PianoOverlayPrefab;
        private GameObject PianoOverlay;
        [Header("Options")]
        public int SizeX = 8;
        public int SizeY = 8;
        public float PitchDifference = 0.05f;
        private bool IsLoop;

        private float ColumnLength;
        public int ColumnInside = 0;    // increases every time entering inside a new column
        public float TotalTime = 4;
        public float TimeSpeed = 1;
        [SerializeField] List<PianoColumn> PianoColumns = new List<PianoColumn>();
        [Header("Debug")]
        public bool IsPlaying;
        public float StartTime;
        float TimePassed;

        // Use this for initialization
        void Start ()
        {
            // Get Sound from datamanager
            //PianoSound = Zeltex.DataManager.Get().GetSound("Sounds", 0);
            GeneratePiano();
        }
	
	    // Update is called once per frame
	    void Update ()
        {
		    if (IsPlaying)
            {
                UpdatePlaying();
            }
	    }

        #region UI
        public void SetLoop(bool IsLoop_)
        {
            IsLoop = IsLoop_;
        }
        #endregion

        #region Spawning
        public int GenerationType = 0;

        public void Generate()
        {
            ToggleAllOff();
            for (int i = 0; i < PianoColumns.Count; i++)
            {
                int j;
                if (GenerationType == 0)
                {
                    j = Random.Range(0, PianoColumns[i].PianoNotes.Count - 1);
                }
                else
                {
                    float Halfway = (PianoColumns[i].PianoNotes.Count - 1) / 2f;
                    float SinInput = (float)(i) / (float)(PianoColumns.Count - 1);
                    //SinInput *= 3.6f;
                    j = Mathf.RoundToInt(Halfway + (Halfway - 3) * Mathf.Sin(SinInput * PianoColumns[i].PianoNotes.Count));  // y offset + sinAmplitude * Sin(Frequency + wave offset);
                }
                Toggle MyPianoNote = PianoColumns[i].PianoNotes[j];
                MyPianoNote.isOn = true;
            }
        }

        /// <summary>
        /// Turn all of them off
        /// </summary>
        private void ToggleAllOff()
        {
            for (int i = 0; i < PianoColumns.Count; i++)
            {
                for (int j = 0; j < PianoColumns[i].PianoNotes.Count; j++)
                {
                    Toggle MyPianoNote = PianoColumns[i].PianoNotes[j];
                    MyPianoNote.isOn = false;
                }
            }
        }

        private Vector2 GetOverlaySize()
        {
            Vector2 KeySize = KeyPrefab.GetComponent<RectTransform>().sizeDelta;
            return new Vector2(SizeX * KeySize.x, SizeY * KeySize.y);
        }

        /// <summary>
        /// Spawn a column of toggles
        /// </summary>
        private void GeneratePiano()
        {
            for (int i = 0; i < SizeX; i++)
            {
                PianoColumn MyColumn = new PianoColumn(i);
                PianoColumns.Add(MyColumn);
                for (int j = 0; j < SizeY; j++)
                {
                    GameObject ThisInstance = Instantiate(KeyPrefab, new Vector3(), Quaternion.identity);
                    ThisInstance.name = "Key [" + i + ", " + j + "]";
                    ThisInstance.transform.SetParent(transform);
                    ThisInstance.GetComponent<AudioSource>().clip = PianoSound;
                    RectTransform MyRect = ThisInstance.GetComponent<RectTransform>();
                    MyRect.localScale = new Vector3(1, 1, 1);
                    ColumnLength = MyRect.sizeDelta.x;
                    MyRect.anchoredPosition3D = new Vector3(
                        i * MyRect.sizeDelta.x - (SizeX / 2) * MyRect.sizeDelta.x,
                        j * MyRect.sizeDelta.y - (SizeY / 2) * MyRect.sizeDelta.y,
                        0)
                        + new Vector3(MyRect.sizeDelta.x, MyRect.sizeDelta.y, 0) / 2f;
                    if (j < SizeY / 2)
                    {
                        ThisInstance.GetComponent<AudioSource>().pitch = 1 + (SizeY / 2 - j) * PitchDifference;
                    }
                    else
                    {
                        ThisInstance.GetComponent<AudioSource>().pitch = 1 + (j - SizeY / 2) * PitchDifference;
                    }
                    MyColumn.Add(ThisInstance.GetComponent<Toggle>());
                }
            }
            PianoOverlay = Instantiate(PianoOverlayPrefab);
            PianoOverlay.GetComponent<RectTransform>().sizeDelta = GetOverlaySize();
            PianoOverlay.transform.SetParent(transform);
            RectTransform OverlayRect = PianoOverlay.GetComponent<RectTransform>();
            OverlayRect.localScale = new Vector3(1, 1, 1);
            OverlayRect.anchoredPosition3D = new Vector3(
               -GetOverlaySize().x,//transform.GetComponent<RectTransform>().sizeDelta.x,
               0,
               0);
            ColumnInside = -1;
        }
        #endregion

        #region Playing
        public void Play()
        {
            IsPlaying = true;
            StartTime = Time.time;
        }

        public void Pause()
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying)
            {
                StartTime = Mathf.Clamp(Time.time - TimePassed, 0, TotalTime);
            }
            else
            {
                TimePassed = (Time.time - StartTime);
            }
        }

        public void Stop()
        {
            StartTime = Time.time;// (Time.time - TotalTime / TimeSpeed);    //  equal to total time!
            UpdatePlaying(false);
            IsPlaying = false;
            ColumnInside = -1;
        }

        private void UpdatePlaying()
        {
            UpdatePlaying(true);
        }

        public void UpdatePlaying(bool CanPlaySounds)
        {
            TimePassed = TimeSpeed * (Time.time - StartTime);
            if (TimePassed >= TotalTime)
            {
                TimePassed = TotalTime;
                IsPlaying = false;
            }
            float PositionX = Mathf.Lerp(-GetOverlaySize().x, 0, (TimePassed / (TotalTime)));
            PianoOverlay.transform.localPosition = new Vector3(
                PositionX,
                PianoOverlay.transform.localPosition.y,
                PianoOverlay.transform.localPosition.z);
            // play sounds as it passes them
            int NewColumnInside = PianoColumns.Count + Mathf.RoundToInt(PositionX / ColumnLength);
            // if now inside a new column!
            if (ColumnInside != NewColumnInside)
            {
                ColumnInside = NewColumnInside;
                if (CanPlaySounds)
                {
                    if (ColumnInside < PianoColumns.Count && ColumnInside >= 0)
                    {
                        if (PianoColumns[ColumnInside].IsAnyZelGuiOn())    // this plays the sound
                        {
                            //MySource.PlayOneShot(PianoSound);
                        }
                    }
                }
            }

            if (IsPlaying == false && IsLoop == true)
            {
                IsPlaying = true;
                float TimePerColumn = ColumnLength / 650;
                StartTime = Time.time + TimePerColumn;
            }
        }
        #endregion
    }
}
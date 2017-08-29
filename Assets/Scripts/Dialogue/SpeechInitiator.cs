using UnityEngine;
using Zeltex.AI;
using Zeltex.Characters;
using Zeltex.Guis;

/*namespace Zeltex.Dialogue
{

    public class SpeechInitiator : MonoBehaviour
    {
        [Header("Testing")]
        public bool IsInitiateSpeech;
        bool DebugLog;

        private SpeechHandler MySpeechBubble;
        [Header("Speech")]
        public bool IsSpeaking = false;
        public bool CanTalk = true;
        [Tooltip("These functions are called when dialogue begins!")]
        public UnityEngine.Events.UnityEvent OnBeginTalking;
        [Tooltip("These functions are called when dialogue ends!")]
        public UnityEngine.Events.UnityEvent OnEndTalking;
        [Header("Sounds")]
        private AudioSource MySource;
        [Tooltip("Played when dialogue begins")]
        [SerializeField]
        private AudioClip OnBeginTalkingSound;
        [Tooltip("Played when dialogue ends")]
        [SerializeField]
        private AudioClip OnEndTalkingSound;
        SpeechInitiator SpeechPartner;

        void Start()
        {
            // Grab my speech handler!
            MySpeechBubble = gameObject.GetComponent<SpeechHandler>();
            MySource = gameObject.GetComponent<AudioSource>();
            if (MySource == null)
            {
                MySource = gameObject.AddComponent<AudioSource>();
            }
        }

        void Update()
        {
            if (IsInitiateSpeech || (gameObject.GetComponent<Player>() && Input.GetKeyDown(KeyCode.E)))
            {
                IsInitiateSpeech = false;
                BeginSpeech();
            }
        }

        public SpeechHandler GetSpeechHandler()
        {
            if (MySpeechBubble == null)
                MySpeechBubble = gameObject.GetComponent<SpeechHandler>();
            return MySpeechBubble;
        }
        public void BeginSpeech()
        {
            RaycastHit MyHit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out MyHit))
            {
                GameObject HitObject = MyHit.collider.gameObject;
                if (HitObject.GetComponent<Zeltex.Characters.Character>())
                {
                    if (DebugLog)
                        Debug.Log(name + " is beginning speech with " + HitObject.name);
                    SpeechInitiator SpeechPartner = HitObject.GetComponent<SpeechInitiator>();
                    if (SpeechPartner == null)
                        SpeechPartner = HitObject.AddComponent<SpeechInitiator>();
                    BeginTalk(SpeechPartner);
                }
            }
        }
        public bool CanBeginTalk()
        {
            // return (CanTalk);
            //Debug.Log(name + " can speak? " + GetComponent<SpeechHandler>().HasSpeech());
            return (CanTalk && GetComponent<SpeechHandler>().HasSpeech());
        }
        public void BeginTalk(SpeechInitiator MySpeechPartner)
        {
            if (CanTalk && MySpeechPartner.CanBeginTalk())
            {
                if (!MySpeechBubble.IsTalking())
                {
                    if (IsSpeaking == false)
                    {
                        if (DebugLog)
                            Debug.Log (gameObject.name + " -Beginning dialogue with: " + MySpeechPartner.name);
                        SpeechPartner = MySpeechPartner;
                        IsSpeaking = true;
                        MySpeechPartner.IsSpeaking = true;
                        MySpeechPartner.BeginDialogue(this);
                    }
                }
            }
        }
        public void BeginDialogue(SpeechInitiator ConversationStarter)
        {
            // Now have speech bubbles pop up
            if (!GetSpeechHandler().IsTalking() && !ConversationStarter.GetSpeechHandler().IsTalking())
            {
                if (gameObject.GetComponent<Bot>())
                {
                    gameObject.GetComponent<Bot>().BegunSpeech(ConversationStarter.gameObject);
                }
                else if (gameObject.GetComponent<Zeltex.Characters.Player>())
                {
                    //gameObject.GetComponent<Zeltex.Characters.Player>().BegunSpeech(ConversationStarter.gameObject);  // disable player movement if in conversation
                }
                MySpeechBubble.Begin(ConversationStarter.gameObject);
                if (OnBeginTalking != null)
                    OnBeginTalking.Invoke();
                if (OnBeginTalkingSound)
                    MySource.PlayOneShot(OnBeginTalkingSound);
            }
        }

        public void EndTalk()
        {
            if (IsSpeaking)
            {
                Debug.Log("Ending talk in gui manager! " + name);
                IsSpeaking = false;
                if (gameObject.GetComponent<GuiManager>())
                {
                    gameObject.GetComponent<GuiManager>().GetZelGui("Dialogue").TurnOff();
                }
                if (gameObject.GetComponent<BotMovement>())
                {
                    gameObject.GetComponent<BotMovement>().LookAt(null);
                }
                //SwitchMode("Label");  // close dialogue gui
                if (OnEndTalking != null)
                    OnEndTalking.Invoke();
                if (OnEndTalkingSound)
                    MySource.PlayOneShot(OnEndTalkingSound);
            }
        }
    }
}*/
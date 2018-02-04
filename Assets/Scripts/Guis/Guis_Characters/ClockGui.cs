using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zeltex.Guis;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Animates a clock!
    /// </summary>
	public class ClockGui : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent OnMinutePassed;
		public UnityEvent OnHourPassed;
		public UnityEvent OnDayPassed;
        [Header("References")]
        public Text MyDayText;
		public Text MyTimeText;
        public RectTransform MyStartPoint;
        public RectTransform MyParent;
        public Material MyMaterial;
        [Header("Options")]
        public float PresetTime = 0;
        public float HoursPerDay = 0.3f;    // this should be used with sun
        [Header("Dimensions")]
        public float SecondsHandLength = 0.1f;
        public float MinutesHandLength = 0.06f;
        public float HoursHandLength = 0.03f;
        public float SecondsHandWidth = 8;
        public float MinutesHandWidth = 12;
        public float HoursHandWidth = 18;
        //privates
        private float ForwardBuffer = -0.05f;
        private LineRenderer MySecondsLine;
        private LineRenderer MyMinutesLine;
        private LineRenderer MyHourLine;
        //private float DaysPassedTotal;
        // time passed
        private float SecondsPassedTotal;
        private float MinutesPassedTotal;
        private float HoursPassedTotal;
        private float SecondsPassed;
        private float MinutesPassed;
        private float HoursPassed;

        // Use this for initialization
        void Start ()
        {
			//SecondsHandLength = MyStartPoint.GetSize ().magnitude/3f;
			//MinutesHandLength = SecondsHandLength*0.9f;
			//HoursHandLength = MinutesHandLength*0.9f;
			//SecondsHandLength *= transform.lossyScale.x;
			//MinutesHandLength *= transform.lossyScale.x;
			//HoursHandLength *= transform.lossyScale.x;
			MySecondsLine = CreateClockLine (SecondsHandWidth);
			MyMinutesLine = CreateClockLine (MinutesHandWidth);
			MyHourLine = CreateClockLine (HoursHandWidth);
            ForwardBuffer = MyStartPoint.localPosition.z * MyStartPoint.lossyScale.z;
        }

		public LineRenderer CreateClockLine(float MyWidth)
        {
			GameObject MySecondsLineObject = new GameObject ();
			MySecondsLineObject.transform.SetParent (MyParent);
			MySecondsLineObject.transform.position = MyParent.transform.position;
			LineRenderer NewLine = MySecondsLineObject.AddComponent<LineRenderer> ();
			NewLine.positionCount = 2;
			NewLine.sharedMaterial = MyMaterial;
			NewLine.startWidth = MyWidth*transform.lossyScale.x;
            NewLine.endWidth = MyWidth * transform.lossyScale.x / 1.1f;
            NewLine.SetPosition (0, MyStartPoint.position);
			NewLine.SetPosition (1, MyStartPoint.position);
			return NewLine;
		}

		// Update is called once per frame
		void Update () 
		{
            if (gameObject.GetComponent<ZelGui>().GetState())  // if turned on
            {
                TickClock();
                SetTexts();
            }
        }

        void TickClock()
        {
            SecondsPassedTotal = (PresetTime + Time.time);
            MinutesPassedTotal = (SecondsPassedTotal / 60f);
            HoursPassedTotal = MinutesPassedTotal / 60f;
            float DaysPassedTotal = HoursPassedTotal / HoursPerDay;

            SecondsPassed = SecondsPassedTotal - ((int)MinutesPassedTotal) * 60;
            MinutesPassed = MinutesPassedTotal - ((int)HoursPassedTotal) * 60;
            HoursPassed = HoursPassedTotal - ((int)DaysPassedTotal) * 24;   //(Modulus days passed)

            MySecondsLine.SetPosition(0, MyStartPoint.position);
            MyMinutesLine.SetPosition(0, MyStartPoint.position);
            MyHourLine.SetPosition(0, MyStartPoint.position);

            MySecondsLine.SetPosition(1, MyStartPoint.position +
                                       SecondsHandLength * MyStartPoint.up * Mathf.Sin(Mathf.PI / 2f + (Mathf.PI * 2f * (SecondsPassedTotal / 60f))) +
                                       -SecondsHandLength * MyStartPoint.right * Mathf.Cos(Mathf.PI / 2f + (Mathf.PI * 2f * (SecondsPassedTotal / 60f)))
                                       + MyStartPoint.forward * ForwardBuffer);
            MyMinutesLine.SetPosition(1, MyStartPoint.position +
                                       MinutesHandLength * MyStartPoint.up * Mathf.Sin(Mathf.PI / 2f + (Mathf.PI * 2f * (MinutesPassed / 60f))) +
                                       -MinutesHandLength * MyStartPoint.right * Mathf.Cos(Mathf.PI / 2f + (Mathf.PI * 2f * (MinutesPassed / 60f)))
                                       + transform.forward * ForwardBuffer);

            MyHourLine.SetPosition(1, MyStartPoint.position +
                                    HoursHandLength * MyStartPoint.up * Mathf.Sin(Mathf.PI / 2f + (Mathf.PI * 2f * (HoursPassed / 12f))) +
                                    -HoursHandLength * MyStartPoint.right * Mathf.Cos(Mathf.PI / 2f + (Mathf.PI * 2f * (HoursPassed / 12f)))
                                       + transform.forward * ForwardBuffer);
        }

        void SetTexts()
        {
            string HoursPassedString = ((int)HoursPassed).ToString();
            if (HoursPassedString.Length == 1)
                HoursPassedString = HoursPassedString.Insert(0, "0");

            string MinutesPassedString = ((int)MinutesPassed).ToString();
            if (MinutesPassedString.Length == 1)
                MinutesPassedString = MinutesPassedString.Insert(0, "0");

            string SecondsPassedString = ((int)SecondsPassed).ToString();
            if (SecondsPassedString.Length == 1)
                SecondsPassedString = SecondsPassedString.Insert(0, "0");

            if (MyTimeText)
                MyTimeText.text = HoursPassedString + ":" + MinutesPassedString + ":" + SecondsPassedString;
            //if (MyDayText)
            //    MyDayText.text = "Day " + ((int)(DaysPassedTotal + 1)).ToString();
        }
	}
}
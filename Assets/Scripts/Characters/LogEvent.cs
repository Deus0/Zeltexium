using UnityEngine;
using System.Collections;

namespace Zeltex.Characters
{
    /// <summary>
    /// Class that stores a game event.
    /// </summary>
	[System.Serializable]
    public class LogEvent
    {
        public float TimeHappened = 0f;
        public string EventType = "";

        public LogEvent(string NewEvent, float TimeHappened_)
        {
            EventType = NewEvent;
            TimeHappened = TimeHappened_;
        }

        public string GetLabelText()
        {
            float MyTime = ((int)(TimeHappened * 100f)) / 100f;
            TimeHappened = TimeHappened % 60f;
            string MyTimeLabel = MyTime.ToString();
            string MyTabs = "\t";
            if (MyTimeLabel.Length == 1)
                MyTabs += "\t";
            if (MyTimeLabel.Length >= 4)
                MyTabs = "";
            return "[" + MyTimeLabel + "] : " + MyTabs + EventType;
        }
    }
}
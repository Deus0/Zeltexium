using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeltex
{
    /// <summary>
    /// The log manager manages logbooks
    /// </summary>
    [System.Serializable]
    public class Logbook
    {
        public string Name = "NewLog";
        public List<string> LogLines = new List<string>();
        private bool IsDrawLog = false;

        public Logbook(string NewName)  
        {
            Name = NewName;
        }

        public void Log(string NewLine)
        {
            LogLines.Add(NewLine);
            if (LogLines.Count > LogManager.MaxLog)
            {
                LogLines.RemoveAt(0);
            }
        }

        public void DrawLog()
        {
            IsDrawLog = GUILayout.Toggle(IsDrawLog, Name);
            if (IsDrawLog)
            {
                for (int i = 0; i < LogLines.Count; i++)
                {
                    GUILayout.Label(LogLines[i]);
                }
            }
        }

        public void Clear()
        {
            LogLines.Clear();
        }
    }

    [System.Serializable]
    public class LogDraw
    {
        public string Name = "";
        public UnityEvent DrawEvent = new UnityEvent();
        public bool IsDraw = false;

        public void Draw()
        {
            IsDraw = GUILayout.Toggle(IsDraw, Name);
            if (IsDraw)
            {
                DrawEvent.Invoke();
            }
        }
    }
    /// <summary>
    /// Manages debugging on client and consoles
    /// </summary>
    public class LogManager : ManagerBase<LogManager>
    {
        public static int MaxLog = 30;
        [SerializeField]
        private bool IsDrawLog = true;
        [SerializeField]
        private int InitialSpacePixels = 100;
        [SerializeField]
        private Dictionary<string, Logbook> LogBooks = new Dictionary<string, Logbook>();
        [SerializeField]
        private List<LogDraw> LogDraws = new List<LogDraw>();
    
        public void Log(string NewLine, string Channel = "Default")
        {
            if (LogBooks.ContainsKey(Channel) == false)
            {
                LogBooks.Add(Channel, new Logbook(Channel));
            }
            Logbook MyBook = LogBooks[Channel];
            MyBook.Log(NewLine);
        }

        private void OnGUI()
        {
            GUILayout.Space(InitialSpacePixels);
            IsDrawLog = GUILayout.Toggle(IsDrawLog, "L");
            if (IsDrawLog)
            {
                if (GUILayout.Button("Clear"))
                {
                    ClearLogs();
                }
                GUILayout.Label("LogDraws: " + LogDraws.Count);
                for (int i = 0; i < LogDraws.Count; i++)
                {
                    LogDraws[i].Draw();
                }
                GUILayout.Label("Logbook Types: " + LogBooks.Count);
                foreach (KeyValuePair<string, Logbook> MyKeyValue in LogBooks)
                {
                    if (MyKeyValue.Value.LogLines.Count > 0)
                    {
                        MyKeyValue.Value.DrawLog();
                    }
                }
            }
        }

        public void AddDraw(UnityAction DrawAction, string DrawName = "Logs")
        {
            if (DrawAction != null)
            {
                LogDraw NewDraw = new LogDraw();
                NewDraw.Name = DrawName;
                NewDraw.DrawEvent.AddEvent(DrawAction);
                LogDraws.Add(NewDraw);
            }
        }

        public void ClearLogs()
        {
            foreach (KeyValuePair<string, Logbook> MyKeyValue in LogBooks)
            {
                MyKeyValue.Value.Clear();
            }
        }
    }
}
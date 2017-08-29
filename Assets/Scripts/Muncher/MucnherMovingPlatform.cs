using UnityEngine;
using System.Collections;

namespace MuncherSystem
{
    public class MucnherMovingPlatform : MonoBehaviour
    {
        public float MoveDistance = 1;
        public float MoveSpeed = 3;
        public float InitialPause = 0;
        float BeginY;
        float TimeBegin;
        bool MovingUp = true;
        // Use this for initialization
        void Start ()
        {
            TimeBegin = Time.time + InitialPause;
            BeginY = transform.GetComponent<RectTransform>().anchoredPosition.y;
        }
	
	    // Update is called once per frame
	    void Update ()
        {
            if (MovingUp)
            {
                MoveUp();
            }
            else
            {
                MoveDown();
            }
            if (Time.time - TimeBegin >= MoveSpeed)
            {
                TimeBegin = Time.time;
                MovingUp = !MovingUp;
                BeginY = transform.GetComponent<RectTransform>().anchoredPosition.y;
            }
	    }
        void MoveUp()
        {
            Vector2 MyPosition = transform.GetComponent<RectTransform>().anchoredPosition;
            transform.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(new Vector2(MyPosition.x, BeginY),
                new Vector2(MyPosition.x, BeginY + MoveDistance),
                (Time.time - TimeBegin) / MoveSpeed);
        }
        void MoveDown()
        {
            Vector2 MyPosition = transform.GetComponent<RectTransform>().anchoredPosition;
            transform.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(new Vector2(MyPosition.x, BeginY),
                new Vector2(MyPosition.x, BeginY - MoveDistance),
                (Time.time - TimeBegin) / MoveSpeed);
        }
    }
}
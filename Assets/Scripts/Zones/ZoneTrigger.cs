using UnityEngine;
using Zeltex.Quests;
using Zeltex.Util;
using Zeltex.Characters;

namespace Zeltex.WorldUtilities
{
    /// <summary>
    /// Triggers quests
    /// </summary>
	public class ZoneTrigger : MonoBehaviour
    {
		public EventObject OnEnterZone;
		public EventObject OnLeaveZone;
		//public QuestLog MyCharacter;
		
		void OnTriggerEnter(Collider other)
        {
			//Debug.LogError ("I have gotten away from seth! " + other.gameObject.name);
			Character MyLeavingCharacter = other.gameObject.GetComponent<Character>();
			if (MyLeavingCharacter)
			{
                QuestLog MyQuestLog = MyLeavingCharacter.GetQuestLog();
                if (MyQuestLog != null)
                {
                    Debug.Log("Player: " + MyLeavingCharacter.name + " has entered zone " + name + " at " + Time.time);
                    MyQuestLog.OnZone(gameObject.name, "EnterZone");
                }
			}
			OnEnterZone.Invoke (other.gameObject);
		}

		void OnTriggerExit(Collider other)
        {
            //Debug.LogError ("I have gotten away from seth! " + other.gameObject.name);
            Character MyLeavingCharacter = other.gameObject.GetComponent<Character>();
            if (MyLeavingCharacter)
            {
                QuestLog MyQuestLog = MyLeavingCharacter.GetQuestLog();
                if (MyQuestLog != null)
                {
                    Debug.Log("Player: " + MyLeavingCharacter.name + " has left zone " + name + " at " + Time.time);
                    MyQuestLog.OnZone(gameObject.name, "LeaveZone");
                }
			}
			OnLeaveZone.Invoke (other.gameObject);
		}
	}
}
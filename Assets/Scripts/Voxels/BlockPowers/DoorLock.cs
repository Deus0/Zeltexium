using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

namespace Zeltex.WorldUtilities {
	public class DoorLock : MonoBehaviour
	{
		public UnityEvent OnUnlock;
		public string Password = "3387";
		public string AttemptText = "";
		public Text MyInputText;
		public Door MyDoor;
		public int AtttemptCount = 0;
		public int MaxDigits = 4;

		public void InputKey(int InputType) {
			if (AttemptText.Length < MaxDigits) {
				AttemptText += InputType;
				RefreshText ();
			}
		}

		public void BackSpace ()
		{
			if (AttemptText.Length > 0) {
				AttemptText = AttemptText.Remove(AttemptText.Length - 1,1);
				RefreshText ();
			}
		}
		public void RefreshText()
		{
			MyInputText.text = AttemptText;
		}
		public void Enter()
		{
			if (AttemptText == Password) {
				//MyDoor.Unlock();
				if (OnUnlock != null)
					OnUnlock.Invoke();
			} else {
				AtttemptCount++;
			}
			Clear ();
		}
		public void Clear() {
			AttemptText = "";
			RefreshText ();
		}
	}
}

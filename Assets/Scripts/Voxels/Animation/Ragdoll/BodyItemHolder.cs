using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*[SerializeField]
public class MeshThing {
	public GameObject MyItemToSpawn;
}*/

namespace Zeltex.AnimationUtilities {
	public class BodyItemHolder : MonoBehaviour {
		[Header("Debugging")]
		public bool IsDebugMode = false;
		public KeyCode ShiftItemRight;
		public KeyCode ShiftItemLeft;
		[Header("Data")]
		public List<GameObject> MyBodyParts = new List<GameObject>();
		[Tooltip("This is where the item will be spawned.")]
		[Header("Data2")]
		public int LeftItemIndex = -1;
		public GameObject MyLeftHandBone;
		private GameObject LeftItemReference;
		public int RightItemIndex = -1;
		public GameObject MyRightHandBone;
		private GameObject RightItemReference;
		// Use this for initialization
		void Start () {
			LeftItemReference = UpdateItem (LeftItemIndex, MyLeftHandBone, LeftItemReference);
			RightItemReference = UpdateItem (RightItemIndex, MyRightHandBone, RightItemReference);
		}
		public GameObject UpdateItem(int MyIndex, GameObject BoneObject, GameObject ObjectReference) {
			if (ObjectReference) 
			{
				Destroy (ObjectReference);
			}
			if (MyIndex >= 0 && MyIndex < MyBodyParts.Count && BoneObject) 
			{
				ObjectReference = (GameObject)Instantiate (MyBodyParts [MyIndex], 
				                                           BoneObject.transform.position,	// + MyBodyParts [EquiptedIndex].transform.position, 	
				                                           BoneObject.transform.rotation  * MyBodyParts [MyIndex].transform.rotation);	// 
				ObjectReference.transform.SetParent (BoneObject.transform);
				ObjectReference.transform.localPosition = MyBodyParts [MyIndex].transform.position;
				//SpawnedItem.transform.rotation = MyBodyParts [EquiptedIndex].transform.rotation;
				ObjectReference.transform.localScale = MyBodyParts [MyIndex].transform.localScale;
			}
			return ObjectReference;
		}

		void Update() {
			if (IsDebugMode) {
				if (Input.GetKeyDown(ShiftItemRight)) {
					LeftItemIndex++;
					//LeftItemIndex = Mathf.Clamp(LeftItemIndex, 0, MyBodyParts.Count);
					if (LeftItemIndex >= MyBodyParts.Count) {
						LeftItemIndex = -1;
					}
					LeftItemReference = UpdateItem (LeftItemIndex, MyLeftHandBone, LeftItemReference);
				} else if (Input.GetKeyDown(ShiftItemLeft)) {
					RightItemIndex++;
					if (RightItemIndex >= MyBodyParts.Count) {
						RightItemIndex = -1;
					}
					RightItemReference = UpdateItem (RightItemIndex, MyRightHandBone, RightItemReference);
				}
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Zeltex.Guis;

/*namespace Zeltex.Items
{

	public class ItemHold : MonoBehaviour
    {
		public UnityEvent OnFinishAnimation;
		Item MyItem;
		GameObject NewSpawn;
		bool IsMoving = false;
		float TimeBegan;
		Vector3 FromPosition;
		GameObject ToPosition;
		public float AnimationSpeed = 1.5f;

		// Use this for initialization
		void Start ()
        {
		
		}
		
		void Update() 
		{
			if (IsMoving) 
			{
					NewSpawn.transform.position = Vector3.Lerp(FromPosition, 
					                                           ToPosition.transform.position, 
					                                           (Time.time-TimeBegan)/AnimationSpeed);
			}
		}
		public bool BeginHoldItem(GameObject MyPlayer, GuiSystem.ItemHandler MyItemHandler, Item NewItem)
        {
			if (!IsMoving)
            {
				MyItem = new Item(NewItem);
				NewItem.SetQuantity(0);
				MyItemHandler.MyInventory.HandleAddItemEvent();
				NewSpawn = new GameObject ();
				NewSpawn.transform.position = MyItemHandler.transform.position;
				NewSpawn.transform.rotation = MyItemHandler.transform.rotation;
				NewSpawn.transform.localScale = MyItemHandler.transform.localScale;
				RectTransform MyRect = NewSpawn.AddComponent<RectTransform> ();
				MyRect.sizeDelta = MyItemHandler.GetComponent<RectTransform> ().sizeDelta;
				RawImage NewRawImage = NewSpawn.AddComponent<RawImage> ();
				NewRawImage.texture = MyItemHandler.transform.GetChild (0).GetComponent<RawImage> ().texture;
				NewSpawn.AddComponent<GUI3D.Billboard> ();
				NewSpawn.transform.SetParent (transform, false);
			
				FromPosition = MyItemHandler.transform.position;
				ToPosition = MyPlayer.transform.parent.FindChild ("Crosshair").gameObject;
				IsMoving = true;
				TimeBegan = Time.time;
				return true;
			}
            else
            {
				NewItem.SetQuantity(MyItem.GetQuantity());
				MyItemHandler.MyInventory.HandleAddItemEvent();
				Destroy (NewSpawn);
				IsMoving = false;
			}
			return false;
		}
	}
}
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Items;

namespace Zeltex.Quests
{
	/*
This class will look for either:
	Character
	ItemObject
	Zone
And use it as a target
It will then render either an ai line, or a arrow pointing in the way we need to go.
	 */
	public class QuestHelper : MonoBehaviour
    {
		private bool IsArrow = true;
		public GameObject MyTarget;
		private LineRenderer MyLine;
		private float LineWidth = 0.04f;
		private float LineWidthEnd = 0.01f;
		public Material MyLineMaterial;
		public Color32 MyLineColor;
		private float LineDistance = 0.6f;
		public Vector3 LineOffset = new Vector3(0,-0.58f,0);

		public Sprite MyTexture;
		public float CircleRadius = 0.5f;
		public Color32 MySpriteColor;
		public Vector3 SpriteOffset = new Vector3(0,-0.6f,0);
		private bool IsTowardsTarget = true;

		// Use this for initialization
		void Start () 
		{
			// spawn the line
			GameObject LineObject = new GameObject ();
			LineObject.transform.SetParent (transform);
			LineObject.name = "MyArrow";
			LineObject.transform.eulerAngles = new Vector3 (90f, 0, 0);
			LineObject.transform.localScale = new Vector3 (CircleRadius,CircleRadius,CircleRadius);
			MyLine = LineObject.AddComponent<LineRenderer> ();
			MyLine.positionCount = (2);
			MyLine.startWidth = (LineWidth);
            MyLine.endWidth = LineWidthEnd;
            MyLine.material = MyLineMaterial;
			MyLine.startColor = (MyLineColor);
            MyLine.endColor = MyLineColor;
            LineDistance = CircleRadius*0.65f/3f;
			SpriteRenderer MySprite = LineObject.AddComponent<SpriteRenderer> ();
			MySprite.sprite = MyTexture;
			MySprite.color = MySpriteColor;
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (Input.GetKeyDown (KeyCode.J)) 
			{
				IsArrow = !IsArrow;
			}
			if (IsArrow) 
			{
				PointArrowToTarget ();
			} 
			else 
			{
				RenderLineToTarget();
			}
		}
		public void UpdateSpriteCircle() 
		{
			MyLine.gameObject.transform.position = transform.position + SpriteOffset;
			MyLine.gameObject.transform.eulerAngles = new Vector3 (90f, 0, 0);
		}

		public void RenderLineToTarget() 
		{
			MyLine.gameObject.SetActive(MyTarget != null);
			if (MyTarget) 
			{
				UpdateSpriteCircle();
				MyLine.SetPosition (0, transform.position + LineOffset);
				Vector3 Position2 = MyTarget.transform.position;
				Position2.y = (transform.position+LineOffset).y;
				MyLine.SetPosition (1,Position2);	// sets the second point to the target
			}
		}

		public void PointArrowToTarget()
		{
			MyLine.gameObject.SetActive(MyTarget != null);
			if (MyTarget) {
				UpdateSpriteCircle ();
				MyLine.SetPosition (0, transform.position + LineOffset);
				Vector3 Direction;
				if (IsTowardsTarget)
					Direction = (MyTarget.transform.position - transform.position).normalized;
				else
					Direction = (transform.position - MyTarget.transform.position).normalized;
				Vector3 Position2 = transform.position + LineOffset + Direction * LineDistance;
				Position2.y = (transform.position + LineOffset).y;
				MyLine.SetPosition (1, Position2);	// sets the second point to the target
			}
            else
            {
				//FindCurrentQuestTarget ();
			}
		}

		public void OnFinishQuest() 
		{
			MyTarget = null;
		}

		public void FindCurrentQuestTarget()
        {
			//Debug.LogError ("Finding new quest target " + Time.time);
			MyTarget = null;
			QuestLog MyQuestLog = gameObject.GetComponent<QuestLog> ();
			for (int i = 0; i < MyQuestLog.GetSize(); i++) 
			{
                Quest MyQuest = MyQuestLog.Get(i);
				if (!MyQuest.IsHandedIn) 
				{
					FindTarget(MyQuest);
					return;
				}
			}
		}

		// problem is the item gets destroyed after its found
		public void FindTarget(Quest MyQuest)
		{
			IsTowardsTarget = true;
			//Debug.LogError (MyQuest.ConditionIndex + ": \t ConditionType " + MyQuest.GetCurrentCondition ().ConditionType);
			if (!MyQuest.HasCompleted ())
            {
				if (MyQuest.GetCurrentCondition ().IsInventory ())
                {   // for all the item objects in the world, search for the item needed in the quest
                    //Debug.LogError ("\t ConditionType " + MyQuest.GetCurrentCondition ().ObjectName);
                    List<ItemObject> MyItems = ItemManager.Get().MyItemObjects;
					//Debug.LogError("Found a total of " + MyItems.Count + " Item Objects.");
					for (int i = 0; i < MyItems.Count; i++)
                    {
						//ItemObject MyItemObject = MyItems [i].GetComponent<ItemObject> ();
						if (!MyItems[i].HasUsed ())
                        {
							//Debug.LogError("Checking " + MyItemObject.MyItem.Name);
							if (MyQuest.GetCurrentCondition ().ObjectName == MyItems[i].GetItem().Name)
                            {
								MyTarget = MyItems [i].gameObject;
								return;
							}
						}
					}
					//Debug.LogError("None of them matched the item we seaked.");
				}
                else if (MyQuest.GetCurrentCondition ().IsEnterZone () || MyQuest.GetCurrentCondition ().IsLeaveZone ())
                {
					/*if (MyQuest.GetCurrentCondition ().IsLeaveZone ())
						IsTowardsTarget = false;
					List<GameObject> MyItems = ItemManager.GatherAllItemObjects ("Zeltex.WorldUtilities.ZoneTrigger");
					for (int i = 0; i < MyItems.Count; i++)
                    {
						Zeltex.WorldUtilities.ZoneTrigger MyItemObject = MyItems[i].GetComponent<Zeltex.WorldUtilities.ZoneTrigger> ();
						if (MyQuest.GetCurrentCondition ().ObjectName == MyItemObject.name)
                        {
							MyTarget = MyItems [i];
							return;
						}
					}*/
				}
			}
			if (MyQuest.GetCurrentCondition ().IsTalkTo () || MyQuest.HasCompleted()) 
			{
				/*List<GameObject> MyItems = Zeltex.Items.ItemManager.GatherAllItemObjects("Zeltex.Characters.Character");
				for (int i = 0; i < MyItems.Count; i++) 
				{
					Zeltex.Characters.Character MyItemObject = MyItems[i].GetComponent<Zeltex.Characters.Character>();
					string NpcName = "";
					if (MyQuest.HasCompleted()) 
					{
						NpcName = MyQuest.QuestGiver.name;
					} else {
						NpcName = MyQuest.GetCurrentCondition().ObjectName;
					}
					if (NpcName == MyItemObject.name) 
					{
						MyTarget = MyItems[i];
						return;
					}
				}*/
			}
		}
	}
}

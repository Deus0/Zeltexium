/*using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Zeltex.Dialogue;
using Zeltex.Guis;
using MyCustomDrawers;
using Zeltex.Items;
using Zeltex.Characters;

public class GuiPrefabUpdater : MonoBehaviour
{
	// New Network functions - updating

	private static bool IsOwner(Transform MyCharacter)
    {
		return (MyCharacter.GetComponent<PhotonView> ().owner.name != PhotonNetwork.playerName);
	}

	private static void UpdateGui(Transform MyCharacter, Transform NewGui, string Name) 
	{
		NewGui.SetParent (MyCharacter);
		NewGui.name = Name;

		ZelGui MyZelGui = NewGui.gameObject.GetComponent<ZelGui> ();
		if (MyZelGui) 
		{
			if (IsOwner(MyCharacter))
				MyZelGui.SetTarget(MyCharacter);
			GuiSystem.GuiManager MyGuiManager =  MyCharacter.GetComponent<GuiSystem.GuiManager> ();
			MyGuiManager.AddZelGui(MyZelGui);
		}
		// link these to the main body
		Orbitor MyOrbitor = NewGui.gameObject.GetComponent<Orbitor> ();
		Transform CameraBone = MyCharacter.FindChild ("CameraBone");
		if (MyOrbitor) 
		{
			if (CameraBone)
				MyOrbitor.SetTarget(CameraBone);
			MyOrbitor.IsFollowUserAngleAddition = true;
		}
			Billboard MyBillboard = NewGui.gameObject.GetComponent<Billboard> ();
			if (MyBillboard) 
			{
				MyBillboard.SetTarget (MyCharacter.FindChild ("CameraBone"));
			}
		//}
		Follower MyFollower = NewGui.gameObject.GetComponent<Follower> ();
		if (MyFollower)
		{
			MyFollower.UpdateTarget(MyCharacter);
		}
	}

}
*/
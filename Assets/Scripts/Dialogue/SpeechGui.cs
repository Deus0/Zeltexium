using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Guis;

namespace Zeltex.Dialogue
{

	public static class SpeechGui
    {
		public static List<GameObject> CreateAnswerButtons(int OptionsCount, Transform MyCharacter, Vector3 Direction) 
		{
			float BubbleWidth = 0.4f;
			Material MyMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
			Texture2D MyTexture = Resources.Load("BubbleTexture", typeof(Texture2D)) as Texture2D;

			List<GameObject> NewButtons = new List<GameObject> ();
			for (int i = 0; i < OptionsCount; i++)
            {
				//GameObject MyBlockThing = MyCharacter2.GetComponent<SpeechHandler>().MyDialogueText.transform.FindChild ("BlockThing" + i).gameObject;
				//MyBlockThing.SetActive (true);
				//MyBlockThing.GetComponent<RawImage> ().color = new Color32 ((byte)(0.25f), 
				//                                                            (byte)(0.25f), 
				//                                                            (byte)(0.25f), 
				//                                                            80);
				
				GameObject NewChild = new GameObject ();
				NewChild.name = "Option" + (i);
				
				RawImage MyImage = NewChild.AddComponent<RawImage> ();
				MyImage.texture = MyTexture;
				Button MyButton = NewChild.AddComponent<Button> ();
				MyButton.targetGraphic = MyImage;
				ColorBlock MyColors = MyButton.colors;
				MyColors.normalColor = Color.grey;
				MyColors.highlightedColor = Color.cyan;
				MyColors.pressedColor = Color.green;
				MyButton.colors = MyColors;
				NewChild.transform.position = MyCharacter.transform.position; 
				NewChild.transform.rotation = Quaternion.identity;	//MyDialogueText.transform.rotation;
				//NewChild.transform.localScale = MyCharacter.transform.localScale;
				NewChild.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
				//Vector3 OffsetPosition = new Vector3 (i * BubbleWidth * 2f - (OptionsCount) * BubbleWidth * 1.3f / 2f,
				//                                      -BubbleHeight / 2f - BubbleHeight / 2f, 0.3f);
				Vector3 OffsetPosition;
				OffsetPosition = Direction*Random.Range(-0.3f, -0.3f)+new Vector3(0,0.4f,0);	//new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
				//Vector3 DirectionUp = new Vector3(Direction.x, Direction.z, Direction.y);
				//OffsetPosition += DirectionUp*Random.Range(-0.1f, 0.1f);

				//Vector3 DirectionLeft = new Vector3(Direction.z, Direction.y, Direction.x);
				Vector3 DirectionLeft =  Quaternion.Euler(0, 90, 0)*Direction;
				OffsetPosition += (i*BubbleWidth-(OptionsCount-1)*BubbleWidth/2f)*DirectionLeft;
				NewChild.transform.position += OffsetPosition;	
				NewChild.transform.SetParent (MyCharacter.transform.parent);
				
				GameObject TextChild = new GameObject();
				TextChild.name = "Options Label: " + i;
				//TextChild.transform.position = NewChild.transform.position;
				Text MyText = TextChild.AddComponent<Text> ();
				if (OptionsCount > 1)
					MyText.text = "[" + (i+1) + "]";
				else 
					MyText.text = "[>]";
				TextChild.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
				Font MyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
				MyText.font = MyFont;
				MyText.resizeTextForBestFit = true;

				MyText.alignment = TextAnchor.MiddleCenter;
				TextChild.transform.SetParent(NewChild.transform);
				TextChild.transform.localPosition = new Vector3(0,0,0);
				TextChild.transform.localScale = new Vector3(1, 1, 1);

				Zeltex.AnimationUtilities.AnimateLine MyAnimationLine = NewChild.AddComponent<Zeltex.AnimationUtilities.AnimateLine>();
				MyAnimationLine.SetTarget(MyCharacter.gameObject);
				MyAnimationLine.TargetOffset = new Vector3(0,0.4f,0);
				MyAnimationLine.MyLineMaterial = MyMaterial;

				NewChild.AddComponent<Billboard>();

				Canvas MyCanvas = NewChild.AddComponent<Canvas>();
				MyCanvas.renderMode = RenderMode.WorldSpace;
				GraphicRaycaster MyRaycaster = NewChild.AddComponent<GraphicRaycaster>();

				NewButtons.Add(NewChild);
			}
			return NewButtons;
		}
	}
}
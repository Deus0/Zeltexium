using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Zeltex.AnimationUtilities 
{

    /// <summary>
    ///  Fades the text and floats upwards.
    ///  Applies randomness to the x-z axis while floating up
    /// </summary>
    public class OnHitText : MonoBehaviour 
	{
		public static float MoveToHeight = 2f;
		public Vector3 RandomDifference;
		public float DifferenceFromHitPlayer = 0;
		public Vector3 OriginalPosition = new Vector3(0,0,0);
		Color32 OriginalColor;
		Color32 FadeColor;
		float Variation = 0.5f;
		float FadeDuration = 5f;
		float TimeStarted;
		float TimeToStartFade;
		bool HasInitiated = false;

		// Use this for initialization
		void Start () 
		{
			RandomDifference = new Vector3 (Random.Range (-Variation, Variation), Random.Range (Variation, Variation), Random.Range (-Variation, Variation));
			OriginalColor = gameObject.GetComponent<TextMesh> ().color;
			FadeColor = OriginalColor;
			FadeColor.a = 0;
			TimeStarted = Time.time;
			TimeToStartFade = Time.time+1f;
			//gameObject.GetComponent<MeshRenderer> ().materials[0].shader = GetManager.GetDataManager ().MyTextShader;
			OriginalPosition = transform.position;
			Destroy (gameObject,FadeDuration);	// for now, make a on finish fade destroy it
		}

		// Update is called once per frame
		void Update () 
		{
			if (!HasInitiated)
			{
				OriginalPosition = transform.position;
				HasInitiated = true;
			}
			float LerpPercent = Mathf.Lerp(0.0f, 1.0f, (Time.time-TimeStarted) / (FadeDuration+1f));
			transform.position = OriginalPosition + new Vector3(0, MoveToHeight * LerpPercent, 0) + RandomDifference*LerpPercent;

			Color32 NewColor = OriginalColor; 

			float AlphaLerp = Mathf.Lerp(1.0f, 0.0f, (Time.time-TimeToStartFade) / FadeDuration);
			OriginalColor.a = (byte)Mathf.RoundToInt(255*AlphaLerp);
			gameObject.GetComponent<TextMesh>().color = NewColor;
			gameObject.GetComponent<MeshRenderer> ().materials[0].SetColor("_Color", NewColor);

            if (CameraManager.Get() && CameraManager.Get().GetMainCamera())
            {
                transform.LookAt(CameraManager.Get().GetMainCamera().transform.position);
            }
			transform.Rotate (new Vector3 (0, 180, 0));
		}
	}

	public static class StatPopUp  
	{
        static Transform PopupParent;

        public static void CreateTextPopup(Vector3 SpawnPosition, string PopupText, Font MyFont, Color FontColor)
		{
			CreateTextPopup(SpawnPosition, PopupText, 120+Random.Range(0,40), MyFont, FontColor);
		}

		public static void CreateTextPopup(Vector3 SpawnPosition, string PopupText, int FontSize, Font MyFont, Color FontColor)
		{
			GameObject NewText = new GameObject();
            if (PopupParent == null)
            {
                PopupParent = GameObject.Find("Popups").transform;
            }
            NewText.transform.SetParent(PopupParent);

            NewText.transform.position = SpawnPosition;
			NewText.name = "Damage Popup " + Time.time.ToString ();
			TextMesh MyText = NewText.AddComponent<TextMesh>();
			MyText.text = PopupText;
            MyText.font = MyFont;
            MyText.fontStyle = FontStyle.Normal;
			MyText.fontSize = FontSize;
            MyText.alignment = TextAlignment.Center;
            MyText.anchor = TextAnchor.MiddleCenter;
            //MyText.alignment = 
            NewText.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
            MeshRenderer MyMeshRenderer = NewText.GetComponent<MeshRenderer>();
            //MyMeshRenderer.material = FontMaterial;
            if (MyFont)
            {
                MyMeshRenderer.material = new Material(MyFont.material);
            }
            MyText.color = FontColor;
            MyMeshRenderer.material.color = FontColor;
            OnHitText MyOnHit = NewText.AddComponent<OnHitText>();
        }
    }
}

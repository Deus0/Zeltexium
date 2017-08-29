using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Zeltex.Guis
{
    /// <summary>
    /// Fades a gui
    /// </summary>
    public class FadeGuiTree : MonoBehaviour
    {
	    public UnityEvent OnFinishFade;
	    public UnityEvent OnFinishFadeDissapear;
	    public float FadeTime = 8f;
	    public float MaxAlpha = 200f;
	    private float TimeStarted = -1;
	    private List<RawImage> MyRawImages = new List<RawImage>();
	    private List<Text> MyTexts = new List<Text>();
	    private List<Image> MyImages = new List<Image>();
	    private float BeginAlpha;
	    private float EndAlpha;
	    private float LerpedAlpha = 0;	// state value of alpha

	    // Use this for initialization
	    void Awake () {
		    RefeshTree ();
		    //Animate (false);
	    }
	    // Update is called once per frame
	    void Update () {
		    if (TimeStarted != -1) {
			    float TimeSince = Time.time - TimeStarted;
			    if (TimeSince <= FadeTime) {
				    RefeshTree ();
				    LerpedAlpha = Mathf.Lerp (BeginAlpha, EndAlpha, TimeSince / FadeTime);
				    SetAlphas (LerpedAlpha);
			    } else {
				    //if (TimeStarted != -1) {
					    TimeStarted = -1;
					    LerpedAlpha = EndAlpha;
					    SetAlphas (EndAlpha);
					    if (LerpedAlpha == 0)
						    OnFinishFadeDissapear.Invoke ();
					    else
						    OnFinishFade.Invoke ();
				    //}
			    }
		    }
	    }

	    public void UpdateTime(float NewFadeTime) {
		    FadeTime = NewFadeTime;
	    }

	    public void Animate(bool IsForward) {
		    TimeStarted = Time.time;
		    if (IsForward) {
			    if (LerpedAlpha == 0)
				    BeginAlpha = MaxAlpha;
			    else
				    BeginAlpha = LerpedAlpha;
			    EndAlpha = 0;
		    } else {
			    if (LerpedAlpha == MaxAlpha)
				    BeginAlpha = 0;
			    else
				    BeginAlpha = LerpedAlpha;
			    //BeginAlpha = 0;
			    EndAlpha = MaxAlpha;
		    }
	    }
	    public void Clear() {
		    MyRawImages.Clear ();
		    MyTexts.Clear ();
		    MyImages.Clear ();
	    }
	
	    public void RefeshTree() {
		    Clear ();
		    FillTree (transform);
	    }
	    public void FillTree(Transform MyTransform) {
		    //Debug.LogError (MyTransform.name + ":" + MyTransform.childCount);
		    for (int i = 0; i < MyTransform.childCount; i++) {
			    Transform ChildTransform = MyTransform.GetChild(i);
			    if (ChildTransform.gameObject.GetComponent<Mask>() == null) {
				    if (ChildTransform.gameObject.GetComponent<RawImage>()) {
					    MyRawImages.Add (ChildTransform.gameObject.GetComponent<RawImage>());
				    }
				    if (ChildTransform.gameObject.GetComponent<Text>()) {
					    MyTexts.Add (ChildTransform.gameObject.GetComponent<Text>());
				    }
				    if (ChildTransform.gameObject.GetComponent<Image>()) {
					    MyImages.Add (ChildTransform.gameObject.GetComponent<Image>());
				    }
			    }
			    FillTree(ChildTransform);
		    }
	    }
	    public void SetAlphas(float LerpedAlpha) {
		    for (int i = 0; i < MyRawImages.Count; i++) {
			    MyRawImages[i].color = new Color32((byte)(MyRawImages[i].color.r*255f), (byte)(MyRawImages[i].color.g*255f), (byte)(MyRawImages[i].color.b*255f), (byte)LerpedAlpha);
		    }
		    for (int i = 0; i < MyTexts.Count; i++) {
			    MyTexts[i].color = new Color32((byte)(MyTexts[i].color.r*255f), (byte)(MyTexts[i].color.g*255f), (byte)(MyTexts[i].color.b*255f), (byte)LerpedAlpha);
		    }
		    for (int i = 0; i < MyImages.Count; i++) {
			    MyImages[i].color = new Color32((byte)(MyImages[i].color.r*255f), (byte)(MyImages[i].color.g*255f), (byte)(MyImages[i].color.b*255f), (byte)LerpedAlpha);
		    }
	    }
    }
}
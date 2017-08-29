using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Zeltex.AnimationUtilities
{
    public class AnimateTexture : MonoBehaviour
    {
	    float LastChanged = 0;
	    RawImage MyImage;
	    public float Cooldown = 3f;
	    public bool IsLooping = true;
	    public Vector2 CycleAddition = new Vector2(0.25f,0);
	    public Vector2 TextureSize = new Vector2(0.25f,0);

	    // Use this for initialization
	    void Start () 
	    {
		    LastChanged = Time.time;
		    MyImage = gameObject.GetComponent<RawImage> ();
	    }
	
	    // Update is called once per frame
	    void Update () 
	    {
		    if (Time.time - LastChanged > Cooldown) 
		    {
			    LastChanged = Time.time;
			    Rect MyRect = MyImage.uvRect;
			    MyRect.center += CycleAddition;
			    MyRect.width = TextureSize.x;
			    MyRect.height = TextureSize.y;
			    MyImage.uvRect = MyRect;
		    }
        }
    }
}

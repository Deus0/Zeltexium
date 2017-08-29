using UnityEngine;

namespace Zeltex.AnimationUtilities
{

    public class MirrorMaterial : MonoBehaviour
    {
	    public MirrorMaterial OtherPortal;
	    public RenderTexture renderTexture;
	    public int renderTextureSize = 512;
	    private Camera MyCamera;
	    // Use this for initialization
	    void Start () 
	    {
		    if (renderTexture == null)
			    renderTexture = new RenderTexture( renderTextureSize, renderTextureSize, 16 );
		    renderTexture.isPowerOfTwo = true;
		    GameObject CameraObject = new GameObject ();
		    CameraObject.transform.SetParent (transform, false);
		    CameraObject.transform.rotation = transform.rotation;
		    //CameraObject.transform.Rotate (new Vector3 (-90, 0, 0));
		    CameraObject.transform.Rotate (new Vector3 (-90, 0, 0));
		    //CameraObject.transform.localEulerAngles += CameraObject.transform.TransformDirection(new Vector3 (-90, 0, 0));
		    //CameraObject.transform.LookAt (transform.forward);
		    MyCamera = CameraObject.AddComponent<Camera>();
		    Camera mainCam = Camera.main;
		    //MyCamera.targetTexture = renderTexture;
		    MyCamera.clearFlags = mainCam.clearFlags;
		    MyCamera.cullingMask = mainCam.cullingMask;
		    MyCamera.backgroundColor = mainCam.backgroundColor;
		    MyCamera.nearClipPlane = mainCam.nearClipPlane;
		    MyCamera.farClipPlane = mainCam.farClipPlane;
		    MyCamera.fieldOfView = mainCam.fieldOfView;
		    gameObject.GetComponent<MeshRenderer> ().material.mainTexture = renderTexture;	//SetTexture("_Texture", renderTexture);
		    UpdateCamera ();
	    }
	    public void UpdateCamera()
        {
		    if (OtherPortal)
            {
			    if (OtherPortal.MyCamera)
                {
				    OtherPortal.MyCamera.targetTexture = renderTexture;
				    MyCamera.targetTexture = OtherPortal.renderTexture;
			    }
		    }
	    }
    }
}
/*
 * // 'org' is the original portal transform, and 'dest' the destination.
private static void CalculatePortalMatrix (ref Matrix4x4 portalMat, Transform org, Transform dest) {
  Vector3 translation = dest.position - org.position;
  Quaternion rotation = rotation = dest.rotation * Quaternion.Inverse(org.rotation); // I though it would be the opposite, but trial and error made it look otherwise
  Vector3 scale = new Vector3(1f,1f,-1f); // the last negative scale makes it point in the right direction
 
  portalMat = Matrix4x4.TRS(translation, rotation, scale).inverse;
}
*/

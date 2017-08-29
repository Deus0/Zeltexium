using UnityEngine;
using System.Collections;


namespace Zeltex.AI 
{
    /// Communicate with BasicController and tell the character to rotate

    /// To make an FPS style character:
    /// - Create a capsule.
    /// - Add a rigid body to the capsule
    /// - Add the MouseLook script to the capsule.
    ///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
    /// - Add FPSWalker script to the capsule

    /// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
    /// - Add a MouseLook script to the camera.
    ///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)

    /// <summary>
    /// MouseLook rotates the transform based on the mouse delta.
    /// Minimum and Maximum values can be used to constrain the possible rotation
    /// </summary>
    /*public class MouseLook : MonoBehaviour 
	{
		public float sensitivityX = 15F;
		public float sensitivityY = 15F;
		public float minimumX = -360F;
		public float maximumX = 360F;
		public float minimumY = -60F;
		public float maximumY = 60F;
		float rotationX = 0F;
		float rotationY = 0F;
		Quaternion originalRotation;

		public Transform MyBodyTransform = null;
		public Transform MyCameraBone = null;

		public void UpdateBody(Transform MyBody) 
		{
			MyBodyTransform = MyBody;
		}

		public void UpdateCamera(Transform MyCameraBone2)
		{
			MyCameraBone = MyCameraBone2;
		}

		void Start ()
		{
			originalRotation = transform.localRotation;
			if (MyCameraBone == null)
				MyCameraBone = transform;
		}

		void Update ()
		{
            RotateCamera();
        }

        private void RotateCamera()
        {
            if (MyCameraBone == null || MyBodyTransform == null)
                return;
            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            //Debug.Log("InputY: " + Input.GetAxis("Mouse Y"));
            //rotationX += Input.GetAxis("Horizontal2") * sensitivityX;
            //rotationY += Input.GetAxis("Vertical2") * sensitivityY;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

            MyCameraBone.rotation = originalRotation * xQuaternion * yQuaternion;
            //MyBodyTransform.rotation = originalRotation * xQuaternion;
            MyBodyTransform.GetComponent<BasicController>().InputRotation((xQuaternion).eulerAngles);   //originalRotation * 
        }
		public static float ClampAngle (float angle, float min, float max)
		{
			if (angle <= -360F)
				angle += 360F;
			if (angle >= 360F)
				angle -= 360F;
			return Mathf.Clamp (angle, min, max);
		}
	}*/
}

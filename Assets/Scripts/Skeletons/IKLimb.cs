using UnityEngine;
using System.Collections;
using Zeltex.Game;
using Zeltex;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// Basic IK script used for aiming.
    /// </summary>
    public class IKLimb: MonoBehaviour 
    {
	    [Header("Debug")]
	    public bool DebugMode = true;
	    public bool DebugLines = false;
	    public KeyCode MyToggleKey = KeyCode.F;
	    [Header("Options")]
        public float LerpSpeed = 0.25f;
        public float ReverseLerpSpeed = 0.1f;
        private bool IsOptimize = false;
	    private bool IsEnabled = false;
	    [Header("References")]
        public bool IsAnimatingBackwards = false;
        //public CustomAnimator MyAnimator;
        public Transform upperArm, forearm, hand;
	    public Transform target, elbowTarget;
	    private Quaternion upperArmStartRotation, forearmStartRotation, handStartRotation;
	    private Vector3 targetRelativeStartPosition, elbowTargetRelativeStartPosition;
	    //helper GOs that are reused every frame
	    private GameObject upperArmAxisCorrection, forearmAxisCorrection, handAxisCorrection;
	    //hold last positions so recalculation is only done if needed
	    private Vector3 lastUpperArmPosition, lastTargetPosition, lastElbowTargetPosition;
        // For Animation Part
        private float TimeBegun;
        private Quaternion DesiredUpperArmRotation;
        private Quaternion DesiredForearmRotation;
        private Quaternion DesiredHandRotation;
        private Quaternion OriginalUpperArmRotation;
        private Quaternion OriginalForearmRotation;
        private Quaternion OriginalHandRotation;
        public Player MyPlayer;

        void Start()
	    {
		    BeginIK ();
	    }
	
	    void Update ()
        {
            if (target == null || upperArm == null || elbowTarget == null)
            {
                return;
            }
            if (MyPlayer) 
		    {
			    if (Input.GetKeyDown (MyToggleKey)) 
			    {
				    ToggleAim ();
			    }
            }
            if (IsEnabled) 
		    {
			    CalculateIK ();
			    AnimateRotations ();
		    } 
		    else
		    {
			    if (IsAnimatingBackwards)
				    AnimateRotationsBackwards ();
		    }
	    }

	    void AnimateRotations() 
	    {
		    float AnimationSpeed = LerpSpeed * Time.deltaTime * (1000f/60f);
		    upperArm.rotation = Quaternion.Slerp(DesiredUpperArmRotation, upperArm.rotation, AnimationSpeed);
		    forearm.rotation = Quaternion.Slerp(DesiredForearmRotation, forearm.rotation, AnimationSpeed);
		    hand.rotation = Quaternion.Slerp(DesiredHandRotation, hand.rotation, AnimationSpeed);
	    }

	    void AnimateRotationsBackwards() 
	    {
		    float AnimationSpeed = ReverseLerpSpeed * Time.deltaTime * (1000f/60f);
		    upperArm.rotation = Quaternion.Slerp(upperArm.rotation, OriginalUpperArmRotation, AnimationSpeed);
		    forearm.rotation = Quaternion.Slerp(forearm.rotation, OriginalForearmRotation, AnimationSpeed);
		    hand.rotation = Quaternion.Slerp(hand.rotation, OriginalHandRotation, AnimationSpeed);
	    }

	    public void ToggleAim() 
	    {
		    if (IsEnabled) 
		    {
			    StopAiming ();
		    }
		    else
		    {
			    StartAiming ();
		    }
	    }

	    public void StartAiming() 
	    {
		    if (!IsEnabled) 
		    {
			    //Debug.LogError ("Started aiming at: " + Time.time);
			    IsEnabled = true;
			    TimeBegun = Time.time;
                // Turn on Masking
			    //MyAnimator.IsMasking = true;
		    }
	    }

	    public void StopAiming()
	    {
		    if (IsEnabled) 
		    {
			    //Debug.LogError ("Stopped aiming at: " + Time.time);
			    IsEnabled = false;
			    StartCoroutine (StartAnimator ());
		    }
	    }

	    public bool IsAiming() 
	    {
		    if (IsEnabled && Time.time - TimeBegun > 0.5f) 
		    {
			    return true;
		    }
		    else
		    {
			    return false;
		    }
	    }

	    IEnumerator StartAnimator() 
	    {
		    IsAnimatingBackwards = true;
		    yield return new WaitForSeconds(1f);
		    if (!IsEnabled) 	// if already enabled aiming again
		    {
			    //MyAnimator.IsMasking = false;	// starts the arms animation tweening again
		    }
		    IsAnimatingBackwards = false;
	    }

	    private void BeginIK()
        {
            if (target == null || upperArm == null || elbowTarget == null)
            {
                return;
            }
            LerpSpeed = Mathf.Clamp01(LerpSpeed);
		    OriginalUpperArmRotation = upperArm.rotation;
		    OriginalForearmRotation = forearm.rotation;
		    OriginalHandRotation = hand.rotation;
		    upperArmStartRotation = upperArm.rotation;
		    forearmStartRotation = forearm.rotation;
		    handStartRotation = hand.rotation;
		    //targetRelativeStartPosition = target.position - upperArm.position;
		    elbowTargetRelativeStartPosition = elbowTarget.position - upperArm.position;
		    //create helper GOs
		    upperArmAxisCorrection = new GameObject("upperArmAxisCorrection");
		    forearmAxisCorrection = new GameObject("forearmAxisCorrection");
		    handAxisCorrection = new GameObject("handAxisCorrection");
		    //set helper hierarchy
		    upperArmAxisCorrection.transform.parent = transform;
		    forearmAxisCorrection.transform.parent = upperArmAxisCorrection.transform;
		    handAxisCorrection.transform.parent = forearmAxisCorrection.transform;
		    //guarantee first-frame update
		    lastUpperArmPosition = upperArm.position + 5*Vector3.up;
	    }
	    private void CalculateIK()
        {
            if (target == null || upperArm == null || elbowTarget == null)
            {
                targetRelativeStartPosition = Vector3.zero;
                return;
            }
            float TimeBegin = Time.realtimeSinceStartup;
		
		    if(targetRelativeStartPosition == Vector3.zero && target != null && upperArm != null)
		    {
			    targetRelativeStartPosition = target.position - upperArm.position;
		    }
			
		    if(IsOptimize &&
			    lastUpperArmPosition == upperArm.position &&
			    lastTargetPosition == target.position &&
			    lastElbowTargetPosition == elbowTarget.position) 
		    {
			    if(DebugLines) 
			    {
				    Debug.DrawLine(forearm.position, elbowTarget.position, Color.yellow);
				    Debug.DrawLine(upperArm.position, target.position, Color.red);
			    }
			
			    return;
		    }
		
		    lastUpperArmPosition = upperArm.position;
		    lastTargetPosition = target.position;
		    lastElbowTargetPosition = elbowTarget.position;
	
		    //Calculate ikAngle variable.
		    float upperArmLength = Vector3.Distance(upperArm.position, forearm.position);
		    float forearmLength = Vector3.Distance(forearm.position, hand.position);
		    float armLength = upperArmLength + forearmLength;
		    float hypotenuse = upperArmLength;
		
		    float targetDistance = Vector3.Distance(upperArm.position, target.position);	
		    targetDistance = Mathf.Min(targetDistance, armLength - 0.0001f); //Do not allow target distance be further away than the arm's length.
		
		    //var adjacent : float = (targetDistance * hypotenuse) / armLength;
		    //var adjacent : float = (Mathf.Pow(hypotenuse,2) - Mathf.Pow(forearmLength,2) + Mathf.Pow(targetDistance,2))/(2*targetDistance);
		    float adjacent = (hypotenuse*hypotenuse - forearmLength*forearmLength + targetDistance*targetDistance) /(2*targetDistance);
		
		    float ikAngle  = Mathf.Acos(adjacent/hypotenuse) * Mathf.Rad2Deg;
		
		    //Store pre-ik info.
		    Vector3 targetPosition = target.position;
		    Vector3 elbowTargetPosition = elbowTarget.position;
		
		    Transform upperArmParent = upperArm.parent;
		    Transform forearmParent = forearm.parent;
		    Transform handParent = hand.parent; 
		
		    Vector3 upperArmScale = upperArm.localScale;
		    Vector3 forearmScale = forearm.localScale;
		    Vector3 handScale = hand.localScale;
		    Vector3 upperArmLocalPosition = upperArm.localPosition;
		    Vector3 forearmLocalPosition = forearm.localPosition;
		    Vector3 handLocalPosition = hand.localPosition;
		
		    DesiredUpperArmRotation = upperArm.rotation;
		    DesiredForearmRotation = forearm.rotation;
		    DesiredHandRotation = hand.rotation;
		    //Quaternion DesiredHandLocalRotation = hand.localRotation;
		
		    //Reset arm.
		    target.position = targetRelativeStartPosition + upperArm.position;
		    elbowTarget.position = elbowTargetRelativeStartPosition + upperArm.position;
		    upperArm.rotation = upperArmStartRotation;
		    forearm.rotation = forearmStartRotation;
		    hand.rotation = handStartRotation;
		
		    //Work with temporaty game objects and align & parent them to the arm.
		    transform.position = upperArm.position;
		    transform.LookAt(targetPosition, elbowTargetPosition - transform.position);
		
		    upperArmAxisCorrection.transform.position = upperArm.position;
		    //upperArmAxisCorrection.transform.LookAt(forearm.position, transform.root.up);
		    upperArmAxisCorrection.transform.LookAt(forearm.position, upperArm.up);
		    upperArm.parent = upperArmAxisCorrection.transform;
		
		    forearmAxisCorrection.transform.position = forearm.position;
		    //forearmAxisCorrection.transform.LookAt(hand.position, transform.root.up);
		    forearmAxisCorrection.transform.LookAt(hand.position, forearm.up);
		    forearm.parent = forearmAxisCorrection.transform;
		
		    handAxisCorrection.transform.position = hand.position;
		    hand.parent = handAxisCorrection.transform;
		
		    //Reset targets
		    target.position = targetPosition;
		    elbowTarget.position = elbowTargetPosition;	
		
		    //Apply rotation for temporary game objects
		    upperArmAxisCorrection.transform.LookAt(target,elbowTarget.position - upperArmAxisCorrection.transform.position);
		
		    upperArmAxisCorrection.transform.localRotation = //.x -= ikAngle;
			    Quaternion.Euler(upperArmAxisCorrection.transform.localRotation.eulerAngles - new Vector3(ikAngle, 0, 0));
		
		    forearmAxisCorrection.transform.LookAt(target,elbowTarget.position - upperArmAxisCorrection.transform.position);
		    handAxisCorrection.transform.rotation = target.rotation;
		
		    //Restore limbs
		    upperArm.parent = upperArmParent;
		    forearm.parent = forearmParent;
		    hand.parent = handParent;
		    upperArm.localScale = upperArmScale;
		    forearm.localScale = forearmScale;
		    hand.localScale = handScale;
		    upperArm.localPosition = upperArmLocalPosition;
		    forearm.localPosition = forearmLocalPosition;
		    hand.localPosition = handLocalPosition;
			
		    //Debug.
		    if (DebugLines)
		    {
			    Debug.DrawLine(forearm.position, elbowTarget.position, Color.yellow);
			    Debug.DrawLine(upperArm.position, target.position, Color.red);
			    Debug.Log("[IK Limb] adjacent: " + adjacent);

			    Debug.LogError ("Time taken to calculate IK [" + (Time.realtimeSinceStartup - TimeBegin) + "]");
		    }
	    }
    }
}

/*switch(handRotationPolicy)
		{
			case HandRotations.KeepLocalRotation:
				hand.localRotation = handLocalRotation;
				break;
			case HandRotations.KeepGlobalRotation:
				hand.rotation = handRotation;
				break;
			case HandRotations.UseTargetRotation:
				hand.rotation = target.rotation;
				break;
		}*/

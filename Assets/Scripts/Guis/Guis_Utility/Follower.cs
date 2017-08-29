using UnityEngine;
using System.Collections;
using Zeltex.Skeletons;
using Zeltex.Characters;

/*	Use it for A gui, to simply follow a bot with lerp
 * 	
*/

namespace Zeltex.Guis
{
    /// <summary>
    /// Follows the character or anything really.
    /// </summary>
	public class Follower : MonoBehaviour 
	{
		//[Header("Options")]
        private float Speed = 1.3f;
        [SerializeField]
        private Transform TargetObject;
        [SerializeField]
        private Skeleton TargetSkeleton;
        [SerializeField]
        private Character TargetCharacter;

        private Vector3 AboveCharacterHeadOffset = new Vector3(0,0.1f,0);

        private RectTransform MyRect;
        private float AboveHeadHeight = 0;
        //private Vector3 DirectionOffset;

        #region Updates

        private void Awake()
        {
            MyRect = gameObject.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            Reposition(Time.deltaTime);
        }

        /// <summary>
        /// Reposition the gui every frame
        /// </summary>
        private void Reposition(float DeltaTime)
        {
            if (TargetObject != null)   //IsActive && 
            {
                Vector3 TargetPosition = TargetObject.transform.position + GetAboveHeadPosition();// + DirectionOffset.z * transform.forward;
                transform.position = Vector3.Lerp(transform.position, TargetPosition, DeltaTime * Speed);
            }
        }
        #endregion

        #region Initiation

        /// <summary>
        /// Sets a new target offset
        /// </summary>
        public void SetTargetOffset(Vector3 NewOffset)
        {
			AboveCharacterHeadOffset = NewOffset;
		}


        private Vector3 GetAboveHeadPosition()
        {
            if (TargetSkeleton)
            {
                AboveHeadHeight = TargetSkeleton.GetBounds().extents.y * TargetSkeleton.transform.parent.localScale.y;
                AboveHeadHeight += transform.lossyScale.y * MyRect.GetHeight() * 0.6f;
                return TargetSkeleton.GetBounds().center + new Vector3(0, AboveHeadHeight, 0);
            }
            else
            {
                return AboveCharacterHeadOffset;
            }
        }

		public void SetTarget(Transform NewTarget) 
		{
            if (NewTarget != TargetObject)
            {
                TargetObject = NewTarget;
                TargetCharacter = NewTarget.GetComponent<Character>();
                if (TargetCharacter)
                {
                    TargetSkeleton = TargetCharacter.GetSkeleton();
                }
                if (TargetObject != null)
                {
                    Reposition(1 / Speed);   // instant reposition
                }
            }
        }
        #endregion
    }
}

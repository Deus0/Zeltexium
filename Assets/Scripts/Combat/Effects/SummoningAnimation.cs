using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.AI;
using Zeltex.Characters;
using Zeltex.Skeletons;
using MakerGuiSystem;
using Zeltex.Util;

namespace Zeltex.Combat
{
    /// <summary>
    /// The class responsible for summoning. It animates the summoning movement.
    /// The Character spawns underneath the ground, inside a cube with a depth mask shader attached.
    /// To Do:
    ///     - Wait for skeleton to finish loading before I start summoning
    ///     - Make it depend on skeletons size
    /// </summary>
    public class SummoningAnimation : MonoBehaviour
    {
        #region Variables
        // Summoning Data
        private GameObject MySummonerCharacter;
        private string MySummonedName = "";
        private string MyClassName = "Minion";
        private string MyRaceName = "Human";
        // object references
        [Header("References")]
        public GameObject MySummonedObject;
        public GameObject MyDepthMask;  // 
        public GameObject MyPortal;     // portal will disapear after by shrinking
        public ParticleSystem MyParticlesTop;
        [Header("Animation Options")]
        public float OscillateSpeed = 2f;
        public float OscillateSize = 0.1f;
        // animation times
        public float AnimationTimeStage0 = 1.8f;
        public float AnimationTimeStage1 = 2.5f;
        public float AnimationTimeStage2 = 1.8f;

        private bool HasLoaded = false;
        private SkeletonHandler MySkeleton;
        // portal animation
        private float PortalDelayTime = 0.8f;
        private float PortalBottomEmissionRate = 100;
        private Vector3 PositionEnd;
        private Vector3 PortalScaleBegin;
        private Vector3 PortalBigScale;
        // time references
        private float TimeBeginStage0 = -1;
        private float TimeBeginStage1 = -1;
        private float TimeBeginStage2 = -1;
        private float TimeSinceGottenBig;
        #endregion

        #region Mono
        void Update ()
        {
            Animate();
        }
        #endregion

        #region Begin
        /// <summary>
        /// Initiates summoning variables
        /// </summary>
        private void Initiate()
        {
            // Debug.LogError("Thing: " + (Mathf.Sin(-Mathf.PI/2f)).ToString());
            //if (MySummonedObject.GetComponent<CharacterController>() != null)
            //Skeleton MySkeleton = MySummonedObject.transform.Find("Body").GetComponent<Skeleton>();
            /*if (MySkeleton)
            {
                Bounds MyBounds = MySkeleton.GetBounds();
                PortalScaleBegin = MyPortal.transform.localScale;
                MyPortal.transform.localScale = new Vector3(0, 0, 0);
                float MyHeight = MyBounds.extents.y * 2;
                MyDepthMask.transform.localScale = new Vector3(
                    MyBounds.extents.x * 2 * 1.05f,
                    MyHeight, 
                    MyBounds.extents.z * 2 * 1.05f
                    );
                //Debug.LogError("New SummoningHeight = " + MovementY);
                float PortalHeight = transform.Find("CubeMask").localScale.y;  // 0.02f;
                MyDepthMask = transform.Find("CubeMask").gameObject;
                MyDepthMask.transform.localPosition = - new Vector3(0, MyHeight / 2f, 0);  // 0.01 is the size of the portal // + PortalHeight * 2f
                PositionEnd = MyDepthMask.transform.position + new Vector3(0, MyHeight, 0);
                //PositionBegin = MySummonedObject.transform.position;
                // PositionEnd = PositionBegin + new Vector3(0, MovementY, 0);
            }*/
        }

        /// <summary>
        /// Called by the Summoner Class
        /// </summary>
        /*public void SpawnClass(GameObject MyCharacter, string NewClassName, string NewRaceName)
        {
            MySummonerCharacter = MyCharacter;
            MyClassName = NewClassName;
            MyRaceName = NewRaceName;
            Debug.Log("Summoning Minion: " + MyClassName + ":" + MyRaceName);
            BeginAnimation();
        }*/

        public void SpawnClass(string ClassName, string RaceName, GameObject MyCharacter = null)
        {
            SpawnClass(ClassName, RaceName, "", MyCharacter);
        }
        public void SpawnClass(string ClassName, string RaceName, string CharacterName = "", GameObject MyCharacter = null)
        {
            MySummonerCharacter = MyCharacter;
            MySummonedName = CharacterName;
            if (MySummonedName == "")
            {
                MySummonedName = Zeltex.NameGenerator.GenerateVoxelName();
            }
            MyClassName = ClassName;
            MyRaceName = RaceName;
            Debug.Log("Summoning Minion: " + CharacterName + ":" + MyClassName + ":" + MyRaceName);
            BeginAnimation();
        }

        /// <summary>
        /// Begins the animation process
        /// </summary>
        public void BeginAnimation()
        {
            StartCoroutine(BeginAnimationRoutine());
        }
		#endregion

		#region Spawning
		Character SummonedCharacter;
		/// <summary>
		/// Spawn the character, loads its race and class script.
		/// </summary>
		public IEnumerator BeginAnimationRoutine()
		{
			//List<string> MySkeletonScript = FileUtil.ConvertToList(Zeltex.DataManager.Get().Get("Skeletons", 0));
            //List<string> MyClassScript = FileUtil.ConvertToList(Zeltex.DataManager.Get().Get("Classes", 0));
			//if (MySkeletonScript.Count > 0)
            {
                // Spawn the Summoned object
                SummonedCharacter = CharacterManager.Get().GetPoolObject();
               //     Zeltex.NameGenerator.GenerateVoxelName(),
                //    new Vector3(-100, -100, -100),  // away from stuff 
                //    Quaternion.identity);
				if (SummonedCharacter)
				{
                    // Set layer
					MySummonedObject = SummonedCharacter.gameObject;
					//SummonedCharacter.SetLayerMaskObject(gameObject);

					if (SummonedCharacter != null)
					{
						SummonedCharacter.MySummonedCharacter = MySummonedObject;
					}
					if (MySummonedName != "")
					{
						MySummonedObject.name = MySummonedName;
					}
					//SummonedCharacter.SetMovement(false);
					Initiate();
					SummonedCharacter.DontRespawn();
					MyPortal.GetComponent<ParticleSystem>().Play();
					TimeBeginStage0 = Time.time;
					HasLoaded = false;
                    MySkeleton = SummonedCharacter.GetSkeleton();
                    SummonedCharacter.SetRace(MyRaceName);
                    //yield return MySkeleton.RunScriptRoutine(MySkeletonScript);
                    SummonedCharacter.SetClassName(MyClassName);
                    //yield return SummonedCharacter.RunScriptRoutine(MyClassScript);
					//SetRenderQue(MySummonedObject, true);
					Initiate();
					if (TimeBeginStage1 != -1)
					{
						TimeBeginStage1 = Time.time + PortalDelayTime;
						TimeSinceGottenBig = Time.time;
					}
					HasLoaded = true;
				}
				else
				{
					Debug.LogError("Cannont summon from netherworld as too many characters in scene.");
                    Destroy(gameObject);
                }
			}
			//else
            {
                //Debug.LogError("Skeleton Script null. Class Script: " + MyClassScript.Count);
                //Destroy(gameObject);
            }
            yield return null;
            //Debug.LogError("Began animation at " + TimeBeginStage0);
        }
        /// <summary>
        /// End the animation!
        /// </summary>
        void EndAnimation()
        {
			//SummonedCharacter.SetMovement(true);
            /*if (MySummonedObject.GetComponent<CharacterMapChecker>())
            {
                MySummonedObject.GetComponent<CharacterMapChecker>().enabled = true;
            }
            if (MySummonedObject.GetComponent<CharacterLimiter>())
            {
                MySummonedObject.GetComponent<CharacterLimiter>().enabled = true;
            }
            MySummonedObject.GetComponent<Character>().enabled = true;
            if (MySummonedObject.GetComponent<CharacterController>())
            {
                MySummonedObject.GetComponent<CharacterController>().enabled = true;
            }*/
            /* for (int i = 0; i < MySkeleton.MyBones.Count; i++)
             {    // add this to it's mesh objects?
                 Transform MyJoint = MySkeleton.MyBones[i].MyJointCube;
                 if (MyJoint != null && MyJoint.GetComponent<SetRenderQueue>() != null)
                 {
                     Destroy(MyJoint.GetComponent<SetRenderQueue>());
                 }
                 Transform MyBodyCube = MySkeleton.MyBones[i].BodyCube;
                 if (MyBodyCube != null && MyBodyCube.GetComponent<SetRenderQueue>() != null)
                 {
                     Destroy(MyBodyCube.GetComponent<SetRenderQueue>());
                 }
                 Transform MyMesh = MySkeleton.MyBones[i].VoxelMesh;
                 if (MyMesh != null && MyMesh.GetComponent<SetRenderQueue>() != null)
                 {
                     Destroy(MyMesh.GetComponent<SetRenderQueue>());
                 }
             }*/
            //SetRenderQue(MySummonedObject, false);
            //MySummonedObject.GetComponent<Bot>().WasSummoned(MySummonerCharacter);
            StartCoroutine(NetworkDestroy(2f));
        }
        /// <summary>
        /// Render que sets the shader properties
        /// </summary>
        /*private void SetRenderQue(GameObject MyCharacter, bool IsRenderQue)
        {
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            {    // add this to it's mesh objects?
                Transform MyJoint = MySkeleton.MyBones[i].MyJointCube;
                if (MyJoint != null)
                {
                    if (IsRenderQue)
                    {
                        MyJoint.gameObject.AddComponent<SetRenderQueue>();
                    }
                    else
                    {
                        Destroy(MyJoint.GetComponent<SetRenderQueue>());
                    }
                }
                Transform MyBodyCube = MySkeleton.MyBones[i].BodyCube;
                if (MyBodyCube != null)
                {
                    if (IsRenderQue)
                    {
                        MyBodyCube.gameObject.AddComponent<SetRenderQueue>();
                    }
                    else
                    {
                        Destroy(MyBodyCube.GetComponent<SetRenderQueue>());
                    }
                }
                Transform MyMesh = MySkeleton.MyBones[i].VoxelMesh;
                if (MyMesh != null)
                {
                    if (IsRenderQue)
                    {
                        MyMesh.gameObject.AddComponent<SetRenderQueue>();
                    }
                    else
                    {
                        Destroy(MyMesh.GetComponent<SetRenderQueue>());
                    }
                }
            }
        }*/

        IEnumerator NetworkDestroy(float TimeDelay)
        {
            yield return new WaitForSeconds(TimeDelay);
            Destroy(gameObject);
        }
        #endregion

        #region Animation
        /// <summary>
        /// Goes through 3 stages of the portal animation
        /// </summary>
        void Animate()
        {
            if (MySummonedObject != null && MyDepthMask != null)
            {
                MyDepthMask.transform.rotation = MySummonedObject.transform.rotation;
            }
            // Stage 0
            if (TimeBeginStage0 != -1)
            {
                OpenPortal(Time.time - TimeBeginStage0);
            }
            // in between animating
            if (TimeBeginStage1 != -1 || (TimeBeginStage2 != -1 && (Time.time - TimeBeginStage2) <= 0))
            {
                OscillatePortal();
            }
            // Stage 1 - cube comes out of the portal
            if (HasLoaded && TimeBeginStage1 != -1)
            {
                float TimePassed = (Time.time - TimeBeginStage1);
                if (TimePassed >= 0)
                {
                    LeavePortal(TimePassed);
                }
            }
            // Stage 2  - portal closes
            if (TimeBeginStage2 != -1)
            {
                float TimePassed = (Time.time - TimeBeginStage2);
                if (TimePassed >= 0)
                {
                    ClosePortal(TimePassed);
                }
            }
        }

        private void OpenPortal(float TimePassed)
        {
            MyPortal.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), PortalScaleBegin, TimePassed / AnimationTimeStage0);
            MyPortal.GetComponent<ParticleSystem>().emissionRate = PortalBottomEmissionRate * (TimePassed / AnimationTimeStage0);
            if (TimePassed >= AnimationTimeStage0)
            {
                TimeBeginStage0 = -1;
                TimeBeginStage1 = Time.time + PortalDelayTime;
                TimeSinceGottenBig = Time.time;
            }
        }

        private void OscillatePortal()
        {
            float TimePassed = Time.time - TimeSinceGottenBig;
            MyPortal.transform.localScale = PortalScaleBegin + OscillateSize * (new Vector3(
                1f + Mathf.Sin(TimePassed * OscillateSpeed - Mathf.PI / 2f),
                 0,
                1f + Mathf.Sin(TimePassed * OscillateSpeed - Mathf.PI / 2f)
             ));
            PortalBigScale = MyPortal.transform.localScale;
        }

        private void ClosePortal(float TimePassed)
        {
            if (MyPortal)
                MyPortal.transform.localScale = Vector3.Lerp(PortalBigScale, new Vector3(0, 0, 0), TimePassed / AnimationTimeStage2);
            if (MyDepthMask)
                MyDepthMask.transform.localScale = new Vector3(0, 0, 0);// Vector3.Lerp(OriginalDepthMaskScale, new Vector3(0, 0, 0), TimePassed / AnimationTimeStage2);
            if (TimePassed >= AnimationTimeStage2)
            {
                TimeBeginStage2 = -1;
                if (MyPortal)
                {
                    Destroy(MyPortal, 5f);
                    Destroy(MyDepthMask);
                }
                EndAnimation();
            }
        }

        private void LeavePortal(float TimePassed)
        {
            if (!MyParticlesTop.isPlaying)
                MyParticlesTop.Play();
            float AnimationTime = TimePassed / AnimationTimeStage1; // between 0 and 1
            MySummonedObject.transform.position = Vector3.Lerp(MyDepthMask.transform.position, PositionEnd, AnimationTime);
            MyPortal.GetComponent<ParticleSystem>().emissionRate = PortalBottomEmissionRate - PortalBottomEmissionRate * AnimationTime;
            MyParticlesTop.emissionRate = PortalBottomEmissionRate - PortalBottomEmissionRate * AnimationTime;
            if (TimePassed >= AnimationTimeStage1)
            {
                //if (MyDepthMask)
                //    Destroy(MyDepthMask);
                TimeBeginStage1 = -1;
                TimeBeginStage2 = Time.time + PortalDelayTime;
                MyParticlesTop.emissionRate = 0;
                MyPortal.GetComponent<ParticleSystem>().Stop();
                MyParticlesTop.Stop();
            }
        }
        #endregion
    }

}

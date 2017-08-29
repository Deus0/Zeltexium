using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Skeletons;
using Zeltex.Characters;
using Zeltex.Guis;

namespace Zeltex.Combat 
{
    /// <summary>
    /// Used for character to shoot bullets.
    /// Uses BulletData class for properties of the bullets.
    /// To Do: 
    ///     - Charge Beam
    ///     - Shoot out of but for acceleration into air
    /// </summary>
    public class Shooter : Skill
    {
        #region Variables
        private GameObject BulletPrefab;
        [Header("Debug")]
        [SerializeField]
        private bool IsDebug;
        // privates
        public Transform HotSpotTransform;
        private IKLimb MyIKHelper;
        private float LastFired = 0f;
        static Transform BulletsParent;
        #endregion

        #region Debug

        public void Awake()
        {
            BulletPrefab = BulletManager.Get().GetPrefab(0);
        }

        private void Update()
        {
            if (IsDebug)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
            }
        }
        #endregion

        #region Initialization

        public Spell GetSpell()
        {
            return Data;
        }
        /// <summary>
        /// When initialized, it will set the spell
        /// This is set from skillbar, inside the character
        /// </summary>
        public void SetSpell(Spell MySpell_)
        {
            LastFired = Time.time;
            MyCharacter = GetComponent<Character>();
            if (HotSpotTransform == null && MyCharacter && MyCharacter.GetSkeleton())
            {
                HotSpotTransform = MyCharacter.GetSkeleton().GetCameraBone();
            }
            Transform MyIKLimb = transform.Find("IKHelper");
            if (MyIKLimb)
            {
                MyIKHelper = MyIKLimb.GetComponent<IKLimb>();
            }
            if (MySpell_ != null)
            {
                Data = MySpell_.Clone();
                Data.MyCharacter = MyCharacter;
                if (MyIdentity == null)
                    MyIdentity = GetComponent<NetworkIdentity>();

                if (MyIdentity)
                {
                    Data.NetworkID = MyIdentity.netId;
                }
                else
                {
                    Debug.LogError(name + " not getting network id");
                }
            }
        }
        
        override public void ActivateOnNetwork()   // sheild checks enery and sends negative state on activate
        {
            //Debug.LogError ("Activating Summoning");
            //if (isLocalPlayer)
            {
                if (NewState && HasEnergy())
                {
                    Shoot();
                }
            }
        }

        public void Activate3()
        {
            if (Data != null && Data.FireOnHold)
            {
                Shoot();
            }
        }
        #endregion

        #region IK

        /// <summary>
        /// Stops the aiming.
        /// </summary>
        public void StopAiming()
        {
            if (MyIKHelper)
            {
                MyIKHelper.StopAiming();
            }
        }
        #endregion

        #region Shooting

        /// <summary>
        /// If skill has enough energy, launch bullet
        /// </summary>
        private void Shoot() 
		{
			//Debug.LogError(name + " is shooting!");
			if (HotSpotTransform != null)
            {
                if (MyIKHelper)
                {
                    if (MyIKHelper && !MyIKHelper.IsAiming())
                    {
                        MyIKHelper.StartAiming();
                        return; // wait until aiming has completed
                    }
                }
                float TimePassed = Time.time - LastFired;
                if (Data != null && TimePassed >= Data.FireRate)
                {
                    if (HasEnergy())
                    {
                        LastFired = Time.time;
                        // get shooting direction
                        UseEnergy();
                        //CreateBullet();
                        if (isServer)
                        {
                            ServerCreateBullet(GetSpawnPosition(), HotSpotTransform.rotation, 
                                new Vector3(+Random.Range(-Data.Randomness, Data.Randomness), +Random.Range(-Data.Randomness, Data.Randomness), +Random.Range(-Data.Randomness, Data.Randomness)));
                        }
                        else
                        {
                            ClientCreateBullet(GetSpawnPosition(), HotSpotTransform.rotation,
                                new Vector3(+Random.Range(-Data.Randomness, Data.Randomness), +Random.Range(-Data.Randomness, Data.Randomness), +Random.Range(-Data.Randomness, Data.Randomness)));
                        }
                    }
                    else
                    {
                        if (Data != null)
                        {
                            Debug.Log(name + " needs more " + Data.StatUseName + " to fire.");
                        }
                        else
                        {
                            Debug.LogError(name + " has a null spell");
                        }
                    }
                }
                else
                {
                    //Debug.LogError(name + " needs to wait longer.");
                }
            }
            else
            {
                Debug.LogError("HotSpotTransform is null inside: " + name);
                return;
            }
		}

        private Vector3 GetSpawnPosition()
        {
            Vector3 BulletSpawnPosition = HotSpotTransform.position + HotSpotTransform.forward * 0.2f;
            if (HotSpotTransform.GetComponent<MeshRenderer>())
            {
                Bounds HotspotBounds = HotSpotTransform.GetComponent<MeshRenderer>().bounds;
                float HeadDistance = (HotspotBounds.extents.z) * HotSpotTransform.lossyScale.z + Bullet.ConvertSize(Data.Size).z * 1.5f;// (HotspotBounds.center.z + HotspotBounds.extents.z) * HotSpotTransform.lossyScale.z ;
                //Debug.LogWarning("HeadDistance: " + HeadDistance + ": extentsZ: " + HotspotBounds.extents.z +":scaleZ: " + HotSpotTransform.lossyScale.z);  // + Bullet.ConvertSize(MySpell.Size).z * 1.01f
                //Debug.DrawLine(HotSpotTransform.position, HotSpotTransform.position + HotSpotTransform.forward * HeadDistance * 2f, Color.red, 5f);
                BulletSpawnPosition = HotSpotTransform.position + HotSpotTransform.forward * HeadDistance;//.position + BulletDirection * HeadDistance;  // should be added from the bounds of the bullet
            }
            return BulletSpawnPosition;
        }

        [Client] // called only on client
        public void ClientCreateBullet(Vector3 SpawnPosition, Quaternion SpawnRotation, Vector3 RandomForce)
        {
            LogManager.Get().Log(("Client: Creating Bullet on " + name + " -isLocalPlayer? " + isLocalPlayer + "- Has Authority? " + GetComponent<NetworkIdentity>().hasAuthority), "Bullets");
            GetComponent<Zeltex.Networking.Player>().CmdCreateBullet(SpawnPosition, SpawnRotation, RandomForce);
        }

        /// <summary>
        /// Spawns the bullet on the network
        /// This gets called only on the server
        /// </summary>
        [Server]
        public void ServerCreateBullet(Vector3 SpawnPosition, Quaternion SpawnRotation, Vector3 RandomForce)
        {
            LogManager.Get().Log(("Creating Bullet on " + name + " on server? " + isServer), "Bullets");
            ClientScene.RegisterPrefab(BulletPrefab);
            GameObject MyBullet = Instantiate(BulletPrefab, SpawnPosition, SpawnRotation);
            NetworkServer.Spawn(MyBullet);
            MyBullet.gameObject.GetComponent<Bullet>().RpcInitialize(
                GetComponent<NetworkIdentity>(), RandomForce);
        }
        #endregion
    }
}

// old code
//protected new string SkillName = "Shooter";
//[Header("Options")]
//public bool IsShootFacingDirection = false;
//public bool IsReverseDirection = false;
//public bool IsShootFacingDirection2 = false;
//public bool IsChargeMode = false;	// hold down left click to charge up a bullet

/*
if (IsShootFacingDirection)
{
    BulletDirection = HotSpotTransform.forward;
}
else if (IsReverseDirection)
{
    BulletDirection.y = 0;
    if (transform.localScale.x < 0)
        BulletDirection.x = 1;
    else
        BulletDirection.x = -1;
                        }
 * // escape thrusters
if (Time.time - LastBoost >= BoostRate) {
	LastBoost = Time.time;
	if (CrossPlatformInputManager.GetButton ("Fire2")) {
		Vector2 BulletDirection = new Vector2 (0, 0);
		if (transform.localScale.x < 0)
			BulletDirection.x = 1;
		else
			BulletDirection.x = -1;
		Vector3 BulletOffset = new Vector3 (-MyRect.lossyScale.x / 3f, 0, 0);
		BulletOffset = transform.parent.TransformDirection(BulletOffset);
		CreateBullet (BulletDirection, BulletOffset, BulletForce, Size, BoostLifeTime, BoostPrefab);
	}
	// Jetpack physics
	if (CrossPlatformInputManager.GetButton ("Fire3")) {
		Vector3 BulletOffset = new Vector3 (0, -Mathf.Abs (MyRect.lossyScale.x) / 3f, 0);
		BulletOffset = transform.parent.TransformDirection(BulletOffset);
		Vector2 BulletDirection = new Vector2 (0, -1);
		Vector3 GravityDirection = gameObject.GetComponent<ArtificialGravity>().GravityForce.normalized;
		BulletDirection = new Vector2(GravityDirection.x, 
		                              GravityDirection.y);
		BulletOffset = Mathf.Abs (MyRect.lossyScale.x/3f)*( new Vector3(GravityDirection.x/2f, GravityDirection.y,0));
		CreateBullet (BulletDirection, BulletOffset, BulletForce, Size, BoostLifeTime, RocketBoostPrefab);
	}
}*/

/*Transform FindChildBone(Transform ParentBone)
{
    return FindChildBone(ParentBone, "Hand(mirrored)");
}
Transform FindChildBone(Transform ParentBone, string SeekingBone) 
{
    for (int i = 0; i < ParentBone.childCount; i++)
    {
        Transform LeChild = ParentBone.GetChild (i);
        if (LeChild.name == SeekingBone)
            return LeChild;
        Transform PossibleBone = FindChildBone (LeChild);
        if (PossibleBone != null)
            return PossibleBone;

    }
    return null;	// if not found
}*/

//Debug.LogError ("Creating new bullet at: " + Time.time);
/*if (IsShootFacingDirection2) 
{
    BulletDirection = HotSpotTransform.forward*BulletDirection.z - HotSpotTransform.right*BulletDirection.x + HotSpotTransform.up*BulletDirection.y;
    BulletDirection.Normalize();
}*/

/*BulletDirection.y = 0;
        if (transform.localScale.x < 0)
            BulletDirection.x = -1;
        else
            BulletDirection.x = 1;
        */
/*Vector3 TransformedBulletOffset = new Vector3(transform.lossyScale.x*BulletOffset.x, 
                                              transform.lossyScale.y*BulletOffset.y,
                                              transform.lossyScale.z*BulletOffset.z);*/
//MyRect.lossyScale.x * BulletOffset2
//TransformedBulletOffset = HotSpotTransform.TransformDirection(TransformedBulletOffset);
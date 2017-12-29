using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Zeltex.Characters;
using Zeltex.AI;
using Zeltex.AnimationUtilities;
using Zeltex.Voxels;

namespace Zeltex.Combat 
{
    /// <summary>
    /// Bullet is the main projectile in the game.
    /// </summary>
	public class Bullet : NetworkBehaviour 
	{
        #region Variables
        //private PhotonView MyPhoton;
		[SerializeField]
        private Spell Data = new Spell();
		AudioSource MySource;
		bool IsUsed = false;	// has hit character
		bool HasHitTerrain = false;
		[Header("Options")]
		public float DissapearingTime = 0.1f;
		private float TimeDied;
		private static float MaxDissapearTime = 30f;
        private GameObject StuckToObject;
		private Vector3 StuckToObjectDifference;
		[Header("Special Effects")]
		public GameObject ExplosionEffectPrefab;
        //public float ExplosionSize = 1f;
        public Font PopupFont;
        public Material PopupMaterialDamage;
        public Material PopupMaterialHeal;
        private bool HasExploded = false;
        private Vector3 OriginalForce;
        #endregion

        #region Mono
        void Start()
        {
            MySource = gameObject.AddComponent<AudioSource>();
        }

        void Update()
        {
            StickToObject();
            SeekTarget();
        }
        #endregion

        #region GettersAndSetters

        /// <summary>
        /// Has bullet been used up
        /// </summary>
        public bool HasUsed() 
		{
			return IsUsed;
		}

        /// <summary>
        /// Get character that spawned the bullet
        /// </summary>
		public Character GetSpawner() 
		{
			return Data.MyCharacter;
        }

        /// <summary>
        /// Setting Spell Data
        /// </summary>
        [ClientRpc]
        public void RpcInitialize(NetworkIdentity CharacterID, Vector3 RandomForce)
        {
            //GameObject MyCharacter = NetworkServer.FindLocalObject(CharacterID);
            GameObject MyCharacter = CharacterID.gameObject;
            Initialize(MyCharacter, RandomForce);
        }

        public void Initialize(GameObject MyCharacter, Vector3 RandomForce)
        {
            if (MyCharacter)
            {
                LogManager.Get().Log("Initializing bullet at " + Time.time, "Bullets");
                //Shooter MyShooter = MyCharacter.GetComponent<Shooter>();
               // if (MyShooter)
                {
                    Data = MyCharacter.GetComponent<Character>().GetSkillbar().GetSelectedSpell();
                    transform.SetParent(BulletManager.Get().transform);
                    LayerManager.Get().SetLayerBullet(gameObject);
                    gameObject.name = "Bullet" + Random.Range(1, 100000);
                    Rigidbody MyRigid = GetComponent<Rigidbody>();
                    OriginalForce = (new Vector3(transform.forward.x,
                                                    transform.forward.y + Random.Range(-Data.Randomness, Data.Randomness),
                                                    transform.forward.z + Random.Range(-Data.Randomness, Data.Randomness)
                                                    ) * Data.BulletForce);
                    MyRigid.AddForce(OriginalForce);
                    //MyRigid.isKinematic = true;
                    transform.localScale = ConvertSize(Data.Size);
                    gameObject.GetComponent<MeshRenderer>().material.color = Data.ColorTint;
                    gameObject.GetComponent<TrailRenderer>().material.color = Data.ColorTint;
                    Zeltex.Sounds.SoundManager.CreateNewSound(transform.position, Data.GetSpawnSound(), Data.GetSoundVolume());
                    StartCoroutine(FindTarget());
                    StartCoroutine(ImplodeInTime(Data.LifeTime));
                }
               // else
                {
                    //Debug.LogError(MyCharacter.name + " does not have a shooter script");
                   // Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogError(name + " spawned bullet has a missing character. Can not exist.");
                Destroy(gameObject);
            }
        }

        public static Vector3 ConvertSize(float Size)
        {
            return new Vector3(1, 1, 1) * 0.06f * Size;
        }
        #endregion

        #region Seek
        private Transform MyTarget;
        private IEnumerator FindTarget()
        {
            if (Data.IsSeek)
            {
                yield return new WaitForSeconds(Data.SeekDelay);
                Character[] MyCharacters = GameObject.FindObjectsOfType<Character>();
                float ClosestDistance = Data.SeekDistance;
                for (int i = 0; i < MyCharacters.Length; i++)
                {
                    CharacterStats MyStats = MyCharacters[i].GetComponent<CharacterStats>();
                    if (MyStats.IsDead() == false && Data.MyCharacter != MyCharacters[i].gameObject)
                    {
                        float ThisDistance = Vector3.Distance(transform.position, MyCharacters[i].transform.position);
                        if (ThisDistance <= ClosestDistance)
                        {
                            MyTarget = MyCharacters[i].transform;
                            ClosestDistance = ThisDistance;
                        }
                    }
                }
            }
        }
        private void SeekTarget()
        {
            if (Data.IsSeek && MyTarget)
            {
                gameObject.GetComponent<Rigidbody>().AddForce((MyTarget.position - transform.position).normalized * 5f);
            }
        }
        #endregion
        // The different methods to destroy the bullet
        #region Destroy

        /// <summary>
        /// When the bullet hits the ground
        /// </summary>
        void BulletDeath(GameObject HitObject, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (Data.IsExplosion)
            {
                Explode(HitObject);
            }
            else if (Data.IsStick)
            {
                StickToObject(HitObject, HitPosition, HitNormal);
            }
            else
            {
                Implode();
            }
        }

        /// <summary>
        /// Explodes the bullet upon hit
        /// </summary>
        public void Explode(GameObject MyHitObject)
        {
            if (!HasExploded)
            {
                HasExploded = true;
                //Debug.LogError("Exploding at time " + Time.time);
                DestroyInTime(DissapearingTime);
                CreateExplosionEffect(MyHitObject);

            }
        }

        /// <summary>
        /// Implosion is like explosion except it just goes inwards
        /// </summary>
        public void Implode()
        {
            if (!HasExploded)
            {
                HasExploded = true;
                DestroyInTime(DissapearingTime);
                Zeltex.Sounds.SoundManager.CreateNewSound(transform.position, Data.GetImplodeSound(), Data.GetSoundVolume());
                CreateExplosionEffect(null, transform.localScale);
            }
        }

        /// <summary>
        /// Create a prefab for the effect
        /// </summary>
        private void CreateExplosionEffect(GameObject MyWorld)
        {
            Vector3 ExplosionSize = new Vector3(
                Data.ExplosionSize * transform.localScale.x * 2f,
                Data.ExplosionSize * transform.localScale.y * 2f,
                Data.ExplosionSize * transform.localScale.z * 2f
                );
            CreateExplosionEffect(MyWorld, ExplosionSize);
        }

        private void CreateExplosionEffect(GameObject MyWorld, Vector3 ExplosionSize)
        {
            if (ExplosionEffectPrefab)
            {
                GameObject MyExplosion = (GameObject)Instantiate(ExplosionEffectPrefab, transform.position, transform.rotation);
                MyExplosion.transform.SetParent(BulletManager.Get().transform);
                MyExplosion.transform.localScale = transform.localScale;
                MyExplosion.GetComponent<AnimateSize>().Begin(ExplosionSize);
                MyExplosion.GetComponent<MeshRenderer>().material.color = Data.ColorTint;

                //Debug.LogError("ExplosionSize:" + MyExplosion.GetComponent<AnimateSize>().MaxSize.ToString());
                if (Data.IsDestroyTerrainOnHit)
                    if (MyWorld)
                    {
                        if (MyWorld.GetComponent<Chunk>())
                        {
                            MyWorld = MyWorld.GetComponent<Chunk>().GetWorld().gameObject;
                            MyExplosion.GetComponent<AnimateSize>().SetWorld(MyWorld);
                        }
                    }
            }
        }

        /// <summary>
        /// Needs to be destroyed in time, from its original creation, since bullets have to be limited.
        /// </summary>
        public void DestroyInTime(float InTime) 
		{
           // if (PhotonNetwork.connected == false || MyPhoton.owner == PhotonNetwork.player)
            {
                StartCoroutine(DestroyInTime2(InTime));	// if i dont do this, they never get destroyed on the server lol
            }
		}

		IEnumerator DestroyInTime2(float LifeTime)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<MeshCollider>().enabled = false;
            yield return new WaitForSeconds(LifeTime);
            Destroy(gameObject);
        }

        IEnumerator ImplodeInTime(float LifeTime)
        {
            yield return new WaitForSeconds(LifeTime);
            Implode();
        }
        #endregion

        #region Stick
        void StickToObject(GameObject HitObject, Vector3 HitPosition, Vector3 HitNormal)
        {
            TimeDied = Time.time;
            StuckToObject = HitObject;
            StuckToObjectDifference = transform.position - HitObject.transform.position - HitNormal * 0.01f;// ;
            //StuckToObjectDifference += gameObject.GetComponent<Rigidbody>().velocity.normalized * 0.2f;
            StuckToObjectDifference += OriginalForce.normalized * 0.01f;
            //Debug.DrawLine(transform.position, transform.position + gameObject.GetComponent<Rigidbody>().velocity.normalized, Color.red, 5);
            //Debug.DrawLine(transform.position, transform.position + OriginalForce.normalized, Color.red, 5);
            //StuckToObjectDifference *= 0.8f;
            //StuckToObjectDifference += HitNormal * 0.2f;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            // turn off colliders
            if (gameObject.GetComponent<MeshCollider>())
            {
                gameObject.GetComponent<MeshCollider>().enabled = false;
            }
        }
        /// <summary>
        /// When sticking to an object, keep position on it
        /// </summary>
        void StickToObject()
        {
            if (StuckToObject)
            {
                transform.position = StuckToObject.transform.position + StuckToObjectDifference;
                float TimePassed = Time.time - TimeDied;
                if (TimePassed >= Data.StickTime && !HasExploded)
                {
                    Explode(StuckToObject);
                }
            }
        }
        #endregion

        #region Collision

        /// <summary>
        /// Finds the root character from a bone
        /// </summary>
        GameObject FindRootCharacter(GameObject MyBone)
        {
            if (MyBone == null)
            {
                return null;
            }
            else if(MyBone.GetComponent<Character>() != null)
            {
                return MyBone;
            }
            else
            {
                if (MyBone.transform.parent == null)
                {
                    return null;
                }
                else
                {
                    return FindRootCharacter(MyBone.transform.parent.gameObject);
                }
            }
        }

        /// <summary>
        /// When collision with something
        /// </summary>
        void OnCollisionEnter(Collision collision) 
		{
			if (Data.MyCharacter != null && collision.gameObject == Data.MyCharacter.gameObject)
            {
                return;
            }
            if (!IsUsed)
            {
                IsUsed = true;
                // if collide with character
                if (collision.gameObject.layer == LayerMask.NameToLayer("Skeleton"))
                {
                    GameObject CharacterObject = FindRootCharacter(collision.gameObject);
                    if (CharacterObject)
                    {
                        BulletCollideWithCharacter(CharacterObject, transform.position, collision.contacts[0].normal);
                    }
                }
                else if (collision.gameObject.GetComponent<Character>())
                {
                    BulletCollideWithCharacter(collision.gameObject, transform.position, collision.contacts[0].normal);// collision.contacts[0].point);
                    return;
                }
                //else if (!HasHitTerrain)
                else if (collision.gameObject.GetComponent<Chunk>() || collision.gameObject.tag == "World")
                {
                    //Chunk MyChunk = collision.gameObject.GetComponent<Chunk>();
                    Zeltex.Sounds.SoundManager.CreateNewSound(transform.position, Data.GetHitTerrainSound(), Data.GetSoundVolume());
                    //Vector3 VelNormal = gameObject.GetComponent<Rigidbody>().velocity.normalized*MyChunk.transform.lossyScale.x/10f;
                    //gameObject.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0,50,0), collision.contacts[0].point);
                    // destroy part of the chunk
                    //Debug.Log("Summoning Minion1");
                    if (Data.IsSummon)
                    {
                        SummonMinion(collision.contacts[0].point, collision.contacts[0].normal);
                    }
                    if (Data.IsPortal)
                    {
                        // Create a portal
                        if (collision.contacts[0].normal == Vector3.forward)
                        {
                            // Create a portal at most suitable position - along the  block where i shot
                        }
                    }
                    BulletDeath(collision.gameObject, transform.position, collision.contacts[0].normal);
                }
                else if (collision.gameObject.tag == "Bone")
                {
                    BulletDeath(collision.gameObject, transform.position, collision.contacts[0].normal);
                }
                else
                {
                    Bullet HitBullet = collision.gameObject.GetComponent<Bullet>();
                    if (HitBullet)
                    {
                        BulletDeath(collision.gameObject, transform.position, collision.contacts[0].normal);
                        HitBullet.BulletDeath(gameObject, transform.position, collision.contacts[0].normal);
                    }
                }
            }
        }

        /// <summary>
        /// Called when a bullet collides with a character
        /// </summary>
        public void BulletCollideWithCharacter(GameObject HitCharacter, Vector3 MyHitPosition, Vector3 HitNormal)
        {
            OnHitCharacter(HitCharacter, MyHitPosition, HitNormal);
        }

        /// <summary>
        /// The network synching of the bullet hits
        /// </summary>
        /*public void BulletCollideWithCharacterNetwork(int CharacterID, Vector3 MyHitPosition, Vector3 HitNormal)
        {
            PhotonView MyCharacterView = PhotonView.Find(CharacterID);
            if (MyCharacterView)
                OnHitCharacter(MyCharacterView.gameObject, MyHitPosition, HitNormal);
        }*/

        /// <summary>
        /// When the bullet hits a character
        /// </summary>
        public void OnHitCharacter(GameObject HitObject, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (Data != null && Data.MyCharacter && HitObject && HitObject != Data.MyCharacter)
            {
                NetworkInstanceId HitNetID = HitObject.GetComponent<NetworkIdentity>().netId;
                if (HitNetID.Value == Data.NetworkID.Value)
                {
                    return;
                }
                Character HitCharacter = HitObject.GetComponent<Character>();
                Bot HitBot = HitObject.GetComponent<Bot>();
                if (HitCharacter)
                {
                    if (HitCharacter.IsAlive())
                    {
                        Zeltex.Sounds.SoundManager.CreateNewSound(HitPosition, Data.GetHitCharacterSound(), Data.GetSoundVolume());
                        // Decrease health, mana, or heal them
                        if (Data.IsAddStat)
                        {
                            float VariedValue = Data.AddStatValue + Random.Range(-Data.AddStatVariance, Data.AddStatVariance);
                            HitCharacter.GetStats().AddStatState(Data.AddStatName, VariedValue);
                            string MyValueString = (Mathf.RoundToInt(VariedValue * 100) / 100f).ToString();
                            if (VariedValue > 0)
                            {
                                StatPopUp.CreateTextPopup(transform.position, MyValueString, PopupFont, PopupMaterialHeal.color);     // Popups
                            }
                            else
                            {
                                StatPopUp.CreateTextPopup(transform.position, MyValueString, PopupFont, PopupMaterialDamage.color);     // Popups
                            }
                        }

                        HitCharacter.LastHitNormal = HitNormal;
                        // Add effect like burning
                        if (Data.OnHitEffect != null && Data.OnHitEffect.Name != "")
                        {
                            HitCharacter.GetData().MyStatsHandler.AddStat(Data.OnHitEffect);
                        }
                        
                        if (HitBot && Data.MyCharacter)
                        {
                            HitBot.WasHit(Data.MyCharacter);
                        }
                        // Check for applied status
                        if (Data.AddStatName == "Health")
                        {
                            if (!HitCharacter.IsAlive())
                            {
                            }
                        }

                        if (Data.DoesDestroyOnHitCharacters)
                        {
                            BulletDeath(HitObject, HitPosition, HitNormal);
                        }
                        else
                        {
                            // Add Bounce Force
                        }

                        if (!HitCharacter.IsAlive())
                        {
                            Data.MyCharacter.KilledCharacter(HitObject);
                        }
                        return;
                    }
                    else
                    {
                        Zeltex.Sounds.SoundManager.CreateNewSound(HitPosition, Data.GetHitTerrainSound(), Data.GetSoundVolume());
                        //MySource.PlayOneShot(Data.GetHitTerrainSound());
                    }
                }
            }
            else
            {
                IsUsed = false; // not used yet!
            }
        }
        #endregion

        #region Summoning

        [SerializeField]
        private GameObject SummoningAnimationPrefab;

        /// <summary>
        /// Summon the minion at a point
        /// </summary>
        private void SummonMinion(Vector3 CollisionPoint, Vector3 Normal)
        {
            if (SummoningAnimationPrefab)
            {
                GameObject NewSpawner = Instantiate(SummoningAnimationPrefab, CollisionPoint, Quaternion.identity);
                SummoningAnimation MyAnimation = NewSpawner.GetComponent<SummoningAnimation>();
                MyAnimation.SpawnClass(Data.MinionClass, Data.MinionRace, Data.MinionName, Data.MyCharacter.gameObject);
            }
            else
            {
                Debug.LogError("Bullet not linked up properly in bullet prefab.");
            }
        }
        #endregion
    }
}

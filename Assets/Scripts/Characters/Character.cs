using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Events;
using Zeltex.AI;
using Zeltex.Items;
using Zeltex.Voxels;
using Zeltex.WorldUtilities;
using Zeltex.Skeletons;
using Zeltex.Game;
using Zeltex.Dialogue;
using Zeltex.Guis;
using Zeltex.Combat;
using Zeltex.Util;
using Zeltex.Quests;
using Zeltex.Physics;

namespace Zeltex.Characters
{
    /// <summary>
    /// Character class 
    /// Attached to every moving object that has interactions with items and the world.
    ///     NPC's class
    ///     Contains movement State
    ///     Raytrace for other characters and quest items
    /// To Do:
    ///     - Manage the updating better. Only update when data updates
    ///     
    ///     - Seperate out interaction part of the character
    ///     
    /// </summary>
    [ExecuteInEditMode]
	public class Character : NetworkBehaviour
    {
        #region Variables
        [Header("Actions")]
        [SerializeField]
		private EditorAction ActionSaveToLevel = new EditorAction();
		[SerializeField]
		private EditorAction ActionImportVox = new EditorAction();
		[SerializeField]
		private EditorAction PushVoxToDataManager = new EditorAction();
		[SerializeField]
		private Transform ActionBone = null;

        [Header("Data")]
        [SerializeField]
        private CharacterData Data = new CharacterData();
        // The level the character is loaded in

        [Header("Character")]
        public bool IsPlayer;
        private int KillCount = 0;
        [HideInInspector]
        public GameObject MySummonedCharacter;

        // Raycasting
        private static float RaycastRange = 5;
        [HideInInspector]
        public Vector3 LastHitNormal;
        [HideInInspector]
        public UnityEvent OnEndTalkEvent;
        [HideInInspector]
        public EventObject OnReturnToPool;

        // Components
        private DialogueHandler MyDialogueHandler;
        private Skillbar MySkillbar;
        private SkeletonHandler MySkeleton;
        private Bot MyBot;
        private Rigidbody MyRigidbody;
        //private BasicController MyController;
        private Mover MyMover;
        private CapsuleCollider MyCollider;

        // Saving?
        private bool HasInitialized;
        private Zeltine DeathHandle;
        #endregion

        #region Mono
        public Vector3 GetForwardDirection()
        {
            if (Data.MySkeleton.GetCameraBone())
            {
                return Data.MySkeleton.GetCameraBone().transform.forward;
            }
            else
            {
                return transform.forward;
            }
        }
        public CharacterData GetData()
        {
            return Data;
        }

        public void SetGuisActive(bool NewState)
        {
            Data.MyGuis.SetGuisActive(NewState);
        }

        public void ToggleGuis()
        {
            Data.MyGuis.ToggleGuis();
        }

        /// <summary>
        /// Teleports the character and any attached guis
        /// </summary>
        public void Teleport(Vector3 NewPosition)
        {
            Vector3 DifferencePosition = NewPosition - transform.position;
            transform.position = NewPosition;
            for (int i = 0; i < Data.MyGuis.GetSize(); i++)
            {
                ZelGui MyZelGui = Data.MyGuis.GetZelGui(i);
                if (MyZelGui)
                {
                    MyZelGui.transform.position += DifferencePosition;
                }
            }
        }

        private void OnGUI()
        {
            Data.MyStatsHandler.OnGUI();
        }

        /*private void Awake()
        {
            if (gameObject.activeSelf)
            {
                Initialize(true);
            }
        }*/

        private void Update()
        {
            if (Application.isPlaying)
            {
                Data.MyStatsHandler.SetCharacter(this);
                Data.MyStatsHandler.UpdateScript();
            }
            if (ActionSaveToLevel.IsTriggered())
            {
                Data.InLevel.SaveCharacterToLevel(this, "", true);
            }
			if (ActionImportVox.IsTriggered()) 
			{
				ImportVoxToBone(ActionBone);
			}
			if (PushVoxToDataManager.IsTriggered())
			{
				PushBoneVoxelModel(ActionBone);
			}
            if (Data.MyStats != null && Data.MyStatsHandler.ActionLoadStats.IsTriggered())
            {
                string LoadStatsName = Data.MyStatsHandler.ActionStatsName;
                Stats DataStats = DataManager.Get().GetElement(DataFolderNames.StatGroups, 0) as Stats;
                if (DataStats != null)
                {
                    Data.MyStats = Data.MyStats.Load(DataStats.GetSerial(), typeof(Stats)) as Stats;
                }
                else
                {
                    Debug.LogError("Cannot find: " + LoadStatsName);
                }
            }
            if (Data.MyGuis != null)
            {
                if (Application.isPlaying == false)
                {
                    Data.MyGuis.SetCharacter(this);
                }
                Data.MyGuis.Update();
            }
		}

		private void PushBoneVoxelModel(Transform ActionBone) 
		{
			ActionBone = GetActionBone(ActionBone);
			if (ActionBone != null)
			{
				Bone MyBone = MySkeleton.GetSkeleton().GetBone(ActionBone);
				if (MyBone != null)
				{
					VoxelModel MyModel = MyBone.GetVoxelModel();
					if (MyModel != null)
					{
						DataManager.Get().PushElement(DataFolderNames.VoxelModels, MyModel);
					}
					else
					{
						Debug.LogError("Model is null. Bone has no mesh: " + MyBone.Name);
					}
				}
			}
		}

		private Transform GetActionBone(Transform ActionBone) 
		{
			if (ActionBone == null && GetSkeleton().GetSkeleton().MyBones.Count > 0)
			{
				ActionBone = GetSkeleton().GetSkeleton().MyBones[0].MyTransform;
			}
			return ActionBone;
		}

		private void ImportVoxToBone(Transform ActionBone) 
		{
			ActionBone = GetActionBone(ActionBone);
			if (ActionBone != null)
			{
				Bone MyBone = MySkeleton.GetSkeleton().GetBone(ActionBone);
				if (MyBone != null)
				{
					World BoneWorld = MyBone.VoxelMesh.gameObject.GetComponent<World>();
					RoutineManager.Get().StartCoroutine(DataManager.Get().LoadVoxFile(BoneWorld));
					MyBone.MeshName = BoneWorld.name;
					MyBone.OnModified();
					GetSkeleton().GetSkeleton().OnModified();
					GetData().OnModified();
					// Also push to datamanager, to make sure it can be reloaded again
					VoxelModel MyModel = MyBone.GetVoxelModel();
					DataManager.Get().PushElement(DataFolderNames.VoxelModels, MyModel);
				}
				else
				{
					Debug.LogError(name + " bone is null.");
				}
			}
			else
			{
				Debug.LogError(name + " Has no bones.");
			}
		}

        public void ForceInitialize()
        {
            Data.MySkeleton.ForceStopLoad();
            StopAllCoroutines();
            Initialize(true);
        }

        public void SetPlayer(bool NewState)
        {
            IsPlayer = NewState;
            MyMover.IsPlayer = NewState;
            if (MyBot)
            {
                MyBot.enabled = !IsPlayer;
            }
        }

        /// <summary>
        /// Once character is loaded into a world
        /// </summary>
        public void OnActivated()
        {
            World MyWorld = Data.GetInWorld();
            if (MyWorld == null)
            {
                //Debug.LogError(name + " has a null world, so cannot position.");
                return;
            }
            //Debug.LogError("[" + name + "] has been loaded into world of [" + MyWorld.name + "]");
            // Scale its position - for now until i fixed the current levels stuff
            //transform.position = new Vector3(transform.position.x * MyWorld.GetUnit().x, transform.position.y * MyWorld.GetUnit().y, transform.position.z * MyWorld.GetUnit().z);
            if (GetSkeleton() != null && GetSkeleton().GetSkeleton() != null)
            {
                Bounds MyBounds = GetSkeleton().GetSkeleton().GetBounds();
                int MaxChecks = 100;
                int ChecksCount = 0;
                transform.position = new Vector3(transform.position.x, transform.position.y - MyBounds.extents.y + MyBounds.center.y, transform.position.z);
                Vector3 VoxelPosition = MyWorld.RealToBlockPosition(transform.position).GetVector();
                // find new voxel position that is air or non block
                while (true)
                {
                    Voxel MyVoxel = MyWorld.GetVoxel(VoxelPosition.ToInt3());
                    if (MyVoxel == null)
                    {
                        break;
                    }
                    int VoxelType = MyVoxel.GetVoxelType();
                    if (VoxelType == 0)
                    {
                        break;
                    }
                    VoxelMeta MyMeta = MyWorld.GetVoxelMeta(VoxelType);
                    if (MyMeta == null)
                    {
                        break;
                    }
                    if (MyMeta.ModelID != "Block")
                    {
                        break;
                    }
                    VoxelPosition.y++;
                    ChecksCount++;
                    if (ChecksCount >= MaxChecks)
                    {
                        Debug.LogError("Could not find a new position for " + name);
                        break;
                    }
                }
                // Convert position to real world
                VoxelPosition = MyWorld.BlockToRealPosition(VoxelPosition);// new Vector3(VoxelPosition.x * MyWorld.GetUnit().x, VoxelPosition.y * MyWorld.GetUnit().y, VoxelPosition.z * MyWorld.GetUnit().z);
                LogManager.Get().Log("[" + name + "] has bounds of [" + MyBounds.center.ToString() + " - " + MyBounds.size.ToString() + "] at position [" + VoxelPosition.ToString() + "]", "CharacterLoading");
                transform.position = new Vector3(VoxelPosition.x, VoxelPosition.y + MyBounds.extents.y - MyBounds.center.y - MyWorld.GetUnit().y / 2f, VoxelPosition.z);
            }
            else
            {
                Debug.LogError("No bounds in skeleton of: " + name);
            }
        }


        public void SetData(CharacterData NewData, Level MyLevel = null, bool IsClone = true, bool IsSpawnGuis = true)
        {
            RoutineManager.Get().StartCoroutine(SetDataRoutine(NewData, MyLevel, IsClone, IsSpawnGuis));
        }

        public IEnumerator SetDataRoutine(CharacterData NewData, Level MyLevel = null, bool IsClone = true, bool IsSpawnGuis = true, bool IsActivateSkeleton = true)
        {
            if (Data != NewData && NewData != null)
            {
                if (IsClone)
                {
                    Data = NewData.Clone<CharacterData>();
                    if (Data == null)
                    {
                        Debug.LogError("Cloned data is null");
                        yield break;
                    }
                }
                else
                {
                    Data = NewData;
                }
                Data.OnInitialized();
                name = Data.Name;
                RefreshComponents();
                Data.SetCharacter(this);
                Data.MyStatsHandler.SetCharacter(this);
                Data.MyQuestLog.Initialise(this);
                Data.SetInLevel(MyLevel);
                //InLevel = MyLevel;
                if (MyLevel != null)
                {
                    transform.position = Data.LevelPosition;
                    transform.eulerAngles = Data.LevelRotation;
                    Data.SetWorld(MyLevel.GetWorld());
                    Data.RefreshTransform(true);
                    MyLevel.AddCharacter(this);
                }
                //MySkeleton.gameObject.SetActive(false);
                MyMover.IsPlayer = IsPlayer;
                MySkeleton.SetSkeletonData(Data.MySkeleton);
                if (!IsActivateSkeleton)
                {
                    SetMovement(false);
                }
                if (IsPlayer == false)
                {
                    if (MyBot == null)
                    {
                        MyBot = gameObject.AddComponent<Bot>();
                        GetComponent<Mover>().SetBot(MyBot);
                    }
                }
                // Set in chunk
                // Unhide Skeleton
                //MySkeleton.gameObject.SetActive(true);
                if (IsActivateSkeleton)
                {
                    Data.MyGuis.SetCharacter(this, false);
                    yield return ActivateCharacter();
                    //yield return MySkeleton.GetSkeleton().ActivateRoutine();
                    // MyMover.SetCameraBone(GetCameraBone());
                }
                else
                {
                    Data.MyGuis.SetCharacter(this, IsSpawnGuis);
                    if (IsSpawnGuis)
                    {
                        RoutineManager.Get().StartCoroutine(SetGuiStatesAfter());
                    }
                }
            }
            else if (NewData == null)
            {
                Debug.LogError("New Data is null inside " + name);
            }
        }

        /// <summary>
        /// Activates a character after a chunk finishes building
        /// </summary>
        public IEnumerator ActivateCharacter()
        {
            yield return MySkeleton.GetSkeleton().ActivateRoutine();
            if (GetComponent<Shooter>())
            {
                GetComponent<Shooter>().SetCameraBone(GetCameraBone());
            }
            MyMover.SetCameraBone(GetCameraBone());
            Data.MyGuis.SpawnAllGuis();
            OnActivated();  // Fix position
            RoutineManager.Get().StartCoroutine(SetGuiStatesAfter());
            SetMovement(true);
        }

        /// <summary>
        /// Has to wait for spawning of guis before i set them
        /// </summary>
        private IEnumerator SetGuiStatesAfter() 
        {
            for (int i = 0; i < 300; i++)
            {
                yield return null;
            }
            Data.MyGuis.SetGuiStates();
        }
        /// <summary>
        ///  Called after spawning a character by CharacterManager
        ///  Set on the network to ensure data updated accross all machines
        ///  -only updates on the players character - when updating a character into a controlled one
        /// </summary>
        public void Initialize(bool IsForce = false)    //string Name = "Character"
        {
            if (!HasInitialized || IsForce)
                //&& CharacterManager.Get() && gameObject.activeInHierarchy)
            {
                HasInitialized = true;
                /*if (Name == "Character")
                {
                    Name = "Character_" + Random.Range(1, 10000);
                }*/
                //name = Name;
                LogManager.Get().Log("Initialised character: " + name, "Characters");
                RefreshComponents();
                Data.MyStatsHandler.SetCharacter(this);
                Data.MyQuestLog.Initialise(this);
                Data.MyGuis.SetCharacter(this);

                // spawn guis 
                //if (LayerManager.Get())
                {
                    //LayerManager.Get().SetLayerCharacter(gameObject);
                }
                // Default character to load!
                //if (Data.Class == "" && DataManager.Get())
                {
                    //LogManager.Get().Log("Loading new class and skeleton for character: " + name + " - " + 0, "Characters");
                    //RunScript(Zeltex.Util.FileUtil.ConvertToList(DataManager.Get().Get(DataFolderNames.Classes, 0)));
                    //GetSkeleton().RunScript(Zeltex.Util.FileUtil.ConvertToList(DataManager.Get().Get(DataFolderNames.Skeletons, 0)));
                }
                //CharacterManager.Get().Register(this);
            }
        }

        private void OnEnable()
        {
            if (MyCollider)
            {
                MyCollider.enabled = true;
            }
        }
        #endregion

        #region Combat

        public bool IsAlive()
        {
            if (Data.MyStats != null)
            {
                return !Data.MyStatsHandler.IsDead();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Sets the character to not respawn, using the character respawner class
        /// </summary>
        public void DontRespawn()
        {
            Data.CanRespawn = false;
        }

        /// <summary>
        /// Called when stats health reaches 0
        ///     -Disable Bot and Player Controls
        /// Need to add reviving functionality
        /// </summary>
        public void OnDeath(GameObject MyCharacter = null)
        {
            if (MyCharacter == null)
            {
                MyCharacter = gameObject;   // killed yourself
            }

            if (Application.isPlaying)
            {
                StartCoroutine(DeathRoutine());
            }
            else
            {
                if (DeathHandle == null)
                {
                    //UniversalCoroutine.CoroutineManager.StopCoroutine(DeathHandle);
                    DeathHandle = RoutineManager.Get().StartCoroutine(DeathRoutine());
                }
                else
                {
                    Debug.LogError(name + " cannot die as already dying?");
                }
            }
        }

        public IEnumerator DeathRoutine()
        {
            //Debug.LogError(name + " Has started dying.");
            float DeathTime = 6 + Random.Range(0,8);
            /*if (IsPlayer)
            {
                DeathTime = 13;
            }*/
            yield return null;
            if (MyBot)
            {
                MyBot.StopFollowing();
            }
            if (IsPlayer)
            {
                CameraManager.Get().GetMainCamera().GetComponent<Player>().SetMouse(false);
            }
            Data.MyGuis.SaveStates();
            Data.MyGuis.HideAll();
            MySkillbar.OnDeath();

            if (MyBot)
            {
                MyBot.Disable();
            }

            SetMovement(false);
            // Apply Animations on death - fade effect
            if (MySkeleton != null)
            {
                Zanimator MyAnimator = MySkeleton.GetComponent<Zanimator>();
                if (MyAnimator)
                {
                    MyAnimator.Stop();
                }
                Ragdoll MyRagDoll = MySkeleton.GetComponent<Ragdoll>();
                if (MyRagDoll)
                {
                    MyRagDoll.RagDoll();
                }
                MySkeleton.GetSkeleton().DestroyBodyCubes();
            }
            else
            {
                Debug.LogError(name + " Has no skeleton");
            }

            // burnt, sliced, crushed, chocolified, decapitated, exploded
            if (!Data.CanRespawn)
            {
                gameObject.name += "'s Corpse"; // burnt, sliced, crushed, chocolified, decapitated, exploded
            }
            float ReviveTime = 10;
            if (IsPlayer)
            {
                System.Action<ZelGui> OnFinishSpawning = (RespawnZelGui) =>
                {
                    if (RespawnZelGui)
                    {
                        RespawnZelGui.TurnOn();     // turn gui on when reviving begins
                        RespawnGui MyRespawner = RespawnZelGui.GetComponent<RespawnGui>();
                        if (MyRespawner)
                        {
                            StartCoroutine(MyRespawner.CountDown(() => { GetGuis().GetZelGui("RespawnGui").TurnOff(); }, (int)(ReviveTime + DeathTime)));
                        }
                        else
                        {
                            Debug.LogError("Respawn gui doesn't have respawn gui.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Could not find RespawnGui in guis for: " + name);
                    }
                    if (RespawnZelGui)
                    {
                        RespawnZelGui.gameObject.SetActive(true);     // turn gui on when reviving begins
                    }
                };
                GetGuis().Spawn("RespawnGui", OnFinishSpawning);
            }
            float TimeBeginRespawn = Time.time;
            while (Time.time - TimeBeginRespawn < DeathTime)
            {
                yield return null;
            }
            // if (Data.CanRespawn)
            {
                yield return RoutineManager.Get().StartCoroutine(Respawn(ReviveTime));
            }
            /*else
            {
                if (IsPlayer)
                {
                    Camera.main.gameObject.GetComponent<Player>().RemoveCharacter();
                }
                Data.MyGuis.Clear();
                CharacterManager.Get().ReturnObject(this);
            }*/
            DeathHandle = null;
        }

        private IEnumerator Respawn(float ReviveTime)
        {
            yield return null;
            MySkeleton.GetComponent<Ragdoll>().ReverseRagdoll(ReviveTime);
            float TimeStarted = Time.time;
            while (Time.time - TimeStarted <= ReviveTime)
            {
                yield return null;
            }
            yield return null;
            Debug.Log("Reviving Character [" + name + "].");
            Data.MyGuis.RestoreStates();
            Data.MyStatsHandler.RestoreFullHealth();
            SetMovement(true);
            if (IsPlayer)
            {
                CameraManager.Get().GetMainCamera().transform.localPosition = Vector3.zero;
                CameraManager.Get().GetMainCamera().transform.localRotation = Quaternion.identity;
                CameraManager.Get().GetMainCamera().GetComponent<Player>().SetMouse(true);
            }
            else
            {
                if (MyBot)
                {
                    MyBot.EnableBot();
                }
            }
            MySkillbar.SetItem(-1);
            MySkillbar.SetItem(0);
            DeathHandle = null;
        }

        public bool StopDeath()
        {
            if (DeathHandle != null)
            {
                RoutineManager.Get().StopCoroutine(DeathHandle);
                DeathHandle = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void KilledCharacter(GameObject DeadCharacter)
        {
            Debug.Log(name + " has killed " + DeadCharacter.name);
            AddScore(1);
            Data.MyStatsHandler.AddExperience(1);
        }
        #endregion

        #region GameModeScores

        // Add To Log Class - used to log events
        // quick score system, put it somewhere else later
        public void AddScore(int Addition)
        {
            KillCount += Addition;
            OnScoreChange();
        }

        public void SetScore(int NewScore)
        {
            KillCount = NewScore;
        }

        public int GetScore()
        {
            return KillCount;
        }

        void OnScoreChange()
        {
            if (GameMode.Get())
            {
                GameMode.Get().CheckKillCondition(GetScore());
            }
        }
        #endregion
        
        #region Collision

        /// <summary>
        /// When player collides with something
        /// </summary>
        void OnTriggerEnter(Collider MyCollider)
        {
            ItemHandler MyItem = MyCollider.gameObject.GetComponent<ItemHandler>();
            if (MyItem)
            {
                MyItem.OnContact(gameObject);
            }
            Teleporter MyTeleporter = MyCollider.gameObject.GetComponent<Teleporter>();
            if (MyTeleporter)
            {
                MyTeleporter.OnContact(gameObject);
            }
        }

        #endregion

        #region RayTrace

        public void RayTrace(int SelectionType = 0)
        {
            Debug.Log(name + " is raytracing " + SelectionType);
            if (IsRayHitGui() == false && MySkeleton)
            {
                RayTraceSelections(Data.MySkeleton.MyCameraBone, SelectionType);
            }
            else
            {
                Debug.LogError(name + " could not interact with anything.");
            }
        }

        private bool RayTraceSelections(Transform CameraBone, int SelectionType)
		{
			RaycastHit MyHit;
			if (UnityEngine.Physics.Raycast(CameraBone.position, CameraBone.forward, out MyHit, RaycastRange, LayerManager.Get().GetInteractLayer()))
			{
                Debug.Log(name + " has interacted with: " + MyHit.collider.gameObject.name);

                if (LayerManager.AreLayersEqual(LayerManager.Get().GetItemsLayer(), MyHit.collider.gameObject.layer))
                {
                    ItemHandler HitItemHandler = MyHit.collider.gameObject.GetComponent<ItemHandler>();
                    if (HitItemHandler != null)
                    {
                        Debug.Log(name + " has picked up item: " + HitItemHandler.name);
                        return PickupItem(HitItemHandler, SelectionType);
                    }
                    Chunk ItemChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                    if (ItemChunk)
                    {
                        ItemHandler HitItemHandler2 = ItemChunk.transform.parent.gameObject.GetComponent<ItemHandler>();
                        if (HitItemHandler2 != null)
                        {
                            Debug.Log(name + " has picked up Voxel Item: " + HitItemHandler2.name);
                            return PickupItem(HitItemHandler2, SelectionType);
                        }
                    }
                    Debug.Log(name + " Hit a items layer without item object");
                }
                else if (LayerManager.AreLayersEqual(LayerManager.Get().GetSkeletonLayer(), MyHit.collider.gameObject.layer))
                {
                    Character MyCharacter = MyHit.collider.transform.FindRootCharacter();
                    if (MyCharacter)
                    {
                        Debug.Log(name + " Hit a character [" + MyCharacter.name + "]");
                        //Character HitCharacter = MyCharacter.GetComponent<Character>();// MyHit.collider.gameObject.GetComponent<Character>();
                        Debug.Log(name + " has talked to character: " + MyCharacter.name);
                        return TalkToCharacter(MyCharacter, SelectionType);
                    }
                    else
                    {
                        Debug.Log(name + " Hit a characters layer without character root.");
                    }
                }
                else if (LayerManager.AreLayersEqual(LayerManager.Get().GetWorldsLayer(), MyHit.collider.gameObject.layer))
                {
                    /*Door MyDoor = MyHit.collider.gameObject.GetComponent<Door>();
                    if (MyDoor)
                    {
                        //Debug.Log("Toggling door!");
                        Debug.Log(name + " has toggled door: " + MyDoor.name);
                        MyDoor.ToggleDoor();
                        return true;
                    }*/

                    Vector3 BlockPosition = World.RayHitToBlockPosition(MyHit);
                    World MyWorld = RayHitToWorld(MyHit);
                    if (MyWorld != null)
                    {
                        Debug.Log(name + " Hit a world [" + MyWorld.name + "]");
                        Voxel MyVoxel = MyWorld.GetVoxel(new Int3(BlockPosition));
                        //VoxelManager MetaData = MyWorld.MyDataBase;
                        if (MyVoxel != null)// && MyVoxel.GetVoxelType() >= 0 && MyVoxel.GetVoxelType() < MetaData.Data.Count)
                        {
                            // Get the meta data from the index
                            VoxelMeta MyMeta = MyWorld.GetVoxelMeta(MyVoxel.GetVoxelType());// MetaData.Data[MyVoxel.GetVoxelType()];
                            if (MyMeta != null && MyHit.collider != null)
                            {
                                //Debug.LogError("Activated Voxel: " + MyMeta.Name + ": " + MyMeta.IsMultipleBlockModel() + ":" + BlockPosition.ToString());
                                Chunk MyChunk = MyHit.collider.GetComponent<Chunk>();// MetaData, 
                                MyMeta.CharacterActivate(this, MyChunk, BlockPosition, MyVoxel);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Debug.Log(name + " Hit a worlds layer without world component.");
                    }
                }
                else
                {
                    Debug.Log(name + " Has not selected anything");
                }
            }
            else
            {
                Debug.Log(name + " Has not selected anything");
            }
            return false;
        }

        public static World RayHitToWorld(RaycastHit MyHit)
        {
            World MyWorld;
            Chunk HitChunk = MyHit.collider.GetComponent<Chunk>();
            if (HitChunk)
            {
                MyWorld = HitChunk.GetWorld();
            }
            else
            {
                MyWorld = MyHit.collider.GetComponent<World>();
            }
            return MyWorld;
        }

        /// <summary>
        /// Checks to see if the mouse is hitting the gui
        /// </summary>
        public static bool IsRayHitGui() 
		{
			var pointer = new PointerEventData(EventSystem.current);
			pointer.position = (Input.mousePosition);
			List<RaycastResult> raycastResults = new List<RaycastResult>();
            if (EventSystem.current)
            {
                EventSystem.current.RaycastAll(pointer, raycastResults);
                if (raycastResults.Count > 0)
                {
                    return true;
                }
            }
			return false;
		}

		/// <summary>
        /// Dialogue begins!
        /// </summary>
		public bool TalkToCharacter(Character OtherCharacter, int InteractType) 
		{
            if (OtherCharacter.GetData().MyDialogue.GetSize() > 0)
            {
                System.Action<ZelGui> OnFinishSpawning = (MyDialogueGui) =>
                {
                    if (MyDialogueGui)
                    {
                        Debug.Log(name + " Is Talking to" + OtherCharacter.name + " with " +
                            OtherCharacter.GetData().MyDialogue.GetSize() + " dialogue sections.");
                        OnBeginTalk(OtherCharacter);
                        OtherCharacter.OnBeginTalk(this);
                        MyDialogueGui.TurnOn();
                        DialogueHandler MyCharacterDialogue = MyDialogueGui.GetComponent<DialogueHandler>();
                        MyCharacterDialogue.MyTree = OtherCharacter.GetData().MyDialogue;    // set gui tree as talked to characters dialogue
                        MyCharacterDialogue.SetCharacters(this, OtherCharacter);
                        //MyCharacterDialogue.OtherCharacter = OtherCharacter;
                        MyCharacterDialogue.OnConfirm();//begin the talk
                    }
                    else
                    {
                        Debug.LogError(name + " does not have Dialogue Gui");
                    }
                };
                Data.MyGuis.Spawn("Dialogue", OnFinishSpawning);
            }
            else
            {
                Debug.Log(name + " cannot talk to " + OtherCharacter.name + " due to 0 size.");
            }
            return true;
		}

        /// <summary>
        /// Called on the character
        /// </summary>
        public void OnBeginTalk(Character Character2)
        {
            // Hide all guis
            this.Data.MyGuis.SaveStates();
            this.Data.MyGuis.HideAll();
            if (MyBot)
            {
                MyBot.FollowTarget(Character2.gameObject);
            }
            this.OnEndTalkEvent.SetEvent(OnEndTalk);
            SetMovement(false);

            if (IsPlayer && CameraManager.Get() && CameraManager.Get().GetMainCamera())
            {
                Player MyPlayer = CameraManager.Get().GetMainCamera().GetComponent<Player>();
                if (MyPlayer)
                {
                    MyPlayer.SetFreeze(true);
                }
            }
        }

        public void OnEndTalk()
        {
            SetMovement(true);
            if (IsPlayer && CameraManager.Get() && CameraManager.Get().GetMainCamera())
            {
                Player MyPlayer = CameraManager.Get().GetMainCamera().GetComponent<Player>();
                if (MyPlayer)
                {
                    MyPlayer.SetFreeze(false);
                }
            }
            Data.MyGuis.RestoreStates();
            Bot OtherBot = gameObject.GetComponent<Bot>();
            if (OtherBot && OtherBot.enabled)
            {
                OtherBot.Wander();
            }
            ZelGui MyDialogueGui = Data.MyGuis.GetZelGui("Dialogue");
            if (MyDialogueGui)
            {
                MyDialogueGui.TurnOff();
            }
        }

        /// <summary>
        /// When activate button pressed on item object
        /// </summary>
		public bool PickupItem(ItemHandler HitItemHandler, int InteractType) 
		{
			if (InteractType == 0)
			{
                    // passes in a ray object for any kind of picking action
                    // does things like destroy, activates the special function
                    HitItemHandler.CharacterPickup(this);
			}
            else if (InteractType == 1)
            {
                HitItemHandler.ToggleGui();
            }
			return true;
        }
        #endregion

        #region Controls

        /// <summary>
        /// Enable and disable movement
        /// </summary>
        public void SetMovement(bool NewState)
        {
            if (MyRigidbody)
            {
                MyRigidbody.isKinematic = !NewState;
            }
            else
            {
                Debug.LogError(name + " has no rigidbody..");
            }
            if (MyMover)
            {
                MyMover.enabled = NewState;
            }
            else
            {
                Debug.LogError(name + " has no Movement..");
            }
            enabled = NewState;
        }
        #endregion


        #region Utility

        public bool CanRespawn()
        {
            return Data.CanRespawn;
        }

        public Stats GetStats()
        {
            return Data.MyStats;
        }
        public Guis.Characters.CharacterGuis GetGuis()
        {
            return Data.MyGuis;
        }
        /// <summary>
        /// Sets new race name and sets it to nonunique
        /// </summary>
        public void SetRace(string NewRaceName)
        {
            Data.Race = NewRaceName;
            //IsStaticRace = !string.IsNullOrEmpty(Data.Race);    // sets true if not null string
        }
        /// <summary>
        /// A race name used for cosmetic purposes
        /// Saves the characters race uniquelly
        /// </summary>
        public void SetRaceUnique(string NewRaceName)
        {
            Data.Race = NewRaceName;
            //IsStaticRace = false;
        }

        private void SetClass(string NewClassName)
        {
            Data.Class = NewClassName;
            //IsStaticClass = !string.IsNullOrEmpty(Data.Class);
        }

        /// <summary>
        /// Sets the guis target
        /// </summary>
        public void SetGuisTarget(Transform NewTarget)
        {
            for (int i = 0; i < Data.MyGuis.GetSize(); i++)
            {
                Transform MyGui = Data.MyGuis.GetZelGui(i).transform;
                if (MyGui.GetComponent<Orbitor>())
                {
                    MyGui.GetComponent<Orbitor>().SetTarget(NewTarget);
                }
                if (MyGui.GetComponent<Billboard>())
                {
                    MyGui.GetComponent<Billboard>().SetTarget(NewTarget);
                }
            }
        }

        /// <summary>
        /// Returns players inventory
        /// </summary>
        public Inventory GetSkillbarItems()
        {
            return Data.Skillbar;
        }

        public Inventory GetBackpackItems()
        {
            return Data.Backpack;
        }

        public Inventory GetEquipment()
        {
            return Data.GetEquipment();
        }

        public QuestLog GetQuestLog()
        {
            return Data.MyQuestLog;
        }

        /// <summary>
        /// Gets a characters skeleton
        /// </summary>
        public SkeletonHandler GetSkeleton()
        {
            if (MySkeleton == null)
            {
                Debug.LogError("Set Skeleton Handler in Editor: " + name);
            }
            return MySkeleton;
        }

        public Transform GetCameraBone()
        {
            if (MySkeleton)
            {
                return Data.MySkeleton.GetCameraBone();
            }
            else
            {
                return transform;
            }
        }

        /// <summary>
        ///  Refresh all the references to components and datamanager
        /// </summary>
        private void RefreshComponents()
        {
            if (MyBot == null)
            {
                MyBot = GetComponent<Bot>();
                if (MyBot == null)
                {
                    MyBot = gameObject.AddComponent<Bot>();
                }
            }
            if (MyRigidbody == null)
            {
                MyRigidbody = GetComponent<Rigidbody>();
                if (MyRigidbody == null)
                {
                    MyRigidbody = gameObject.AddComponent<Rigidbody>();
                }
            }
            if (MyMover == null)
            {
                MyMover = GetComponent<Mover>();
                if (MyMover == null)
                {
                    MyMover = gameObject.AddComponent<Mover>();
                }
            }
            if (MyDialogueHandler == null)
            {
                MyDialogueHandler = gameObject.GetComponent<DialogueHandler>();
            }
            if (MyCollider == null)
            {
                MyCollider = GetComponent<CapsuleCollider>();
                if (MyCollider == null)
                {
                    MyCollider = gameObject.AddComponent<CapsuleCollider>();
                }
            }
            if (MySkillbar == null)
            {
                MySkillbar = GetComponent<Skillbar>();
                if (MySkillbar == null)
                {
                    MySkillbar = gameObject.AddComponent<Skillbar>();
                }
            }
            if (MySkeleton == null)
            {
                MySkeleton = transform.GetComponentInChildren<SkeletonHandler>();
            }
            if (MySkeleton == null)
            {
                Transform BodyTransform = null;
                if (BodyTransform == null)
                {
                    Debug.Log("Creating body for character: " + name);
                    GameObject NewBody = new GameObject();
                    NewBody.transform.SetParent(transform);
                    NewBody.transform.localPosition = Vector3.zero;
                    NewBody.transform.eulerAngles = Vector3.zero;
                    NewBody.name = "Body";
                    MySkeleton = NewBody.AddComponent<SkeletonHandler>();
                }
            }
        }

        public Skillbar GetSkillbar()
        {
            return MySkillbar;
        }
        #endregion

        #region Naming

        public void UpdateName(string NewName)
        {
            transform.name = NewName;
        }
        #endregion

        /// <summary>
        /// Clears character data
        /// </summary>
        private void Clear()
        {
            Data.Clear();
        }

        public List<string> GetStatistics()
        {
            List<string> MyData = new List<string>();
            MyData.Add("Character [" + name + "]\n" +
                "   Stats: " + Data.MyStats.GetSize() + "\n" +
                "   Skillbar Items: " + GetSkillbarItems().GetSize() + "\n" +
                "   Backpack Items: " + GetBackpackItems().GetSize() + "\n" +
                "   Quests: " + Data.MyQuestLog.GetSize() + "\n" +
                "   Dialogue: " + MyDialogueHandler.MyTree.GetSize() + "\n" +
                "   Skeleton: " + Data.MySkeleton.MyBones.Count + "\n");

            MyData.Add("   Equipment: " + Data.Equipment.GetSize() + "\n");
            return MyData;
        }

        #region Positioning 
        public World GetInWorld()
        {
            return Data.GetInWorld();
        }

        public Int3 GetChunkPosition()
        {
            return Data.GetChunkPosition();
        }

        public Chunk GetInChunk()
        {
            return Data.GetInChunk();
        }

        /// <summary>
        /// Sets a new world for a character
        /// </summary>
        public void SetWorld(World NewWorld)
        {
            Data.SetWorld(NewWorld);
        }

        #endregion

    }
}
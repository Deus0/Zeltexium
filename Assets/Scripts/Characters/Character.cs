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
using Zeltex.Guis.Characters;
using Zeltex.Combat;
using Zeltex.Util;
using Zeltex.Quests;

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
	public partial class Character : NetworkBehaviour
    {
        #region Variables
        [Header("Actions")]
        [SerializeField]
        private EditorAction ActionLoad = new EditorAction();

        [Header("Data")]
        [SerializeField]
        private CharacterData Data = new CharacterData();
        public CharacterGuis MyGuis = new CharacterGuis();

        [Header("Character")]
        public bool IsPlayer;
        private int KillCount = 0;
        public GameObject MySummonedCharacter;

        [Header("Interact")]
        [SerializeField]
        private LayerMask InteractLayer;
        [SerializeField]
        private LayerMask ItemsLayer;
        [SerializeField]
        private LayerMask WorldsLayer;
        [SerializeField]
        private LayerMask CharacterLayer;
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
        private Skeleton MySkeleton;
        private Bot MyBot;
        private Rigidbody MyRigidbody;
        private BasicController MyController;
        private CharacterMapChecker MyCharacterMapChecker;
        private CapsuleCollider MyCollider;

        private bool HasInitialized;
        #endregion

        #region Mono
        public CharacterData GetData()
        {
            return Data;
        }

        public void SetData(CharacterData NewData)
        {
            Data = NewData;
        }

        public void SetGuisActive(bool NewState)
        {
            MyGuis.SetGuisActive(NewState);
        }

        public void ToggleGuis()
        {
            MyGuis.ToggleGuis();
        }

        /// <summary>
        /// Teleports the character and any attached guis
        /// </summary>
        public void Teleport(Vector3 NewPosition)
        {
            Vector3 DifferencePosition = NewPosition - transform.position;
            transform.position = NewPosition;
            for (int i = 0; i < MyGuis.GetSize(); i++)
            {
                ZelGui MyZelGui = MyGuis.GetZelGui(i);
                if (MyZelGui)
                {
                    MyZelGui.transform.position += DifferencePosition;
                }
            }
        }

        private void OnGUI()
        {
            Data.MyStats.OnGUI();
        }

        private void Awake()
        {
            Data.MyStats.SetCharacter(transform);
        }

        private void Update()
        {
            /*if (HasInitialized == false)
            {
                Initialize();
            }*/
            if (Application.isPlaying)
            {
                Data.MyStats.SetCharacter(transform);
                Data.MyStats.UpdateScript();
            }
            if (ActionLoad.IsTriggered())
            {

            }
            if (Data.MyStats != null && Data.MyStats.ActionLoadStats.IsTriggered())
            {
                string LoadStatsName = Data.MyStats.ActionStatsName;
                Stats DataStats = DataManager.Get().GetElement(DataFolderNames.StatGroups, 0) as Stats;
                if (DataStats != null)
                {
                    Data.MyStats = Data.MyStats.Load(DataStats.GetSerial(), typeof(CharacterStats)) as CharacterStats;
                }
                else
                {
                    Debug.LogError("Cannot find: " + LoadStatsName);
                }
            }
            if (MyGuis != null)
            {
                MyGuis.SetCharacter(this);
                MyGuis.Update();
            }
        }

        public void ForceInitialize()
        {
            HasInitialized = false;
            Data.Class = "";
            MySkeleton.SkeletonName = "";
            MySkeleton.ForceStopLoad();
            StopAllCoroutines();
            Initialize();
        }

        /// <summary>
        ///  Called after spawning a character by CharacterManager
        ///  Set on the network to ensure data updated accross all machines
        ///  -only updates on the players character - when updating a character into a controlled one
        /// </summary>
        public void Initialize(string Name = "Character")
        {
            if (!HasInitialized)
                //&& CharacterManager.Get() && gameObject.activeInHierarchy)
            {
                HasInitialized = true;
                if (Name == "Character")
                {
                    Name = "Character_" + Random.Range(1, 10000);
                }
                name = Name;
                LogManager.Get().Log("Initialised character: " + name, "Characters");
                RefreshComponents();
                Data.MyStats.SetCharacter(transform);
                if (Data.MyStats != null)
                {
                    Data.MyStats.OnDeath.RemoveEvent<GameObject>(OnDeath);
                    Data.MyStats.OnDeath.AddEvent<GameObject>(OnDeath);
                }
                // spawn guis 
                MyGuis.SetCharacter(this);
                if (LayerManager.Get())
                {
                    LayerManager.Get().SetLayerCharacter(gameObject);
                }
                // Default character to load!
                if (Data.Class == "" && DataManager.Get())
                {
                    RefreshComponents();
                    LogManager.Get().Log("Loading new class and skeleton for character: " + name + " - " + 0, "Characters");
                    //RunScript(Zeltex.Util.FileUtil.ConvertToList(DataManager.Get().Get(DataFolderNames.Classes, 0)));
                    GetSkeleton().RunScript(Zeltex.Util.FileUtil.ConvertToList(DataManager.Get().Get(DataFolderNames.Skeletons, 0)));
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
                return !Data.MyStats.IsDead();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Add in auto revive
        /// </summary>
        public void AddRespawning()
        {
            // first auto revive
            if (CharacterRespawner.Get())
            {
                Data.MyStats.OnDeath.AddEvent(CharacterRespawner.Get().RevivePlayer);
            }
        }

        /// <summary>
        /// Sets the character to not respawn, using the character respawner class
        /// </summary>
        public void DontRespawn()
        {
            Data.CanRespawn = false;
        }

        private UniversalCoroutine.Coroutine DeathHandle;
        /// <summary>
        /// Called when stats health reaches 0
        ///     -Disable Bot and Player Controls
        /// Need to add reviving functionality
        /// </summary>
        public void OnDeath(GameObject MyCharacter = null)
		{
            if (MyCharacter == null)
            {
                MyCharacter = gameObject;
            }
			MyGuis.SaveStates();
            MyGuis.HideAll();
            // Disable collision for ragdoll - this can be handled in ragdoll
            /*if (MyCollider)
            {
                MyCollider.enabled = false;
            }*/
            // drain all other stats, ie energy and mana
            // stop saving when dead
            //Bot MyBot = gameObject.GetComponent<Bot>();
            if (MyBot)
            {
                MyBot.Disable();
            }

            // Apply Animations on death - fade effect
            if (MySkeleton)
            {
                Ragdoll MyRagDoll = MySkeleton.GetComponent<Ragdoll>();
                if (MyRagDoll)
                {
                    MyRagDoll.RagDoll();
                }
                /*SkeletonAnimator MyAnimator = MySkeleton.GetComponent<SkeletonAnimator>();
                if (MyAnimator)
                {
                    MyAnimator.Stop();
                }*/
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
            SetMovement(false);
            if (DeathHandle != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(DeathHandle);
            }
            DeathHandle = UniversalCoroutine.CoroutineManager.StartCoroutine(DeathRoutine());
        }

        private System.Collections.IEnumerator DeathRoutine()
        {
            float TimeStarted = Time.time;
            float DeathTime = Random.Range(14f, 30f);
            if (IsPlayer)
            {
                DeathTime = 5f;
            }
            while (Time.time - TimeStarted <= DeathTime)
            {
                yield return null;
            }
            if (IsInvinsible)
            {
                MySkeleton.GetComponent<Ragdoll>().ReverseRagdoll();
                TimeStarted = Time.time;
                while (Time.time - TimeStarted <= 5.1f)
                {
                    yield return null;
                }
                Debug.LogError("Reviving Character.");
                Data.MyStats.RestoreFullHealth();
                MyGuis.RestoreStates();
                DeathHandle = null;
                if (IsPlayer)
                {
                    CameraManager.Get().GetMainCamera().transform.localPosition = Vector3.zero;
                    CameraManager.Get().GetMainCamera().transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                if (IsPlayer)
                {
                    Camera.main.gameObject.GetComponent<Player>().RemoveCharacter();
                }
                MyGuis.Clear();
                CharacterManager.Get().ReturnObject(this);
            }
        }

        [SerializeField]
        private bool IsInvinsible;

        public bool StopDeath()
        {
            if (DeathHandle != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(DeathHandle);
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
            Data.MyStats.AddExperience(1);
        }

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
            GameObject GameManager = GameObject.Find("GameManager");
            if (GameManager)
            {
                GameMode MyMode = GameManager.GetComponent<GameMode>();
                MyMode.CheckKillCondition(GetScore());
            }
            else
            {
                Debug.LogError("No Game Manager for scoring.");
            }
        }
        #endregion

        #region Collision
        /// <summary>
        /// When player collides with something
        /// </summary>
        void OnControllerColliderHit(ControllerColliderHit MyHit)
        {
            ItemObject MyItem = MyHit.gameObject.GetComponent<ItemObject>();
            if (MyItem)
            {
                MyItem.OnContact(gameObject);
            }
            Teleporter MyTeleporter = MyHit.gameObject.GetComponent<Teleporter>();
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
                RayTraceSelections(MySkeleton.MyCameraBone, SelectionType);
            }
            else
            {
                Debug.LogError(name + " could not interact with anything.");
            }
        }

        /*public static bool GetRayFromCamera(Transform MyCamera, float Range, out RaycastHit MyHit)
        {
            var CharactersLayer = (1 << 15);
            var ExcludeCharactersLayer = ~CharactersLayer;  // everything except characters
            bool DoesHit = Physics.Raycast(MyCamera.transform.position, MyCamera.transform.forward,
                out MyHit, Range, ExcludeCharactersLayer);
            return DoesHit;
        }*/
        private bool AreLayersEqual(LayerMask MyLayer, int GameObjectLayer)
        {
            return (MyLayer.value == 1 << GameObjectLayer);
        }

        private bool RayTraceSelections(Transform CameraBone, int SelectionType)
		{
			RaycastHit MyHit;
			if (Physics.Raycast(CameraBone.position, CameraBone.forward, out MyHit, RaycastRange, InteractLayer))
			{
                Debug.Log(name + " has interacted with: " + MyHit.collider.gameObject.name);

                if (AreLayersEqual(ItemsLayer, MyHit.collider.gameObject.layer))
                {
                    ItemObject HitItemObject = MyHit.collider.gameObject.GetComponent<ItemObject>();
                    if (HitItemObject != null)
                    {
                        Debug.Log(name + " has picked up item: " + HitItemObject.name);
                        return PickupItem(HitItemObject, SelectionType);
                    }
                    Chunk ItemChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                    if (ItemChunk)
                    {
                        ItemObject HitItemObject2 = ItemChunk.transform.parent.gameObject.GetComponent<ItemObject>();
                        if (HitItemObject2 != null)
                        {
                            Debug.Log(name + " has picked up Voxel Item: " + HitItemObject2.name);
                            return PickupItem(HitItemObject2, SelectionType);
                        }
                    }
                    Debug.Log(name + " Hit a items layer without item object");
                }
                else if (AreLayersEqual(CharacterLayer, MyHit.collider.gameObject.layer))
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
                else if (AreLayersEqual(WorldsLayer, MyHit.collider.gameObject.layer))
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
                        VoxelManager MetaData = MyWorld.MyDataBase;
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
            if (MyHit.collider.GetComponent<Chunk>())
                MyWorld = MyHit.collider.GetComponent<Chunk>().GetWorld();
            else
                MyWorld = MyHit.collider.GetComponent<World>();
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
            // Initialize that characters dialogue system with (MainCharacter->Lotus dialogue file)
            DialogueHandler OtherCharacterDialogue = OtherCharacter.GetComponent<DialogueHandler>();
            Debug.Log(name + " Is Talking to" + OtherCharacterDialogue.name + " with " +
                OtherCharacterDialogue.MyTree.GetSize() + " dialogue sections.");
            if (OtherCharacterDialogue.MyTree.GetSize() == 0)
            {
                DialogueData MyDialoguesData = new DialogueData();
                MyDialoguesData.Name = "Talk";

                SpeechLine MySpeechLine = new SpeechLine();
                MySpeechLine.Speaker = DialogueData.SpeakerName1;
                MySpeechLine.Speech = "Good Evening Sir";
                MyDialoguesData.SpeechLines.Add(MySpeechLine);

                SpeechLine MySpeechLine2 = new SpeechLine();
                MySpeechLine2.Speaker = DialogueData.SpeakerName2;
                MySpeechLine2.Speech = "Good evening";
                MyDialoguesData.SpeechLines.Add(MySpeechLine2);

                SpeechLine MySpeechLine3 = new SpeechLine();
                MySpeechLine3.Speaker = DialogueData.SpeakerName1;
                MySpeechLine3.Speech = "Goodbye Sir";
                MyDialoguesData.SpeechLines.Add(MySpeechLine3);

                OtherCharacterDialogue.MyTree.Add(MyDialoguesData);
            }
            if (OtherCharacterDialogue.MyTree.GetSize() > 0)
            {
                //GuiManager MyGuiManager = GetComponent<GuiManager>();

                ZelGui MyDialogueGui = this.MyGuis.GetZelGui("Dialogue");
                if (MyDialogueGui)
                {

                    OnBeginTalk(OtherCharacter);
                    OtherCharacter.OnBeginTalk(this);
                    MyDialogueGui.TurnOn();
                    DialogueHandler MyCharacterDialogue = MyDialogueGui.GetComponent<DialogueHandler>();
                    MyCharacterDialogue.MyTree = OtherCharacterDialogue.MyTree;    // set gui tree as talked to characters dialogue
                    MyCharacterDialogue.MyCharacter = this;
                    MyCharacterDialogue.OtherCharacter = OtherCharacter;
                    MyCharacterDialogue.OnConfirm();//begin the talk
                }
            }
            else
            {
                Debug.Log(name + " cannot talk to " + OtherCharacter.name + " due to 0size.");
            }
            return true;
		}

        /// <summary>
        /// Called on the character
        /// </summary>
        public void OnBeginTalk(Character Character2)
        {
            // Hide all guis
            this.MyGuis.SaveStates();
            this.MyGuis.HideAll();
            // display the dialogue one
            ZelGui MyDialogueGui = this.MyGuis.GetZelGui("Dialogue");
            if (MyDialogueGui == null)
            {
                this.MyGuis.Spawn("Dialogue");
                Character2.MyGuis.Spawn("Dialogue");
                MyDialogueGui = this.MyGuis.GetZelGui("Dialogue");
                if (MyDialogueGui)
                {
                    MyDialogueGui.TurnOff();
                }
                else
                {
                    Debug.LogError("Failed to create dialogue");
                }
            }
            this.GetComponent<Bot>().FollowTarget(Character2.gameObject);
            this.OnEndTalkEvent.SetEvent(OnEndTalk);
            SetMovement(false);

            if (IsPlayer)
            {
                Player MyMouseLocker2 = Camera.main.GetComponent<Player>();
                MyMouseLocker2.SetFreeze(true);
            }
        }

        public void OnEndTalk()
        {
            SetMovement(true);
            Player MyMouseLocker = Camera.main.GetComponent<Player>();
            MyMouseLocker.SetFreeze(false);
            MyGuis.RestoreStates();
            Bot OtherBot = gameObject.GetComponent<Bot>();
            if (OtherBot && OtherBot.enabled)
            {
                OtherBot.Wander();
            }
            ZelGui MyDialogueGui = this.MyGuis.GetZelGui("Dialogue");
            MyDialogueGui.TurnOff();
        }

        /// <summary>
        /// When activate button pressed on item object
        /// </summary>
		public bool PickupItem(ItemObject HitItemObject, int InteractType) 
		{
			if (InteractType == 0)
			{
                    // passes in a ray object for any kind of picking action
                    // does things like destroy, activates the special function
                    HitItemObject.CharacterPickup(this);
			}
            else if (InteractType == 1)
            {
                HitItemObject.ToggleGui();
            }
			return true;
        }
        #endregion

        #region Controls

        /*public override void OnStartServer()
        {
            base.OnStartServer();
        }*/

        /// <summary>
        /// This is used in the character selector to hide characters depending on permissions.
        /// </summary>
        public bool CanPlayerPlay(string PlayerName)
        {
            /*for (int i = 0; i < PlayerPermissions.Count; i++)
            {
                if (PlayerName == PlayerPermissions[i])
                {
                    return true;
                }
            }
            return false;*/
            return true;
        }
       // private CharacterLimiter MyCharacterLimiter;

        private void RefreshComponents2()
        {
            MyBot = GetComponent<Bot>();
            MyRigidbody = GetComponent<Rigidbody>();
            MyController = GetComponent<BasicController>();
            MyCharacterMapChecker = GetComponent<CharacterMapChecker>();
            //MyCharacterLimiter = GetComponent<CharacterLimiter>();
        }

        /// <summary>
        /// Enable and disable movement
        /// </summary>
        public void SetMovement(bool NewState)
        {
            //Debug.LogError("InSetMovement: " + NewState);
            RefreshComponents2();
            /*if (MyCharacterMapChecker)
            {
                MyCharacterMapChecker.enabled = NewState;
            }
            if (MyCharacterLimiter)
            {
                MyCharacterLimiter.enabled = NewState;
            }*/
            if (MyController)
            {
                MyController.enabled = NewState;
            }
            if (MyRigidbody)
            {
                MyRigidbody.isKinematic = !NewState;
            }
            /*if (MyBot)
            {
                MyBot.enabled = NewState;
            }
            if (MyBotMovement)
            {
                MyBotMovement.enabled = NewState;
            }*/
            enabled = NewState;
        }
		#endregion
    }
}


/*public static void SetTransformText(Transform MyTransform, string[] MyData)
{
    List<string> NewData = new List<string>();
    for (int i = 0; i < MyData.Length; i++)
        NewData.Add(MyData[i]);
    SetTransformText(MyTransform, NewData);
}*/

/*public void RunScriptNetwork(List<string> MySaveFile)
{
    string MyScript = FileUtil.ConvertToSingle(MySaveFile);
    if (MyScript.Length < 32767)
    {
        gameObject.GetComponent<PhotonView>().RPC("RunScriptNetwork2",
            PhotonTargets.All,  // send just to the player that requested the data
            MyScript
            );
    }
    else
    {
        Debug.LogError("Character string length over max, breaking up into parts.\n Current length: " + MyScript.Length);
        //Debug.LogError(MyScript);
        // this is primarily due to texture size being so high. Need to break this up into many component scripts to send accross.
    }
}
[PunRPC]
public void RunScriptNetwork2(string MyScript)
{
    RunScript(FileUtil.ConvertToList(MyScript));
}*/

/*public void RunScript(string[] MySaveFile)
{
    List<string> MyScriptList = new List<string>();
    for (int i = 0; i < MySaveFile.Length; i++)
    {
        MyScriptList.Add(MySaveFile[i]);
    }
    RunScript(MyScriptList);
}*/

/*if (MySelections[i].ComponentName != "" && MySelections[i].FunctionName != "") 
{
    Component MyComponent = (Component) MyHit.collider.gameObject.GetComponent(MySelections[i].ComponentName);
    MyComponent.BroadcastMessage(MySelections[i].FunctionName);
}*/
//Debug.LogError ("HitObject! " + MyHit.collider.gameObject.name);
//MySelections[i].HandleOnSelect(InteractType, MyHit.collider.gameObject);
/*Zeltex.AnimationUtilities.BodyPart MyBodyPart = MyHit.collider.gameObject.GetComponent<Zeltex.AnimationUtilities.BodyPart>();
if (MyBodyPart)
{
    if (MyBodyPart.RagdollBrain.OnInteract.Count > InteractType  && InteractType >= 0)
        MyBodyPart.RagdollBrain.OnInteract[InteractType].Invoke(MyHit.collider.gameObject);
}
*/
//Debug.LogError("Hit object: " + MyHit.collider.gameObject.name);
// add a value to statistics! for things 
//if (gameObject.GetComponent<QuestLog>())
//	gameObject.GetComponent<QuestLog>().RefreshQuestsGui();
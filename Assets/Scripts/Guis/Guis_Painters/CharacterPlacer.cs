using UnityEngine;
using System.Collections;
using Zeltex.Voxels;
using Zeltex.Combat;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zeltex.Util;
using Zeltex.Characters;

namespace Zeltex.Guis.Maker
{

	/// <summary>
	/// The type of action used for painting characters
	/// </summary>
    public enum CharacterPainterType
    {
        None,
        Select,
        Move,
        Spawn,
        Edit,
        Rotate,
        Scale
    }

    /// <summary>
    /// Used for level editor to place characters
    /// </summary>
    public class CharacterPlacer : GuiBasic
    {
        #region Variables
        [Header("Prefabs")]
        public GameObject MySummonPrefab;
        [Header("Materials")]
        public Material HighlightedMaterial;
        public Material SelectedMaterial;
        [Header("Summoning")]
        public string MinionName = "Marz";
       // public string MinionClass = "Mage";
        //public string MinionRace = "Human";
        // Selection Cube
        private GameObject HighLightedCube;
        private GameObject SelectedCube;
        // Raycasting
        private Character LastHitCharacter;
        [SerializeField] private Character SelectedCharacter;
        private World LastHitWorld;
        private bool DidRayHitGui;
        //private Vector3 LastHitBlockPosition;
        //private Vector3 LastHitWorldPosition;
        private Vector3 LastHitWorldPosition2;
        private Vector3 VoxelUnit;  // one unit of voxel space
        [SerializeField]
        private Vector3 HighlightedCenter;
        //private Vector3 Normal;
        private Vector3 CharacterSize;  // one unit of voxel space
        private Vector3 SelectedCenter;
        private bool IsVisible = true;
        public CharacterPainterType PaintType;
        private ObjectViewer MyViewer;
        private bool DidRayHit;
        private RaycastHit MyHit;
        public bool IsUniquePlacement;
        [Header("Gizmos")]
        [SerializeField]
        private GameObject GizmoPrefab;
        private GameObject Gizmo;
        #endregion

        #region Mono

        void OnEnable()
        {
            // create cube
            CreateCubes();
        }

        void OnDisable()
        {
            Destroy(HighLightedCube);
            Destroy(SelectedCube);
        }

        void Update()
        {
            if (PaintType != CharacterPainterType.None)
            {
                Raycast();
                if (IsVisible)
                {
                    HandleInput();
                }
                AlterBrush();
            }
            HandleShortcuts();
        }
        #endregion

        #region ZelGui
        /// <summary>
        /// When gui begins
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();
            FillTheDropdowns();
        }
        #endregion

        #region Input
        /// <summary>
        /// Uses shortcut keys for switching between paint types
        /// </summary>
        private void HandleShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetPaintTypeInternal(CharacterPainterType.None);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetPaintTypeInternal(CharacterPainterType.Select);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetPaintTypeInternal(CharacterPainterType.Move);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetPaintTypeInternal(CharacterPainterType.Spawn);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetPaintTypeInternal(CharacterPainterType.Edit);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SetPaintTypeInternal(CharacterPainterType.Rotate);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SetPaintTypeInternal(CharacterPainterType.Scale);
            }
        }
        /// <summary>
        /// Handles user input
        /// </summary>
        private void HandleInput()
		{
			if (DidRayHitGui == false && Input.GetMouseButtonDown(0))
			{
                if (PaintType == CharacterPainterType.Spawn)
                {
                    if (LastHitCharacter == null && LastHitWorld != null)   // summon
                    {
                        SpawnCharacter();
                    }
                }
                else if (PaintType == CharacterPainterType.Select)
                {
				    if (LastHitCharacter != null)
                    {
                        SelectCharacter();
                    }
                    else
                    {
                        Deselect();
                    }
                }
			}
			if (SelectedCube.activeSelf)
			{
				SelectedCube.transform.position = SelectedCharacter.transform.position + SelectedCenter;
				SelectedCube.transform.rotation = SelectedCharacter.transform.rotation;
			}
            if (HighLightedCube.activeSelf)
            {
                HighLightedCube.transform.position = LastHitCharacter.transform.position + HighlightedCenter;
                HighLightedCube.transform.rotation = LastHitCharacter.transform.rotation;
                //HighLightedCube.transform.localScale = VoxelUnit * 1.1f;
            }
		}

        /// <summary>
        /// Spawns a character in the ray hit world position
        /// </summary>
        private void SpawnCharacter()
        {
            GameObject NewSpawner = (GameObject)Instantiate(MySummonPrefab, LastHitWorldPosition2, Quaternion.identity);
            SummoningAnimation MyAnimation = NewSpawner.GetComponent<SummoningAnimation>();
            if (DidRayHit && MyViewer)
            {
                // use same layer as world!
                //LayerMask MyLayer = 1 << MyViewer.GetSpawn().layer;
                //ObjectViewer.SetLayerRecursive(MyAnimation.gameObject, MyLayer);
                //Debug.LogError("Setting Spawn Animations layer to: " + LayerMask.LayerToName(MyLayer));

                //MyAnimation.gameObject.GetComponent<Character>().SetLayerMaskObject(MyViewer.GetSpawn());
            }
            MyAnimation.SpawnClass(GetCharacterClassName(), GetCharacterRaceName(), MinionName);
            GetList("CharactersList").Add(GetCharacterClassName());
        }
        #endregion

        #region PaintType
        /// <summary>
        /// Sets the mouse icon to the type of paint brush
        /// </summary>
        private void AlterBrush()
        {
            if (PaintType != CharacterPainterType.None)
            {
                if (PaintType == CharacterPainterType.Select)
                {
                    MouseCursor.Get().SetMouseIcon("CharacterSelect");
                }
                else if (PaintType == CharacterPainterType.Move)
                {
                    MouseCursor.Get().SetMouseIcon("CharacterMove");
                }
                else if (PaintType == CharacterPainterType.Spawn)
                {
                    MouseCursor.Get().SetMouseIcon("CharacterSpawn");
                }
            }
            else
            {
                MouseCursor.Get().SetMouseIcon("DefaultMouse");
            }
        }
        /// <summary>
        /// Sets paint type as well as dropdown
        /// </summary>
        private void SetPaintTypeInternal(CharacterPainterType NewPaintType)
        {
            GetDropdown("PaintTypeDropdown").value = (int)NewPaintType;
            SetPaintType(NewPaintType);
        }

        /// <summary>
        /// Sets paint type
        /// </summary>
        private void SetPaintType(CharacterPainterType NewPaintType)
        {
            if (PaintType != NewPaintType)
            {
                if (NewPaintType != CharacterPainterType.Select && NewPaintType != CharacterPainterType.Edit
                     && NewPaintType != CharacterPainterType.Move && NewPaintType != CharacterPainterType.Rotate && NewPaintType != CharacterPainterType.Scale)
                {
                    Deselect();
                }
                PaintType = NewPaintType;
                if (PaintType == CharacterPainterType.Edit)
                {
                    // Lock Camera on skeleton
                    if (SelectedCharacter)
                    {
                        SelectedCharacter.GetSkeleton().GetSkeleton().SetMeshColliders(true);
                    }
                }
                else if (PaintType == CharacterPainterType.Move || PaintType == CharacterPainterType.Rotate || PaintType == CharacterPainterType.Scale)
                {
                    // Lock Camera on skeleton
                    if (SelectedCharacter)
                    {
                        SelectedCharacter.GetSkeleton().GetSkeleton().SetMeshColliders(false);
                    }
                }
            }
        }
        #endregion

        #region UI

        /// <summary>
        /// 
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "ClassDropdown")
            {
                //MinionClass = MyDropdown.options[MyDropdown.value].text;
            }
            else if (MyDropdown.name == "RaceDropdown")
            {
               // MinionRace = MyDropdown.options[MyDropdown.value].text;
            }
            else if (MyDropdown.name == "PaintTypeDropdown")
            {
                SetPaintType((CharacterPainterType)MyDropdown.value);
            }
        }

        /// <summary>
        /// Checks the character index in the character manager
        /// </summary>
        private int GetCharacterIndex(Character MyCharacter)
        {
            List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
            if (MyCharacters.Contains(MyCharacter))
            {
                return MyCharacters.IndexOf(MyCharacter);
            }
            else
            {
                return -1;
            }
            /*int CharacterIndex = -1;
            for (int i = 0; i < CharacterManager.Get().MyCharacters.Count; i++)
            {
                if (MyCharacter == CharacterManager.Get().MyCharacters[i])
                {
                    CharacterIndex = i;
                    break;
                }
            }
            return CharacterIndex;*/
        }

        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "DeleteCharacter")
            {
                if (GetSelectedCharacter())
                {
                    int CharacterIndex = GetCharacterIndex(GetSelectedCharacter());
                    if (CharacterIndex != -1)
                    {
                        // Delete File!
                        //MyLevelListHandler.DeleteCharacter(GetSelectedCharacter().gameObject);
                        GetList("CharactersList").RemoveAt(CharacterIndex);
                        CharacterManager.Get().ReturnObject(GetSelectedCharacter());
                        // Remove from Scene!
                        GetSelectedCharacter().OnDeath();
                        Deselect();
                        //Destroy(GetSelectedCharacter().gameObject);
                    }
                    else
                    {
                        Debug.LogError("Selected character not in character manager.");
                    }
                }
                else
                {
                    Debug.LogError("No character selected.");
                }
            }
            else if (MyButton.name == "NoneButton")
            {
                SetPaintTypeInternal(CharacterPainterType.None);
            }
            else if (MyButton.name == "SelectButton")
            {
                SetPaintTypeInternal(CharacterPainterType.Select);
            }
            else if (MyButton.name == "MoveButton")
            {
                SetPaintTypeInternal(CharacterPainterType.Move);
            }
            else if (MyButton.name == "SpawnButton")
            {
                SetPaintTypeInternal(CharacterPainterType.Spawn);
            }
            else if (MyButton.name == "PauseAll")
            {
                CharacterManager.Get().SetMovement(false);
            }
            else if (MyButton.name == "ResumeAll")
            {
                CharacterManager.Get().SetMovement(true);
            }
            else if (MyButton.name == "KillAll")
            {
                CharacterManager.Get().SetAggression(true);
            }
        }

        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "RaceDropdown")
            {
                SetVisibility(MyToggle.isOn);
            }
        }

        /// <summary>
        /// The dropdowns!
        /// </summary>
        private void FillTheDropdowns()
        {
            List<string> MyNames = new List<string>();
            MyNames.AddRange(DataManager.Get().GetNames("Classes"));
            Dropdown ClassDropdown = GetDropdown("ClassDropdown");
            MakerGui.FillDropDownWithList(ClassDropdown, MyNames);
            List<string> MyRaces = new List<string>();
            MyRaces.AddRange(DataManager.Get().GetNames("Skeletons"));
            Dropdown RaceDropdown = GetDropdown("RaceDropdown");
            MakerGui.FillDropDownWithList(RaceDropdown, MyRaces);
        }

        public override void UseInput(GuiList MyList)
        {
            if (MyList.name == "CharactersList")
            {
                Character ListCharacter = CharacterManager.Get().GetSpawn(MyList.GetSelected());
                if (ListCharacter)
                {
                    SelectCharacter(ListCharacter);   // select a bone by a name
                    SetPaintTypeInternal(CharacterPainterType.Select);
                }
            }
        }

        public override void FillList(GuiList MyList)
        {
            if (MyList.name == "CharactersList")
            {
                MyList.Clear();
                for (int i = 0; i < CharacterManager.Get().GetSize(); i++)
                {
                    Character MyCharacter = CharacterManager.Get().GetSpawn(i);
                    if (MyCharacter)
                    {
                        MyList.Add(MyCharacter.name);
                    }
                }
            }
        }
        #endregion

        #region Selection
        public Character GetSelectedCharacter()
        {
            return SelectedCharacter;
        }

        /// <summary>
        /// Selects a character from the list
        /// </summary>
        private void SelectCharacter(Character NewCharacter)
        {
            if (SelectedCharacter)
            {
                Deselect();
            }
            SelectedCharacter = NewCharacter;
            if (NewCharacter)
            {
                Bounds MyBounds = SelectedCharacter.GetSkeleton().GetSkeleton().GetBounds();
                CharacterSize = MyBounds.extents * 2;
                SelectedCenter = MyBounds.center;
                SelectedCube.SetActive(true);
                SelectedCube.layer = SelectedCharacter.gameObject.layer;
                SelectedCube.transform.localScale = CharacterSize * 1.1f;
                OnSelectedCharacter();
            }
        }
        /// <summary>
        /// Selects the previously highlighted character
        /// </summary>
        private void SelectCharacter()
        {
            if (LastHitCharacter)
            {
                if (SelectedCharacter)
                {
                    Deselect();
                }
                SelectedCharacter = LastHitCharacter;
                CharacterSize = VoxelUnit;
                SelectedCenter = HighlightedCenter;
                SelectedCube.SetActive(true);
                SelectedCube.layer = SelectedCharacter.gameObject.layer;
                SelectedCube.transform.localScale = CharacterSize * 1.1f;
                OnSelectedCharacter();
            }
        }

        private void OnSelectedCharacter()
        {
            if (SelectedCharacter)
            {
                GetInput("NameInput").interactable = true;
                GetInput("NameInput").text = SelectedCharacter.name;
                GetLabel("StatisticsText").text = FileUtil.ConvertToSingle(SelectedCharacter.GetStatistics());
                GetList("CharactersList").Select(GetCharacterIndex(SelectedCharacter));
                SelectedCharacter.SetMovement(false);
                Gizmo = Instantiate(GizmoPrefab);
                Gizmo.name = "Gizmo";
                Gizmo.transform.position = SelectedCharacter.transform.position;
                Gizmo.layer = 1 << SelectedCharacter.gameObject.layer;
                Gizmo.GetComponent<Tools.Gizmo>().MyTarget = SelectedCharacter.gameObject;
                SelectedCharacter.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                SelectedCharacter.GetSkeleton().GetSkeleton().SetMeshColliders(false);
                SelectedCharacter.GetSkeleton().GetComponent<Skeletons.Zanimator>().Stop();
            }
        }


        public void Deselect()
        {
            if (SelectedCharacter)
            {
                SelectedCharacter.SetMovement(true);
                SelectedCharacter.gameObject.GetComponent<CapsuleCollider>().enabled = true;
                SelectedCharacter.GetSkeleton().GetSkeleton().SetMeshColliders(true);
                SelectedCharacter.GetSkeleton().GetComponent<Skeletons.Zanimator>().Play();
                SelectedCharacter = null;
            }
            SelectedCube.SetActive(false);
            GetInput("NameInput").interactable = false;
            GetInput("NameInput").text = "";
            GetList("CharactersList").Select(-1);
            GetLabel("StatisticsText").text = "";
            if (Gizmo)
            {
                Destroy(Gizmo);
            }
        }
		#endregion

		#region Raycasting

		private void Raycast()
        {
            // Reset variables every frame
            LastHitCharacter = null;
            HighLightedCube.SetActive(false);
            LastHitWorld = null;
            DidRayHitGui = RaycastViewer();
            if (DidRayHitGui == false)
			{
				RaycastWorld();
            }
        }

		/// <summary>
		/// Character painter hitting gui
		/// </summary>
		private bool RaycastViewer()
		{
			Ray MyRay;
			//Create the PointerEventData with null for the EventSystem
			PointerEventData MyPointerEvent = new PointerEventData(EventSystem.current);
			//Set required parameters, in this case, mouse position
			MyPointerEvent.position = Input.mousePosition;
			//Create list to receive all results
			List<RaycastResult> MyResults = new List<RaycastResult>();
			//Raycast it
			EventSystem.current.RaycastAll(MyPointerEvent, MyResults);
			//Debug.LogError("Raycast results: " + MyResults.Count + " at position: " + MyPointerEvent.position.ToString());
			for (int i = 0; i < MyResults.Count; i++)
			{
				if (MyResults[i].gameObject.GetComponent<ObjectViewer>())
				{
					if (MyViewer != MyResults[i].gameObject.GetComponent<ObjectViewer>())
					{
						MyViewer = MyResults[i].gameObject.GetComponent<ObjectViewer>();
						Debug.Log("New Voxel Viewer Selected: " + MyViewer.name);
					}
					bool DidHit = MyViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
					if (DidHit)
					{
						DidRayHit = true;
                        Tools.GizmoHandle MyGizmoHandle = MyHit.collider.gameObject.GetComponent<Tools.GizmoHandle>();
                        if (MyGizmoHandle)
                        {
                            MyGizmoHandle.OnRayhit(MyRay, MyHit);
                        }
                        // First check if hit chunk
                        Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                        if (MyChunk)
						{
							SelectWorld(MyChunk, MyHit);
						}
						else
						{
							// Check for character hit
							GameObject MyCharacterObject = MyHit.collider.transform.FindRootFromBone();
							if (MyCharacterObject)
							{
								HighlightCharacter(MyCharacterObject.transform);
							}
							else
							{
								Character MyCharacter = MyHit.collider.gameObject.GetComponent<Character>();
								if (MyCharacter)
								{
									HighlightCharacter(MyCharacter.transform);
								}
							}
						}
					}
                    return false;   // make sure user actions work on viewers
				}
			}
			return (MyResults.Count != 0);
		}

		/// <summary>
		/// Ray cast the world
		/// </summary>
		private void RaycastWorld()
		{
			Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			//DidRayHitGui = IsRayHitGui();
			if (UnityEngine.Physics.Raycast(MyRay.origin, MyRay.direction, out MyHit))
			{
				Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                GameObject MyCharacterObject = MyHit.collider.transform.FindRootFromBone();
                Tools.GizmoHandle MyGizmoHandle = MyHit.collider.gameObject.GetComponent<Tools.GizmoHandle>();
                if (MyChunk)
				{
					SelectWorld(MyChunk, MyHit);
                }
                if (MyGizmoHandle)
                {
                    MyGizmoHandle.OnRayhit(MyRay, MyHit);
                }
                if (MyCharacterObject)
				{
					HighlightCharacter(MyCharacterObject.transform);
                }
                else
				{
					Character MyCharacter = MyHit.collider.gameObject.GetComponent<Character>();
					if (MyCharacter)
					{
						HighlightCharacter(MyCharacter.transform);
					}
				}
			}
		}

		private void SelectWorld(Chunk MyChunk, RaycastHit MyHit)
		{
			World MyWorld = MyChunk.GetWorld();
			LastHitWorld = MyWorld;
			//LastHitBlockPosition = MyWorld.RayHitToBlockPosition(MyHit.point, MyHit.normal);
			//Normal = MyHit.normal;
			//LastHitWorldPosition = MyWorld.BlockToRealPosition(LastHitBlockPosition);
			LastHitWorldPosition2 = MyHit.point;//WorldExtra.GetWorldPosition(MyWorld, LastHitBlockPosition + Normal/2f);
			VoxelUnit = new Vector3(
				MyWorld.VoxelScale.x * MyWorld.transform.lossyScale.x,
				MyWorld.VoxelScale.y * MyWorld.transform.lossyScale.y,
				MyWorld.VoxelScale.z * MyWorld.transform.lossyScale.z);
			HighlightedCenter = VoxelUnit / 2f;
		}
		#endregion

		#region SelectionMeshes
		private void CreateCubes()
        {
            HighLightedCube = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Cube);
            HighLightedCube.name = "CharacterHighlighterCube";
            Destroy(HighLightedCube.GetComponent<BoxCollider>());
            HighLightedCube.GetComponent<MeshRenderer>().material = HighlightedMaterial;
            HighLightedCube.SetActive(false);
            SelectedCube = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Cube);
            SelectedCube.name = "CharacterSelectionCube";
            Destroy(SelectedCube.GetComponent<BoxCollider>());
            SelectedCube.GetComponent<MeshRenderer>().material = SelectedMaterial;
            SelectedCube.SetActive(false);
        }

        private void HighlightCharacter(Transform MyCharacter)
        {
            if (MyCharacter != null)
            {
                LastHitCharacter = MyCharacter.GetComponent<Character>();
                Bounds MyBounds = LastHitCharacter.GetSkeleton().GetSkeleton().GetBounds();
                VoxelUnit = MyBounds.extents * 2;
                HighlightedCenter = MyBounds.center;
                HighLightedCube.SetActive(true);
                HighLightedCube.transform.position = LastHitCharacter.transform.position + HighlightedCenter;
                HighLightedCube.transform.rotation = LastHitCharacter.transform.rotation;
                HighLightedCube.transform.localScale = VoxelUnit * 1.2f;
                //ObjectViewer.SetLayerRecursiveInt(HighLightedCube, LastHitCharacter.gameObject.layer);
            }
        }

        #endregion

        // Need to use charactermanager
        #region Character

        private string GetCharacterClassName()
        {
            Dropdown MyDropdown = GetDropdown("ClassDropdown");
            return MyDropdown.options[MyDropdown.value].text;
        }

        private string GetCharacterRaceName()
        {
            Dropdown MyDropdown = GetDropdown("RaceDropdown");
            return MyDropdown.options[MyDropdown.value].text;
        }

        public void SetVisibility(bool NewState)
        {
            IsVisible = NewState;
            for (int i = 0; i < CharacterManager.Get().GetSize(); i++)
            {
				CharacterManager.Get().GetSpawn(i).gameObject.SetActive(IsVisible);
            }
            if (enabled)
            {
                SelectedCube.SetActive(IsVisible);
                HighLightedCube.SetActive(IsVisible);
            }
        }
        #endregion

        /// <summary>
        /// Checks to see if the mouse is hitting the gui
        /// </summary>
        public static bool IsRayHitGui()
        {
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = (Input.mousePosition);
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
            {
                return true;
            }
            return false;
        }

        /*void OnDrawGizmosSelected()
        {
            if (LastHitWorld != null)
            {
                Gizmos.color = new Color(1, 1, 1, 0.1f);
                Gizmos.DrawCube(LastHitWorldPosition + PositionOffset, VoxelUnit);
            }
        }*/
    }
}
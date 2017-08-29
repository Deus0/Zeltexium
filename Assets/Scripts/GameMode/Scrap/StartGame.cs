

/*
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using MakerGuiSystem;
using Zeltex.Voxels;
using Zeltex.Characters;
using Zeltex.Items;
using Zeltex.Skeletons;
using Zeltex.Guis;
using Zeltex.Dialogue;

namespace GuiSystem
{

  /// <summary>
  /// Different options for the terrain generation
  /// </summary>
  [System.Serializable]
  public enum TerrainGenerationType
  {
      HeightMap,
      Dungeon,
      Level,
      Room
  }
  /// <summary>
  /// A quick mode to play a game
  /// Will join with other players if a lobby is open
  /// </summary>
  public class StartGame : MonoBehaviour
  {
      #region Variables
      public bool IsOffline;
      //public int GenerateTerrainType; // 0 for terrain, 1 for levels, 2 for plane
      [Header("References")]
      public MapGenerator MyMapGenerator;
      public LobbyGui MyLobby;
      //public MapMaker MyMapMaker;
      public VoxelTerrain MyTerrain;
      public PlayerGui MyGuiManager;
      //public ItemTextureManager MyItemTextureManager;
      public LevelMaker MyLevelHandler;
      [Header("Level")]
      public bool IsGenerateGameData;
      public int LevelIndex = 0;
      private TerrainGenerationType GenerationType = TerrainGenerationType.Level;
      [Header("UI")]
      public ZelGui MyGui;
      public Text MyStatusText;
      private float MaxWaitTime = 10f;    // max attempt to connect to photon network
      private float TimeWaited = 0;
      private string MyStatus = "";
      private float LoadingDelay = 0.075f;
      public static bool IsFreeRoam = false;
      public SpawnPositionFinder MySpawnPoint;
      [Header("RenderDistance")]
      public float RenderDistance = 3;
      private Vector3 NewWorldSize = new Vector3(3, 2, 3);
      public Slider RenderSlider;
      public InputField RenderDistanceInput;
      #endregion

      void Start()
      {
          if (RenderSlider)
          {
              RenderSlider.value = RenderDistance;
          }
          SetRenderDistance(RenderDistance);
      }

      #region UI
      /// <summary>
      /// Set the render distance of the world
      /// </summary>
      /// <param name="NewDistance"></param>
      public void SetRenderDistance(float NewDistance)
      {
          // This is value say 2 = 2 x 2 + 1 = 5 accross
          NewWorldSize = new Vector3(NewDistance * NewDistance + 1, NewWorldSize.y, NewDistance * NewDistance + 1);
          if (RenderDistanceInput)
          {
              RenderDistanceInput.text = NewDistance + "";
          }
      }
      public void SetRenderDistance(InputField MyInputField)
      {
          int MyInput = int.Parse(MyInputField.text);
          MyInput = Mathf.Clamp(MyInput, 1, 8);
          SetRenderDistance(MyInput);
      }
      #endregion

      #region QuickPlayAuto

      /// <summary>
      /// Begins quick play mode!
      /// </summary>
      public void CustomMode(World MyWorld)
      {
          IsFreeRoam = false;
          Camera.main.gameObject.GetComponent<VoxelFreeRoam>().enabled = false;
          GenerationType = TerrainGenerationType.Level;
          StopCoroutine(ConnectToNet(MyWorld));
          StartCoroutine(ConnectToNet(MyWorld));
      }

      public void AdventureMode(World MyWorld)
      {
          IsFreeRoam = true;
          Camera.main.gameObject.GetComponent<VoxelFreeRoam>().enabled = true;
          GenerationType = TerrainGenerationType.HeightMap;
          StopCoroutine(ConnectToNet(MyWorld));
          StartCoroutine(ConnectToNet(MyWorld));
      }

      /// <summary>
      /// Creates a quick temporary map to play in
      /// </summary>
      IEnumerator ConnectToNet(World MyWorld)
      {
          PhotonNetwork.offlineMode = IsOffline;
          MyStatusText.text = "Connecting";
          MyGuiManager.GoTo("Nothing");
          MyGui.TurnOn();
          TimeWaited = 0;
          yield return new WaitForSeconds(1f);
          if (PhotonNetwork.offlineMode)
          {
              MyLobby.CreateOfflineRoom();
              yield return CreateMap(MyWorld);
              MyGui.TurnOff();
              yield return new WaitForSeconds(0.1f);
              yield break;
          }
          else
          {
              MyLobby.StartRefresh();
              // First connect
              while (!PhotonNetwork.insideLobby && TimeWaited < MaxWaitTime)  // wait til joined lobby
              {
                  yield return new WaitForSeconds(1f);
                  TimeWaited++;
              }
              if (TimeWaited >= MaxWaitTime)
              {
                  // offline mode
                  Debug.LogError("Could not connect. Canceling Quick Play.");
                  //MyLobby.CreateOfflineRoom();
                  //yield return CreateMap();
                  yield break;
              }
          }
          // check servers - with quickplay mode
          int MyServersCount = MyLobby.GetServerRooms().Count;
          yield return new WaitForSeconds(0.1f);
          // if any servers, not full, join them
          if (MyServersCount > 0)
          {
              MyStatusText.text = "Joining Server";
              MyLobby.JoinRoom(MyServersCount - 1);
              yield return new WaitForSeconds(LoadingDelay);
              // join server with lowest player count
              // load map from room here?!
          }
          // otherwise host match
          else
          {
              MyStatusText.text = "Creating Server";
              MyLobby.CreateRoom();
              yield return new WaitForSeconds(LoadingDelay);
              // then create temporary dungeon map
              yield return CreateMap(MyWorld);
          }
          yield return new WaitForSeconds(LoadingDelay);
          MyStatusText.text = "";
          MyGui.TurnOff();
      }
      /// <summary>
      /// Creates the map
      /// </summary>
      IEnumerator CreateMap(World MyWorld)
      {
          // Create random terrain at startup
          if (GenerationType == TerrainGenerationType.HeightMap)
          {
              yield return MyWorld.SetWorldSizeRoutine(NewWorldSize);
              MyStatusText.text = "Generating Terrain";
              yield return StartCoroutine(GenerateTerrain(MyWorld));
              yield return new WaitForSeconds(LoadingDelay);

              //GameObject MySpawnPosition = GameObject.Find("PlayerSpawnPoint");
              MySpawnPoint.transform.position = MyWorld.GetWorldSize() / 2f;
              MySpawnPoint.IsFindClosest = true;
              MySpawnPoint.IsRandom = false;
              MySpawnPoint.FindNewPosition();
              MyGuiManager.GoTo("CharacterMaker");    // make a new character to take over
              Camera.main.gameObject.GetComponent<VoxelFreeRoam>().BeginFreeRoam();
              //yield return new WaitForSeconds(LoadingDelay);
          }
          // Turn on level select gui, and turn off this one
          else if (GenerationType == TerrainGenerationType.Level)
          {
              MyLevelHandler.GetComponent<ZelGui>().TurnOn();
              // use a normal level from default pack
          }
          // Generate a single room for arena mode
          else if (GenerationType == TerrainGenerationType.Room)
          {   // use a plane!
              yield return MyWorld.SetWorldSizeRoutine(NewWorldSize);
              GameObject MyPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
              MyPlane.transform.localScale = new Vector3(100, 1, 100);
              MyPlane.transform.position = new Vector3(0, 0.5f, 0);
              MyPlane.tag = "World";
          }
      }

      /// <summary>
      /// Creates the procedural map
      /// </summary>
      private IEnumerator GenerateTerrain(World MyWorld)
      {
          if (GenerationType == TerrainGenerationType.HeightMap)
          {
              //MyTerrain.UpdateBaseFrequency("" + 0 + ".0" + Mathf.RoundToInt(Random.RandomRange(1, 6)));
              //MyTerrain.UpdateAmplitude("" + Mathf.RoundToInt(Random.RandomRange(2, 14)));
              yield return StartCoroutine(MyWorld.StartFreeRoam());
          }
          else if (GenerationType == TerrainGenerationType.Dungeon)
          {
              MyTerrain.GetComponent<RoomGenerator>().GenerateAll();
          }
          else if (GenerationType == TerrainGenerationType.Room)
          {
              int PillarType = 2;     // Get CrackedTiles
              int WallType = 3;       // Get BrickType
              int GroundType = 4;     // Get NoiseType;
              MyTerrain.GetComponent<VoxelPlane>().CreateRoom(MyTerrain.GetComponent<World>(), 1, PillarType, GroundType, WallType);
          }
      }
      #endregion
  }
}*/

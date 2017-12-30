using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Zeltex.Voxels
{
/* Rooms to have floors
		Doors	
		Chairs
		Tables
		Lanterns on walls
		Bookshelves
		Beds
		Chains on walls in some places
		Prison Cells
		Windows
		A level of difficulty . they will spawn enemies
		A number of floors, as some will be battle towers with big boss at top
*/
/*[System.Serializable]
public class DungeonData {
}*/

// need a gui for this
// right click on maze item - edit maze properties
// use AStar to check the paths between rooms

	//[ExecuteInEditMode]
	public class Maze : WorldEditor 
	{
		public List<Vector3> DebugPositions = new List<Vector3>();
		public List<int> DebugDirections = new List<int> ();
		[Header("Actions")]
		public bool IsGenerateMaze = false;

		// Dungeon Data
		[Header("Block Types")]
		public int PathBlockType;
		public int MazeWallType;
		public int MazeRoofType;
		
		[Header("Maze Options")]
		public bool IsPaths = true;
		static int MaxIterations = 10000;
		public Vector3 StartLocation;			// start of maze building
		public Vector3 SpawnPosition;			// spawn point for player
		public float Sparceness;
		public float Linearity;
		public Vector3 PathSizeMinimum;
		public Vector3 PathSizeMaximum;
		
		[Header("Maze Additional Options")]
		public bool IsMazeRoof;
		public bool IsRandomizeStartPosition = false;
		public bool IsMiddleStartLocation = false;
		public bool bIsGenerateBiome;
		public bool IsVoidAxisX;
		public bool IsVoidAxisY;
		public bool IsVoidAxisZ;
		public int MaxSpawnPoints;
		public int SpawnBlockType;
		public int FloorsMax = 1;
		public float MazeWallHeight;
		public bool bIsEdge;
		public bool bIsRemoveSinglePillars;
		public bool bIsMoveToSize;
		public bool bIsPathways = true;
		public bool bIsWrap;

		// Generated maze Data
		List<Vector3> SpawnLocations = new List<Vector3>();	// where to spawn bots when level is loaded
		private Vector3 Position;
		private Vector3 PathSize;
		private int Iterations;
		private int PathDirection = 0;
		private float MaxSparce;
		private float SparcePercentage;
		private bool IsSparceEnough = false;
		
		[Header("Texture Creation")]
		//public GameObject MazeTextureObject;
		//public Texture2D MazeTexture;
		public int MaxMipMapLevel;
		//public RawImage MyRawImage;
		public List<Color32> BlockColors = new List<Color32> ();

		// getters and setters
		public float GetSparcePercent() {return SparcePercentage;}

		public void CheckStartLocationInBounds() {
			if (FloorsMax < 1)
				FloorsMax = 1;
			StartLocation = new Vector3 (Mathf.Clamp (StartLocation.x, 0, GetSize().x - 1), 
			                             Mathf.Clamp (StartLocation.y, 0, GetSize().y - 1), 
			                             Mathf.Clamp (StartLocation.z, 0, GetSize().z - 1));
		}

		void GenerateDungeon() {
			if (IsMiddleStartLocation) {
				StartLocation.x = GetSize().x/2f;
				StartLocation.z = GetSize().z/2f;
			}
			CurrentSparce = 0;
			if (bIsGenerateBiome) {
				GenerateBiomeMap(GetSize());
			}
			else {
				Debug.Log ("Generating  ");
				//GenerateWallsOnEdge(MyBlocks, Size, MazeWallType);
				if (IsPaths && Sparceness > 0) 
				{
					for (int i = 0; i < FloorsMax; i++) {
						GeneratePaths(GetSize(), 
					              new Vector3(StartLocation.x, StartLocation.y, StartLocation.z + (MazeWallHeight + 1)*i), 
					             			PathBlockType, 
					              			Sparceness, 
					              			Linearity);
					}
				}
				if (IsMazeRoof)
				GeneratePathWalls(PathBlockType, 
					              MazeWallHeight,
				                  MazeWallType, 
					              MazeRoofType);
				//RemoveSinglePillars(MyBlocks, Size, PathBlockType);
				//GenerateSpawnLocations(MyBlocks, Size);
				/*if (IsFillEmpty) {
					FillEmptyPath(Size, PathBlockType, MazeWallType);
				}*/
			}
			if (IsRandomizeStartPosition)
				FindStartPosition (GetSize());
		}

		public void FindStartPosition(Int3 Size)
        {
			for (int i = 0; i < 1000; i++) {
				Vector3 CheckPosition = new Vector3(Random.Range (0, Size.x), Random.Range (0, Size.x), Random.Range (0, Size.x));
				if (IsVoidAxisX)
					CheckPosition.x = StartLocation.x;
				if (IsVoidAxisY)
					CheckPosition.y = StartLocation.y;
				if (IsVoidAxisZ)
					CheckPosition.z = StartLocation.z;
				if (GetBlockType(CheckPosition.ToInt3()) == PathBlockType 
				   && GetBlockType(CheckPosition.ToInt3() + new Int3(0,1,0)) == 0)
                {
					SpawnPosition = CheckPosition + new Vector3(0,1.5f,0);
					Debug.Log("Found new position for spawning: " + SpawnPosition.ToString());
					break;
				}
			}
		}
		void GenerateSpawnLocations(Vector3 Size) {
			//MyBlocks.UpdateBlock(StartLocation, SpawnBlockType);
			int MaxBotSpawnBlocks = MaxSpawnPoints;
			int BotSpawnBlocksCount = 0;
			bool bIsBotSpawns = true;
			if (bIsBotSpawns) {
				while (BotSpawnBlocksCount <= MaxBotSpawnBlocks) {
					int i = Mathf.RoundToInt(Random.Range(0f, Size.x-1f));
					int j = Mathf.RoundToInt(Random.Range(0f, Size.y-1f));
					int k = Mathf.RoundToInt(Random.Range(0f, Size.z-1f));
					if (IsVoidAxisZ)
						k = (int)StartLocation.z;
					if (IsVoidAxisY)
						j = (int)StartLocation.y;				
					if (IsVoidAxisX)
						i = (int)StartLocation.x;
					if (GetBlockType(new Int3(i, j, k)) == PathBlockType)
                    {
						SpawnLocations.Add (new Vector3(i,j,k));
						//MyBlocks.UpdateBlock(new Vector3(i, j, k), SpawnBlockType);
						BotSpawnBlocksCount++;
					}
				}
			}
		}


		public void GenerateWallsOnEdge(int MazeWallType)
        {
			for (int i = 0; i < GetSize().x; i++)
            {
                for (int k = 0; k < GetSize().z; k++)
                {
                    if (IsOnEdge(new Vector3(i, StartLocation.y, k), GetSize()) && GetBlockType(new Int3(i, StartLocation.y, k)) == 0)
                    {
                        for (int z = 0; z < MazeWallHeight + 1; z++)
                        {
                            UpdateBlock(new Int3(i, 0 + z, k), MazeWallType);
                        }
                    }
                }
            }
				//for (int j = 0; j < Size.y; j++)
		}

		public void FillEmptyPath(Vector3 Size, int PathBlockType, int FillBlockType)
        {
			int j = Mathf.RoundToInt (StartLocation.y);
			for (int i = 0; i < Size.x; i++)
            {
                for (int k = 0; k < Size.z; k++)
                {
                    if (GetBlockType(new Int3(i, j, k)) == 0)
                    {

                        for (int z = 0; z < MazeWallHeight + 1; z++)
                        {
                            UpdateBlock(new Int3(i, j + z, k), FillBlockType);
                        }
                    }
                }
            }
		}
		
		bool IsOnEdge(Vector3 Position, Int3 Size)
        {
			if (Position.x == Size.x - 1 || Position.x == 0 
			    // ||Position.y == Size.y - 1 || Position.y == 0
			    || Position.z == Size.z - 1 || Position.z == 0
			    )
				return true;
			return false;
		}

		// Runs the maze like algorithm, from starting points of biomes, until a biome type covers the whole surface
		// render it to texture to see
		// generate paths from multiple positionsh
		void GenerateBiomeMap(Int3 Size)
        {
			/*Vector3 MinimumDistanceToOthers;
			Vector3 MaximumDistanceToOthers;
			List<Vector3> StartingLocations;
			int MinimumBiomes = 2;
			int MaximumBiomes = 5;
			int NumberOfBiomes = 3;
			for (int i = 0; i < NumberOfBiomes; i++) {
				if (IsLockedAxis)
					StartingLocations.Add(new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), StartLocation.z));
				else
					StartingLocations.Add(new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z)));
			}
			CurrentSparce = 0;
			for (int i = 0; i < StartingLocations.Count; i++) {
				GeneratePaths(MyBlocks, Size, Dungeon, StartingLocations[i], PathBlockType + i, (Sparceness)*(i + 1), Linearity);
			}
			*/
		}

		int CheckedBlocks = 0;
		bool HasFoundNewPosition = false;
		void CheckForNewPosition()
        {

		}
		// Need a way that gives a maximum amount of up/down blocks at once
		// maybe it should remember the last few directions
		// no it shou	ld create a direction map lol.
		void GeneratePaths(Int3 Size, Vector3 StartLocation,  int PathBlockType, float Sparceness, float Linearity)
        {
			// set defaults
			
			//if (bIsEdge && IsPositionOutsideBounds(StartLocation, Size, bIsEdge)) {
			//	if (StartLocation.y == 0) StartLocation.y = 1;
			//}
			    Debug.Log ("Generating paths!");
			DebugDirections.Clear();
			Iterations = 0;
			IsSparceEnough = false; 
			CurrentSparce = 0;
			MaxSparce = 1;
			if (!IsVoidAxisX)
				MaxSparce *= Size.x;
			if (!IsVoidAxisY)
				MaxSparce *= Size.y;
			if (!IsVoidAxisZ)
				MaxSparce *= Size.z;
			//MaxSparce = Size.x*Size.z;
			//PathSizeMinimum = PathSizeMinimum;
			if (PathSizeMinimum.x < 1) PathSizeMinimum.x = 1;
			if (PathSizeMinimum.y < 1) PathSizeMinimum.y = 1;
			if (PathSizeMinimum.z < 1) PathSizeMinimum.z = 1;
			if (PathSizeMaximum.x < 1) PathSizeMaximum.x = 1;
			if (PathSizeMaximum.y < 1) PathSizeMaximum.y = 1;
			if (PathSizeMaximum.z < 1) PathSizeMaximum.z = 1;
			PathSize = new Vector3 (Random.Range (PathSizeMinimum.x, PathSizeMaximum.x), 1, Random.Range (PathSizeMinimum.z, PathSizeMaximum.z));
			DebugPositions.Clear ();
			Position = StartLocation;
			PathDirection = 0;
            // starting block
            UpdateBlock(Position.ToInt3(), PathBlockType);
            CheckSparce(PathBlockType);
			while (!IsSparceEnough)
            {
				IteratePath(Size, StartLocation, PathBlockType, Sparceness, Linearity);
			}
		}

		public bool IsTypeDungeonBlock(int Type)
        {
			return (Type != 0 && Type != MazeWallType);
		}

		private void IteratePath(Int3 Size, Vector3 StartLocation, int PathBlockType, float Sparceness, float Linearity)
        {

			List<Vector3> Positions = new List<Vector3>();
			// Check for Sparce Percentage
			int TurnChance = Mathf.RoundToInt(Random.Range(1f, 100f));
			if (PathDirection <= 0 || TurnChance >= Linearity) {
				// Randomly Make a new direction	
				if (PathDirection == 0) {
					HasFoundNewPosition = false;
					CheckedBlocks = 0;
					int FindPositionBlock = 200;
					int MaximumChecksForNewPosition = 0;
					while (!HasFoundNewPosition || CheckedBlocks == MaxSparce || MaximumChecksForNewPosition >= 250)
                    {
						MaximumChecksForNewPosition++;
						Position.x = Mathf.RoundToInt(Random.Range(1, Size.x-2));
						Position.z = Mathf.RoundToInt(Random.Range(1, Size.z-2));
						if (IsTypeDungeonBlock(GetBlockType(Position.ToInt3())))
                        {
							HasFoundNewPosition = true;
						}
                        else
                        {
							/*if (MyBlocks.GetBlockType(Position) != FindPositionBlock) {
								CheckedBlocks++;
								MyBlocks.UpdateBlock(Position, FindPositionBlock);
							}*/
						}
					}
					if (MaximumChecksForNewPosition >= 250)
                    {
						Debug.LogError("Wow exceded maximum checks..");
						Position = StartLocation;
					}
					/*for (int i = 0; i < Size.x; i++)
						for (int j = 0; j < Size.z; j++)
					{
						Vector3 NewPosition = new Vector3(i, Position.y, j);
						if (MyBlocks.GetBlockType(NewPosition) == FindPositionBlock)
							MyBlocks.UpdateBlock(Position, 0);
					}*/
				}
				// Search for a new place in the maze, where the path already is, and start digging again
				List<int> PossiblePathDirection = new List<int>();

				if (!IsVoidAxisX)
                {
					PossiblePathDirection.Add (1);
					PossiblePathDirection.Add (2);
				}
				if (!IsVoidAxisY)
                {
					PossiblePathDirection.Add (3);
					PossiblePathDirection.Add (4);
				}
				if (!IsVoidAxisZ)
                {
					PossiblePathDirection.Add (5);
					PossiblePathDirection.Add (6);
				}
				if (PossiblePathDirection.Count > 0)
                {
					int PathDirectionIndex = Mathf.RoundToInt(Random.Range(0f, PossiblePathDirection.Count-1f));
					PathDirection = PossiblePathDirection[PathDirectionIndex];
				}
				//PathDirection = (int)(Random.Range(1,4));
				//Debug.Log ("PathDirection: " + PathDirection + " : Iteration: " + Iterations);
				DebugDirections.Add (PathDirection);
			}
			if (PathDirection == 1)
            {	// going right
				Position.x++;
				if (bIsMoveToSize)
					Position.x += PathSize.x;
			}
			else if (PathDirection == 2)
            {	// going left
				Position.x--;
				if (bIsMoveToSize)
					Position.x -= PathSize.x;
			}
			else if (PathDirection == 3)
            {	// going up
				Position.y++;
				if (bIsMoveToSize)
					Position.y += PathSize.y;
			}
			else if (PathDirection == 4)
            {	// going down
				Position.y--;
				if (bIsMoveToSize)
					Position.y -= PathSize.y;
			}
			else if (PathDirection == 5)
            {	// going forward
				Position.z++;
				if (bIsMoveToSize)
					Position.z += PathSize.z;
			}
			else if (PathDirection == 6)
            {	// going back
				Position.z--;
				if (bIsMoveToSize)
					Position.z -= PathSize.z;
			}
			Positions.Clear();
			for (float i = -PathSize.x; i < PathSize.x + 1; i++)
				for (float j = -PathSize.y; j < PathSize.y + 1; j++)
				for (float k = -PathSize.z; k < PathSize.z + 1; k++)
                    {
					Positions.Add(new Vector3(Position.x + i, Position.y + j, Position.z + k));
					//DebugPositions.Add (new Vector3(Position.x + i, Position.y + j, Position.z + k));
				}
			//DebugDirections.Add (PathDirection);

			// Checks to Stop Growing of Dungeon!
			// now here if Moving position, do checks on blocks etc for pathways
			// if blocks on left and right are not taken, then build path, else don't build path
			if (bIsPathways) 
			{
				// this is now up down
				//Vector3 BackPosition = new Vector3(Position.x, Position.y - 1 - PathSize.y, Position.z);
				//Vector3 ForwardPosition = new Vector3(Position.x, Position.y + 1 + PathSize.y, Position.z);
				if (PathDirection == 3 || PathDirection == 4)
                {
					//if (IsStopPath(MyBlocks.GetBlockType(BackPosition)) || MyBlocks.GetBlockType(ForwardPosition) == PathBlockType) {
						//PathDirection = 0;
					//}
				}

				Vector3 LeftPosition = new Vector3(Position.x - 1 - PathSize.x, Position.y, Position.z);
				Vector3 RightPosition = new Vector3(Position.x + 1 + PathSize.x, Position.y, Position.z);
				
				if (PathDirection == 5 || PathDirection == 6) {
					if (IsStopMaze(GetBlockType(LeftPosition.ToInt3())) || IsStopMaze(GetBlockType(RightPosition.ToInt3())))
                    {
						PathDirection = 0;
					}
				}
				// forward, back
				Vector3 BottomPosition = new Vector3(Position.x, Position.y, Position.z - 1 - PathSize.z);
				//Vector3 TopPosition = new Vector3(Position.x, Position.y, Position.z + 1 + PathSize.z);
				if (PathDirection == 1 || PathDirection == 2)
                {
						if (IsStopMaze(GetBlockType(BottomPosition.ToInt3()))  || IsStopMaze(GetBlockType(BottomPosition.ToInt3())))
                    {
						PathDirection = 0;
					}
				}
			}

			// Now do checks for positions intersecting with path
			if (IsPositionOutsideBounds(Position, Size, bIsEdge))
            {
				if (!bIsWrap)
                {
					//PathDirection = -1;	// Maybe I should just force the direction to turn rather then just end it like this
					PathDirection = 0;	// Maybe I should just force the direction to turn rather then just end it like this
					if (Position.x > Size.x - 1) Position.x = Size.x - 1;
					if (Position.x < 0) Position.x = 0;
					if (Position.y > Size.y - 1) Position.y = Size.y - 1;
					if (Position.y < 0) Position.y = 0;
					if (Position.z > Size.z - 1) Position.z = Size.z - 1;
					if (Position.z < 0) Position.z = 0;
				}
				else {
					PathDirection = 0;	// Maybe I should just force the direction to turn rather then just end it like this
					if (Position.x > Size.x - 1) Position.x = 0;
					if (Position.x < 0) Position.x = Size.x - 1;
					if (Position.y > Size.y - 1) Position.y = 0;
					if (Position.y < 0) Position.y = Size.y - 1;
					if (Position.z > Size.z - 1) Position.z = 0;
					if (Position.z < 0) Position.z = Size.z - 1;
				}
			}

			DebugDirections.Add (PathDirection);
			if (PathDirection > 0)
            {
				for (int z = 0; z < Positions.Count; z++)
                {
					if (GetBlockType(Positions[z].ToInt3()) == 0)
                    {
						//MyBlocks.UpdateBlock(Positions[z], PathBlockType);
						UpdateBlock(Positions[z].ToInt3(), Size, PathBlockType);

					}
				}
			}
			//Debug.Log (Iterations + " : PositionsCount: " + Positions.Count);
			
			Iterations++;
			if (Iterations >= MaxIterations)
            {
				IsSparceEnough = true;
			}
			SparcePercentage = (CurrentSparce / MaxSparce);
			if (100f * SparcePercentage > Sparceness)
            {
				IsSparceEnough = true;
				Debug.Log ("Is Sparce Enough: " + 100f * SparcePercentage + " Percent.");
			}
		}

		bool IsPositionOutsideBounds(Vector3 BoundsPosition, Int3 Size, bool IsEdge)
        {
			bool IsOutOfBounds = false;
			if (!IsVoidAxisX)
				if (BoundsPosition.x >= Size.x - PathSize.x -1 || BoundsPosition.x <= PathSize.x+1)
					IsOutOfBounds = true;
			if (!IsVoidAxisY)
				if (BoundsPosition.y >= Size.y - PathSize.y - 1 || BoundsPosition.y <= PathSize.y + 1)
					IsOutOfBounds = true;
			if (!IsVoidAxisZ)
				if (BoundsPosition.z >= Size.z - PathSize.z - 1 || BoundsPosition.z <= PathSize.z + 1)
				IsOutOfBounds = true;
			/*if (!IsEdge)
				return (BoundsPosition.x > Size.x - 1 || BoundsPosition.x < 0 ||
			        BoundsPosition.y > Size.y - 1 || BoundsPosition.y < 0 ||
			        BoundsPosition.z > Size.z - 1 || BoundsPosition.z < 0);
			else
				return (BoundsPosition.x > Size.x - 2 || BoundsPosition.x < 1 ||
				        BoundsPosition.y > Size.y - 2 || BoundsPosition.y < 1 ||
				        BoundsPosition.z > Size.z - 2 || BoundsPosition.z < 1);*/
			return IsOutOfBounds;
		}

		public bool IsPathOverride = false;	// if override, then only stop path on path blocks, else stop on any other block type thats solid
		public bool IsStopMaze(int BlockType) {
			if (BlockType == 0) {
				return false;
			} else if (!IsPathOverride)
					return true;
			else if (IsPathOverride && !IsTypeDungeonBlock(BlockType)) // stops for anything but itself
				return true;
			else
				return false;
		}
		
		// Searches the Blocks for a empty block, that is surrounded by 4 path blocks, then it removes it or filles it with a special pillar looking block
		void RemoveSinglePillars(Vector3 Size, int PathBlockType) {
			if (bIsRemoveSinglePillars)
				for (int i = 0; i < Size.x; i++)
					for (int j = 0; j < Size.y; j++)
						for (int k = 0; k < Size.z; k++)
					{
						if (GetBlockType(new Int3(i, j, k)) == 0)
                            {
							if (GetBlockType(new Int3(i + 1, j, k)) == PathBlockType && GetBlockType(new Int3(i - 1, j, k)) == PathBlockType &&
							    GetBlockType(new Int3(i, j + 1, k)) == PathBlockType && GetBlockType(new Int3(i, j - 1, k)) == PathBlockType)
                                {
                                    UpdateBlock(new Int3(i, j, k + MazeWallHeight), PathBlockType);
                                }
						}
					}
		}
		
		public void SetDefaultColors() {
			BlockColors.Clear ();
			BlockColors.Add (new Color32(125,165,165,255));
			BlockColors.Add (new Color32(200,200,200,255));	// cobblestone
			BlockColors.Add (new Color32(55,255,255,255));	// grass
			BlockColors.Add (new Color32(66,66,66,255));	// brick - 3 
			BlockColors.Add (new Color32(255,0,0,255));		// red brick - 4
			BlockColors.Add (new Color32(55,155,55,255));		// Hexgon brick - 4
			BlockColors.Add (new Color32(255,255,55,255));		// 1,0 brick - 4
			BlockColors.Add (new Color32(155,155,55,255));		// 1,0 brick - 4
			for (int i = BlockColors.Count; i < 64; i++) {
				BlockColors.Add (new Color32((byte)(i*25),(byte)(i*15),(byte)(i*5),255));
			}
		}
		/*public Texture2D GetMazeTexture() {
			SetDefaultColors ();
			Debug.Log ("Generating Texture.");
			//MeshRenderer MyRenderer = MazeTextureObject.GetComponent<MeshRenderer> ();
			
			// duplicate the original texture and assign to the material
			Texture2D MyTexture2D = new Texture2D (Mathf.FloorToInt(MyBlocks.Size.x), 
			                                       Mathf.FloorToInt(MyBlocks.Size.z));	//MyRenderer.material.mainTexture);
			//MyRenderer.materials[0].SetTexture(0, MyTexture2D);
			//MyRawImage.texture = (MyTexture2D);
			MaxMipMapLevel = MyTexture2D.mipmapCount;
			MyTexture2D.filterMode = FilterMode.Point;
			int MipMapLevel = 0;
			//for (int z = MaxMipMapLevel; MaxMipMapLevel < MaxMipMapLevel; MaxMipMapLevel++) {
			MyTexture2D.Resize (Mathf.RoundToInt (MyBlocks.Size.x), Mathf.RoundToInt (MyBlocks.Size.z));
			//Texture2D MyTexture2D = MyTexture;
			Color32[] NewColors = MyTexture2D.GetPixels32 (MipMapLevel);
			for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				int PixelIndex = Mathf.RoundToInt (i * MyBlocks.Size.x + k);
				NewColors[PixelIndex] = BlockColors[MyBlocks.GetBlockType (new Vector3 (i, StartLocation.y, k))];
			}
			MyTexture2D.SetPixels32 (NewColors, MipMapLevel);
			MyTexture2D.Apply( true );
			Debug.Log ("Applied Texture.");
			return MyTexture2D;
		}*/
	}
}


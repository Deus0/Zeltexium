using UnityEngine;
using System.Collections.Generic;
/*
namespace OldCode {
// used to store blocks - the raw format - the extended version will be the block structures
[System.Serializable]
public class MyBlocksSaver {
	
}

[System.Serializable]
public class MyBlockStructureSaver {
	
}

[System.Serializable]
public enum StructureType {
	Room,
	Building,
	Dungeon,
	Tree,
	Sphere,
	//Table,
	//Sign,
	Tower1,
	Tower2,
	TownHall
	//Tower3
};

// mostly has functions that create shapes
[System.Serializable]
public class BlockStructure	{
	public string Name = "NewBlockStructure";
	public bool HasInitiated = false;
	public Blocks MyBlocks = new Blocks();
	public StructureType MyType = StructureType.Room;
	public List<int> MyBlockTypes = new List<int>();
	public List<int> BlockStructureTypeSizes = new List<int>();		// lists the size of each block structure

	public BlockStructure() {
		MyBlocks.Size = new Vector3 (12, 20, 12);
		MyBlocks.InitilizeData ();
		BlockStructureTypeSizes.Add (5);
		BlockStructureTypeSizes.Add (6);
		BlockStructureTypeSizes.Add (0);
		BlockStructureTypeSizes.Add (2);
		BlockStructureTypeSizes.Add (1);
		BlockStructureTypeSizes.Add (3);
		BlockStructureTypeSizes.Add (3);
	}
	
	public BlockStructure(string DefaultType) {
		if (DefaultType == "Tree") {
			MyBlocks.Size = new Vector3 (6, 9, 6);
			MyType = StructureType.Tree;
		}
		MyBlocks.InitilizeData ();
		UpdateBlockStructureWithType ();
	}
	public BlockStructure (Vector3 Size) {
		MyBlocks.Size = Size;
		MyBlocks.InitilizeData ();
	}
	public void Reset() {
		MyBlocks.InitilizeData ();
	}

	public void UpdateBlockStructureWithType() {
		if (MyType == StructureType.Room)
			Room (MyBlockTypes [0], MyBlockTypes [1], MyBlockTypes [2], MyBlockTypes [3], MyBlockTypes [4]);
		else if (MyType == StructureType.Building) {
			if (MyBlockTypes.Count >= 8)
				Building (MyBlockTypes [0], MyBlockTypes [1], MyBlockTypes [2], MyBlockTypes [3], MyBlockTypes [4], MyBlockTypes [5], MyBlockTypes [6], MyBlockTypes [7]);
		} else if (MyType == StructureType.Dungeon)
			;//Building(9,4,8,0,1);	// Dungeon()
		else if (MyType == StructureType.Tree)
			Tree (MyBlockTypes [0], MyBlockTypes [1]);
		else if (MyType == StructureType.Sphere)
			Sphere (MyBlockTypes [0]);
		else if (MyType == StructureType.Tower1)
			Tower1 (MyBlockTypes [0], MyBlockTypes [1], MyBlockTypes [2]);
		else if (MyType == StructureType.Tower2)
			Tower2 (MyBlockTypes [0], MyBlockTypes [1], MyBlockTypes [2]);
		else if (MyType == StructureType.TownHall)
			TownHall ();
		else Debug.Log ("Trying to build a block Structure I havn't coded for yet!");
	}

	
	// builds it the same size as the blockstructure
	public void Tower1(int FloorType, int WallType, int RoofType) {
		Tower1(new Vector3(), new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z),
		     FloorType, WallType, RoofType);
	}
	// used to add rooms onto an already made structure
	public void Tower1(Vector3 RoomLocation, Vector3 RoomSize, int FloorType, int WallType, int RoofType) {
		float MinX = RoomLocation.x;
		float MaxX = RoomLocation.x+RoomSize.x;
		float MinY = RoomLocation.y;
		float MaxY = RoomLocation.y+RoomSize.y;
		float MinZ = RoomLocation.z;
		float MaxZ = RoomLocation.z+RoomSize.z;
		float BuildingTopStart = MaxY - 3;	// this is where the castle bit starts
		int PillarType = 6;
		for (float i = MinX; i < MaxX; i++)
			for (float j = MinY; j < MaxY; j++)
			for (float k = MinZ; k < MaxZ; k++) {
				if (j == MinY)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
				else if (j == BuildingTopStart)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);
				if (j == BuildingTopStart +1) {
					if (((i == MinX || i == MaxX-1f) && k % 2 == 0) || ((k == MinZ || k == MaxZ-1f) && i % 2 == 0))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);
				}
				if (j < BuildingTopStart && j > MinY) {	
					// pillars
					if ((i == MinX && k == MinZ) || (i == MinX && k == MaxZ-1f) || (i == MaxX-1f && k == MinZ) || (i ==  MaxX-1f && k == MaxZ-1f))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), PillarType);
					// walls
					else if (i == MinX || i ==  MaxX-1f || k == MinZ || k == MaxZ-1f)
						MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);
				}
				
			}
	}
	public void AddNoise(float RandomPercentage) {
		RandomPercentage = Mathf.Clamp (RandomPercentage, 0, 1f);
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++)
			{
				int BlockType = MyBlocks.GetBlockType(new Vector3(i,j,k));
				if (BlockType != 0) {
					if (Random.Range(1,100) >= 100f-RandomPercentage*100f) {
						if (MyBlocks.GetBlockType (new Vector3(i,j+1,k)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i,j+1,k), BlockType+1);
						if (MyBlocks.GetBlockType (new Vector3(i,j,k)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i,j,k), BlockType+1);
						if (MyBlocks.GetBlockType (new Vector3(i+1,j,k)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i+1,j,k), BlockType+1);
						if (MyBlocks.GetBlockType (new Vector3(i-1,j+1,k)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i-1,j+1,k), BlockType+1);
						if (MyBlocks.GetBlockType (new Vector3(i,j,k+1)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i,j,k+1), BlockType+1);
						if (MyBlocks.GetBlockType (new Vector3(i,j+1,k-1)) == 0)
							MyBlocks.UpdateBlock(new Vector3(i,j+1,k-1), BlockType+1);
					}
				}
			}
	}
	// builds it the same size as the blockstructure
	public void Tower2(int FloorType, int WallType, int RoofType) {
		Tower2(new Vector3(), new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z),
		       FloorType, WallType, RoofType);
	}
	// used to add rooms onto an already made structure
	public void Tower2(Vector3 RoomLocation, Vector3 RoomSize, int FloorType, int WallType, int RoofType) {
		float MinX = RoomLocation.x;
		float MaxX = RoomLocation.x+RoomSize.x;
		float MinY = RoomLocation.y;
		float MaxY = RoomLocation.y+RoomSize.y;
		float MinZ = RoomLocation.z;
		float MaxZ = RoomLocation.z+RoomSize.z;
		float BuildingTopStart = MaxY - 6;	// this is where the castle bit starts
		int PillarType = 14;
		for (float i = MinX; i < MaxX; i++)
			for (float j = MinY; j < MaxY; j++)
			for (float k = MinZ; k < MaxZ; k++) {
				if (j == MinY)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
				else if (j == BuildingTopStart)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);
				if (j >= BuildingTopStart +1) {	// triangle top
					float Difference = j-BuildingTopStart;
					//if (((i == MinX || i == MaxX-1f) && k % 2 == 0) || ((k == MinZ || k == MaxZ-1f) && i % 2 == 0))
					if (( i >= MinX+Difference) && ( i <= MaxX-Difference-1) && ( k >= MinZ+Difference) && ( k <= MaxZ-Difference-1))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);
				}
				if (j < BuildingTopStart && j > MinY) {	
					int IsBlock = Random.Range (1,100);
					// pillars
					if ((i == MinX && k == MinZ) || (i == MinX && k == MaxZ-1f) || (i == MaxX-1f && k == MinZ) || (i ==  MaxX-1f && k == MaxZ-1f))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), PillarType);
					// walls
					else if (j > MaxY/2f && (i == MinX || i ==  MaxX-1f || k == MinZ || k == MaxZ-1f))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);
					else if (j == MaxY/2f)
						MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);
					else if (IsBlock > 20 && j < MaxY/2f && (i == MinX || i ==  MaxX-1f || k == MinZ || k == MaxZ-1f))
						MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);
				}
				
			}
	}
	bool IsTransparentDefault = false;
	
	public bool UpdateTerrainAtPosition(RaycastHit RayHit) {
		return UpdateTerrainAtPosition (RayHit, IsTransparentDefault);
	}

	public bool UpdateTerrainAtPosition(RaycastHit RayHit, bool IsTransparent) {
		Vector3 MouseHit = Terrain.GetBlockPosV (RayHit);
		return UpdateTerrainAtPosition (MouseHit, RayHit.collider.GetComponent<Chunk> ().world, IsTransparent);
	}
	public bool UpdateTerrainAtPosition(World HitWorld, Vector3 RayHit) {
		Vector3 MouseHit = Terrain.GetBlockPosV (RayHit);
		return UpdateTerrainAtPosition (MouseHit, HitWorld, IsTransparentDefault);
	}
	public bool UpdateTerrainAtPosition(Vector3 MouseHit, World HitWorld, bool IsTransparent) {
		bool CanCreateBuilding = GetManager.GetZoneManager ().PlaceBuildingZone (MouseHit - new Vector3 (0.5f, 0.5f, 0.5f), MyBlocks.Size);
		if (!CanCreateBuilding)
			return false;

		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++)
			{
				Vector3 TemporaryPoint = new Vector3(MouseHit.x, MouseHit.y, MouseHit.z);
				TemporaryPoint =  new Vector3(MouseHit.x + i, MouseHit.y + j, MouseHit.z + k);
				if (MyBlocks.GetBlockType (new Vector3(i,j,k)) != 0)
					Terrain.SetBlock(HitWorld, TemporaryPoint, new Block(MyBlocks.GetBlockType (new Vector3(i,j,k))));
				if (!IsTransparent)
					if (MyBlocks.GetBlockType (new Vector3(i,j,k)) == 0)
						Terrain.SetBlock(HitWorld, TemporaryPoint, new BlockAir());	// cobble stone
			}
		return true;
	}


	public bool UpdateChunkAtPosition(Chunk chunk, Vector3 NewPosition) {
		bool IsTransparent = false;
		Fill (5);
		Debug.LogError (" BlockSize: " + MyBlocks.Size.ToString () + " : NewPosition: " + NewPosition.ToString ()); 
		Debug.Break ();
		for (int i = Mathf.RoundToInt(NewPosition.x); i < Mathf.RoundToInt(NewPosition.x)+MyBlocks.Size.x; i++)
			for (int j = Mathf.RoundToInt(NewPosition.y); j < Mathf.RoundToInt(NewPosition.y)+MyBlocks.Size.y; j++)
				for (int k = Mathf.RoundToInt(NewPosition.z); k < Mathf.RoundToInt(NewPosition.z)+MyBlocks.Size.z; k++)
			{
				chunk.SetBlock (i,j,k,new BlockGrass());

				//if (!IsTransparent)
				//	if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 0)
				//		Terrain.SetBlock(chunk.world, i,j,k, new BlockAir(), false);	// cobble stone
				if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 1)
					chunk.SetBlock (i,j,k,new Block());
					//Terrain.SetBlock(chunk.world, i,j,k, new Block(), false);	// cobble stone
				else if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 2)
					chunk.SetBlock (i,j,k,new BlockGrass());
					//Terrain.SetBlock(chunk.world, i,j,k, new BlockGrass(), false);
				else if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) >= 3)
					chunk.SetBlock (i,j,k,new Block(MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k))));
				//Terrain.SetBlock(chunk.world, i,j,k, new Block(MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k))), false);
			}
		return true;
	}

	public void UpdateTerrain(World InWorld) {
		bool IsTransparent = false;
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++)
			{
				//RaycastHit NewRayCast = new RaycastHit();
				//NewRayCast.normal = new Vector3(RayHit.normal.x,RayHit.normal.y,RayHit.normal.z);
				//Debug.Log (NewRayCast.point.ToString ());
				if (!IsTransparent)
					if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 0)
						Terrain.SetBlock(InWorld, i, j, k, new BlockAir(), false);	// cobble stone
				
				if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 1)
					Terrain.SetBlock(InWorld,i, j, k, new Block(), false);	// cobble stone
				else if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) == 2)
					Terrain.SetBlock(InWorld,i, j, k, new BlockGrass(), false);	// cobble stone
				else if (MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k)) >= 3)
					Terrain.SetBlock(InWorld,i, j, k, new Block(MyBlocks.GetBlockType (new UnityEngine.Vector3(i,j,k))), false);	// cobble stone
			}
	}

	public void Fill(int Type) {
		int FillType = Type;
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
				for (int k = 0; k < MyBlocks.Size.z; k++) {
				MyBlocks.UpdateBlock (new Vector3(i,j,k), FillType);
				}
	}

	public void Empty() {
		for (int i = 0; i < MyBlocks.Size.x; i++)
			for (int j = 0; j < MyBlocks.Size.y; j++)
			for (int k = 0; k < MyBlocks.Size.z; k++) {
				MyBlocks.UpdateBlock (new Vector3(i,j,k), 0);
			}
	}
	
	public void Sphere(int FillType) {
		Sphere (FillType, 1);
	}
	public void Sphere(int FillType, float Scale) {
		float Radius = MyBlocks.Size.x/2f;
		Radius *= Scale;
		Vector3 MiddlePoint = new Vector3 (MyBlocks.Size.x / 2f, MyBlocks.Size.y / 2f, MyBlocks.Size.z / 2f);
		for (float i = 0f; i < MyBlocks.Size.x; i++)
			for (float j = 0f; j < MyBlocks.Size.y; j++)
				for (float k =  0f; k < MyBlocks.Size.z; k++)
			{
				Vector3 BlockPosition = new Vector3(i,j,k);
				if (Vector3.Distance (BlockPosition, MiddlePoint) < Radius)
				{
					MyBlocks.UpdateBlock (BlockPosition, FillType);
				}
			}
	}

	public void WeirdSphere(int FillType, float Scale) {
		for (float i = MyBlocks.Size.x/2f - MyBlocks.Size.x*Scale/2f; i <MyBlocks.Size.x/2f + MyBlocks.Size.x*Scale/2f; i++)
			for (float j = MyBlocks.Size.y/2f - MyBlocks.Size.y*Scale/2f; j <MyBlocks.Size.y/2f + MyBlocks.Size.y*Scale/2f; j++)
				for (float k =  MyBlocks.Size.z/2f - MyBlocks.Size.z*Scale/2f; k <MyBlocks.Size.z/2f + MyBlocks.Size.z*Scale/2f; k++)
			{
				if (Vector3.Distance (new Vector3(i,j,k), new Vector3(MyBlocks.Size.x/2, MyBlocks.Size.y/2, MyBlocks.Size.z/2)) < MyBlocks.Size.x*Scale/2f)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FillType);
			}
	}

	public void Cylinder(int FillType, float Scale) {
		float Radius = MyBlocks.Size.x/2f;
		Radius *= Scale;
		for (float i = 0f; i < MyBlocks.Size.x; i++)
			for (float j = 0f; j < MyBlocks.Size.y; j++)
			for (float k =  0f; k < MyBlocks.Size.z; k++)
			{
				if (Vector3.Distance (new Vector3(i,j,k), new Vector3(MyBlocks.Size.x/2, j, MyBlocks.Size.z/2)) < Radius)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FillType);
			}
	}
	
	// builds it the same size as the blockstructure
	public void Tree(int WoodType, int LeavesType) {
		Tree(new Vector3(), new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z),
		     WoodType, LeavesType);
	}
	// generic tree
	public void Tree(Vector3 Position, Vector3 Size, int WoodType, int LeavesType) {
		float MinX = Position.x;
		float MaxX = Position.x+Size.x;
		float MinY = Position.y;
		float MaxY = Position.y+Size.y;
		float MinZ = Position.z;
		float MaxZ = Position.z+Size.z;
		float WoodHeight = (MaxY-MinY)/2.0f;
		int WoodPositionX = Mathf.RoundToInt ((MaxX-MinX)/2.0f); 	
		int WoodPositionZ = Mathf.RoundToInt ((MaxZ-MinZ)/2.0f);
		for (float i = MinX; i < MaxX; i++)
			for (float j = MinY; j < MaxY; j++)
			for (float k = MinZ; k < MaxZ; k++) {
				if (j < WoodHeight) {
					if (i == WoodPositionX && k == WoodPositionZ)
						MyBlocks.UpdateBlock (new Vector3(i,j,k), WoodType);
				}
				else {
					MyBlocks.UpdateBlock (new Vector3(i,j,k), LeavesType);
				}
			}
	}
	// builds it the same size as the blockstructure
	public void Room(int FloorType, int WallType, int RoofType, int DoorType, int DoorSize) {
		Room(new Vector3(), new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z),
		     FloorType, WallType, RoofType, DoorType, DoorSize);
	}
	// used to add rooms onto an already made structure
	public void Room(Vector3 RoomLocation, Vector3 RoomSize, int FloorType, int WallType, int RoofType, int DoorType, int DoorSize) {
		float MinX = RoomLocation.x;
		float MaxX = RoomLocation.x+RoomSize.x;
		float MinY = RoomLocation.y;
		float MaxY = RoomLocation.y+RoomSize.y;
		float MinZ = RoomLocation.z;
		float MaxZ = RoomLocation.z+RoomSize.z;
		for (float i = MinX; i < MaxX; i++)
			for (float j = MinY; j < MaxY; j++)
			for (float k = MinZ; k < MaxZ; k++) {
				if (j == MinY)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
				else if (j == MaxY-1)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);

				else if (i-MinX  >= ((MaxX-MinX )/2f)-DoorSize/2f && i-MinX <= ((MaxX-MinX)/2f)+DoorSize/2f-1 && k == MinZ)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), DoorType);

				else if (i == MinX || i ==  MaxX-1f || k == MinZ || k == MaxZ-1f)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);

			}
	}
	public void TownHall() {
		Vector3 BuildingLocation = new Vector3 ();
		Vector3 BuildingSize = new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z);
		float MinX = BuildingLocation.x;
		float MaxX = BuildingLocation.x+BuildingSize.x-1;
		float MinY = BuildingLocation.y;
		float MaxY = BuildingLocation.y+BuildingSize.y-1;
		float MinZ = BuildingLocation.z;
		float MaxZ = BuildingLocation.z+BuildingSize.z-1;
		MyBlocks.UpdateBlock (new Vector3(MaxX/2, 1, MaxZ/2), 9);
		for (int i = 0; i <= MaxX; i++) {
			for (int k = 0; k <= MaxZ; k++) {
				MyBlocks.UpdateBlock (new Vector3(i, 0, k), 2);
			}
		}
	}
	public void Building(int FloorType, int WallType, int RoofType, int DoorType, int DoorSize, int WindowType, int OuterDoorType, int BuildingRooms) {
		Building(new Vector3(), new Vector3(MyBlocks.Size.x,MyBlocks.Size.y,MyBlocks.Size.z),
		         FloorType, WallType, RoofType, DoorType, DoorSize, WindowType,	OuterDoorType, BuildingRooms);
	}
	public void Building(Vector3 BuildingLocation, Vector3 BuildingSize, int FloorType, int WallType, int RoofType, int DoorType, int DoorSize, int WindowType, int OuterDoorType, int BuildingRooms) {
		float DoorHeight = 2;
		bool IsWindows = true;

		float MinX = BuildingLocation.x;
		float MaxX = BuildingLocation.x+BuildingSize.x-1;
		float MinY = BuildingLocation.y;
		float MaxY = BuildingLocation.y+BuildingSize.y-1;
		float MinZ = BuildingLocation.z;
		float MaxZ = BuildingLocation.z+BuildingSize.z-1;
		//int BuildingRooms = 8;
		float RoomHeight = Mathf.FloorToInt((MaxY+1-MinY)/BuildingRooms);	// size of y divided by the amount of rooms
		// or Room Height
		// BuildingRooms = HeightY/RoomHeight

		// outer shell of building
		for (float i = MinX; i <= MaxX; i++)
			for (float j = MinY; j <= MaxY; j++)
			for (float k = MinZ; k <= MaxZ; k++) {
				if (j == MinY)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
				else if (j == MaxY)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), RoofType);
				// corner pillars
				else if ((i == MaxX && k == MaxZ) || (i == MinX && k == MaxZ) || (i == MaxX && k == MinZ) || (i == MinX && k == MinZ))
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
				// MainDoor
				else if (i-MinX  >= ((MaxX-MinX )/2f)-DoorSize/2f && i-MinX <= ((MaxX-MinX)/2f)+DoorSize/2f && k == MinZ && j <= DoorHeight+1)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), DoorType);
				// outer door part
				else if (i-MinX  >= ((MaxX-MinX )/2f)-DoorSize/2f-1 && i-MinX <= ((MaxX-MinX)/2f)+DoorSize/2f+1 && k == MinZ && j <= DoorHeight+2)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), OuterDoorType);
				// walls
				else if (i == MinX || i ==  MaxX || k == MinZ || k == MaxZ)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), WallType);
				// floors between bottom floor and roof
				else if (j%RoomHeight == 0)
					MyBlocks.UpdateBlock (new Vector3(i,j,k), FloorType);
			}

		// Next build Stairs
		int StairsSide = 3;
		for (int z = 0; z < BuildingRooms; z++) {
			Vector3 StartStairsPosition = new Vector3 (0,0,0);
			if (StairsSide == 0) {
				StartStairsPosition = new Vector3 (MinX+1, MinY+z*RoomHeight, MinZ+1);
			}
			else if (StairsSide == 1) {
				StartStairsPosition = new Vector3 (MaxX-1, MinY+z*RoomHeight, MaxZ-1);
			}
			else if (StairsSide == 2) {
				StartStairsPosition = new Vector3 (MinX+1, MinY+z*RoomHeight, MaxZ-1);
			}
			else if (StairsSide == 3) {
				StartStairsPosition = new Vector3 (MaxX-1, MinY+z*RoomHeight, MinZ+1);
			}
			for (int i = 0; i <= RoomHeight-1; i++) {
				//MyBlocks.UpdateBlock (StartStairsPosition, FloorType);
				if (i != 0) 
				{
					StartStairsPosition.y++;
					MyBlocks.UpdateBlock (StartStairsPosition, FloorType);
				}
				if (StairsSide == 0) {
					StartStairsPosition.x++;
				} else if (StairsSide == 1) {
					StartStairsPosition.x--;
				}else if (StairsSide == 2) {
					StartStairsPosition.z--;
				}else if (StairsSide == 3) {
					StartStairsPosition.z++;
				}
				if (i != RoomHeight-1)	// if not last position
					MyBlocks.UpdateBlock (StartStairsPosition, FloorType);
			}
			// clear the floor above stairs
			Vector3 StairsAbove = new Vector3 (0,0,0);
			if (StairsSide == 0) {
				StairsAbove = new Vector3 (StartStairsPosition.x, MinY+z*RoomHeight+RoomHeight, MinZ+1);
				if (StairsAbove.y > MaxY) StairsAbove.y = MaxY;
				//for (int i = 0; i <= MaxX-7; i++) {
				//	StairsAbove.x--;
				for (float i = StairsAbove.x-1; i >= MinX+1; i--) {
					StairsAbove.x = i;
					MyBlocks.UpdateBlock (StairsAbove, 0);
				}
			} else if (StairsSide == 1) {
				StairsAbove = new Vector3 (StartStairsPosition.x, MinY+z*RoomHeight+RoomHeight, MaxZ-1);
				if (StairsAbove.y > MaxY) StairsAbove.y = MaxY;
				for (float i = StairsAbove.x+1; i <= MaxX-1; i++) {
						StairsAbove.x = i;
						MyBlocks.UpdateBlock (StairsAbove, 0);
				}
			} else if (StairsSide == 2) {
				StairsAbove = new Vector3 (MinX+1, MinY+z*RoomHeight+RoomHeight, StartStairsPosition.z);
				if (StairsAbove.y > MaxY) 
					StairsAbove.y = MaxY;
				for (float i = StairsAbove.z+1; i <= MaxZ-1; i++) {
					StairsAbove.z = i;
					MyBlocks.UpdateBlock (StairsAbove, 0);
				}
			} else if (StairsSide == 3) {
				StairsAbove = new Vector3 (MaxX-1, MinY+z*RoomHeight+RoomHeight, StartStairsPosition.z);
				if (StairsAbove.y > MaxY) 
					StairsAbove.y = MaxY;
				for (float i = StairsAbove.z-1; i >= MinZ+1; i--) {
					StairsAbove.z = i;
					MyBlocks.UpdateBlock (StairsAbove, 0);
				}
			}
			if (IsWindows) {
				float WindowHeight = 0;
				int WindowYPosition = Mathf.RoundToInt(z*RoomHeight+2);
				for (float i = MinX; i <= MaxX; i++) {
					for (float j = MinZ; j <= MaxZ; j++) {
						if (i == MinX || j == MinZ || i == MaxX || j == MaxZ) {
							for (float k = -WindowHeight/2.0f; k <= WindowHeight/2.0f; k++)
								if (MyBlocks.GetBlockType(new Vector3(i, WindowYPosition+k, j)) == WallType)
									MyBlocks.UpdateBlock (new Vector3(i, WindowYPosition+k, j), WindowType);
						}
					}
				}
			}
			//IsWindows = !IsWindows;
			// order is 3, 1, 2, 0
			// change side stairs is on
			if (StairsSide == 0)
				StairsSide = 3;
			else if (StairsSide == 1)
				StairsSide = 2;
			else if (StairsSide == 2)
				StairsSide = 0;
			else if (StairsSide == 3)
				StairsSide = 1;
		}
	}
}
}*/


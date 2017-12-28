using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Voxels
{
    /// <summary>
    /// The data for a basic dungeon room
    /// </summary>
    [System.Serializable]
    public class DungeonRoom
    {
        public Vector3 RoomLocation;    // location in the maze
        public Vector3 RoomSize;        // max dimensions of the structure
        public int RoomFloorType;       // type of blocks on the floor of the room
        public int RoomWallType;
        public int RoomStructureType;   // might be a circular room - 0 is a square room
        public bool IsSpawnMonsters;    // should monsters spawn in the room
        public int MonsterType;         // 
        public bool IsPillars;          // pillars may be randomly placed in room
        public bool IsDoors;            // if false there will be no door blocks, just air that stops people coming in
        public bool IsDoorsLocked;
        public bool IsPowerUps;         // powers up spawners in the room?
        public bool IsTreasureChest;    // yeah... gold! or items! pl0x

        public bool IsConnected = false;    // connected to main grid
        public float Closeness = 0;

        public DungeonRoom()
        {

        }
        public Vector3 GetMidPoint()
        {
            return RoomLocation + RoomSize / 2f;
        }
        public void CalculateCloseness(List<DungeonRoom> Rooms)
        {
            Closeness = 0;
            List<float> Distances = new List<float>();
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i] != this)
                    Distances.Add(Vector3.Distance(RoomLocation, Rooms[i].RoomLocation));
            }
            for (int i = 0; i < Distances.Count; i++)
            {
                Closeness += Distances[i];
            }
            Closeness /= Distances.Count;
        }
    }
    /// <summary>
    /// A basic comparer to sort rooms by closeness
    /// </summary>
    public class SortRooms : IComparer<DungeonRoom>
    {
        int IComparer<DungeonRoom>.Compare(DungeonRoom Room1, DungeonRoom Room2)
        {
            //int t1 = _objA.GetComponent<CharacterStats>().initiative;
            //int t2 = _objB.GetComponent<CharacterStats>().initiative;
            return Room1.Closeness.CompareTo(Room2.Closeness);
        }
    }
}
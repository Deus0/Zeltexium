using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Voxels;

namespace Zeltex.AI
{
    [System.Serializable]
    public class PathNode
    {
        Int3 PreviousPosition;          // Points to previous node in the list
        float DistanceToTarget = 1000;  // need to calculate this
        bool IsPath;                    // Is this the path we have walked
        bool IsClosed;                  // Is this an obstacle
    }

    public class BotPathFinder
    {
        #region Variables
        static int MaxChecks = 512;
		private Dictionary<Int3, PathNode> Mynodes = new Dictionary<Int3, PathNode>();
		private List<Int3> OpenNodes = new List<Int3>();                // points in the path we can move from
		private List<float> OpenNodeDistances = new List<float>();
		private List<Int3> OpenNodesPreviousPositions = new List<Int3>();        // Points to the previous position 
		private List<Int3> ClosedNodes = new List<Int3>();              // points in the path we cannot move from
		private List<Int3> ClosedNodesPreviousPositions = new List<Int3>();        // Points to the previous position 
		private float PathRandomness = 0;

		private List<Int3> MyPath = new List<Int3>();
		// Stores the main path found in world
		public List<Vector3> MyPathWorld = new List<Vector3>();
        #endregion

        // initiate
        public BotPathFinder()
        {

        }

        /// <summary>
        /// Clear the nodes
        /// </summary>
        private void Clear()
        {
            Mynodes.Clear();
            OpenNodes.Clear();
            OpenNodeDistances.Clear();
            OpenNodesPreviousPositions.Clear();
            ClosedNodes.Clear();
            ClosedNodesPreviousPositions.Clear();
        }

        /// <summary>
        /// Finds a path within a voxel world
        /// </summary>
        public IEnumerator FindPath(World MyWorld, Vector3 MyPosition, Vector3 TargetPosition, Bounds MyBounds)
        {
			MyPathWorld.Clear();
            if (MyWorld)
            {
                Int3 StartPosition = new Int3(MyWorld.RealToBlockPosition(MyPosition));
                Int3 EndPosition = new Int3(MyWorld.RealToBlockPosition(TargetPosition));
                // convert them both to block positions - using world
                //MyPosition = new Vector3(Mathf.RoundToInt(MyPosition.x), Mathf.RoundToInt(MyPosition.y), Mathf.RoundToInt(MyPosition.z));
                // TargetPosition = new Vector3(Mathf.RoundToInt(TargetPosition.x), Mathf.RoundToInt(TargetPosition.y), Mathf.RoundToInt(TargetPosition.z));
                Int3 PathPosition = StartPosition;

                //List<Int3> MyPath = GetPathBasic(PathPosition, EndPosition);
				yield return GameManager.Get().StartCoroutine(GetPathDistance(MyWorld, PathPosition, EndPosition));

                Vector3 VoxelUnit = MyWorld.GetUnit();
                VoxelUnit.y = MyBounds.extents.y;
                //Debug.LogError("Modifying Path by adding half of: " + VoxelUnit.ToString());
                for (int i = 0; i < MyPath.Count; i++)
                {
                    MyPathWorld.Add(MyWorld.BlockToRealPosition(MyPath[i].GetVector()) + VoxelUnit / 2f);
                    if (i == MyPath.Count - 1)
                    {
                        MyPathWorld[i] = TargetPosition;
                    }
                }
                MyPathWorld.Insert(0, MyPosition);
            }
            // convert ints back to world positions
        }

        /// <summary>
        /// Gets a basic path
        /// </summary>
        private List<Int3> GetPathBasic(Int3 PathPosition, Int3 EndPosition)
        {
            List<Int3> MyPath = new List<Int3>();
            MyPath.Add(PathPosition);
            if (PathPosition != EndPosition)
            {
                if (EndPosition.x > PathPosition.x)
                {
                    for (int i = PathPosition.x; i <= EndPosition.x; i++)
                    {
                        PathPosition.x = i;
                        MyPath.Add(new Int3(PathPosition));
                    }
                }
                else if (EndPosition.x < PathPosition.x)
                {
                    for (int i = PathPosition.x; i >= EndPosition.x; i--)
                    {
                        PathPosition.x = i;
                        MyPath.Add(new Int3(PathPosition));
                    }
                }
                if (EndPosition.z > PathPosition.z)
                {
                    for (int i = PathPosition.z; i <= EndPosition.z; i++)
                    {
                        PathPosition.z = i;
                        MyPath.Add(new Int3(PathPosition));
                    }
                }
                else if (EndPosition.z < PathPosition.z)
                {
                    for (int i = PathPosition.z; i >= EndPosition.z; i--)
                    {
                        PathPosition.z = i;
                        MyPath.Add(new Int3(PathPosition));
                    }
                }
            }
            return MyPath;
        }

        /// <summary>
        /// Gets a basic path
        /// </summary>
        private IEnumerator GetPathDistance(World MyWorld, Int3 PathPosition, Int3 EndPosition)
        {
            Clear();
			MyPath.Clear();
            // initial variables
            Int3 BeginPosition = PathPosition;
            //MyPath.Add(PathPosition);
            OpenNodes.Add(PathPosition);
            OpenNodesPreviousPositions.Add(PathPosition);
            OpenNodeDistances.Add(0);
            int CheckCount = 0;

            while (PathPosition != EndPosition)
            {
                CheckCount++;
                if (CheckCount >= MaxChecks)
                {
                    //Debug.LogError("Path Checks Maxxed pass: " + MaxChecks);
					yield break;
                }
                // Create open nodes around our path position (which was previously open)
                // Move path position next to positions
                AddEdgePositions(MyWorld, PathPosition, EndPosition);

                AddDiagonalPositions(MyWorld, PathPosition, EndPosition);

                // Vertical Movement

                // Pick the closest position out of Open Nodes
                float ClosestDistance = 64; // max distance to check is 64
                int ClosestIndex = -1;
                for (int i = 0; i < OpenNodes.Count; i++)
                {
                    if (OpenNodeDistances[i] < ClosestDistance ||
                        (OpenNodeDistances[i] == ClosestDistance && Random.Range(1,2) == 1))
                    {
                        ClosestDistance = OpenNodeDistances[i];
                        ClosestIndex = i;
                    }

                }
                if (ClosestIndex == -1)
                {
                    Debug.LogError("Closest Index not found.");
					yield break;
                } 
                else
                {
                    // We have our closest node, so move to that position
                    // Move Path Closer
                    // Add Open Nodes at our closest Position
                    // for our first node, at path position

                    // Remove Path Position From Open Nodes, to Closed Nodes
                    Int3 NewPathPosition = OpenNodes[ClosestIndex];
                    int PathPositionIndex = OpenNodes.IndexOf(NewPathPosition);
                    // Add to closed nodes - including previous positions
                    ClosedNodes.Add(NewPathPosition);
                    ClosedNodesPreviousPositions.Add(OpenNodesPreviousPositions[PathPositionIndex]);
                    // Remove from open nodes
                    OpenNodes.RemoveAt(PathPositionIndex);
                    OpenNodeDistances.RemoveAt(PathPositionIndex);
                    OpenNodesPreviousPositions.RemoveAt(PathPositionIndex);
                    // Finally set position
                    PathPosition = NewPathPosition;
                }
				yield return null;
            }

            MyPath = TraceBackPath(MyWorld, BeginPosition, EndPosition, MyPath);
            Clear();
        }

        /// <summary>
        /// Traces the node path from the end position found
        /// </summary>
        private List<Int3> TraceBackPath(World MyWorld, Int3 BeginPosition, Int3 EndPosition, List<Int3> MyPath)
        {
            Int3 PathPosition = EndPosition;
            MyPath.Add(PathPosition);
            int PositionIndex = ClosedNodesPreviousPositions.Count - 1;//ClosedNodesPreviousPositions.IndexOf(EndPosition);
            int CheckCount = 0;
            if (ClosedNodesPreviousPositions.Count == 0)
            {
                //Debug.LogError("No previous position nodes: " + ClosedNodesPreviousPositions.Count);
                return MyPath;
            }
            if (PositionIndex < 0 || PositionIndex >= ClosedNodesPreviousPositions.Count)
            {
                //Debug.LogError("PositionIndex out of bounds: " + PositionIndex);
            }
            Int3 Direction = new Int3();
            Int3 LastPosition;
            while (PathPosition != BeginPosition)
            {
                // Save old position
                LastPosition = PathPosition;
                // Set new path position - to previous position
                PositionIndex = ClosedNodes.IndexOf(PathPosition);
                PathPosition = ClosedNodesPreviousPositions[PositionIndex];
                if (MyPath.Contains(PathPosition))
                {
                    Debug.LogError("Problem in path index");
                    return MyPath;
                }

                Int3 PositionDelta = PathPosition - LastPosition;
                // Initial Direction
                if (Direction == new Int3())
                {
                    Direction = PositionDelta;
                }
                // if a change in direction is made, record new path position, this culls between positions
                if (PositionDelta != Direction)
                {
                    MyPath.Insert(0, LastPosition);
                    Direction = PositionDelta;
                }
                CheckCount++;
                if (CheckCount >= MaxChecks)
                {
                    Debug.LogError("Path Checks Maxxed pass: " + MaxChecks);
                    return MyPath;
                }
            }
            return MyPath;
        }

        /// <summary>
        /// Add adjacent positions - x + 1, x - 1, z + 1, z - 1
        /// </summary>
        private void AddEdgePositions(World MyWorld, Int3 PathPosition, Int3 EndPosition)
        {

            int CanMoveRight = AddNode(MyWorld, PathPosition, EndPosition, new Int3(1, 0, 0));
            int CanMoveLeft = AddNode(MyWorld, PathPosition, EndPosition, new Int3(-1, 0, 0));
            int CanMoveForward = AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, 0, 1));
            int CanMoveBehind = AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, 0, -1));
            // Jump!!
            Voxel VoxelAbove = MyWorld.GetVoxel(PathPosition.Above());
            if (VoxelAbove != null && VoxelAbove.GetVoxelType() == 0)
            {
                if (CanMoveRight == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(1, 1, 0));
                }
                if (CanMoveLeft == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(-1, 1, 0));
                }
                if (CanMoveForward == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, 1, 1));
                }
                if (CanMoveBehind == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, 1, -1));
                }
            }
            // Falling (can fall 1-2 blocks) - Assuming the voxel under the position is missing (obstacleish)
            if (CanMoveRight == 0)
            {
                AddNode(MyWorld, PathPosition, EndPosition, new Int3(1, -1, 0));
            }
            if (CanMoveLeft == 0)
            {
                AddNode(MyWorld, PathPosition, EndPosition, new Int3(-1, -1, 0));
            }
            if (CanMoveForward == 0)
            {
                AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, -1, 1));
            }
            if (CanMoveBehind == 0)
            {
                AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, -1, -1));
            }
        }

        /// <summary>
        /// Diagnol Positions
        /// </summary>
        private void AddDiagonalPositions(World MyWorld, Int3 PathPosition, Int3 EndPosition)
        {
            // add diagnol positions! only if the sideways ones are both empty
            Voxel VoxelA = MyWorld.GetVoxel(PathPosition.Right());
            Voxel VoxelB = MyWorld.GetVoxel(PathPosition.Left());
            Voxel VoxelC = MyWorld.GetVoxel(PathPosition.Front());
            Voxel VoxelD = MyWorld.GetVoxel(PathPosition.Behind());
            if (VoxelA != null && VoxelA.GetVoxelType() == 0)
            {
                // (1, 0, 0) && (0, 0, 1)
                if (VoxelC != null && VoxelC != null && VoxelC.GetVoxelType() == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(1, 0, 1));
                }
                // (1, 0, 0) && (0, 0, -1)
                if (VoxelD != null && VoxelD.GetVoxelType() == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(1, 0, -1));
                }
            }

            if (VoxelB != null && VoxelB.GetVoxelType() == 0)
            {
                // (-1, 0, 0) && (0, 0, 1)
                if (VoxelC != null && VoxelC != null && VoxelC.GetVoxelType() == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(-1, 0, 1));
                }
                // (1, 0, 0) && (0, 0, -1)
                if (VoxelD != null && VoxelD.GetVoxelType() == 0)
                {
                    AddNode(MyWorld, PathPosition, EndPosition, new Int3(-1, 0, -1));
                }
            }
        }

        /// <summary>
        /// Add a node to our path finder
        /// </summary>
        private int AddNode(World MyWorld, Int3 PathPosition, Int3 EndPosition, Int3 AdditionPosition)
        {
            Int3 NewPosition = PathPosition + AdditionPosition;
            if (OpenNodes.Contains(NewPosition) == false && ClosedNodes.Contains(NewPosition) == false)
            {
                Voxel MyVoxel = MyWorld.GetVoxel(NewPosition);
                Voxel VoxelBelow = MyWorld.GetVoxel(NewPosition.Below());
                if (MyVoxel != null && MyVoxel.GetVoxelType() == 0 && VoxelBelow != null && VoxelBelow.GetVoxelType() != 0) // make sure voxel under is solid
                {
                    OpenNodes.Add(NewPosition);
                    OpenNodeDistances.Add(Vector3.Distance(NewPosition.GetVector(), EndPosition.GetVector()) + Random.Range(-PathRandomness, PathRandomness));
                    OpenNodesPreviousPositions.Add(PathPosition);
                    return 1;   // can move here
                }
                else
                {   
                    // no previous position for obstacles
                    ClosedNodes.Add(NewPosition);
                    ClosedNodesPreviousPositions.Add(new Int3());
                    return 0;   // obstacle
                }
            }
            else
            {
                return -1;  // already added
            }
        }

        private void AddVerticalPositions(World MyWorld, Int3 PathPosition, Int3 EndPosition)
        {
            AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, -1, 0));
            AddNode(MyWorld, PathPosition, EndPosition, new Int3(0, 1, 0));
        }
    }
}

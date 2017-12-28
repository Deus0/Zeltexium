using UnityEngine;
using Zeltex.Util;

// Input : Polygons, triangles
// Output : MyBlocks

// for now just make it replace it with any voxel model
// it will add World to the object
namespace Zeltex.Voxels
{
	public class VoxelizationData
    {
		public Vector3 Size;
	}
    // first just debug the grid and bounds

    /// <summary>
    /// Voxelizes a polygonal mesh, turning it into a voxel model.
    /// </summary>
    public class Voxelization : MonoBehaviour
    {
        [Header("Debug")]
        public bool DebugBounds;
        public bool DebugGridBounds;
        public bool DebugGrid;
        [Header("Data")]
        public Mesh OriginalMesh;
        public Mesh VoxelMesh;
        private MeshFilter MyMeshFilter;
        private MeshRenderer MyMeshRenderer;
        private float GridLength = 1f;
        // actions
        [Header("Actions")]
        public bool GenerateVoxelMesh;
        public bool UseVoxelMesh;
        public bool UsePolygonMesh;

        [Header("Scrap")]
	    public int BlockStructureIndex = 0;
	    public bool IsNormalsDebug = false;
	    public float NormalsLength = 0.2f;
		public Bounds MyBounds;
		Vector3 BlockSize;

		//public int GridLength;
		public float BlockLength;

        void OnDrawGizmos()
        {
            if (DebugBounds)
            {
                Vector3 Position = transform.TransformPoint(MyBounds.center);
                Vector3 CubeSize = (new Vector3(MyBounds.size.x*transform.lossyScale.x, 
                    MyBounds.size.y * transform.lossyScale.y,
                    MyBounds.size.z * transform.lossyScale.z));//transform.TransformDirection(MyBounds.size);
                Gizmos.color = Color.white;
                GizmoUtil.DrawCube(Position, CubeSize, transform.rotation);
            }
            if (DebugGridBounds)
            {
                Vector3 Position = transform.TransformPoint(MyBounds.center);
                Vector3 CubeSize = transform.TransformVector(MyBounds.size);
                Gizmos.color = Color.white;
                GizmoUtil.DrawCube(Position, CubeSize);
            }
        }
        void Generate()
        {   
            MyMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            MyMeshFilter = gameObject.GetComponent<MeshFilter>();
            OriginalMesh = MyMeshFilter.sharedMesh;
            MyBounds = OriginalMesh.bounds;

            World MyWorld = gameObject.GetComponent<World>();
            //MyWorld.UpdateBlockType(1, new Vector3(0, 0, 0));
        }

	    void Update() 
        {
            if (GenerateVoxelMesh)
            {
                GenerateVoxelMesh = false;
                Generate();
            }
		    if (Input.GetKeyDown (KeyCode.Z))
            {
			    float TriangleSize = 0.5f;
			    Vector3 Vertex1 = new Vector3(0,0,0);
			    Vector3 Vertex2 = new Vector3(0,TriangleSize,0);
			    Vector3 Vertex3 = new Vector3(TriangleSize,TriangleSize,0);
			    Debug.DrawLine (transform.position+Vertex1,transform.position+Vertex2, Color.blue, 5);
			    Debug.DrawLine (transform.position+Vertex2,transform.position+Vertex3, Color.blue, 5);
			    Debug.DrawLine (transform.position+Vertex3,transform.position+Vertex1, Color.blue, 5);

			    Debug.Break ();
		    }
	    }

		public void ConvertTrianglesToVoxels(VoxelizationData MyBlocks, Mesh InputMesh, float resolution)
        {
		    //resolution = Mathf.Pow (resolution, 2);
		    InputMesh.RecalculateBounds ();
		    MyBounds = InputMesh.bounds;
		    // draws the bounding box
		    //DebugShapes.DrawCube (transform.position + InputMesh.bounds.center, InputMesh.bounds.extents, Color.green);


		    Debug.LogError ("Converting mesh to voxels");
		    //MyBlocks = GetManager.GetDataManager().BlockStructuresList[BlockStructureIndex].MyBlocks;
		    float SizeOfGrid = 1f;
		    //real block size is x2 whatever i set here
		
		    Vector3 Bounds = InputMesh.bounds.extents;

		    float LargestSize = Bounds.x;
		    if (LargestSize < Bounds.y)
			    LargestSize = Bounds.y;
		    if (LargestSize < Bounds.z)
			    LargestSize = Bounds.z;
		    //LargestSize = Mathf.CeilToInt (LargestSize);
		    GridLength = Mathf.CeilToInt (LargestSize * resolution);
		    Vector3 GridSize = new Vector3 (GridLength, GridLength, GridLength);
		    BlockLength = Mathf.CeilToInt (LargestSize) / resolution;
		    BlockSize = new Vector3 (BlockLength, BlockLength, BlockLength);


		    //GridSize = GridSize / 2f;

		    // calculate block size depending on the bounds size of the model
		    // so if bounds size is 3, have block size as 3
		    // then set grid size to block size divided by resolution
		
		    //MyBlocks.Size = GridSize;
		    //MyBlocks.InitilizeData();

		    Vector3[] Verticies = InputMesh.vertices;
		    int[] Indicies = InputMesh.GetIndices (0);
		    Vector3[] Normals = InputMesh.normals;

		    // i have to make the grid size the same as the bounding box
		 
		    //Vector3 BlockSize = new Vector3(resolution, resolution, resolution); 
		    //BlockSize = 2f*(BlockSize/(MyBounds.extents.magnitude));
		    Debug.LogError("Block Size is: " + BlockSize.ToString());
		    Debug.LogError("Indicies Size is: " + Indicies.Length.ToString());
		    for (int z = 0; z < Indicies.Length; z += 3) {
			    Vector3 Vertex1 = Verticies[Indicies[z+0]];
			    Vector3 Vertex2 = Verticies[Indicies[z+1]];
			    Vector3 Vertex3 = Verticies[Indicies[z+2]];
			    if (IsNormalsDebug) {
				    Debug.DrawLine (transform.position+Vertex1,transform.position+Vertex1 + Normals[Indicies[z+0]]*NormalsLength, Color.blue, 5);
				    Debug.DrawLine (transform.position+Vertex2,transform.position+Vertex2 + Normals[Indicies[z+1]]*NormalsLength, Color.blue, 5);
				    Debug.DrawLine (transform.position+Vertex3,transform.position+Vertex3 + Normals[Indicies[z+2]]*NormalsLength, Color.blue, 5);
			    }

			    //Debug.LogError ("Triangle " + (z/3) + ": " + Vertex1.ToString() + "---" + Vertex2.ToString() + "---" + Vertex3.ToString());

			    //Debug.DrawLine (transform.position+new Vector3(startX, startY, startZ),transform.position+new Vector3(endX,endY,endZ), Color.red, 5);
			    // should only do this for the blocks around the triangles
			    // need to optimize this asap
			    // for blocks around triangle?
			    //Vector3 StartBlock = new Vector3();
			    //StartBlock.x = Mathf.FloorToInt();	// minimum size of the Vertex AARB

			    // find the size of each triangle as an Axis Aligned Rectangle Bounds - AARB
			    Vector3 TrianglePosition = (Vertex2 + Vertex3)/2f;
			    Vector3 TriangleMinimum = new Vector3 (Mathf.Min (Vertex1.x, Vertex2.x, Vertex3.x), Mathf.Min (Vertex1.y, Vertex2.y, Vertex3.y), Mathf.Min (Vertex1.z, Vertex2.z, Vertex3.z));
			    Vector3 TriangleMaximum = new Vector3 (Mathf.Max (Vertex1.x, Vertex2.x, Vertex3.x), Mathf.Max (Vertex1.y, Vertex2.y, Vertex3.y), Mathf.Max (Vertex1.z, Vertex2.z, Vertex3.z));
			    Vector3 TriangleSize = new Vector3();
			    TriangleSize = TriangleMaximum - TriangleMinimum;
			    TriangleSize *= 0.5f;
			    TrianglePosition = TriangleMinimum + TriangleSize;
			    TrianglePosition += InputMesh.bounds.extents;
			    int StartX =  Mathf.FloorToInt((TrianglePosition.x-TriangleSize.x));	// before it was 0
			    int EndX = Mathf.CeilToInt((TrianglePosition.x+TriangleSize.x)/resolution);//MyBlocks.Size.x; i++)
			    int StartY =  Mathf.FloorToInt((TrianglePosition.y-TriangleSize.y));	
			    int EndY = Mathf.CeilToInt((TrianglePosition.y+TriangleSize.y)/resolution);
			    int StartZ =  Mathf.FloorToInt((TrianglePosition.z-TriangleSize.z));
			    int EndZ = Mathf.CeilToInt((TrianglePosition.z+TriangleSize.z)/resolution);
			    StartX = 0; StartY = 0; StartZ = 0;
			    EndX = Mathf.CeilToInt(MyBlocks.Size.x)-1; EndY = Mathf.CeilToInt(MyBlocks.Size.y)-1; EndZ = Mathf.CeilToInt(MyBlocks.Size.z)-1;

			    Debug.LogError (z + " || StartX: " + StartX + " - EndX: " + EndX + 
			                    " || StartY: " + StartY + " - EndY: " + EndY + 
			                    " || StartZ: " + StartZ + " - EndZ: " + EndZ);

			    Debug.Break ();
			    for (int i = StartX; i <= EndX; i++)
				    for (int j = StartY; j <= EndY; j++)
					    for (int k = StartZ; k <= EndZ; k++) 
				    {
					    /*if (MyBlocks.GetBlockType(new Vector3(i,j,k)) == 0) {
						    Vector3 BlockPosition = BlockSize + MyBounds.center;
						    BlockPosition += 2f*(new Vector3(i*BlockSize.x,j*BlockSize.y,k*BlockSize.z));
						    if (IsTriangleInGrid(BlockPosition, BlockSize, TrianglePosition, TriangleSize)) {
								    MyBlocks.UpdateBlock(new Vector3(i,j,k), 1);
						    }// else
								    //MyBlocks.UpdateBlock(new Vector3(i,j,k), 0);
								    */
					    }
				    }
			    //break;	// for now test it once
		    //}

		    //Debug.LogError ("Size of grid: " + MyBlocks.Size.ToString ());
		    //if (IsDebugGrid)
		    /*for (int i = 0; i < MyBlocks.Size.x; i++)
				    for (int j = 0; j < MyBlocks.Size.y; j++)
					    for (int k = 0; k < MyBlocks.Size.z; k++) */
				    //{
					    Vector3 BlockPosition = BlockSize + MyBounds.center;
					    //BlockPosition += 2f*(new Vector3(i*BlockSize.x,j*BlockSize.y,k*BlockSize.z));

				    /*if (MyBlocks.GetBlockType(new Vector3(i,j,k)) == 0) {
							    if (IsDebugAllBlocks)
							    DebugShapes.DrawCube (BlockPosition+transform.position - MyBounds.extents,
						                          BlockSize, 
					                          Color.white, true);
				    }
				    else {
						    DebugShapes.DrawCube (BlockPosition+transform.position - MyBounds.extents,
					                          BlockSize, 
				                          Color.red, true);
				                          */
				    //}

		    //}
	    }
	    public bool IsTriangleInGrid(Vector3 GridPosition, Vector3 GridSize, Vector3 TrianglePosition, Vector3 TriangleSize) {


		    Color DrawColor = Color.grey;
		    bool IsIntersect = IsBoxIntersect (GridPosition, GridSize, TrianglePosition, TriangleSize);
		    if (IsIntersect)
			    DrawColor = Color.red;
		    /*if (IsDebugTriangleCubes)
            {
				    if (!(IsDebugAllTriangleCubes && !IsIntersect))
				    DebugShapes.DrawCube (TrianglePosition+transform.position- MyBounds.extents,
				                          TriangleSize, 
				                          DrawColor, 
				                          true);
		    }*/
		    return IsIntersect;
	    }


	    // check the triangles bounding box
	
	    public bool IsBoxIntersect(Vector3 Box1Position, Vector3 Box1Size, Vector3 Box2Position, Vector3 Box2Size)
        {
		    bool IsInsideBox = false;

		    IsInsideBox = IsInBox (Box1Position, Box1Size, Box2Position, Box2Size);

		    return IsInsideBox;
	    }

	    // returns true if its in the box
	    public bool IsInBox(Vector3 Position1, Vector3 Size1, Vector3 Position2, Vector3 Size2)
        {
		    float Delta1 = 0;
		    float Delta2 = 0;
		    float MinX1 = Position1.x - Size1.x-Delta1;
		    float MaxX1 = Position1.x + Size1.x+Delta1;
		    float MinX2 = Position2.x - Size2.x-Delta2;
		    float MaxX2 = Position2.x + Size2.x+Delta2;

		    float MinY1 = Position1.y - Size1.y-Delta1;
		    float MaxY1 = Position1.y + Size1.y+Delta1;
		    float MinY2 = Position2.y - Size2.y-Delta2;
		    float MaxY2 = Position2.y + Size2.y+Delta2;
		
		    float MinZ1 = Position1.z - Size1.z-Delta1;
		    float MaxZ1 = Position1.z + Size1.z+Delta1;
		    float MinZ2 = Position2.z - Size2.z-Delta2;
		    float MaxZ2 = Position2.z + Size2.z+Delta2;
		    if (MinX1 > MaxX2)
			    return false;
		    else if (MaxX1 < MinX2) 
			    return false;
		    else if (MinY1 > MaxY2)
			    return false;	
		    else if (MaxY1 < MinY2) 
			    return false;
		    else if (MinZ1 > MaxZ2) 
			    return false;
		    else if (MaxZ1 < MinZ2) 
			    return false;
		    else
			    return true;

            // if out side of box
            /*if (
			    ((MinX1 <= MinX2 && MinX2 <= MaxX1)	|| (MinX2 <= MinX1 && MinX1 <= MaxX2))
				    &&
			    ((MinY1 <= MinY2 && MinY2 <= MaxY1) || (MinY2 <= MinY1 && MinY1 <= MaxY2))
				    &&
			    ((MinZ1 <= MinZ2 && MinZ2 <= MaxZ1) || (MinZ2 <= MinZ1 && MinZ1 <= MaxZ2))
		        ) {
			    return true;
		    }
		    return false;*/

            /*(MaxX1 < MinX2) ||
            (MinX1 > MaxX2) ||
            (MaxY1 < MinY2) ||
            (MinY1 > MaxY2) ||
            (MaxZ1 < MinZ2) ||
            (MinZ1 > MaxZ2)*/

        }
        // debug function to draw a cube

    }
}
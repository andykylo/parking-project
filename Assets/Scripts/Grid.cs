using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
    public int directionalPenalty;
    public TerrainType[] walkableRegions;
    public RoadType[] roadRegions;
    LayerMask walkableMask;
    LayerMask roadMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    Dictionary<int, int> roadRegionsDictionary = new Dictionary<int, int>();

    Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	void Awake() {
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        foreach (RoadType region in roadRegions)
        {
            walkableMask.value |= region.terrainMask.value;
            roadMask.value |= region.terrainMask.value;
            roadRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.direction);
        }

        CreateGrid();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));

                int movementPenalty = 0;
                int direction = 4;

                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    if (Physics.Raycast(ray, out hit, 100, roadMask))
                    {
                        roadRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out direction);
                    }

                    
                }

				grid[x,y] = new Node(walkable,worldPoint, x,y, movementPenalty, direction);
			}
		}
	}

    public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}
	

	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		return grid[x,y];
	}
	
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable)?Color.white:Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    [System.Serializable]

    public class RoadType
    {
        public LayerMask terrainMask;
        public int direction;
    }
}
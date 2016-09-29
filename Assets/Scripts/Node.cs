using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node> {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;
    public int movementPenalty;

    /*
     *  NYI: direction penalty array
     *
     *  0   1   2   NW  N   NE
     *  
     *  7       3   W       E
     *  
     *  6   5   4   SW  S   SE
     *  
     */
    public int[] directionPenalty;
	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;
	
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
        movementPenalty = _penalty;
	}

    public void DirectionPenalty(char dir)
    {
        if (dir == 'n')
        {
            directionPenalty = new int[] { 0, 0, 0, 0, 10000, 10000, 10000, 0 };
        }
        else if (dir == 'e')
        {
            directionPenalty = new int[] { 100000, 0, 0, 0, 0, 0, 10000, 10000 };
        }
        else if (dir == 's')
        {
            directionPenalty = new int[] { 10000, 10000, 10000, 0, 0, 0, 0, 0 };
        }
        else if (dir == 'w')
        {
            directionPenalty = new int[] { 0, 0, 10000, 10000, 10000, 0, 0, 0 };
        }
    }

	public int fCost {
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}

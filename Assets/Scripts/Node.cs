using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : IHeapItem<Node> {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;
    public int[] movementPenalty = new int[8];
    public int direction;
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
	
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int mp, int _direction, int _dirPen) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
        direction = _direction;
        SetDirectionalPenalty(_direction, mp, _dirPen);
        // movementPenalty = _mp;
	}

    void SetDirectionalPenalty(int dir, int mp, int dirPen)
    {
        // 0 north, 1 east, 2 south, 3 west, 4 notroad

        movementPenalty[0] = (dir == 1 || dir == 2) ? dirPen + mp : mp;
        movementPenalty[1] = (dir == 2) ? dirPen + mp : mp;
        movementPenalty[2] = (dir == 2 || dir == 3) ? dirPen + mp : mp;
        movementPenalty[3] = (dir == 3) ? dirPen + mp : mp;
        movementPenalty[4] = (dir == 0 || dir == 3) ? dirPen + mp : mp;
        movementPenalty[5] = (dir == 0) ? dirPen + mp : mp;
        movementPenalty[6] = (dir == 0 || dir == 1) ? dirPen + mp : mp;
        movementPenalty[7] = (dir == 1) ? dirPen + mp : mp;
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

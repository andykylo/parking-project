﻿using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public bool isPlayer;
    public GameObject targetObject;
    public GameObject playerObject;
	public Transform target;
    public float initialSpeed;
    float speed;
    Vector3[] path;
	int targetIndex;
    bool isNPCIdle = false;
    bool isVerticalGreen = false; // false: green for horizontal | true: green for vertical
    public LayerMask selectionMask;
    public LayerMask buildingMask;
    public LayerMask horizontalStopMask;
    public LayerMask verticalStopMask;

	void Start() {

        speed = initialSpeed;

        if (!isPlayer)
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }

        InvokeRepeating("ToggleRepeat", 0, 5);
    }

    void ToggleRepeat()
    {
        isVerticalGreen = (isVerticalGreen == true) ? false : true;

        Debug.Log(isVerticalGreen);

    }

    void Update()
    {

        if (isPlayer)
        {
            if (Input.GetMouseButtonDown(0) && isPlayer)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    targetObject.transform.position = hit.point;
                    target = targetObject.transform;
                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }
            }
        }

        else
        {
            if (isNPCIdle)
            {
                Debug.Log(isNPCIdle);
                targetObject.transform.position = RandomTargetLocation();
                //target = targetObject.transform;
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                isNPCIdle = false;
                targetIndex = 0;
                Debug.Log(playerObject.transform.position);
                Debug.Log(target.position);
            }
            
        }

        Ray ray2 = new Ray(playerObject.transform.position, playerObject.transform.forward);
        RaycastHit hit2;

        if (Physics.Raycast(ray2, out hit2, 3, selectionMask.value))
        {
            speed = 0;
        }

        else
        {
            ray2 = new Ray(playerObject.transform.position + Vector3.up * 25, Vector3.down);

            if ((Physics.Raycast(ray2, out hit2, 30, verticalStopMask.value) && isVerticalGreen) ||
                (Physics.Raycast(ray2, out hit2, 30, horizontalStopMask.value) && !isVerticalGreen))
            {
                speed = 0;
            }

            else speed = initialSpeed;
        }
    }

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
		Vector3 currentWaypoint = path[0];
		while (true) {
			if (transform.position == currentWaypoint) {
                

                targetIndex++;
				if (targetIndex >= path.Length) {
                    Debug.Log(playerObject.transform.position);
                    Debug.Log("End of Path found");
                    if (!isPlayer) isNPCIdle = true;
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);

			yield return null;

		}
	}

    Vector3 RandomTargetLocation()
    {
        Vector3 location = new Vector3(Random.Range(-118f, 118f), 0, Random.Range(-118f, 118f));
        // bool isBuilding = true;
        Ray ray = new Ray(location + Vector3.up * 50, Vector3.down);
        RaycastHit hit;

        while (Physics.Raycast(ray, out hit, 10000f, buildingMask))
        {
            Debug.Log("Building");
            location = new Vector3(Random.Range(-118f, 118f), 0, Random.Range(-118f, 118f));
            ray = new Ray(location + Vector3.up * 50, Vector3.down);
        }

        location = ShiftAwayFromBuildings(location);

        return location;
    }

    Vector3 ShiftAwayFromBuildings(Vector3 v)
    {
        Ray ray = new Ray(v, Vector3.right);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f, buildingMask))
        {
            return (v + Vector3.left);
        }

        ray = new Ray(v, Vector3.left);

        if (Physics.Raycast(ray, out hit, 2f, buildingMask))
        {
            return (v + Vector3.right);
        }

        ray = new Ray(v, Vector3.forward);

        if (Physics.Raycast(ray, out hit, 2f, buildingMask))
        {
            return (v + Vector3.back);
        }

        ray = new Ray(v, Vector3.back);

        if (Physics.Raycast(ray, out hit, 2f, buildingMask))
        {
            return (v + Vector3.forward);
        }

        return v;
    }

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}
}

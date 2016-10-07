using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public bool isPlayer;
    public GameObject targetObject;
    public GameObject playerObject;
	public Transform target;
	float speed = 20;
	Vector3[] path;
	int targetIndex;
    bool isNPCIdle = false;
    public LayerMask selectionMask;
    public LayerMask buildingMask;

	void Start() {
        if (!isPlayer)
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
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
        if (Physics.Raycast(ray2, out hit2, 5, selectionMask.value))
        {
            speed = 0;
        }

        else speed = 20;
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
        Vector3 location = new Vector3(Random.Range(-120f, 120f), 0, Random.Range(-120f, 120f));
        // bool isBuilding = true;
        Ray ray = new Ray(location + Vector3.up * 50, Vector3.down);
        RaycastHit hit;

        while (Physics.Raycast(ray, out hit, 10000f, buildingMask))
        {
            Debug.Log("Building");
            location = new Vector3(Random.Range(-120f, 120f), 0, Random.Range(-120f, 120f));
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

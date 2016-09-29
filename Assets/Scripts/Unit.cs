using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public bool isPlayer;
    public GameObject targetObject;
	public Transform target;
	float speed = 20;
	Vector3[] path;
	int targetIndex;
    public LayerMask selectionMask;

	void Start() {
        if (!isPlayer)
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0) && isPlayer)
        {
            Debug.Log("Pressed left click.");
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
				targetIndex ++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;

		}
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

using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public bool isPlayer;
    public GameObject targetSpawnObject;
    public GameObject playerObject;
	public Transform target;
    public float initialSpeed;
    public float detectionRange = 3.0f;

    GameObject targetObject;
    GameObject detectedObject;

    float speed;
    Vector3 velocity;
    Vector3[] path;
	int targetIndex;
    bool isNPCIdle = false;
    bool isVerticalGreen = false; // false: green for horizontal | true: green for vertical
    public LayerMask selectionMask;
    public LayerMask buildingMask;
    public LayerMask parkingMask;
    public LayerMask horizontalStopMask;
    public LayerMask verticalStopMask;

	void Start() {

        speed = initialSpeed;

        targetObject = (GameObject)Instantiate(targetSpawnObject, playerObject.transform.position, Quaternion.identity);
        target = targetObject.transform;

        if (!isPlayer)
        {
            

            playerObject.transform.position = RandomTargetLocation(false);
            targetObject.transform.position = RandomTargetLocation(true);
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
            
        }

        StartCoroutine("CalcVelocity");

        InvokeRepeating("ToggleRepeat", 0, 5);
    }

    IEnumerator CalcVelocity()
    {
        while (Application.isPlaying)
        {
            Vector3 previousPosition;

            previousPosition = playerObject.transform.position;

            yield return new WaitForEndOfFrame();

            velocity = previousPosition - playerObject.transform.position;

            //Debug.Log(playerObject.name + " velocity = " + velocity);
        }
    }

    void ToggleRepeat()
    {
        isVerticalGreen = (isVerticalGreen == true) ? false : true;

        //Debug.Log(isVerticalGreen);

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
                //Debug.Log(isNPCIdle);
                targetObject.transform.position = RandomTargetLocation(true);
                //target = targetObject.transform;
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                isNPCIdle = false;
                targetIndex = 0;
                //Debug.Log(playerObject.transform.position);
                //Debug.Log(target.position);
            }
            
        }

        //Vector3 v = Quaternion.AngleAxis(90, Vector3.up) * velocity;
        //Debug.Log(velocity + " | " + v);

        Ray centreRay = new Ray(playerObject.transform.position + velocity, velocity * -1);
        //Ray leftRay = new Ray(playerObject.transform.position, Quaternion.Euler(0, -22.5f, 0) * velocity);
        //Ray rightRay = new Ray(playerObject.transform.position, Quaternion.Euler(0, 22.5f, 0) * velocity);
        RaycastHit hit2;

        //if (Physics.Raycast(centreRay, out hit2, detectionRange, selectionMask.value) ||
        //    Physics.Raycast(leftRay, out hit2, detectionRange, selectionMask.value) ||
        //    Physics.Raycast(rightRay, out hit2, detectionRange, selectionMask.value))

        if (Physics.SphereCast(centreRay, 1.5f, out hit2, detectionRange, selectionMask.value))
        {
            Debug.Log("Vehicle detected for " + playerObject.name + " at " + playerObject.transform.position);

            targetObject = hit2.collider.gameObject;
            speed = 0;
            
        }

        else
        {
            centreRay = new Ray(playerObject.transform.position + Vector3.up * 25, Vector3.down);

            if ((Physics.Raycast(centreRay, out hit2, 30, verticalStopMask.value) && isVerticalGreen) ||
                (Physics.Raycast(centreRay, out hit2, 30, horizontalStopMask.value) && !isVerticalGreen))
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
            Debug.Log(playerObject.name + " path size " + path.Length);
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
        //Debug.Log(playerObject.name + " " + path[0]);
        Debug.Log(playerObject.name + " path size " + path.Length);
		Vector3 currentWaypoint = path[0];
		while (true) {
			if (transform.position == currentWaypoint) {
                

                targetIndex++;
				if (targetIndex >= path.Length) {
                    if (isPlayer)
                    {
                        Debug.Log(playerObject.transform.position);
                        Debug.Log("End of Path found");
                    }

                    if (!isPlayer) isNPCIdle = true;
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

            
            //Debug.Log(playerObject.name + " speed = " + speed);

            //Ray ray = new Ray(playerObject.transform.position + velocity * 5.0f, velocity);
            //RaycastHit hit;

            //if (speed == 0) yield return new WaitForSeconds(0.25f);
                //yield return new WaitWhile(() => Vector3.Distance(playerObject.transform.position, detectedObject.transform.position) < (detectionRange + 2.0f));

            transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);

			yield return null;

		}
	}

    Vector3 RandomTargetLocation(bool isTarget)
    {
        Vector3 location = new Vector3(Random.Range(-114f, 114f), 0, Random.Range(-114f, 114f));
        // bool isBuilding = true;
        Ray ray = new Ray(location + Vector3.up * 50, Vector3.down);
        RaycastHit hit;

        while (Physics.SphereCast(ray, 3f, out hit, 10000f, buildingMask) || !Physics.Raycast(ray, out hit, 10000f, parkingMask))
        {
            //Debug.Log("Building");
            location = new Vector3(Random.Range(-114f, 114f), 0, Random.Range(-114f, 114f));
            ray = new Ray(location + Vector3.up * 50, Vector3.down);
        }

        location = ShiftAwayFromBuildings(location);

        return location;
    }

    Vector3 ShiftAwayFromBuildings(Vector3 v)
    {
        Ray ray = new Ray(v, Vector3.right);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 4f, buildingMask))
        {
            return (v + Vector3.left * 2);
        }

        ray = new Ray(v, Vector3.left);

        if (Physics.Raycast(ray, out hit, 4f, buildingMask))
        {
            return (v + Vector3.right * 2);
        }

        ray = new Ray(v, Vector3.forward);

        if (Physics.Raycast(ray, out hit, 4f, buildingMask))
        {
            return (v + Vector3.back * 2);
        }

        ray = new Ray(v, Vector3.back);

        if (Physics.Raycast(ray, out hit, 4f, buildingMask))
        {
            return (v + Vector3.forward * 2);
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

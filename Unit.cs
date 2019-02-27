using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public bool debugUnitPathing;

    public const float minPathUpdateTime = 0.2f;
    public const float pathUpdateMoveThreshold = 2f;

    public GameObject keepWaypoint, dungeonWaypoint;
    public Transform target;

    // Movement stuff
    public float deliveryBuffer = 3f;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;
    PathFindingPath path;

    // Extra values we need per unit
    public int purchasePrice = 0;
    public bool isDelivering;
    public bool isLoadingPack = false;

    

    // START Pathfinding Methods
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            path = new PathFindingPath(waypoints, transform.position, turnDst, stoppingDst);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    // Handling in derived classes
    public virtual IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds(.3f);
        }
        
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true) {
            if (Vector3.Distance(transform.position, keepWaypoint.transform.position) <= deliveryBuffer) {
                // At Keep, deliver items/money/whatever

                // emptied, now go to dungeon.
                isDelivering = false;
                target = dungeonWaypoint.transform;
            } else if (Vector3.Distance(transform.position, dungeonWaypoint.transform.position) <= deliveryBuffer) {
                // At dungeon, if not loading, start loading and filling pack
                // Otherwise go deliver
                if (isDelivering) {
                    // Full, go deliver to keep.
                    target = keepWaypoint.transform;
                    isLoadingPack = false;
                } else {
                    if (!isLoadingPack) {
                        isLoadingPack = true;
                        // In Courier, we are handling pack loading on a repeating loop
                    }
                }
            }
            yield return new WaitForSeconds(minPathUpdateTime);
            //print(((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }

    IEnumerator FollowPath() {

        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath) {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) {
                    followingPath = false;
                    break;
                }
                else {
                    pathIndex++;
                }
            }

            if (followingPath) {

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f) {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;

        }
    }

    public void OnDrawGizmos() {
        if (path != null && debugUnitPathing) {
            path.DrawWithGizmos();
        }
    }

    // END Pathfinding Code
}

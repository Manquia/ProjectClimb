using UnityEngine;
using System.Collections;

public class ExPathShower : MonoBehaviour {

    public GameObject pathObject;
    public float distAlongPath;
    // Path Drawing
    void OnDrawGizmos()
    {
        if (pathObject)
        {
            var path = pathObject.GetComponent<FFPath>();
            Color tempColor = Gizmos.color;

            if (path)
            {
                // Point given stuff

                // TESTED: T + R + S
                Gizmos.color = Color.blue;
                float distToNearestPoint;
                Vector3 nearestPointAlongPath = path.NearestPointAlongPath(transform.position, out distToNearestPoint);
                Gizmos.DrawLine(transform.position, nearestPointAlongPath);
                Gizmos.color = Color.black; // should be same direction (half black)
                Gizmos.DrawLine(transform.position, transform.position + ((path.PointAlongPath(distToNearestPoint) - transform.position) * 0.5f));

                // TESTED: T + R + S
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.position, path.NearestPoint(transform.position));
            }
            if (path)
            {
                // distance stuff

                // TESTED: T + R + S
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, path.PointAlongPath(distAlongPath));

                // TESTED: T + R + S
                int nearestPointIndex;
                float nearestPointDistance;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, path.NearestPoint(distAlongPath, out nearestPointIndex, out nearestPointDistance));
            }

            Gizmos.color = tempColor;
        }

        distAlongPath += 0.01f;
    }
}

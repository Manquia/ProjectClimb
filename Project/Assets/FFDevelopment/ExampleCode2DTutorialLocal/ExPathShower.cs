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
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, path.NearestPoint(distAlongPath));

                // TESTED: T + R + S
                Gizmos.color = Color.yellow; // next point (half yellow)
                Vector3 nextPoint = path.NextPoint(distAlongPath);
                Gizmos.DrawLine(transform.position, transform.position + (Vector3.Normalize(nextPoint - transform.position) * (Vector3.Magnitude(nextPoint - transform.position) - 0.4f)));

                // TESTED: T + R + S
                Gizmos.color = Color.yellow; // prev point (half yellow)
                Vector3 prevPoint = path.PrevPoint(distAlongPath);
                Gizmos.DrawLine(transform.position, transform.position + (Vector3.Normalize(prevPoint - transform.position) *(Vector3.Magnitude(prevPoint - transform.position) - 0.4f)));
            }

            Gizmos.color = tempColor;
        }

        distAlongPath += 0.01f;
    }
}

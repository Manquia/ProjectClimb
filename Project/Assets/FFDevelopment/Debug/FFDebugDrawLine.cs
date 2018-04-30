using UnityEngine;
using System.Collections;

public class FFDebugDrawLine : MonoBehaviour {

    public Color DrawColor = Color.blue;
    public Color DrawColorSelected = Color.yellow;
    public Vector3 Start = new Vector3(0, 0, 0);
    public Vector3 End = new Vector3(1, 1, 1);

    void OnDrawGizmos() // unselected
    {
        Gizmos.color = this.DrawColor;
        this.DrawLine(this.Start, this.End);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = this.DrawColorSelected;
        this.DrawLine(this.Start, this.End);
    }
    // draws a line relative to this objects's position + rotation + scale
    void DrawLine(Vector3 start, Vector3 end)
    {
        Vector3 startPoint = transform.TransformPoint(start);
        Vector3 endPoint = transform.TransformPoint(end);
        Gizmos.DrawLine(startPoint, endPoint);
    }
}

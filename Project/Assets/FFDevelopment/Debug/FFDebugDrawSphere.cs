using UnityEngine;
using System.Collections;

public class FFDebugDrawSphere : MonoBehaviour {

    public Color DrawColor = Color.blue;
    public Color DrawColorSelected = Color.yellow;
    public Vector3 offset = new Vector3(0, 0, 0);
    public float radius = 1;

    void OnDrawGizmos() // unselected
    {
        Color tempColor = Gizmos.color;
        Gizmos.color = this.DrawColor;
        this.DrawSphere(offset, radius);
        Gizmos.color = tempColor;
    }
    void OnDrawGizmosSelected()
    {
        Color tempColor = Gizmos.color;
        Gizmos.color = this.DrawColorSelected;
        this.DrawSphere(offset, radius);
        Gizmos.color = tempColor;
    }
    // draws a line relative to this objects's position + rotation + scale
    void DrawSphere(Vector3 pos, float radius)
    {
        Vector3 posOfSphere = transform.TransformPoint(pos);
        Gizmos.DrawSphere(posOfSphere, radius);
    }
}

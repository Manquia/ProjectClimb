using UnityEngine;
using System.Collections;

public class FFDebugDrawBox : MonoBehaviour {

    public Color DrawColor = Color.blue;
    public Color DrawColorSelected = Color.yellow;
    public Vector3 offset = new Vector3(0,0,0);
    public Vector3 size = new Vector3(1, 1, 1);

    void OnDrawGizmos() // unselected
    {
        Gizmos.color = this.DrawColor;
        Gizmos.DrawCube(transform.TransformPoint(this.offset), this.size);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = this.DrawColorSelected;
        Gizmos.DrawCube(transform.TransformPoint(this.offset), this.size);
    }
}

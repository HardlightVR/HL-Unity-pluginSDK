using UnityEngine;
using System.Collections;

public class GizmoVisualizer : MonoBehaviour
{
	public bool DrawGizmo = true;
	public enum GizmoVisualizerShape { Cube, Sphere }
	public GizmoVisualizerShape MyShape = GizmoVisualizerShape.Cube;
	public Color MyColor = Color.black;
	public Vector3 scale = Vector3.one * .25f;
	void OnDrawGizmos()
	{
		if (DrawGizmo)
		{
			Gizmos.color = MyColor;
			if (MyShape == GizmoVisualizerShape.Sphere)
			{
				Gizmos.DrawSphere(transform.position, scale.x);
			}
			else if (MyShape == GizmoVisualizerShape.Cube)
			{
				Gizmos.DrawCube(transform.position, scale);
			}
		}
	}
}
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class BoundDrawer : MonoBehaviour
{
	public Bounds bounds;

	public Color color = Color.green;

	private Vector3 v3FrontTopLeft;

	private Vector3 v3FrontTopRight;

	private Vector3 v3FrontBottomLeft;

	private Vector3 v3FrontBottomRight;

	private Vector3 v3BackTopLeft;

	private Vector3 v3BackTopRight;

	private Vector3 v3BackBottomLeft;

	private Vector3 v3BackBottomRight;

	private void Awake()
	{
		Tilemap component = GetComponent<Tilemap>();
		if (component != null)
		{
			bounds = component.localBounds;
		}
	}

	public void ManualUpdateBounds(Bounds bounds)
	{
		this.bounds = bounds;
	}

	private void CalcPositons()
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		v3FrontTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
		v3FrontTopRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
		v3FrontBottomLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		v3FrontBottomRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
		v3BackTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
		v3BackTopRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
		v3BackBottomLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
		v3BackBottomRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
		v3FrontTopLeft = base.transform.TransformPoint(v3FrontTopLeft);
		v3FrontTopRight = base.transform.TransformPoint(v3FrontTopRight);
		v3FrontBottomLeft = base.transform.TransformPoint(v3FrontBottomLeft);
		v3FrontBottomRight = base.transform.TransformPoint(v3FrontBottomRight);
		v3BackTopLeft = base.transform.TransformPoint(v3BackTopLeft);
		v3BackTopRight = base.transform.TransformPoint(v3BackTopRight);
		v3BackBottomLeft = base.transform.TransformPoint(v3BackBottomLeft);
		v3BackBottomRight = base.transform.TransformPoint(v3BackBottomRight);
	}

	private void DrawBox()
	{
		Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, color);
		Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, color);
		Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, color);
		Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, color);
		Debug.DrawLine(v3BackTopLeft, v3BackTopRight, color);
		Debug.DrawLine(v3BackTopRight, v3BackBottomRight, color);
		Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, color);
		Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, color);
		Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, color);
		Debug.DrawLine(v3FrontTopRight, v3BackTopRight, color);
		Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, color);
		Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, color);
	}
}

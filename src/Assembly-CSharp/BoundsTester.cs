using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class BoundsTester : MonoBehaviour
{
	public GameObject go;

	public Tilemap tilemap;

	private void Update()
	{
		if (!(tilemap == null) && !(go == null))
		{
			Bounds localBounds = tilemap.localBounds;
			localBounds.center = tilemap.localBounds.center + tilemap.transform.position;
			Vector3 vector = localBounds.ClosestPoint(go.transform.position);
			Debug.DrawLine(localBounds.center, vector, Color.green);
			Debug.Log("World Pos: " + vector.ToString() + ". Local Pos " + tilemap.transform.InverseTransformPoint(vector).ToString());
		}
	}
}

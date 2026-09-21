using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteInEditMode]
public class RFX4_MaterialQueue : MonoBehaviour
{
	[Tooltip("Background=1000, Geometry=2000, AlphaTest=2450, Transparent=3000, Overlay=4000")]
	public int queue = 2000;

	public int[] queues;

	private void Start()
	{
		Renderer component = GetComponent<Renderer>();
		if ((bool)component && (bool)component.sharedMaterial && queues != null)
		{
			component.sharedMaterial.renderQueue = queue;
			for (int i = 0; i < queues.Length && i < component.sharedMaterials.Length; i++)
			{
				component.sharedMaterials[i].renderQueue = queues[i];
			}
		}
	}

	private void OnValidate()
	{
		Start();
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			Start();
		}
	}
}

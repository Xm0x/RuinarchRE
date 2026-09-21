using UnityEngine;

public class RFX4_RealtimeReflection : MonoBehaviour
{
	private ReflectionProbe probe;

	private Transform camT;

	private void Awake()
	{
		probe = GetComponent<ReflectionProbe>();
		camT = Camera.main.transform;
	}

	private void Update()
	{
		Vector3 position = camT.position;
		probe.transform.position = new Vector3(position.x, position.y * -1f, position.z);
		probe.RenderProbe();
	}
}

using System.Collections;
using UnityEngine;

public class RFX4_CameraShake : MonoBehaviour
{
	public AnimationCurve ShakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public float Duration = 2f;

	public float Speed = 22f;

	public float Magnitude = 1f;

	public float DistanceForce = 100f;

	public float RotationDamper = 2f;

	public bool IsEnabled = true;

	private bool isPlaying;

	[HideInInspector]
	public bool canUpdate;

	private Camera cameraToShake;

	public void PlayShake()
	{
		StopCoroutine(Shake());
		StartCoroutine(Shake());
	}

	private void OnEnable()
	{
		if (Object.FindObjectsOfType(typeof(RFX4_CameraShake)) is RFX4_CameraShake[] array)
		{
			RFX4_CameraShake[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].canUpdate = false;
			}
		}
		canUpdate = true;
		if (cameraToShake == null)
		{
			cameraToShake = GetComponent<Camera>();
		}
	}

	private IEnumerator Shake()
	{
		float elapsed = 0f;
		Transform camT = ((cameraToShake != null) ? cameraToShake.transform : Camera.main.transform);
		Vector3 originalCamRotation = camT.rotation.eulerAngles;
		Vector3 direction = (base.transform.position - camT.position).normalized;
		float time = 0f;
		float randomStart = Random.Range(-1000f, 1000f);
		float distanceDamper = 1f - Mathf.Clamp01((camT.position - base.transform.position).magnitude / DistanceForce);
		Vector3 oldRotation = Vector3.zero;
		while (elapsed < Duration && canUpdate)
		{
			elapsed += Time.deltaTime;
			float num = elapsed / Duration;
			float num2 = ShakeCurve.Evaluate(num) * distanceDamper;
			time += Time.deltaTime * num2;
			camT.position -= direction * Time.deltaTime * Mathf.Sin(time * Speed) * num2 * Magnitude / 2f;
			float num3 = randomStart + Speed * num / 10f;
			float num4 = Mathf.PerlinNoise(num3, 0f) * 2f - 1f;
			float num5 = Mathf.PerlinNoise(1000f + num3, num3 + 1000f) * 2f - 1f;
			float num6 = Mathf.PerlinNoise(0f, num3) * 2f - 1f;
			if (Quaternion.Euler(originalCamRotation + oldRotation) != camT.rotation)
			{
				originalCamRotation = camT.rotation.eulerAngles;
			}
			oldRotation = Mathf.Sin(time * Speed) * num2 * Magnitude * new Vector3(0.5f + num5, 0.3f + num4, 0.3f + num6) * RotationDamper;
			camT.rotation = Quaternion.Euler(originalCamRotation + oldRotation);
			yield return null;
		}
		camT.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
	}
}

using UnityEngine;

public class RFX4_WindCurves : MonoBehaviour
{
	public AnimationCurve WindCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private WindZone windZone;

	private void Awake()
	{
		windZone = GetComponent<WindZone>();
		windZone.windMain = WindCurve.Evaluate(0f);
		windZone.windMain = 0f - WindCurve.Evaluate(0f);
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float num2 = WindCurve.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			windZone.windMain = 0f - num2;
		}
		if (num >= GraphTimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
	}
}

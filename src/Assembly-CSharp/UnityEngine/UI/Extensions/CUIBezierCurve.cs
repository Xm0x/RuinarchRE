using System;

namespace UnityEngine.UI.Extensions;

public class CUIBezierCurve : MonoBehaviour
{
	public static readonly int CubicBezierCurvePtNum = 4;

	[SerializeField]
	protected Vector3[] controlPoints;

	public Action OnRefresh;

	public Vector3[] ControlPoints => controlPoints;

	public void Refresh()
	{
		if (OnRefresh != null)
		{
			OnRefresh();
		}
	}

	public Vector3 GetPoint(float _time)
	{
		float num = 1f - _time;
		return num * num * num * controlPoints[0] + 3f * num * num * _time * controlPoints[1] + 3f * num * _time * _time * controlPoints[2] + _time * _time * _time * controlPoints[3];
	}

	public Vector3 GetTangent(float _time)
	{
		float num = 1f - _time;
		return 3f * num * num * (controlPoints[1] - controlPoints[0]) + 6f * num * _time * (controlPoints[2] - controlPoints[1]) + 3f * _time * _time * (controlPoints[3] - controlPoints[2]);
	}

	public void ReportSet()
	{
		if (controlPoints == null)
		{
			controlPoints = new Vector3[CubicBezierCurvePtNum];
			controlPoints[0] = new Vector3(0f, 0f, 0f);
			controlPoints[1] = new Vector3(0f, 1f, 0f);
			controlPoints[2] = new Vector3(1f, 1f, 0f);
			controlPoints[3] = new Vector3(1f, 0f, 0f);
		}
		_ = 1;
		_ = controlPoints.Length;
		_ = CubicBezierCurvePtNum;
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RFX4_TrailRenderer : MonoBehaviour
{
	public float VertexLifeTime = 2f;

	public float TrailLifeTime = 2f;

	[Range(0.001f, 1f)]
	public float MinVertexDistance = 0.01f;

	public float Gravity = 0.01f;

	public Vector3 Force = new Vector3(0f, 0f, 0f);

	public float InheritVelocity;

	public float Drag = 0.01f;

	[Range(0.001f, 10f)]
	public float Frequency = 1f;

	[Range(0.001f, 10f)]
	public float OffsetSpeed = 0.5f;

	public bool RandomTurbulenceOffset;

	[Range(0.001f, 10f)]
	public float Amplitude = 2f;

	public float TurbulenceStrength = 0.1f;

	public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

	public float AproximatedFlyDistance = -1f;

	public bool SmoothCurves;

	private LineRenderer lineRenderer;

	private List<Vector3> positions;

	private List<float> currentTimes;

	private List<Vector3> velocities;

	[HideInInspector]
	public float currentLifeTime;

	private Transform t;

	private Vector3 prevPosition;

	private Vector3 startPosition;

	private List<Vector3> controlPoints = new List<Vector3>();

	private int curveCount;

	private const float MinimumSqrDistance = 0.01f;

	private const float DivisionThreshold = -0.99f;

	private const float SmoothCurvesScale = 0.5f;

	private float currentVelocity;

	private float turbulenceRandomOffset;

	private bool isInitialized;

	private void Start()
	{
		Init();
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			Init();
		}
	}

	private void Init()
	{
		positions = new List<Vector3>();
		currentTimes = new List<float>();
		velocities = new List<Vector3>();
		currentLifeTime = 0f;
		curveCount = 0;
		currentVelocity = 0f;
		t = base.transform;
		prevPosition = t.position;
		startPosition = t.position;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 0;
		positions.Add(t.position);
		currentTimes.Add(currentLifeTime);
		velocities.Add(Vector3.zero);
		turbulenceRandomOffset = (RandomTurbulenceOffset ? (Random.Range(0f, 10000f) / 1000f) : 0f);
	}

	private void Update()
	{
		if (!((double)Time.timeScale < 0.001))
		{
			UpdatePositionsCount();
			UpdateForce();
			UpdateImpulse();
			UpdateVelocity();
			int lastDeletedIndex = GetLastDeletedIndex();
			RemovePositionsBeforeIndex(lastDeletedIndex);
			if (SmoothCurves && positions.Count > 2)
			{
				InterpolateBezier(positions, 0.5f);
				List<Vector3> drawingPoints = GetDrawingPoints();
				lineRenderer.positionCount = drawingPoints.Count;
				lineRenderer.SetPositions(drawingPoints.ToArray());
			}
			else
			{
				lineRenderer.positionCount = positions.Count;
				lineRenderer.SetPositions(positions.ToArray());
			}
		}
	}

	private int GetLastDeletedIndex()
	{
		int result = -1;
		int count = currentTimes.Count;
		for (int i = 1; i < count; i++)
		{
			currentTimes[i] -= Time.deltaTime;
			if (currentTimes[i] <= 0f)
			{
				result = i;
			}
		}
		return result;
	}

	private void UpdatePositionsCount()
	{
		if (!(TrailLifeTime > 0.0001f) || !(currentLifeTime > TrailLifeTime))
		{
			currentLifeTime += Time.deltaTime;
			Vector3 vector = ((positions.Count != 0) ? positions[positions.Count - 1] : Vector3.zero);
			if (Mathf.Abs((t.position - vector).magnitude) > MinVertexDistance && positions.Count > 0)
			{
				AddInterpolatedPositions(vector, t.position);
			}
		}
	}

	private void AddInterpolatedPositions(Vector3 start, Vector3 end)
	{
		int num = (int)((start - end).magnitude / MinVertexDistance);
		float num2 = currentTimes.LastOrDefault();
		Vector3 zero = Vector3.zero;
		for (int i = 1; i <= num - 1; i++)
		{
			Vector3 item = start + (end - start) * i * 1f / num;
			float item2 = num2 + (VertexLifeTime - num2) * (float)i * 1f / (float)num;
			positions.Add(item);
			currentTimes.Add(item2);
			velocities.Add(zero);
		}
	}

	private void RemovePositionsBeforeIndex(int lastDeletedIndex)
	{
		if (lastDeletedIndex != -1)
		{
			if (positions.Count - lastDeletedIndex == 1)
			{
				positions.Clear();
				currentTimes.Clear();
				velocities.Clear();
			}
			else
			{
				positions.RemoveRange(0, lastDeletedIndex);
				currentTimes.RemoveRange(0, lastDeletedIndex);
				velocities.RemoveRange(0, lastDeletedIndex);
			}
		}
	}

	private void UpdateForce()
	{
		if (positions.Count < 1)
		{
			return;
		}
		Vector3 vector = Gravity * Vector3.down * Time.deltaTime;
		Vector3 vector2 = t.rotation * Force * Time.deltaTime;
		for (int i = 0; i < positions.Count; i++)
		{
			Vector3 zero = Vector3.zero;
			if (TurbulenceStrength > 1E-06f)
			{
				Vector3 vector3 = positions[i] / Frequency;
				float num = (Time.time + turbulenceRandomOffset) * OffsetSpeed;
				vector3 -= num * Vector3.one;
				zero.x += (Mathf.PerlinNoise(vector3.z, vector3.y) * 2f - 1f) * Amplitude * Time.deltaTime * TurbulenceStrength / 10f;
				zero.y += (Mathf.PerlinNoise(vector3.x, vector3.z) * 2f - 1f) * Amplitude * Time.deltaTime * TurbulenceStrength / 10f;
				zero.z += (Mathf.PerlinNoise(vector3.y, vector3.x) * 2f - 1f) * Amplitude * Time.deltaTime * TurbulenceStrength / 10f;
			}
			Vector3 vector4 = vector + vector2 + zero;
			if (AproximatedFlyDistance > 0.01f)
			{
				float num2 = Mathf.Abs((positions[i] - startPosition).magnitude);
				vector4 *= VelocityByDistance.Evaluate(Mathf.Clamp01(num2 / AproximatedFlyDistance));
			}
			velocities[i] += vector4;
		}
	}

	private void UpdateImpulse()
	{
		if (velocities.Count != 0)
		{
			currentVelocity = (t.position - prevPosition).magnitude / Time.deltaTime;
			Vector3 normalized = (t.position - prevPosition).normalized;
			prevPosition = t.position;
			velocities[velocities.Count - 1] += currentVelocity * InheritVelocity * normalized * Time.deltaTime;
		}
	}

	private void UpdateVelocity()
	{
		if (velocities.Count == 0)
		{
			return;
		}
		int count = positions.Count;
		for (int i = 0; i < count; i++)
		{
			if (Drag > 1E-05f)
			{
				velocities[i] -= Drag * velocities[i] * Time.deltaTime;
			}
			if (velocities[i].magnitude < 1E-05f)
			{
				velocities[i] = Vector3.zero;
			}
			positions[i] += velocities[i] * Time.deltaTime;
		}
	}

	public void InterpolateBezier(List<Vector3> segmentPoints, float scale)
	{
		controlPoints.Clear();
		if (segmentPoints.Count < 2)
		{
			return;
		}
		for (int i = 0; i < segmentPoints.Count; i++)
		{
			if (i == 0)
			{
				Vector3 vector = segmentPoints[i];
				Vector3 vector2 = segmentPoints[i + 1] - vector;
				Vector3 item = vector + scale * vector2;
				controlPoints.Add(vector);
				controlPoints.Add(item);
			}
			else if (i == segmentPoints.Count - 1)
			{
				Vector3 vector3 = segmentPoints[i - 1];
				Vector3 vector4 = segmentPoints[i];
				Vector3 vector5 = vector4 - vector3;
				Vector3 item2 = vector4 - scale * vector5;
				controlPoints.Add(item2);
				controlPoints.Add(vector4);
			}
			else
			{
				Vector3 vector6 = segmentPoints[i - 1];
				Vector3 vector7 = segmentPoints[i];
				Vector3 vector8 = segmentPoints[i + 1];
				Vector3 normalized = (vector8 - vector6).normalized;
				Vector3 item3 = vector7 - scale * normalized * (vector7 - vector6).magnitude;
				Vector3 item4 = vector7 + scale * normalized * (vector8 - vector7).magnitude;
				controlPoints.Add(item3);
				controlPoints.Add(vector7);
				controlPoints.Add(item4);
			}
		}
		curveCount = (controlPoints.Count - 1) / 3;
	}

	public List<Vector3> GetDrawingPoints()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < curveCount; i++)
		{
			List<Vector3> list2 = FindDrawingPoints(i);
			if (i != 0)
			{
				list2.RemoveAt(0);
			}
			list.AddRange(list2);
		}
		return list;
	}

	private List<Vector3> FindDrawingPoints(int curveIndex)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3 item = CalculateBezierPoint(curveIndex, 0f);
		Vector3 item2 = CalculateBezierPoint(curveIndex, 1f);
		list.Add(item);
		list.Add(item2);
		FindDrawingPoints(curveIndex, 0f, 1f, list, 1);
		return list;
	}

	private int FindDrawingPoints(int curveIndex, float t0, float t1, List<Vector3> pointList, int insertionIndex)
	{
		Vector3 vector = CalculateBezierPoint(curveIndex, t0);
		Vector3 vector2 = CalculateBezierPoint(curveIndex, t1);
		if ((vector - vector2).sqrMagnitude < 0.01f)
		{
			return 0;
		}
		float num = (t0 + t1) / 2f;
		Vector3 vector3 = CalculateBezierPoint(curveIndex, num);
		Vector3 normalized = (vector - vector3).normalized;
		Vector3 normalized2 = (vector2 - vector3).normalized;
		if (Vector3.Dot(normalized, normalized2) > -0.99f || Mathf.Abs(num - 0.5f) < 0.0001f)
		{
			int num2 = 0;
			num2 += FindDrawingPoints(curveIndex, t0, num, pointList, insertionIndex);
			pointList.Insert(insertionIndex + num2, vector3);
			num2++;
			return num2 + FindDrawingPoints(curveIndex, num, t1, pointList, insertionIndex + num2);
		}
		return 0;
	}

	public Vector3 CalculateBezierPoint(int curveIndex, float t)
	{
		int num = curveIndex * 3;
		Vector3 p = controlPoints[num];
		Vector3 p2 = controlPoints[num + 1];
		Vector3 p3 = controlPoints[num + 2];
		Vector3 p4 = controlPoints[num + 3];
		return CalculateBezierPoint(t, p, p2, p3, p4);
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = 1f - t;
		float num2 = t * t;
		float num3 = num * num;
		float num4 = num3 * num;
		float num5 = num2 * t;
		return num4 * p0 + 3f * num3 * t * p1 + 3f * num * num2 * p2 + num5 * p3;
	}
}

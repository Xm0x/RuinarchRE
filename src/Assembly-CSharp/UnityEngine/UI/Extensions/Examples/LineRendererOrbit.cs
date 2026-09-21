using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.Examples;

[RequireComponent(typeof(UILineRenderer))]
public class LineRendererOrbit : MonoBehaviour
{
	private UILineRenderer lr;

	private Circle circle;

	public GameObject OrbitGO;

	private RectTransform orbitGOrt;

	private float orbitTime;

	[SerializeField]
	private float _xAxis = 3f;

	[SerializeField]
	private float _yAxis = 3f;

	[SerializeField]
	private int _steps = 10;

	public float xAxis
	{
		get
		{
			return _xAxis;
		}
		set
		{
			_xAxis = value;
			GenerateOrbit();
		}
	}

	public float yAxis
	{
		get
		{
			return _yAxis;
		}
		set
		{
			_yAxis = value;
			GenerateOrbit();
		}
	}

	public int Steps
	{
		get
		{
			return _steps;
		}
		set
		{
			_steps = value;
			GenerateOrbit();
		}
	}

	private void Awake()
	{
		lr = GetComponent<UILineRenderer>();
		orbitGOrt = OrbitGO.GetComponent<RectTransform>();
		GenerateOrbit();
	}

	private void Update()
	{
		orbitTime = ((orbitTime > (float)_steps) ? (orbitTime = 0f) : (orbitTime + Time.deltaTime));
		orbitGOrt.localPosition = circle.Evaluate(orbitTime);
	}

	private void GenerateOrbit()
	{
		circle = new Circle(_xAxis, _yAxis, _steps);
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < _steps; i++)
		{
			list.Add(circle.Evaluate(i));
		}
		list.Add(circle.Evaluate(0f));
		lr.Points = list.ToArray();
	}

	private void OnValidate()
	{
		if (lr != null)
		{
			GenerateOrbit();
		}
	}
}

using System;

namespace UnityEngine.UI.Extensions;

public class Circle
{
	[SerializeField]
	private float xAxis;

	[SerializeField]
	private float yAxis;

	[SerializeField]
	private int steps;

	public float X
	{
		get
		{
			return xAxis;
		}
		set
		{
			xAxis = value;
		}
	}

	public float Y
	{
		get
		{
			return yAxis;
		}
		set
		{
			yAxis = value;
		}
	}

	public int Steps
	{
		get
		{
			return steps;
		}
		set
		{
			steps = value;
		}
	}

	public Circle(float radius)
	{
		xAxis = radius;
		yAxis = radius;
		steps = 1;
	}

	public Circle(float radius, int steps)
	{
		xAxis = radius;
		yAxis = radius;
		this.steps = steps;
	}

	public Circle(float xAxis, float yAxis)
	{
		this.xAxis = xAxis;
		this.yAxis = yAxis;
		steps = 10;
	}

	public Circle(float xAxis, float yAxis, int steps)
	{
		this.xAxis = xAxis;
		this.yAxis = yAxis;
		this.steps = steps;
	}

	public Vector2 Evaluate(float t)
	{
		float num = 360f / (float)steps;
		float f = (float)Math.PI / 180f * num * t;
		float x = Mathf.Sin(f) * xAxis;
		float y = Mathf.Cos(f) * yAxis;
		return new Vector2(x, y);
	}

	public void Evaluate(float t, out Vector2 eval)
	{
		float num = 360f / (float)steps;
		float f = (float)Math.PI / 180f * num * t;
		eval.x = Mathf.Sin(f) * xAxis;
		eval.y = Mathf.Cos(f) * yAxis;
	}
}

using System;

[Serializable]
public struct PointFloat
{
	public float X;

	public float Y;

	public PointFloat(float x, float y)
	{
		X = x;
		Y = y;
	}

	public PointFloat Sum(PointFloat otherPoint)
	{
		return new PointFloat(otherPoint.X + X, otherPoint.Y + Y);
	}

	public float Product()
	{
		return X * Y;
	}

	public override string ToString()
	{
		return $"({X}, {Y})";
	}
}

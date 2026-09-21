using System;

[Serializable]
public struct Point
{
	public int X;

	public int Y;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Point Sum(Point otherPoint)
	{
		return new Point(otherPoint.X + X, otherPoint.Y + Y);
	}

	public int Product()
	{
		return X * Y;
	}

	public override string ToString()
	{
		return "(" + X + ", " + Y + ")";
	}
}

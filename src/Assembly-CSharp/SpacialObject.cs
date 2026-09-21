public abstract class SpacialObject
{
	public Point Location;

	public int X => Location.X;

	public int Y => Location.Y;

	public SpacialObject(int x, int y)
		: this(new Point(x, y))
	{
	}

	public SpacialObject(Point location)
	{
		Location = location;
	}

	public override string ToString()
	{
		return $"[{X}, {Y}]";
	}
}

public class GoapNode
{
	public int cost;

	public int level;

	public GoapAction action;

	public IPointOfInterest target;

	public void Initialize(int cost, int level, GoapAction action, IPointOfInterest target)
	{
		this.cost = cost;
		this.level = level;
		this.action = action;
		this.target = target;
	}

	public void Reset()
	{
		cost = 0;
		level = 0;
		action = null;
		target = null;
	}
}

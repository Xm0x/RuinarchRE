using Traits;

public static class DelegateTypes
{
	public delegate void OnAllBurningExtinguished(BurningSource source);

	public delegate void OnBurningObjectAdded(ITraitable poi);

	public delegate void OnBurningObjectRemoved(ITraitable poi);
}

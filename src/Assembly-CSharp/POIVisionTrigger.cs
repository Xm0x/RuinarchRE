public abstract class POIVisionTrigger : BaseVisionTrigger
{
	public IPointOfInterest poi { get; private set; }

	public override void Initialize(IDamageable damageable)
	{
		base.Initialize(damageable);
		poi = damageable as IPointOfInterest;
	}

	public abstract bool IgnoresStructureDifference();

	public abstract bool IgnoresRoomDifference();

	public override void Reset()
	{
		base.Reset();
		poi = null;
		if (projectileReceiver != null)
		{
			projectileReceiver.Reset();
		}
	}
}

using System;

public class ShearableAnimal : Animal
{
	public int count { get; set; }

	public bool isAvailableForShearing { get; set; }

	public override Type serializedData => typeof(SaveDataShearableAnimal);

	public ShearableAnimal(SUMMON_TYPE summonType, string className, RACE race)
		: base(summonType, className, race)
	{
		count = 80;
	}

	public ShearableAnimal(SaveDataShearableAnimal data)
		: base(data)
	{
		count = data.count;
		isAvailableForShearing = data.isAvailableForShearing;
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		}
	}

	private void OnDayStarted()
	{
		if (!base.isDead)
		{
			isAvailableForShearing = true;
		}
	}
}

namespace Traits;

public class MusicLover : Trait
{
	public override bool isSingleton => true;

	public MusicLover()
	{
		name = "Music Lover";
		description = "Has an obsessive love for music.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Music Hater" };
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		switch (action)
		{
		case INTERACTION_TYPE.SING:
			cost += -15;
			break;
		case INTERACTION_TYPE.PLAY_GUITAR:
			cost += -15;
			break;
		}
	}
}

using UnityEngine;

public class CharacterVisionTrigger : POIVisionTrigger
{
	[SerializeField]
	private bool _ignoreStructureDifference;

	[SerializeField]
	private bool _ignoreRoomDifference;

	public override void Initialize(IDamageable damageable)
	{
		base.Initialize(damageable);
		VoteToMakeVisibleToCharacters();
	}

	public override bool IgnoresStructureDifference()
	{
		return _ignoreStructureDifference;
	}

	public override bool IgnoresRoomDifference()
	{
		return _ignoreRoomDifference;
	}
}

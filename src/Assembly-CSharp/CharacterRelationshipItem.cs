using EZObjectPools;
using UnityEngine;

public class CharacterRelationshipItem : PooledObject
{
	[SerializeField]
	private CharacterPortrait portrait;

	private Character target;

	private Character owner;

	public void Initialize(Character owner, Character target)
	{
		this.owner = owner;
		this.target = target;
		portrait.GeneratePortrait(target);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void HideSmallInfo()
	{
		UIManager.Instance.HideSmallInfo();
	}
}

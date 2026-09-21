using UnityEngine;

public class WallObjectVisionTrigger : BaseVisionTrigger
{
	[SerializeField]
	protected ProjectileReceiver _projectileReceiver;

	public override ProjectileReceiver projectileReceiver => _projectileReceiver;

	public override void Initialize(IDamageable damageable)
	{
		if (_mainCollider == null)
		{
			BoxCollider2D boxCollider2D = base.gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.size = new Vector2(0.7f, 0.7f);
			_mainCollider = boxCollider2D;
		}
		base.Initialize(damageable);
		_projectileReceiver.gameObject.SetActive(value: true);
		_projectileReceiver.Initialize(damageable);
		VoteToMakeInvisibleToCharacters();
	}
}

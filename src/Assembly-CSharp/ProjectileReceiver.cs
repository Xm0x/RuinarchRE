using UnityEngine;

public abstract class ProjectileReceiver : MonoBehaviour
{
	[SerializeField]
	protected Collider2D _collider;

	protected IDamageable owner { get; private set; }

	private void Awake()
	{
		if (_collider == null)
		{
			_collider = GetComponent<Collider2D>();
		}
	}

	public void Initialize(IDamageable owner)
	{
		this.owner = owner;
	}

	public void SetColliderState(bool state)
	{
		if (_collider != null)
		{
			_collider.enabled = state;
		}
	}

	public abstract void OnTriggerEnter2D(Collider2D collision);

	public void Reset()
	{
		owner = null;
	}
}

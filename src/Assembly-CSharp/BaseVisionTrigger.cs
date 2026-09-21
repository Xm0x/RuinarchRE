using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseVisionTrigger : MonoBehaviour
{
	[FormerlySerializedAs("mainCollider")]
	[SerializeField]
	protected Collider2D _mainCollider;

	private int _filterVotes;

	public IDamageable damageable { get; private set; }

	public int filterVotes => _filterVotes;

	public virtual ProjectileReceiver projectileReceiver => null;

	public Collider2D mainCollider => _mainCollider;

	public virtual void Initialize(IDamageable damageable)
	{
		base.name = $"{damageable} collision trigger";
		this.damageable = damageable;
		_mainCollider.isTrigger = true;
		_mainCollider.enabled = true;
		SetAllCollidersState(state: true);
	}

	public virtual void SetAllCollidersState(bool state)
	{
		_mainCollider.enabled = state;
	}

	public virtual void SetVisionTriggerCollidersState(bool state)
	{
		_mainCollider.enabled = state;
	}

	public virtual void Reset()
	{
		_filterVotes = 0;
		if (_mainCollider != null)
		{
			_mainCollider.enabled = false;
		}
		TransferToNonFilteredLayer();
		damageable = null;
	}

	public virtual void SetFilterVotes(int votes)
	{
		_filterVotes = votes;
		DetermineLayerBasedOnVotes();
	}

	public virtual void VoteToMakeVisibleToCharacters()
	{
		_filterVotes = filterVotes + 1;
		DetermineLayerBasedOnVotes();
	}

	public virtual void VoteToMakeInvisibleToCharacters()
	{
		_filterVotes = filterVotes - 1;
		DetermineLayerBasedOnVotes();
	}

	private void DetermineLayerBasedOnVotes()
	{
		if (filterVotes > 0)
		{
			TransferToFilteredLayer();
		}
		else
		{
			TransferToNonFilteredLayer();
		}
	}

	private void TransferToFilteredLayer()
	{
		base.gameObject.layer = LayerMask.NameToLayer(GameUtilities.Filtered_Object_Layer);
	}

	private void TransferToNonFilteredLayer()
	{
		base.gameObject.layer = LayerMask.NameToLayer(GameUtilities.Unfiltered_Object_Layer);
	}
}

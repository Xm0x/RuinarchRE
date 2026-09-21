using UnityEngine;
using UtilityScripts;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseVisionCollider : BaseMonoBehaviour
{
	private int _filterVisionVotes;

	public int filterVotes => _filterVisionVotes;

	public void VoteToFilterVision()
	{
		_filterVisionVotes++;
		DetermineVisionLayerGivenVotes();
	}

	public void VoteToUnFilterVision()
	{
		_filterVisionVotes--;
		DetermineVisionLayerGivenVotes();
	}

	private void DetermineVisionLayerGivenVotes()
	{
		if (_filterVisionVotes > 0)
		{
			FilterVision();
		}
		else
		{
			UnFilterVision();
		}
	}

	private void FilterVision()
	{
		base.gameObject.layer = LayerMask.NameToLayer(GameUtilities.Filtered_Vision_Layer);
	}

	private void UnFilterVision()
	{
		base.gameObject.layer = LayerMask.NameToLayer(GameUtilities.Unfiltered_Vision_Layer);
	}

	public void SetFilterVisionVotes(int amount)
	{
		_filterVisionVotes = amount;
		DetermineVisionLayerGivenVotes();
	}

	public virtual void Reset()
	{
		_filterVisionVotes = 0;
	}
}

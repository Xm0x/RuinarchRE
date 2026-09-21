using System;
using UnityEngine;

public class WardLightGameObject : TileObjectGameObject
{
	private Action<Character> _onCharacterInRangeAdded;

	private Action<Character> _onCharacterInRangeRemoved;

	[SerializeField]
	private GameObject _rangeHighlightGO;

	[SerializeField]
	private BoxCollider2D _rangeCollider;

	public void SetInRangeActions(Action<Character> p_onCharacterInRangeAdded, Action<Character> p_onCharacterInRangeRemoved)
	{
		_onCharacterInRangeAdded = p_onCharacterInRangeAdded;
		_onCharacterInRangeRemoved = p_onCharacterInRangeRemoved;
	}

	public void ExecuteOnAddAction(Character p_character)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			_onCharacterInRangeAdded?.Invoke(p_character);
		}
	}

	public void ExecuteOnRemoveAction(Character p_character)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			_onCharacterInRangeRemoved?.Invoke(p_character);
		}
	}

	public void SetRangeHighlightState(bool p_state)
	{
		_rangeHighlightGO.SetActive(p_state);
	}

	public void SetRangeColliderState(bool p_state)
	{
		_rangeCollider?.gameObject.SetActive(p_state);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_rangeCollider != null)
		{
			SetRangeColliderState(p_state: false);
		}
		_onCharacterInRangeAdded = null;
		_onCharacterInRangeRemoved = null;
	}
}

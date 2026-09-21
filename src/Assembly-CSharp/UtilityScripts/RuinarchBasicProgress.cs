using System;
using UnityEngine;

namespace UtilityScripts;

[Serializable]
public class RuinarchBasicProgress : RuinarchProgressable
{
	[SerializeField]
	private string name;

	[SerializeField]
	private BOOKMARK_TYPE _bookmarkType;

	public override string progressableName => name;

	public override BOOKMARK_TYPE bookmarkType => _bookmarkType;

	public RuinarchBasicProgress(string p_name, BOOKMARK_TYPE p_bookmarkType)
	{
		name = p_name;
		_bookmarkType = p_bookmarkType;
	}

	public void Initialize(int p_minValue, int p_maxValue)
	{
		Setup(p_minValue, p_maxValue);
	}

	public void SetName(string p_name)
	{
		name = p_name;
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}
}

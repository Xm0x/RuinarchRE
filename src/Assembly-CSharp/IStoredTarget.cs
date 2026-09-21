using Inner_Maps.Location_Structures;
using UnityEngine;

public interface IStoredTarget : IBookmarkable
{
	string persistentID { get; }

	STORED_TARGET_TYPE storedTargetType { get; }

	string name { get; }

	string iconRichText { get; }

	bool isTargetted { get; set; }

	bool isStoredAsTarget { get; }

	bool IsValidForStoreTarget();

	bool IsValidTargetForPartyStructure(LocationStructure p_structure);

	bool CanBeStoredAsTarget();

	void SetAsStoredTarget(bool p_state);

	Sprite GetPortraitSprite();
}

using System;
using UnityEngine;

[Serializable]
public struct TileObjectSlotSetting
{
	public string slotName;

	public Vector3 usedPosition;

	public Vector3 unusedPosition;

	public Vector3 assetRotation;

	public Sprite slotAsset;
}

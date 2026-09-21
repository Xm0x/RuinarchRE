using System;
using UnityEngine;

public class BigTreeObject : TreeObject
{
	public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);

	public override Vector3 worldPosition
	{
		get
		{
			Vector3 position = mapVisual.transform.position;
			position.x += 0.5f;
			position.y += 0.5f;
			return position;
		}
	}

	public override Type serializedData => typeof(SaveDataBigTreeObject);

	public BigTreeObject()
		: base(TILE_OBJECT_TYPE.BIG_TREE_OBJECT)
	{
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public BigTreeObject(SaveDataBigTreeObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Big Tree " + base.id;
	}
}

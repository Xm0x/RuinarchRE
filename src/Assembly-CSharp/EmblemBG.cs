using System;
using UnityEngine;

[Serializable]
public struct EmblemBG
{
	public Sprite frame;

	public Sprite tint;

	public Sprite outline;

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public bool Equals(EmblemBG other)
	{
		if (frame.name.Equals(other.frame.name) && tint.name.Equals(other.tint.name) && outline.name.Equals(other.outline.name))
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

using System;

[Serializable]
public struct RegionTemplate
{
	public readonly int width;

	public readonly int height;

	public RegionTemplate(int width, int height)
	{
		this.width = width;
		this.height = height;
	}
}

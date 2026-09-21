using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necromancy.UI;

[Serializable]
public struct SymbolsTextureData
{
	public Texture texture;

	public char[] chars;

	private Dictionary<char, Vector2> charsDict;

	public void Initialize()
	{
		charsDict = new Dictionary<char, Vector2>();
		for (int i = 0; i < chars.Length; i++)
		{
			char key = char.ToLowerInvariant(chars[i]);
			if (!charsDict.ContainsKey(key))
			{
				Vector2 value = new Vector2(i % 10, 9 - i / 10);
				charsDict.Add(key, value);
			}
		}
	}

	public Vector2 GetTextureCoordinates(char c)
	{
		c = char.ToLowerInvariant(c);
		if (charsDict == null)
		{
			Initialize();
		}
		if (charsDict.TryGetValue(c, out var value))
		{
			return value;
		}
		return Vector2.zero;
	}
}

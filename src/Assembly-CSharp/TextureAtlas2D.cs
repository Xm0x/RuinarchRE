using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureAtlas2D
{
	private Texture2D _atlas;

	private Dictionary<string, PackedTexture2D> _packedTextures = new Dictionary<string, PackedTexture2D>();

	private List<Texture2D> _texturesToPack;

	public void AddTextureToAtlas(string p_id, Texture2D p_texture, int p_pixelsPerUnit)
	{
		_packedTextures.Add(p_id, new PackedTexture2D(p_pixelsPerUnit));
		AddTextureToList(p_texture);
	}

	public void AddTexturePathToAtlas(string p_id, string p_fullPath, int p_pixelsPerUnit)
	{
		_packedTextures.Add(p_id, new PackedTexture2D(p_pixelsPerUnit));
		_packedTextures[p_id].SetPath(p_fullPath);
	}

	private void AddTextureToList(Texture2D p_texture)
	{
		if (_texturesToPack == null)
		{
			_texturesToPack = new List<Texture2D>();
		}
		_texturesToPack.Add(p_texture);
	}

	public bool HasTextureID(string p_id)
	{
		if (_packedTextures != null)
		{
			return _packedTextures.ContainsKey(p_id);
		}
		return false;
	}

	public Sprite GetSpriteByKey(string p_key)
	{
		if (_packedTextures.ContainsKey(p_key))
		{
			return _packedTextures[p_key].sprite;
		}
		return null;
	}

	public PackedTexture2D GetPackedTextureByKey(string p_key)
	{
		if (_packedTextures.ContainsKey(p_key))
		{
			return _packedTextures[p_key];
		}
		return null;
	}

	public IEnumerator SetPackedAtlasData(Rect[] p_rects)
	{
		int i = 0;
		foreach (PackedTexture2D value in _packedTextures.Values)
		{
			value.SetData(_atlas, p_rects[i]);
			i++;
			yield return null;
		}
	}

	public IEnumerator SetPackedAtlasData(Rect[] p_rects, string p_loadingString)
	{
		int batchSize = 4;
		int batchCount = 0;
		int i = 0;
		_ = string.Empty;
		foreach (PackedTexture2D value in _packedTextures.Values)
		{
			value.SetData(_atlas, p_rects[i]);
			i++;
			string p_text = p_loadingString + $"({i}/{p_rects.Length})";
			MainMenuUI.Instance.ShowHeaderText(p_text);
			if (batchCount == batchSize)
			{
				batchCount = 0;
				yield return null;
			}
			else
			{
				batchCount++;
			}
		}
	}

	public Rect[] PackAtlasRaw()
	{
		_atlas = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		return _atlas.PackTextures(_texturesToPack.ToArray(), 0, 8192, makeNoLongerReadable: false);
	}

	public IEnumerator LoadTexturePathsFromFile()
	{
		int batchSize = 100;
		int batchCount = 0;
		foreach (PackedTexture2D value in _packedTextures.Values)
		{
			if (File.Exists(value.fullPath))
			{
				Texture2D p_texture = LoadTexture2DFromFile(value.fullPath);
				AddTextureToList(p_texture);
				batchCount++;
				if (batchCount == batchSize)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
	}

	private Texture2D LoadTexture2DFromFile(string p_fullPath)
	{
		byte[] data = File.ReadAllBytes(p_fullPath);
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		texture2D.LoadImage(data);
		return texture2D;
	}

	public void ReleaseMemory()
	{
		_texturesToPack?.Clear();
		_texturesToPack = null;
	}
}

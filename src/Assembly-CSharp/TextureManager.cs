using System.IO;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
	public static TextureManager Instance;

	private TextureAtlas2D _characterSpritesAtlas;

	public TextureAtlas2D characterSpritesAtlas
	{
		get
		{
			if (_characterSpritesAtlas == null)
			{
				_characterSpritesAtlas = new TextureAtlas2D();
			}
			return _characterSpritesAtlas;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public Texture2D LoadTexture2DFromFile(string p_filePath, string p_textureName)
	{
		string p_fullPath = Path.Combine(p_filePath, p_textureName);
		return LoadTexture2DFromFile(p_fullPath);
	}

	public Texture2D LoadTexture2DFromFile(string p_fullPath)
	{
		byte[] data = File.ReadAllBytes(p_fullPath);
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
		texture2D.LoadImage(data);
		return texture2D;
	}
}

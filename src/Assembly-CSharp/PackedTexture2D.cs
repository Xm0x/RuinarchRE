using UnityEngine;

public class PackedTexture2D
{
	private Texture2D _atlas;

	private Rect _uvRect;

	private Rect _spriteRect;

	private Sprite _sprite;

	private int _pixelsPerUnit;

	private string _fullPath;

	public Texture2D atlas => _atlas;

	public Rect uvRect => _uvRect;

	public Rect spriteRect => _spriteRect;

	public Sprite sprite => _sprite;

	public string fullPath => _fullPath;

	public PackedTexture2D(int p_pixelsPerUnit)
	{
		_pixelsPerUnit = p_pixelsPerUnit;
	}

	public PackedTexture2D(Texture2D p_atlas, Rect p_uvRect)
	{
		SetData(p_atlas, p_uvRect);
	}

	public void SetData(Texture2D p_atlas, Rect p_uvRect)
	{
		_atlas = p_atlas;
		_uvRect = p_uvRect;
		_spriteRect = new Rect(_uvRect.x * (float)_atlas.width, _uvRect.y * (float)_atlas.height, _uvRect.width * (float)_atlas.width, _uvRect.height * (float)_atlas.height);
		_sprite = Sprite.Create(_atlas, _spriteRect, new Vector2(0.5f, 0.5f), _pixelsPerUnit, 1u, SpriteMeshType.FullRect, Vector4.zero, generateFallbackPhysicsShape: false);
	}

	public void SetPath(string p_fullPath)
	{
		_fullPath = p_fullPath;
	}
}

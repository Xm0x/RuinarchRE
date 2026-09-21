using System.Collections.Generic;
using UnityEngine;

namespace SpriteGlow;

public class SpriteGlowMaterial : Material
{
	private const string outlineShaderName = "Sprites/Outline";

	private const string outsideMaterialKeyword = "SPRITE_OUTLINE_OUTSIDE";

	private static readonly Shader outlineShader = Shader.Find("Sprites/Outline");

	private static List<SpriteGlowMaterial> sharedMaterials = new List<SpriteGlowMaterial>();

	public Texture SpriteTexture => base.mainTexture;

	public bool DrawOutside => IsKeywordEnabled("SPRITE_OUTLINE_OUTSIDE");

	public bool InstancingEnabled => base.enableInstancing;

	public SpriteGlowMaterial(Texture spriteTexture, bool drawOutside = false, bool instancingEnabled = false)
		: base(outlineShader)
	{
		if (!outlineShader)
		{
			Debug.LogError(string.Format("{0} shader not found. Make sure the shader is included to the build.", "Sprites/Outline"));
		}
		base.mainTexture = spriteTexture;
		if (drawOutside)
		{
			EnableKeyword("SPRITE_OUTLINE_OUTSIDE");
		}
		if (instancingEnabled)
		{
			base.enableInstancing = true;
		}
	}

	public static Material GetSharedFor(SpriteGlowEffect spriteGlow)
	{
		for (int i = 0; i < sharedMaterials.Count; i++)
		{
			if (sharedMaterials[i].SpriteTexture == spriteGlow.Renderer.sprite.texture && sharedMaterials[i].DrawOutside == spriteGlow.DrawOutside && sharedMaterials[i].InstancingEnabled == spriteGlow.EnableInstancing)
			{
				return sharedMaterials[i];
			}
		}
		SpriteGlowMaterial spriteGlowMaterial = new SpriteGlowMaterial(spriteGlow.Renderer.sprite.texture, spriteGlow.DrawOutside, spriteGlow.EnableInstancing);
		spriteGlowMaterial.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
		sharedMaterials.Add(spriteGlowMaterial);
		return spriteGlowMaterial;
	}
}

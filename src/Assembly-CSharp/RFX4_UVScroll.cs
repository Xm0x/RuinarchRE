using UnityEngine;

public class RFX4_UVScroll : MonoBehaviour
{
	public Vector2 UvScrollMultiplier = new Vector2(1f, 0f);

	public RFX4_TextureShaderProperties TextureName;

	private Vector2 uvOffset = Vector2.zero;

	private Material mat;

	private void Start()
	{
		Renderer component = GetComponent<Renderer>();
		if (component == null)
		{
			Projector component2 = GetComponent<Projector>();
			if (component2 != null)
			{
				if (!component2.material.name.EndsWith("(Instance)"))
				{
					component2.material = new Material(component2.material)
					{
						name = component2.material.name + " (Instance)"
					};
				}
				mat = component2.material;
			}
		}
		else
		{
			mat = component.material;
		}
	}

	private void Update()
	{
		uvOffset += UvScrollMultiplier * Time.deltaTime;
		if (mat != null)
		{
			mat.SetTextureOffset(TextureName.ToString(), uvOffset);
		}
	}
}

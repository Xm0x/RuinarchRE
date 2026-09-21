using UnityEngine;
using UtilityScripts;

public class CustomHorizaontalLayout : MonoBehaviour
{
	[SerializeField]
	private RectTransform[] children;

	[SerializeField]
	private float spacing;

	[ExecuteInEditMode]
	private void OnEnable()
	{
		UpdateChildren();
	}

	[ExecuteInEditMode]
	[ContextMenu("Execute")]
	public void Execute()
	{
		if (children != null)
		{
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < children.Length; i++)
			{
				RectTransform rectTransform = children[i];
				rectTransform.localPosition = zero;
				zero.x += rectTransform.sizeDelta.x + spacing;
			}
		}
	}

	[ExecuteInEditMode]
	[ContextMenu("Update Children")]
	public void UpdateChildren()
	{
		children = GameUtilities.GetComponentsInDirectChildren<RectTransform>(base.gameObject);
	}
}

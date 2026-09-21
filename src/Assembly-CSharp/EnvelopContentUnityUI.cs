using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnvelopContentUnityUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform otherTransform;

	[SerializeField]
	private bool followWidth;

	[SerializeField]
	private bool followHeight;

	[SerializeField]
	private Vector2 padding;

	private bool executeOnEnable;

	private void OnEnable()
	{
		if (executeOnEnable)
		{
			Execute();
			executeOnEnable = false;
		}
	}

	[ContextMenu("Execute")]
	public void Execute()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Envelop());
		}
		else
		{
			executeOnEnable = true;
		}
	}

	private IEnumerator Envelop()
	{
		yield return null;
		RectTransform obj = base.transform as RectTransform;
		Vector2 sizeDelta = obj.sizeDelta;
		if (followWidth)
		{
			sizeDelta.x = otherTransform.sizeDelta.x;
			sizeDelta.x += padding.x;
		}
		if (followHeight)
		{
			sizeDelta.y = otherTransform.sizeDelta.y;
			sizeDelta.y += padding.y;
		}
		obj.sizeDelta = sizeDelta;
		if (base.transform.parent is RectTransform layoutRoot)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
		}
	}
}

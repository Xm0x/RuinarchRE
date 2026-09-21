using UnityEngine;
using UnityEngine.UI;

public class ItemDropBtn : MonoBehaviour
{
	public Text buttonText;

	public void Set(string name, float rate)
	{
		base.name = name;
		buttonText.text = $"{name} ({rate}%)";
	}

	public void OnClickItemDrop()
	{
		MonsterPanelUI.Instance.SetItemDropBn(this);
	}
}

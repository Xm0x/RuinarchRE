using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChanceTheWrapper : MonoBehaviour
{
	[SerializeField]
	private GameObject goChanceItemPrefab;

	[SerializeField]
	private ScrollRect scrollRectChanceItems;

	public void Initialize()
	{
		foreach (KeyValuePair<CHANCE_TYPE, int> integerChance in ChanceData.integerChances)
		{
			Object.Instantiate(goChanceItemPrefab, Vector3.zero, Quaternion.identity, scrollRectChanceItems.content).GetComponent<ChanceItem>().Initialize(integerChance.Key);
		}
	}
}

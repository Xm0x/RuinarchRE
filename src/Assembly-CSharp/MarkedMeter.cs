using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class MarkedMeter : MonoBehaviour
{
	[SerializeField]
	private Image meterBG;

	[SerializeField]
	private Image meterFill;

	[SerializeField]
	private GameObject meterMarkPrefab;

	[SerializeField]
	private Transform meterMarksParent;

	public void ResetMarks()
	{
		Utilities.DestroyChildren(meterMarksParent);
	}

	public void AddMark(float percent, Color color)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(meterMarkPrefab.name, Vector3.zero, Quaternion.identity, meterMarksParent);
		Vector3 localPosition = obj.transform.localPosition;
		localPosition.x = meterBG.rectTransform.sizeDelta.x * percent;
		obj.transform.localPosition = localPosition;
		obj.GetComponent<MeterMark>().SetColor(color);
	}

	public void SetFillAmount(float amount)
	{
		meterFill.fillAmount = amount;
	}
}

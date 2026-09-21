using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickedWaveAnimation : MonoBehaviour
{
	public GameObject WaveObject;

	public GameObject CanvasMain;

	public int PoolSize;

	private Pool poolClass;

	private void Start()
	{
		poolClass = base.gameObject.AddComponent<Pool>();
		poolClass.CreatePool(WaveObject, PoolSize);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			GameObject gameObject = UiHitted();
			if ((bool)gameObject)
			{
				CreateWave(gameObject.transform);
			}
		}
	}

	private void CreateWave(Transform Parent)
	{
		GameObject gameObject = poolClass.GetObject();
		if ((bool)gameObject)
		{
			gameObject.transform.SetParent(CanvasMain.transform);
			gameObject.GetComponent<MaskableGraphic>().color = Parent.GetComponent<MaskableGraphic>().color - new Color(0.1f, 0.1f, 0.1f);
			Vector3 vector = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			vector.x = vector.x * (float)Screen.width - (float)Screen.width / 2f;
			vector.y = vector.y * (float)Screen.height - (float)Screen.height / 2f;
			vector.z = 0f;
			gameObject.GetComponent<RectTransform>().localPosition = vector / CanvasMain.transform.localScale.x;
			gameObject.transform.SetParent(Parent);
			gameObject.GetComponent<EasyTween>().OpenCloseObjectAnimation();
		}
	}

	public GameObject UiHitted()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			if ((bool)list[i].gameObject.GetComponent<Button>() && (bool)list[i].gameObject.GetComponent<Mask>())
			{
				return list[i].gameObject;
			}
		}
		return null;
	}
}

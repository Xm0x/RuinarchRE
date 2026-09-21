using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;
using UnityEngine.UI;

public class LogsTagButton : PooledObject
{
	[SerializeField]
	private Image mainTagImage;

	[SerializeField]
	private GameObject additionalTagsPlusObject;

	[SerializeField]
	private GameObject additionalTagsGO;

	[SerializeField]
	private RectTransform additionalTagsRect;

	[SerializeField]
	private GameObject tagWithNamePrefab;

	private List<LOG_TAG> localTags;

	private bool hasPopulatedTagsGO;

	private List<PooledObject> _logTagObjects;

	public string logID;

	private void Awake()
	{
		localTags = new List<LOG_TAG>();
		_logTagObjects = new List<PooledObject>();
	}

	public void SetTags(List<LOG_TAG> tags)
	{
		LOG_TAG lOG_TAG = ((tags == null || tags.Count <= 0) ? LOG_TAG.Work : tags[0]);
		mainTagImage.sprite = UIManager.Instance.GetLogTagSprite(lOG_TAG);
		localTags.AddRange(tags);
		additionalTagsPlusObject.gameObject.SetActive(tags.Count > 1);
	}

	public void ShowAllTags()
	{
		if (!hasPopulatedTagsGO)
		{
			CreateTagItems();
		}
		additionalTagsGO.gameObject.SetActive(value: true);
	}

	private void CreateTagItems()
	{
		for (int i = 0; i < localTags.Count; i++)
		{
			LOG_TAG lOG_TAG = localTags[i];
			LogTagWithName component = ObjectPoolManager.Instance.InstantiateObjectFromPool(tagWithNamePrefab.name, Vector3.zero, Quaternion.identity, additionalTagsRect).GetComponent<LogTagWithName>();
			component.SetTag(lOG_TAG);
			_logTagObjects.Add(component);
		}
		hasPopulatedTagsGO = true;
	}

	public void HideAllTags()
	{
		additionalTagsGO.gameObject.SetActive(value: false);
	}

	public override void Reset()
	{
		hasPopulatedTagsGO = false;
		localTags.Clear();
		for (int i = 0; i < _logTagObjects.Count; i++)
		{
			PooledObject pooledObject = _logTagObjects[i];
			ObjectPoolManager.Instance.DestroyObjectWithoutCheckingChildren(pooledObject);
		}
		_logTagObjects.Clear();
		logID = string.Empty;
	}
}

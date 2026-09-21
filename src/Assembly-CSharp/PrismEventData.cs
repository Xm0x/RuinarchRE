using UnityEngine;

[CreateAssetMenu(fileName = "New Prism Event Data", menuName = "Scriptable Objects/Prism Event Data")]
public class PrismEventData : ScriptableObject
{
	public PRISM_EVENT eventType;

	public string keyName;

	public string keyDescription;

	public int manaCost;

	private string _eventName;

	private string _eventDescription;

	public string eventName
	{
		get
		{
			if (string.IsNullOrEmpty(_eventName))
			{
				_eventName = LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", keyName);
			}
			return _eventName;
		}
	}

	public string eventDescription
	{
		get
		{
			if (string.IsNullOrEmpty(_eventDescription))
			{
				_eventDescription = LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", keyDescription);
			}
			return _eventDescription;
		}
	}
}

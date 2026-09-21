using Settings;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSettingsData", menuName = "Scriptable Objects/DefaultSettingsData")]
public class DefaultSettingsData : ScriptableObject
{
	public global::Settings.Settings defaultSettings;
}

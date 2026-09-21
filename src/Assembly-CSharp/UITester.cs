using UnityEngine;

public class UITester : MonoBehaviour
{
	private void Start()
	{
		ObjectPoolManager.Instance.InitializeObjectPools();
	}

	private void OnValidate()
	{
	}

	private void OnGUI()
	{
		GUI.Button(new Rect(10f, 10f, 50f, 50f), "Test");
	}
}

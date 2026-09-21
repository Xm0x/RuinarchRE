using UnityEngine;

public class BiolabUITester : MonoBehaviour
{
	[SerializeField]
	private BiolabUIController _biolabUIController;

	private void Start()
	{
		_biolabUIController.Init();
		_biolabUIController.Open();
	}
}

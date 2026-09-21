using System.Collections;
using TMPro;
using UnityEngine;

public class TextTyper : MonoBehaviour
{
	public float revealSpeed = 0.05f;

	private TMP_Text m_textMeshPro;

	private void Awake()
	{
		m_textMeshPro = GetComponent<TMP_Text>();
		m_textMeshPro.enableWordWrapping = true;
	}

	public void Execute()
	{
		StartCoroutine(Type());
	}

	private IEnumerator Type()
	{
		m_textMeshPro.ForceMeshUpdate();
		int totalVisibleCharacters = m_textMeshPro.textInfo.characterCount;
		int counter = 0;
		while (counter != totalVisibleCharacters + 1)
		{
			int maxVisibleCharacters = counter % (totalVisibleCharacters + 1);
			m_textMeshPro.maxVisibleCharacters = maxVisibleCharacters;
			counter++;
			yield return new WaitForSeconds(revealSpeed);
		}
	}
}

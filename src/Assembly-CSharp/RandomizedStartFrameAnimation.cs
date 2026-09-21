using UnityEngine;

public class RandomizedStartFrameAnimation : MonoBehaviour
{
	private Animator _animator;

	private void Start()
	{
		float normalizedTime = Random.Range(0f, 1f);
		_animator = GetComponent<Animator>();
		_animator.Play("Wiggle", 0, normalizedTime);
	}
}

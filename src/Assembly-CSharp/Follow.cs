using DG.Tweening;
using UnityEngine;

public class Follow : MonoBehaviour
{
	public Transform target;

	private Vector3 targetLastPos;

	private Tweener tween;

	private void Start()
	{
		tween = base.transform.DOMove(target.position, 2f).SetAutoKill(autoKillOnCompletion: false);
		targetLastPos = target.position;
	}

	private void Update()
	{
		if (!(targetLastPos == target.position))
		{
			tween.ChangeEndValue(target.position, snapStartValue: true).Restart();
			targetLastPos = target.position;
		}
	}
}

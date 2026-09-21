namespace UITween;

public interface IAniamtionPartProxy
{
	bool IsObjectOpened();

	void ChangeStatus();

	void SetAniamtioDuration(float duration);

	float GetAnimationDuration();
}

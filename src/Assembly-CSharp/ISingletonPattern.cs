public interface ISingletonPattern
{
	void Initialize();

	void AddCleanupListener();

	void CleanUpAndRemoveCleanUpListener();
}

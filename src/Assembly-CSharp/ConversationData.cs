public class ConversationData
{
	public string text;

	public DialogItem.Position position;

	public Character character;

	public void Reset()
	{
		text = null;
		position = DialogItem.Position.Left;
		character = null;
	}
}

using System;

[Serializable]
public class SaveDataNonActionEventsComponent : SaveData<NonActionEventsComponent>
{
	public GameDate lastConversationDate;

	public bool canChatOrFlirt;

	public GameDate chatOrFlirtReenableDate;

	public override void Save(NonActionEventsComponent data)
	{
		lastConversationDate = data.lastConversationDate;
		canChatOrFlirt = data.canChatOrFlirt;
		chatOrFlirtReenableDate = data.chatOrFlirtReenableDate;
	}

	public override NonActionEventsComponent Load()
	{
		return new NonActionEventsComponent(this);
	}
}

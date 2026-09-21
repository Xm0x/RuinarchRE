public class SaveDataBookmarkComponent : SaveData<BookmarkComponent>
{
	public override void Save(BookmarkComponent data)
	{
		base.Save(data);
	}

	public override BookmarkComponent Load()
	{
		return new BookmarkComponent();
	}
}

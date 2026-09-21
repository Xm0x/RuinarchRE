using System.Collections.Generic;

public class SaveTileObjectThreadQueueItem
{
	public List<TileObject> list;

	public bool isDone;

	public void Reset()
	{
		if (list != null)
		{
			list.TrimExcess();
			list?.Clear();
			list = null;
		}
		isDone = false;
	}
}

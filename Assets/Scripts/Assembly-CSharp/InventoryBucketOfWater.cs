using System.Collections;
using PowerTools.Quest;

public class InventoryBucketOfWater : InventoryScript<InventoryBucketOfWater>
{
	private IEnumerator OnInteractInventory(IInventory thisItem)
	{
		yield return QuestScript.E.Break;
	}
}

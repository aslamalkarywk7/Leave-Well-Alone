namespace PowerTools.Quest
{
	public interface IInventory
	{
		string Description { get; set; }

		string Anim { get; set; }

		string AnimGui { get; set; }

		string AnimCursor { get; set; }

		string AnimCursorInactive { get; set; }

		string ScriptName { get; }

		bool Active { get; set; }

		bool Owned { get; set; }

		bool EverCollected { get; }

		bool FirstUse { get; }

		bool FirstLook { get; }

		int UseCount { get; }

		int LookCount { get; }

		Inventory Data { get; }

		void Add(int quantity = 1);

		void AddAsActive(int quantity = 1);

		void Remove(int quantity = 1);

		void SetActive();

		T GetScript<T>() where T : InventoryScript<T>;
	}
}

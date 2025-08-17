using UnityEngine;

namespace PowerTools.Quest
{
	public interface IHotspot : IQuestClickableInterface
	{
		string Description { get; set; }

		string ScriptName { get; }

		MonoBehaviour Instance { get; }

		bool Clickable { get; set; }

		float Baseline { get; set; }

		Vector2 WalkToPoint { get; set; }

		Vector2 LookAtPoint { get; set; }

		string Cursor { get; set; }

		bool FirstUse { get; }

		bool FirstLook { get; }

		int UseCount { get; }

		int LookCount { get; }

		Hotspot Data { get; }

		void Show();

		void Hide();

		void Enable();

		void Disable();
	}
}

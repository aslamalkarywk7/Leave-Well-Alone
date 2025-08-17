using UnityEngine;

namespace PowerTools.Quest
{
	public interface IRegion
	{
		string ScriptName { get; }

		MonoBehaviour Instance { get; }

		bool Enabled { get; set; }

		bool Walkable { get; set; }

		Color Tint { get; set; }

		Region Data { get; }

		bool GetCharacterOnRegion(ICharacter character = null);

		bool ContainsCharacter(ICharacter character = null);

		bool ContainsPoint(Vector2 position);
	}
}

using System;

namespace PowerTools.Quest
{
	[Serializable]
	public class CameraShakeData
	{
		public float m_intensity = 1f;

		public float m_duration = 0.1f;

		public float m_falloff = 0.15f;
	}
}

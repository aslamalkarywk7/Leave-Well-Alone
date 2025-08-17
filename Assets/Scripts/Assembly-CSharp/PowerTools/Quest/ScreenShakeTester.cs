using UnityEngine;

namespace PowerTools.Quest
{
	public class ScreenShakeTester : MonoBehaviour
	{
		[Header("Add to scene, then run & press SPACE to shake")]
		[SerializeField]
		private float m_intensity = 1f;

		[SerializeField]
		private float m_duration;

		[SerializeField]
		private float m_falloff = 0.15f;

		private void Start()
		{
		}

		private void Update()
		{
			if (Singleton<PowerQuest>.Get.GameHasKeyboardFocus && Input.GetKeyDown(KeyCode.Space))
			{
				Singleton<PowerQuest>.Get.GetCamera().Shake(m_intensity, m_duration, m_falloff);
			}
		}
	}
}

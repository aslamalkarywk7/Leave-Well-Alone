using UnityEngine;

namespace PowerTools.Quest
{
	public interface ICamera
	{
		bool Enabled { get; set; }

		bool IgnoreBounds { get; set; }

		float Zoom { get; set; }

		Vector2 Position { get; set; }

		QuestCameraComponent GetInstance();

		ICharacter GetCharacterToFollow();

		void SetCharacterToFollow(ICharacter character, float overTime = 0f);

		Vector2 GetPositionOverride();

		bool GetHasPositionOverride();

		bool GetHasPositionOverrideOrTransition();

		bool GetTransitioning();

		void SetPositionOverride(float x, float y = 0f, float transitionTime = 0f);

		void SetPositionOverride(Vector2 positionOverride, float transitionTime = 0f);

		void ResetPositionOverride(float transitionTime = 0f);

		float GetZoom();

		bool GetHasZoom();

		bool GetHasZoomOrTransition();

		void SetZoom(float zoom, float transitionTime = 0f);

		void ResetZoom(float transitionTime = 0f);

		Vector2 GetPosition();

		void Snap();

		void Shake(float intensity = 1f, float duration = 0.1f, float falloff = 0.15f);

		void Shake(CameraShakeData data);
	}
}

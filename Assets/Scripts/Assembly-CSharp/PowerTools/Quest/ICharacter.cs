using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public interface ICharacter : IQuestClickableInterface
	{
		string Description { get; set; }

		string ScriptName { get; }

		MonoBehaviour Instance { get; }

		IRoom Room { get; set; }

		IRoom LastRoom { get; }

		Vector2 Position { get; set; }

		Vector2 TargetPosition { get; }

		float Baseline { get; set; }

		Vector2 WalkSpeed { get; set; }

		bool TurnBeforeWalking { get; set; }

		bool TurnBeforeFacing { get; set; }

		float TurnSpeedFPS { get; set; }

		bool AdjustSpeedWithScaling { get; set; }

		Vector2 SolidSize { get; set; }

		bool UseSpriteAsHotspot { get; set; }

		eFace Facing { get; set; }

		bool Clickable { get; set; }

		bool Visible { get; set; }

		bool VisibleInRoom { get; }

		bool Solid { get; set; }

		bool Moveable { get; set; }

		bool Walking { get; }

		bool Talking { get; }

		bool Animating { get; }

		bool IsPlayer { get; }

		Color TextColour { get; set; }

		Vector2 TextPositionOverride { get; set; }

		Vector2 TextPositionOffset { get; set; }

		string AnimIdle { get; set; }

		string AnimWalk { get; set; }

		string AnimTalk { get; set; }

		string AnimMouth { get; set; }

		string AnimPrefix { get; set; }

		string Cursor { get; set; }

		bool UseRegionTinting { get; set; }

		bool UseRegionScaling { get; set; }

		bool FirstUse { get; }

		bool FirstLook { get; }

		int UseCount { get; }

		int LookCount { get; }

		Vector2 WalkToPoint { get; set; }

		Vector2 LookAtPoint { get; set; }

		string Animation { get; set; }

		string FootstepSound { get; set; }

		bool AntiGlide { get; set; }

		int ClickableColliderId { get; set; }

		IInventory ActiveInventory { get; set; }

		string ActiveInventoryName { get; set; }

		bool HasActiveInventory { get; }

		Character Data { get; }

		void SetPosition(float x, float y, eFace face = eFace.None);

		void SetPosition(Vector2 position, eFace face = eFace.None);

		void SetPosition(IQuestClickableInterface atClickable, eFace face = eFace.None);

		void ResetWalkSpeed();

		void SetTextPosition(Vector2 worldPosition);

		void SetTextPosition(float worldPosX, float worldPosY);

		void LockTextPosition();

		void ResetTextPosition();

		Coroutine WalkTo(float x, float y, bool anywhere = false);

		Coroutine WalkTo(Vector2 pos, bool anywhere = false);

		Coroutine WalkTo(IQuestClickableInterface clickable, bool anywhere = false);

		void WalkToBG(float x, float y, bool anywhere = false, eFace thenFace = eFace.None);

		void WalkToBG(Vector2 pos, bool anywhere = false, eFace thenFace = eFace.None);

		void WalkToBG(IQuestClickableInterface clickable, bool anywhere = false, eFace thenFace = eFace.None);

		Coroutine WalkToClicked(bool anywhere = false);

		void StopWalking();

		void AddWaypoint(float x, float y, eFace thenFace = eFace.None);

		void AddWaypoint(Vector2 pos, eFace thenFace = eFace.None);

		Coroutine MoveTo(float x, float y, bool anywhere = false);

		Coroutine MoveTo(Vector2 pos, bool anywhere = false);

		Coroutine MoveTo(IQuestClickableInterface clickable, bool anywhere = false);

		void MoveToBG(float x, float y, bool anywhere = false);

		void MoveToBG(Vector2 pos, bool anywhere = false);

		void MoveToBG(IQuestClickableInterface clickable, bool anywhere = false);

		Coroutine ChangeRoom(IRoom room);

		void ChangeRoomBG(IRoom room);

		[Obsolete("Show(bool clickable) is obsolete. Use Show(), and Clickable property. Note that Show/Hide functions now remember previous state of visible/clickable/solid and restore it.")]
		void Show(bool clickable);

		void Show(Vector2 pos = default(Vector2), eFace facing = eFace.None);

		void Show(float posX, float posy, eFace facing = eFace.None);

		void Show(eFace facing);

		void Hide();

		void Enable();

		[Obsolete("Show(bool clickable) is obsolete. Use Show(), and Clickable property. Note that Show/Hide functions now remember previous state of visible/clickable/solid and restore it.")]
		void Enable(bool clickable);

		void Disable();

		Coroutine Face(eFace direction, bool instant = false);

		Coroutine Face(IQuestClickable clickable, bool instant = false);

		Coroutine Face(IQuestClickableInterface clickable, bool instant = false);

		Coroutine FaceDown(bool instant = false);

		Coroutine FaceUp(bool instant = false);

		Coroutine FaceLeft(bool instant = false);

		Coroutine FaceRight(bool instant = false);

		Coroutine FaceUpRight(bool instant = false);

		Coroutine FaceUpLeft(bool instant = false);

		Coroutine FaceDownRight(bool instant = false);

		Coroutine FaceDownLeft(bool instant = false);

		Coroutine Face(float x, float y, bool instant = false);

		Coroutine Face(Vector2 location, bool instant = false);

		Coroutine FaceClicked(bool instant = false);

		Coroutine FaceAway(bool instant = false);

		Coroutine FaceDirection(Vector2 directionV2, bool instant = false);

		void FaceBG(eFace direction, bool instant = false);

		void FaceBG(IQuestClickable clickable, bool instant = false);

		void FaceBG(IQuestClickableInterface clickable, bool instant = false);

		void FaceDownBG(bool instant = false);

		void FaceUpBG(bool instant = false);

		void FaceLeftBG(bool instant = false);

		void FaceRightBG(bool instant = false);

		void FaceUpRightBG(bool instant = false);

		void FaceUpLeftBG(bool instant = false);

		void FaceDownRightBG(bool instant = false);

		void FaceDownLeftBG(bool instant = false);

		void FaceBG(float x, float y, bool instant = false);

		void FaceBG(Vector2 location, bool instant = false);

		void FaceClickedBG(bool instant = false);

		void FaceAwayBG(bool instant = false);

		void FaceDirectionBG(Vector2 directionV2, bool instant = false);

		void StartFacingCharacter(ICharacter character, float minWaitTime = 0.2f, float maxWaitTime = 0.4f);

		void StopFacingCharacter();

		Coroutine Say(string dialog, int id = -1);

		Coroutine SayBG(string dialog, int id = -1);

		void CancelSay();

		Coroutine PlayAnimation(string animName);

		void PlayAnimationBG(string animName, bool pauseAtEnd = false);

		void PauseAnimation();

		void ResumeAnimation();

		void StopAnimation();

		void SkipTransition();

		void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action);

		void RemoveAnimationTrigger(string triggerName);

		Coroutine WaitForAnimTrigger(string eventName);

		Coroutine WaitForTransition(bool skippable = false);

		Coroutine WaitForIdle(bool skippable = false);

		float GetInventoryItemCount();

		float GetInventoryQuantity(string itemName);

		bool HasInventory(string itemName);

		bool GetEverHadInventory(string itemName);

		void AddInventory(string itemName, float quantity = 1f);

		void RemoveInventory(string itemName, float quantity = 1f);

		float GetInventoryQuantity(IInventory item);

		bool HasInventory(IInventory item);

		bool GetEverHadInventory(IInventory item);

		void AddInventory(IInventory item, float quantity = 1f);

		void RemoveInventory(IInventory item, float quantity = 1f);

		void ClearInventory();

		void ReplaceInventory(IInventory oldItem, IInventory newItem);

		T GetScript<T>() where T : CharacterScript<T>;
	}
}

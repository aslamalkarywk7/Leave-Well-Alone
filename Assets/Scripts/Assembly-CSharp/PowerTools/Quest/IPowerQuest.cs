using System;
using System.Collections.Generic;
using UnityEngine;

namespace PowerTools.Quest
{
	public interface IPowerQuest
	{
		YieldInstruction Break { get; }

		YieldInstruction ConsumeEvent { get; }

		bool IsDebugBuild { get; }

		ICamera Camera { get; }

		ICursor Cursor { get; }

		bool GameHasKeyboardFocus { get; }

		Color FadeColor { get; set; }

		Color FadeColorDefault { get; set; }

		bool Paused { get; set; }

		ICharacter Player { get; set; }

		IInventory ActiveInventory { get; set; }

		int InlineDialogResult { get; }

		float VerticalResolution { get; }

		float DefaultVerticalResolution { get; }

		QuestSettings Settings { get; }

		float TransitionFadeTime { get; set; }

		string DisplayBoxGui { get; set; }

		string DialogTreeGui { get; set; }

		string CustomSpeechGui { get; set; }

		bool AlwaysShowDisplayText { get; set; }

		eSpeechStyle SpeechStyle { get; set; }

		Coroutine Wait(float time = 0.5f);

		Coroutine WaitSkip(float time = 0.5f);

		Coroutine WaitForTimer(string timerName, bool skippable = false);

		Coroutine WaitFor(PowerQuest.DelegateWaitForFunction functionToWaitFor, bool autoLoadQuestScript = true);

		Coroutine WaitWhile(Func<bool> condition, bool skippable = false);

		Coroutine WaitUntil(Func<bool> condition, bool skippable = false);

		Coroutine WaitForDialog();

		Coroutine WaitForGui(IGui gui);

		void DelayedInvoke(float time, Action functionToInvoke);

		void InterruptNextLine(float secondsBeforeEndOfLine);

		void SkipDialog(bool preventEarlySkip = true);

		bool SkipCutscene();

		bool GetBlocked();

		IGui GetFocusedGui();

		bool NavigateGui(eGuiNav input = eGuiNav.Ok);

		Coroutine Display(string dialog, int id = -1);

		Coroutine DisplayBG(string dialog, int id = -1);

		void StartCutscene();

		void EndCutscene();

		bool GetSkippingCutscene();

		Coroutine FadeIn(float time = 0.2f, bool skippable = true);

		Coroutine FadeOut(float time = 0.2f, bool skippable = true);

		void FadeInBG(float time = 0.2f);

		void FadeOutBG(float time = 0.2f);

		Coroutine FadeIn(float time, string source, bool skippable = true);

		Coroutine FadeOut(float time, string source, bool skippable = true);

		void FadeInBG(float time, string source);

		void FadeOutBG(float time, string source);

		bool GetFading();

		void FadeColorRestore();

		void Pause(string source = null);

		void UnPause(string source = null);

		void SetTimer(string name, float time);

		bool GetTimerExpired(string name);

		float GetTimer(string name);

		void ChangeRoomBG(IRoom room);

		Coroutine ChangeRoom(IRoom room);

		Room GetCurrentRoom();

		void DebugSetPreviousRoom(IRoom room);

		Room GetRoom(string scriptName);

		Character GetPlayer();

		void SetPlayer(ICharacter character, float cameraPanTime = 0f);

		Character GetCharacter(string scriptName);

		Inventory GetInventory(string scriptName);

		DialogTree GetDialogTree(string scriptName);

		Coroutine WaitForInlineDialog(params string[] options);

		Gui GetGui(string scriptName);

		GameObject GetSpawnablePrefab(string name);

		Camera GetCameraGui();

		Canvas GetCanvas();

		Vector2 GetMousePosition();

		Vector2 GetMousePositionGui();

		IQuestClickable GetMouseOverClickable();

		eQuestClickableType GetMouseOverType();

		string GetMouseOverDescription();

		Vector2 GetLastLookAt();

		Vector2 GetLastWalkTo();

		bool ProcessClick(eQuestVerb verb);

		bool ProcessClick(eQuestVerb verb, IQuestClickable clickable, Vector2 mousePosition);

		Coroutine HandleInteract(IHotspot target);

		Coroutine HandleLookAt(IHotspot target);

		Coroutine HandleInventory(IHotspot target, IInventory item);

		Coroutine HandleInteract(IProp target);

		Coroutine HandleLookAt(IProp target);

		Coroutine HandleInventory(IProp target, IInventory item);

		Coroutine HandleInteract(ICharacter target);

		Coroutine HandleLookAt(ICharacter target);

		Coroutine HandleInventory(ICharacter target, IInventory item);

		Coroutine HandleInteract(IInventory target);

		Coroutine HandleLookAt(IInventory target);

		Coroutine HandleInventory(IInventory target, IInventory item);

		Coroutine HandleOption(IDialogTree dialog, string optionName);

		void DisableCancel();

		bool FirstOccurrence(string uniqueString);

		int Occurrence(string uniqueString);

		int GetOccurrenceCount(string uniqueString);

		void Restart();

		void Restart(IRoom room, string playFromFunction = null);

		void DisableAllClickablesExcept();

		void DisableAllClickablesExcept(params string[] exceptions);

		void DisableAllClickablesExcept(params IQuestClickableInterface[] exceptions);

		void RestoreAllClickables();

		void SetAllClickableCursors(string cursor, params string[] exceptions);

		void RestoreAllClickableCursors();

		List<QuestSaveSlotData> GetSaveSlotData();

		QuestSaveSlotData GetSaveSlotData(int slot);

		QuestSaveSlotData GetLastSaveSlotData();

		bool SaveSettings();

		bool Save(int slot, string description, Texture2D imageOverride = null);

		bool RestoreSave(int slot);

		bool RestoreLastSave();

		bool DeleteSave(int slot);

		bool GetRestoringGame();

		void AddSaveData(string name, object data, Action OnPostRestore = null);

		void RemoveSaveData(string name);

		T GetScript<T>() where T : QuestScript;
	}
}

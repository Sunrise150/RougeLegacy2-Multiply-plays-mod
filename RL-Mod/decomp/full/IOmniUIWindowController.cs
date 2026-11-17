using Rewired;
using UnityEngine.Events;

public interface IOmniUIWindowController
{
	int SelectedCategoryIndex { get; }

	int HighlightedCategoryIndex { get; }

	int SelectedEntryIndex { get; }

	bool IsInCategories { get; }

	bool CanReset { get; }

	Player RewiredPlayer { get; }

	OmniUICommonSerializedFields CommonFields { get; }

	UnityEvent SelectOptionEvent { get; }

	void SetEntryNavigationEnabled(bool enable);

	void SetKeyboardEnabled(bool enable);

	void SetSelectedCategoryIndex(int index, bool playSFX);

	void SetHighlightedCategoryIndex(int index, bool playSFX);

	void SetHighlightedCategory(BaseOmniUICategoryEntry categoryEntry);

	void SetSelectedEntryIndex(int index, bool playSFX, bool usingMouse);
}

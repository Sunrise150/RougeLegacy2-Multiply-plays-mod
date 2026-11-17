using Rewired;

public interface ITextChangedObj
{
	void OnTextChanged();

	void LastControllerChanged(Controller controller);
}

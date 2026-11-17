using System;

public class LanguageChangedEventArgs : EventArgs
{
	private LanguageType m_newLanguage;

	public LanguageType NewLanguage { get; private set; }

	public LanguageChangedEventArgs(LanguageType newLanguage)
	{
		Initialize(newLanguage);
	}

	public void Initialize(LanguageType newLanguage)
	{
		NewLanguage = newLanguage;
	}
}

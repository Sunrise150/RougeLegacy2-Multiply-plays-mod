namespace Sigtrap.Relays.Binding;

public class RelayBinding<TDelegate> : IRelayBinding where TDelegate : class
{
	protected IRelayLinkBase<TDelegate> _relay { get; private set; }

	protected TDelegate _listener { get; private set; }

	public bool enabled { get; private set; }

	public bool allowDuplicates { get; set; }

	public uint listenerCount => _relay.listenerCount;

	private RelayBinding()
	{
	}

	public RelayBinding(IRelayLinkBase<TDelegate> relay, TDelegate listener, bool allowDuplicates, bool isListening)
		: this()
	{
		_relay = relay;
		_listener = listener;
		this.allowDuplicates = allowDuplicates;
		enabled = isListening;
	}

	public bool Enable(bool enable)
	{
		if (enable)
		{
			if (!enabled && _relay.AddListener(_listener, allowDuplicates))
			{
				enabled = true;
				return true;
			}
		}
		else if (enabled && _relay.RemoveListener(_listener))
		{
			enabled = false;
			return true;
		}
		return false;
	}
}

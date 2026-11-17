namespace Sigtrap.Relays.Link;

public abstract class RelayLinkBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate : class
{
	protected RelayBase<TDelegate> _relay;

	public uint listenerCount => _relay.listenerCount;

	public uint oneTimeListenersCount => _relay.oneTimeListenersCount;

	private RelayLinkBase()
	{
	}

	public RelayLinkBase(RelayBase<TDelegate> relay)
	{
		_relay = relay;
	}

	public bool Contains(TDelegate listener)
	{
		return _relay.Contains(listener);
	}

	public bool AddListener(TDelegate listener, bool allowDuplicates = false)
	{
		return _relay.AddListener(listener, allowDuplicates);
	}

	public IRelayBinding BindListener(TDelegate listener, bool allowDuplicates = false)
	{
		return _relay.BindListener(listener, allowDuplicates);
	}

	public bool AddOnce(TDelegate listener, bool allowDuplicates = false)
	{
		return _relay.AddOnce(listener, allowDuplicates);
	}

	public bool RemoveListener(TDelegate listener)
	{
		return _relay.RemoveListener(listener);
	}

	public bool RemoveOnce(TDelegate listener)
	{
		return _relay.RemoveOnce(listener);
	}

	public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = true)
	{
		_relay.RemoveAll(removePersistentListeners, removeOneTimeListeners);
	}
}

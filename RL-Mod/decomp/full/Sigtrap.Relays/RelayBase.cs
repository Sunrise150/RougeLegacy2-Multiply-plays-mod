using System;
using Sigtrap.Relays.Binding;

namespace Sigtrap.Relays;

public abstract class RelayBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate : class
{
	protected bool _hasLink;

	protected TDelegate[] _listeners = new TDelegate[1];

	protected uint _count;

	protected uint _cap = 1u;

	protected TDelegate[] _listenersOnce;

	protected uint _onceCount;

	protected uint _onceCap;

	protected static IndexOutOfRangeException _eIOOR = new IndexOutOfRangeException("Fewer listeners than expected. See guidelines in Relay.cs on using RemoveListener and RemoveAll within Relay listeners.");

	public uint listenerCount => _count;

	public uint oneTimeListenersCount => _onceCount;

	public bool Contains(TDelegate listener)
	{
		return Contains(_listeners, _count, listener);
	}

	public bool AddListener(TDelegate listener, bool allowDuplicates = false)
	{
		if (!allowDuplicates && Contains(listener))
		{
			return false;
		}
		if (_count == _cap)
		{
			_cap *= 2u;
			_listeners = Expand(_listeners, _cap, _count);
		}
		_listeners[_count] = listener;
		_count++;
		return true;
	}

	public IRelayBinding BindListener(TDelegate listener, bool allowDuplicates = false)
	{
		if (AddListener(listener, allowDuplicates))
		{
			return new RelayBinding<TDelegate>(this, listener, allowDuplicates, isListening: true);
		}
		return null;
	}

	public bool AddOnce(TDelegate listener, bool allowDuplicates = false)
	{
		if (!allowDuplicates && Contains(_listenersOnce, _onceCount, listener))
		{
			return false;
		}
		if (_onceCount == _onceCap)
		{
			if (_onceCap == 0)
			{
				_onceCap = 1u;
			}
			else
			{
				_onceCap *= 2u;
			}
			_listenersOnce = Expand(_listenersOnce, _onceCap, _onceCount);
		}
		_listenersOnce[_onceCount] = listener;
		_onceCount++;
		return true;
	}

	public bool RemoveListener(TDelegate listener)
	{
		bool result = false;
		for (uint num = 0u; num < _count; num++)
		{
			if (_listeners[num].Equals(listener))
			{
				RemoveAt(num);
				result = true;
				break;
			}
		}
		return result;
	}

	public bool RemoveOnce(TDelegate listener)
	{
		bool result = false;
		for (uint num = 0u; num < _onceCount; num++)
		{
			if (_listenersOnce[num].Equals(listener))
			{
				RemoveOnceAt(num);
				result = true;
				break;
			}
		}
		return result;
	}

	public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = true)
	{
		if (removePersistentListeners)
		{
			Array.Clear(_listeners, 0, (int)_cap);
			_count = 0u;
		}
		if (removeOneTimeListeners && _onceCount != 0)
		{
			Array.Clear(_listenersOnce, 0, (int)_onceCap);
			_onceCount = 0u;
		}
	}

	protected void RemoveAt(uint i)
	{
		_count = RemoveAt(_listeners, _count, i);
	}

	protected void RemoveOnceAt(uint i)
	{
		_onceCount = RemoveAt(_listenersOnce, _onceCount, i);
	}

	protected uint RemoveAt(TDelegate[] arr, uint count, uint i)
	{
		count--;
		for (uint num = i; num < count; num++)
		{
			arr[num] = arr[num + 1];
		}
		arr[count] = null;
		return count;
	}

	private bool Contains(TDelegate[] arr, uint c, TDelegate d)
	{
		for (uint num = 0u; num < c; num++)
		{
			if (arr[num].Equals(d))
			{
				return true;
			}
		}
		return false;
	}

	private TDelegate[] Expand(TDelegate[] arr, uint cap, uint count)
	{
		TDelegate[] array = new TDelegate[cap];
		for (int i = 0; i < count; i++)
		{
			array[i] = arr[i];
		}
		return array;
	}
}

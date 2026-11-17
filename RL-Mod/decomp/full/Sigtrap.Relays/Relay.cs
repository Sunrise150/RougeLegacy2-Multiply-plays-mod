using System;
using Sigtrap.Relays.Link;

namespace Sigtrap.Relays;

public class Relay : RelayBase<Action>, IRelayLink, IRelayLinkBase<Action>
{
	private IRelayLink _link;

	public IRelayLink link
	{
		get
		{
			if (!_hasLink)
			{
				_link = new RelayLink(this);
				_hasLink = true;
			}
			return _link;
		}
	}

	public void Dispatch()
	{
		for (uint num = _count; num != 0; num--)
		{
			if (num > _count)
			{
				throw RelayBase<Action>._eIOOR;
			}
			if (_listeners[num - 1] != null)
			{
				_listeners[num - 1]();
			}
			else
			{
				RemoveAt(num - 1);
			}
		}
		for (uint num2 = _onceCount; num2 != 0; num2--)
		{
			Action action = _listenersOnce[num2 - 1];
			_onceCount = RemoveAt(_listenersOnce, _onceCount, num2 - 1);
			action?.Invoke();
		}
	}
}
public class Relay<T> : RelayBase<Action<T>>, IRelayLink<T>, IRelayLinkBase<Action<T>>
{
	private IRelayLink<T> _link;

	public IRelayLink<T> link
	{
		get
		{
			if (!_hasLink)
			{
				_link = new RelayLink<T>(this);
				_hasLink = true;
			}
			return _link;
		}
	}

	public void Dispatch(T t)
	{
		for (uint num = _count; num != 0; num--)
		{
			if (num > _count)
			{
				throw RelayBase<Action<T>>._eIOOR;
			}
			if (_listeners[num - 1] != null)
			{
				_listeners[num - 1](t);
			}
			else
			{
				RemoveAt(num - 1);
			}
		}
		for (uint num2 = _onceCount; num2 != 0; num2--)
		{
			Action<T> action = _listenersOnce[num2 - 1];
			_onceCount = RemoveAt(_listenersOnce, _onceCount, num2 - 1);
			action?.Invoke(t);
		}
	}
}
public class Relay<T, U> : RelayBase<Action<T, U>>, IRelayLink<T, U>, IRelayLinkBase<Action<T, U>>
{
	private IRelayLink<T, U> _link;

	public IRelayLink<T, U> link
	{
		get
		{
			if (!_hasLink)
			{
				_link = new RelayLink<T, U>(this);
				_hasLink = true;
			}
			return _link;
		}
	}

	public void Dispatch(T t, U u)
	{
		for (uint num = _count; num != 0; num--)
		{
			if (num > _count)
			{
				throw RelayBase<Action<T, U>>._eIOOR;
			}
			if (_listeners[num - 1] != null)
			{
				_listeners[num - 1](t, u);
			}
			else
			{
				RemoveAt(num - 1);
			}
		}
		for (uint num2 = _onceCount; num2 != 0; num2--)
		{
			Action<T, U> action = _listenersOnce[num2 - 1];
			_onceCount = RemoveAt(_listenersOnce, _onceCount, num2 - 1);
			action?.Invoke(t, u);
		}
	}
}
public class Relay<T, U, V> : RelayBase<Action<T, U, V>>, IRelayLink<T, U, V>, IRelayLinkBase<Action<T, U, V>>
{
	private IRelayLink<T, U, V> _link;

	public IRelayLink<T, U, V> link
	{
		get
		{
			if (!_hasLink)
			{
				_link = new RelayLink<T, U, V>(this);
				_hasLink = true;
			}
			return _link;
		}
	}

	public void Dispatch(T t, U u, V v)
	{
		for (uint num = _count; num != 0; num--)
		{
			if (num > _count)
			{
				throw RelayBase<Action<T, U, V>>._eIOOR;
			}
			if (_listeners[num - 1] != null)
			{
				_listeners[num - 1](t, u, v);
			}
			else
			{
				RemoveAt(num - 1);
			}
		}
		for (uint num2 = _onceCount; num2 != 0; num2--)
		{
			Action<T, U, V> action = _listenersOnce[num2 - 1];
			_onceCount = RemoveAt(_listenersOnce, _onceCount, num2 - 1);
			action?.Invoke(t, u, v);
		}
	}
}
public class Relay<T, U, V, W> : RelayBase<Action<T, U, V, W>>, IRelayLink<T, U, V, W>, IRelayLinkBase<Action<T, U, V, W>>
{
	private IRelayLink<T, U, V, W> _link;

	public IRelayLink<T, U, V, W> link
	{
		get
		{
			if (!_hasLink)
			{
				_link = new RelayLink<T, U, V, W>(this);
				_hasLink = true;
			}
			return _link;
		}
	}

	public void Dispatch(T t, U u, V v, W w)
	{
		for (uint num = _count; num != 0; num--)
		{
			if (num > _count)
			{
				throw RelayBase<Action<T, U, V, W>>._eIOOR;
			}
			if (_listeners[num - 1] != null)
			{
				_listeners[num - 1](t, u, v, w);
			}
			else
			{
				RemoveAt(num - 1);
			}
		}
		for (uint num2 = _onceCount; num2 != 0; num2--)
		{
			Action<T, U, V, W> action = _listenersOnce[num2 - 1];
			_onceCount = RemoveAt(_listenersOnce, _onceCount, num2 - 1);
			action?.Invoke(t, u, v, w);
		}
	}
}

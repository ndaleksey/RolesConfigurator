using System;
using System.Collections.Generic;
using System.Configuration;

namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal abstract class ElementCollectionBase<T, TKey> : ConfigurationElementCollection,
		ICollection<T>
		where T : ConfigurationElement, new()
	{
		#region Constructors

		protected ElementCollectionBase()
		{
		}

		#endregion

		#region Indexers

		public T this[int index] => (T) BaseGet(index);

		public T this[TKey key] => (T) BaseGet(key);

		bool ICollection<T>.IsReadOnly => IsReadOnly();

		#endregion

		#region Methods

		public void Add(T item)
		{
			BaseAdd(item);
		}

		public void Clear()
		{
			BaseClear();
		}

		public bool Contains(T item)
		{
			return BaseGet(GetElementKeyCore(item)) != null;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			CopyTo((ConfigurationElement[]) array, arrayIndex);
		}

		public new IEnumerator<T> GetEnumerator()
		{
			var e = base.GetEnumerator();

			try
			{
				while (e.MoveNext())
				{
					yield return (T) e.Current;
				}
			}
			finally
			{
				(e as IDisposable)?.Dispose();
			}
		}

		public bool Remove(T item)
		{
			try
			{
				BaseRemove(GetElementKeyCore(item));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected sealed override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		protected sealed override object GetElementKey(ConfigurationElement element)
		{
			return GetElementKeyCore((T) element);
		}

		protected abstract TKey GetElementKeyCore(T element);

		#endregion
	}
}
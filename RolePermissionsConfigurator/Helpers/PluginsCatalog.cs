using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	[XmlRoot(ElementName = "plugIns", IsNullable = false)]
	public class PluginsCatalog : ICollection<PluginMetaData>
	{
		private readonly List<PluginMetaData> _plugins;

		public PluginsCatalog()
		{
			_plugins = new List<PluginMetaData>();
		}

		public PluginsCatalog(List<PluginMetaData> plugins)
		{
			_plugins = plugins;
		}

		public int Count => _plugins.Count;

		public bool IsReadOnly => false;

		public void Add(PluginMetaData item)
		{
			_plugins.Add(item);
		}

		public void Clear()
		{
			_plugins.Clear();
		}

		public bool Contains(PluginMetaData item)
		{
			return _plugins.Contains(item);
		}

		public void CopyTo(PluginMetaData[] array, int arrayIndex)
		{
			_plugins.CopyTo(array, arrayIndex);
		}

		public IEnumerator<PluginMetaData> GetEnumerator()
		{
			return _plugins.GetEnumerator();
		}

		public bool Remove(PluginMetaData item)
		{
			return _plugins.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _plugins.GetEnumerator();
		}
	}
}
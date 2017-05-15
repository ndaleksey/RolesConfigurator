using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Utils;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public static class DbService
	{
		private static DbParameter CreateParameter(this DbCommand command, string name, DbType dbType, object value)
		{
			var param = command.CreateParameter();
			param.ParameterName = name;
			param.DbType = dbType;
			param.Value = value;
			return param;
		}


		public static Task<List<Role>> GetRolesWithAccountsAsync(DbConnection connection)
		{
			return Task.Run(() => GetRolesWithAccounts(connection));
		}

		public static List<Role> GetRolesWithAccounts(DbConnection connection)
		{
			var roles = new List<Role>();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText =
					"SELECT r.id, r.cluster_id, r.number, r.name, r.description, a.login, a.name, a.description " +
					"FROM permission.role r FULL JOIN permission.account a ON r.id = a.role_id";
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return roles;

					while (reader.Read())
					{
						var roleId = reader.GetGuid(0);
						var clusterId = reader.GetGuid(1);
						var roleNumber = reader.GetInt32(2);
						var roleName = reader.GetString(3);
						var roleDescription = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
						var accLogin = reader.GetString(5);
						var accName = reader.GetString(6);
						var accDescription = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);

						var role = roles.FirstOrDefault(r => r.Id == roleId);

						if (role == null)
						{
							role = new Role(roleId, clusterId, roleNumber, roleName, roleDescription);
							roles.Add(role);
						}

						role.Accounts.Add(new Account(accLogin, accName, accDescription, role));
					}
				}
			}
			return roles;
		}


		public static Task<List<Role>> GetRolesByClusterIdAsync(DbConnection connection, Guid clusterId)
		{
			return Task.Run(() => GetRoles(connection, clusterId));
		}

		public static List<Role> GetRoles(DbConnection connection, Guid clusterId)
		{
			var roles = new List<Role>();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT id, number, name, description " +
				                  "FROM permission.role " +
				                  "WHERE cluster_id = @cluster_id ORDER BY number";
				cmd.Parameters.Add(cmd.CreateParameter("cluster_id", DbType.Guid, clusterId));
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return roles;

					while (reader.Read())
					{
						var id = reader.GetGuid(0);
						var number = reader.GetInt32(1);
						var name = reader.GetString(2);
						var description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
						roles.Add(new Role(id, clusterId, number, name, description));
					}
				}
			}
			return roles;
		}

		public static Task<List<Account>> GetAccountsAsync(DbConnection connection)
		{
			return Task.Run(() => GetAccounts(connection));
		}

		public static List<Account> GetAccounts(DbConnection connection)
		{
			var accounts = new List<Account>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT login, name, description, role_id " +
				                  "FROM permission.account ORDER BY login";

				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return accounts;

					while (reader.Read())
					{
						var login = reader.GetString(0);
						var name = reader.GetString(1);
						var description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
						var roleId = reader.IsDBNull(3) ? (Guid?) null : reader.GetGuid(3);
						var role = roleId == null ? null : new Role(roleId.Value, Guid.Empty, 0, string.Empty);
						accounts.Add(new Account(login, name, description, role));
					}
				}
			}

			return accounts;
		}


		public static Task<List<int>> GetSubsystemNumbersByRoleIdAsync(NpgsqlConnection connection,
			Guid role)
		{
			return Task.Run(() => GetSubsystemNumbersByRoleId(connection, role));
		}

		public static List<int> GetSubsystemNumbersByRoleId(NpgsqlConnection connection, Guid role)
		{
			var subsystemNumbers = new List<int>();

			var sql = $"SELECT subsystem_number FROM permission.role_subsystem_permission WHERE role_id = '{role}'";

			using (var cmd = new NpgsqlCommand(sql, connection))
			{
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return subsystemNumbers;

					while (reader.Read())
					{
						var subsystemNumber = reader.GetInt32(0);
						subsystemNumbers.Add(subsystemNumber);
					}
				}
			}

			return subsystemNumbers;
		}

		public static Task<List<Tuple<int, Guid>>> GetSubsystemsPermissionsAsTuppleAsync(DbConnection connection)
		{
			return Task.Run(() => GetSubsystemsPermissionsAsTupple(connection));
		}

		public static List<Tuple<int, Guid>> GetSubsystemsPermissionsAsTupple(DbConnection connection)
		{
			var permissions = new List<Tuple<int, Guid>>();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText =
					"SELECT subsystem_number, role_id FROM permission.role_subsystem_permission " +
					"ORDER BY subsystem_number, role_id";
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return permissions;

					while (reader.Read())
					{
						var subsystem = reader.GetInt32(0);
						var role = reader.GetGuid(1);
						permissions.Add(new Tuple<int, Guid>(subsystem, role));
					}
				}
			}
			return permissions;
		}

		public static Task<List<Subsystem>> GetSubsystemsAsync(DbConnection connection)
		{
			return Task.Run(() => GetSubsystems(connection));
		}

		public static List<Subsystem> GetSubsystems(DbConnection connection)
		{
			var subsystems = new List<Subsystem>();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT number, name FROM permission.subsystem ORDER BY number";
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return subsystems;

					while (reader.Read())
					{
						var number = reader.GetInt32(0);
						var name = reader.GetString(1);
						subsystems.Add(new Subsystem(number, name));
					}
				}
			}
			return subsystems;
		}

		public static Task InsertRoleAsync(DbConnection connection, Role role)
		{
			return Task.Run(() => InsertRole(connection, role));
		}

		public static void InsertRole(DbConnection connection, Role role)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "INSERT INTO permission.role (id, cluster_id, number, name, description) VALUES " +
				                  "(@id, @cluster_id, @number, @name, @description)";

				cmd.Parameters.Add(cmd.CreateParameter("id", DbType.Guid, role.Id));
				cmd.Parameters.Add(cmd.CreateParameter("cluster_id", DbType.Guid, role.ClusterId));
				cmd.Parameters.Add(cmd.CreateParameter("number", DbType.Int32, role.Number));
				cmd.Parameters.Add(cmd.CreateParameter("name", DbType.String, role.Name));
				cmd.Parameters.Add(cmd.CreateParameter("description", DbType.String, role.Description));
				cmd.ExecuteNonQuery();
			}
		}


		public static Task UpdateRoleAsync(NpgsqlConnection connection, Role role)
		{
			return Task.Run(() => UpdateRole(connection, role));
		}

		public static void UpdateRole(NpgsqlConnection connection, Role role)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "UPDATE permission.role SET " +
				                  "cluster_id = @cluster_id, " +
				                  "number = @number, " +
				                  "name = @name, " +
				                  "description = @description " +
				                  "WHERE id = @id";

				cmd.Parameters.Add(cmd.CreateParameter("id", DbType.Guid, role.Id));
				cmd.Parameters.Add(cmd.CreateParameter("cluster_id", DbType.Guid, role.ClusterId));
				cmd.Parameters.Add(cmd.CreateParameter("number", DbType.Int32, role.Number));
				cmd.Parameters.Add(cmd.CreateParameter("name", DbType.String, role.Name));
				cmd.Parameters.Add(cmd.CreateParameter("description", DbType.String, role.Description));
				cmd.ExecuteNonQuery();
			}
		}

		public static Task DeleteRoleAsync(NpgsqlConnection connection, Role role)
		{
			return Task.Run(() => DeleteRole(connection, role));
		}

		public static void DeleteRole(NpgsqlConnection connection, Role role)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "DELETE FROM permission.role WHERE id = @id";
				cmd.Parameters.Add(cmd.CreateParameter("id", DbType.Guid, role.Id));
				cmd.ExecuteNonQuery();
			}
		}

		public static Task UpdateEnabledAccountsForRoleAsync(DbConnection connection, IEnumerable<Account> accounts,
			Guid roleId)
		{
			return Task.Run(() => UpdateEnabledAccountsForRole(connection, accounts, roleId));
		}

		public static void UpdateEnabledAccountsForRole(DbConnection connection, IEnumerable<Account> accounts, Guid roleId)
		{
			const string sql = "UPDATE permission.account SET " +
			                   "login = @login, " +
			                   "name = @name, " +
			                   "description = @description, " +
			                   "role_id = @role_id WHERE login = @login";

			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = sql;

				foreach (var account in accounts)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("login", DbType.String, account.Login));
					cmd.Parameters.Add(cmd.CreateParameter("name", DbType.String, account.Name));
					cmd.Parameters.Add(cmd.CreateParameter("description", DbType.String, account.Description ?? (object) DBNull.Value));
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, account.IsSelected ? (object) roleId : DBNull.Value));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task DeleteSubsystemPermissionsAsync(DbConnection connection,
			IEnumerable<SubsystemPermission> permissions)
		{
			return Task.Run(() => DeleteSubsystemPermissions(connection, permissions));
		}

		public static void DeleteSubsystemPermissions(DbConnection connection, IEnumerable<SubsystemPermission> permissions)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "DELETE FROM permission.role_subsystem_permission " +
				                  "WHERE subsystem_number = @subsystem_number AND role_id = @role_id";

				foreach (var p in permissions)
				{

					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("subsystem_number", DbType.Int32, p.Subsystem.Number));
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, p.Role.Id));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task InsertSubsystemPermissionsAsync(DbConnection connection,
			IEnumerable<SubsystemPermission> permissions)
		{
			return Task.Run(() => InsertSubsystemPermissions(connection, permissions));
		}

		public static void InsertSubsystemPermissions(DbConnection connection, IEnumerable<SubsystemPermission> permissions)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "INSERT INTO permission.role_subsystem_permission (subsystem_number, role_id) " +
				                  "VALUES (@subsystem_number, @role_id)";

				foreach (var p in permissions)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("subsystem_number", DbType.Int32, p.Subsystem.Number));
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, p.Role.Id));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task<List<Tuple<Guid, string>>> GetRolePluginsAsTuppleAsync(DbConnection connection)
		{
			return Task.Run(() => GetRolePluginsAsTupple(connection));
		}

		private static List<Tuple<Guid, string>> GetRolePluginsAsTupple(DbConnection connection)
		{
			var plugins = new List<Tuple<Guid, string>>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT role_id, plugin_name FROM permission.role_plugin ORDER BY role_id, plugin_name";

				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return plugins;

					while (reader.Read())
					{
						var roleId = reader.GetGuid(0);
						var pluginName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
						plugins.Add(new Tuple<Guid, string>(roleId, pluginName));
					}
				}
			}

			return plugins;
		}

		public static Task DeleteRolePluginsAsync(DbConnection connection, IEnumerable<Plugin> plugins)
		{
			return Task.Run(() => DeleteRolePlugins(connection, plugins));
		}

		private static void DeleteRolePlugins(DbConnection connection, IEnumerable<Plugin> plugins)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = cmd.Connection;
				cmd.CommandText = "DELETE FROM permission.role_plugin WHERE role_id = @role_id AND plugin_name = @plugin_name";

				foreach (var plugin in plugins.Where(p => p?.Role != null))
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, plugin.Role.Id));
					cmd.Parameters.Add(cmd.CreateParameter("plugin_name", DbType.String, plugin.Name));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task InsertRolePluginsAsync(DbConnection connection, IEnumerable<Plugin> plugins)
		{
			return Task.Run(() => InsertRolePlugins(connection, plugins));
		}

		private static void InsertRolePlugins(DbConnection connection, IEnumerable<Plugin> plugins)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "INSERT INTO permission.role_plugin (role_id, plugin_name) " +
				                  "VALUES (@role_id, @plugin_name)";

				foreach (var plugin in plugins.Where(p => p?.Role != null && !string.IsNullOrEmpty(p.Name)))
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, plugin.Role.Id));
					cmd.Parameters.Add(cmd.CreateParameter("plugin_name", DbType.String, plugin.Name));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task InsertPluginPermissionsAsync(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			return Task.Run(() => InsertPluginPermissions(connection, permissions));
		}

		private static void InsertPluginPermissions(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText =
					"INSERT INTO permission.role_plugin_permission (role_id, plugin_name, permission_name, permission_value) " +
					"VALUES (@role_id, @plugin_name, @permission_name, @permission_value)";

				foreach (var permission in permissions)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, permission.Plugin.Role.Id));
					cmd.Parameters.Add(cmd.CreateParameter("plugin_name", DbType.String, permission.Plugin.Name));
					cmd.Parameters.Add(cmd.CreateParameter("permission_name", DbType.String, permission.InvariantName));
					cmd.Parameters.Add(cmd.CreateParameter("permission_value", DbType.Single, permission.Value.Value));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task UpdatePluginPermissionsAsync(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			return Task.Run(() => UpdatePluginPermissions(connection, permissions));
		}

		private static void UpdatePluginPermissions(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "UPDATE permission.role_plugin_permission SET " +
				                  "permission_value = @permission_value WHERE " +
				                  "role_id = @role_id AND plugin_name = @plugin_name AND permission_name = @permission_name";

				foreach (var permission in permissions)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, permission.Plugin.Role.Id));
					cmd.Parameters.Add(cmd.CreateParameter("plugin_name", DbType.String, permission.Plugin.Name));
					cmd.Parameters.Add(cmd.CreateParameter("permission_name", DbType.String, permission.InvariantName));
					cmd.Parameters.Add(cmd.CreateParameter("permission_value", DbType.Single, permission.Value.Value));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task DeletePluginPermissionsAsync(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			return Task.Run(() => DeletePluginPermissions(connection, permissions));
		}

		private static void DeletePluginPermissions(DbConnection connection, IEnumerable<PluginPermission> permissions)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "DELETE FROM permission.role_plugin_permission " +
				                  "WHERE role_id = @role_id AND plugin_name = @plugin_name";

				foreach (var permission in permissions)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("role_id", DbType.Guid, permission.Plugin.Role.Id));
					cmd.Parameters.Add(cmd.CreateParameter("plugin_name", DbType.String, permission.Plugin.Name));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task<List<Tuple<Guid, string, string, short>>> GetPluginsPermissionsAsTuppleAsync(
			DbConnection connection)
		{
			return Task.Run(() => GetPluginsPermissionsAsTupple(connection));
		}

		private static List<Tuple<Guid, string, string, short>> GetPluginsPermissionsAsTupple(DbConnection connection)
		{
			var permissions = new List<Tuple<Guid, string, string, short>>();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT role_id, plugin_name, permission_name, permission_value " +
				                  "FROM permission.role_plugin_permission ORDER BY role_id, plugin_name";

				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return permissions;

					while (reader.Read())
					{
						var roleId = reader.GetGuid(0);
						var pluginName = reader.GetString(1);
						var permissionName = reader.GetString(2);
						var permissionValue = reader.GetInt16(3);
						permissions.Add(new Tuple<Guid, string, string, short>(roleId, pluginName, permissionName, permissionValue));
					}
				}

			}
			return permissions;
		}

		public static Task SynchronizeAccountsAndUsersAsync(DbConnection connection)
		{
			return Task.Run(() => SynchronizeAccountsAndUsers(connection));
		}

		private static void SynchronizeAccountsAndUsers(DbConnection connection)
		{
			var users = new List<string>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT usename FROM pg_user";
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.HasRows)
						while (reader.Read())
							users.Add(reader.GetString(0));
				}
			}

			var accounts = GetAccounts(connection);

			var accountsForDelete = new List<Account>();

			foreach (var a in accounts)
			{
				if (users.Any(u => string.Equals(a.Login, u, StringComparison.InvariantCultureIgnoreCase)))
					continue;

				if (!users.Any(u => string.Equals(a.Login, u, StringComparison.InvariantCultureIgnoreCase)))
					accountsForDelete.Add(a);
			}

			DeleteAccounts(connection, accountsForDelete);

			var usersForInsert = new List<string>();

			foreach (var user in users)
			{
				if (accounts.All(a => !string.Equals(a.Login, user, StringComparison.InvariantCultureIgnoreCase)))
					usersForInsert.Add(user);
			}
			InsertAccounts(connection, usersForInsert);

		}

		private static void InsertAccounts(DbConnection connection, IEnumerable<string> users)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "INSERT INTO permission.account (login, name) VALUES(@login, @name)";

				foreach (var user in users)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("login", DbType.String, user));
					cmd.Parameters.Add(cmd.CreateParameter("name", DbType.String, user));
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void DeleteAccounts(DbConnection connection, IEnumerable<Account> accounts)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "DELETE FROM permission.account WHERE login = @login";

				foreach (var account in accounts)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(cmd.CreateParameter("login", DbType.String, account.Login));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task<string> GetDepartmentNameByClusterIdAsync(DbConnection connection, Guid clusterId)
		{
			return Task.Run(() => GetDepartmentNameByClusterId(connection, clusterId));
		}

		public static string GetDepartmentNameByClusterId(DbConnection connection, Guid clusterId)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT name FROM dynamic.obj WHERE id = @cluster_id";
				cmd.Parameters.Add(cmd.CreateParameter("cluster_id", DbType.Guid, clusterId));

				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return string.Empty;

					reader.Read();
					return reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
				}
			}
		}

		public static Task<Guid?> GetCurrentRoleClusterAsync(DbConnection connection)
		{
			return Task.Run(() => GetCurrentRoleCluster(connection));
		}

		public static Guid? GetCurrentRoleCluster(DbConnection connection)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = "SELECT r.cluster_id FROM permission.role r " +
				                  "JOIN permission.account a ON r.id = a.role_id " +
				                  "WHERE a.login = (SELECT current_user)";
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.HasRows) return null;

					reader.Read();
					return reader.IsDBNull(0) ? (Guid?) null : reader.GetGuid(0);
				}
			}
		}

		public static Task<List<MilitaryUnit>> GetMilitaryUnitsByGroupingNameAsync(DbConnection connection,
			string groupingName)
		{
			return Task.Run(() => GetGroupingUnitsByGroupingName(connection, groupingName));
		}

		public static List<MilitaryUnit> GetGroupingUnitsByGroupingName(DbConnection connection, string groupingName)
		{
			var units = new List<MilitaryUnit>();

			var sql = "SELECT g.id, g.parent_id, g.name, c.number AS cluster " +
			          $"FROM {groupingName} g " +
			          "FULL JOIN permission.cluster c ON g.id = c.id " +
			          "ORDER BY parent_id DESC;";

			using (var command = connection.CreateCommand())
			{
				command.Connection = connection;
				command.CommandText = sql;

				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows) return units;

					while (reader.Read())
					{
						if (reader.IsDBNull(0)) continue;

						var id = reader.GetGuid(0).ToString();
						var parentId = reader.IsDBNull(1) ? Guid.Empty.ToString() : reader.GetGuid(1).ToString();
						var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
						var cluster = reader.IsDBNull(3) ? (int?) null : reader.GetInt32(3);

						units.Add(new MilitaryUnit(id, parentId, name, cluster));
					}
				}
			}
			return units;
		}
	}
}
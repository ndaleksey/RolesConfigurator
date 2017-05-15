using System;
using System.Data;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class Transaction : IDisposable
	{
		#region Fields

		private NpgsqlTransaction _transaction;

		#endregion

		#region Properties

		public NpgsqlConnection Connection { get; }

		#endregion

		#region Constructors

		public Transaction()
		{
			Connection = new NpgsqlConnection(Settings.Default.ConnectionString);
			Connection.Open();
			_transaction = Connection.BeginTransaction(IsolationLevel.Snapshot);
		}

		#endregion

		#region Methods

		public void Commit()
		{
			if (_transaction == null)
				throw new InvalidOperationException("Ошибка записи данных в БД. Ошибка создания транзакции");

			_transaction.Commit();
			_transaction = null;
		}

		public void Dispose()
		{
			_transaction = null;
			Connection.Close();
		}

		#endregion
	}
}
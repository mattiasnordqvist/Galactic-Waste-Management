using NUnit.Framework;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace GalacticWasteManagement.FrameworkTests
{
    public class TestConnectionFactory
    {
        [Test]
        public void Can_Create_A_Connection()
        {
            var connectionFactory = new TestableConnectionFactory();
            Should.NotThrow(() => connectionFactory.CreateOpenConnection().ShouldNotBeNull()); 
        }

        protected class TestableConnectionFactory : ConnectionFactory
        {
            public TestableConnectionFactory() : base(null, null)
            {
            }

            protected override DbConnection CreteConnection() => new TestConnection();
        }

        public class TestConnection : DbConnection
        {
            private readonly List<DbTransaction> _transactions;
            private List<TestCommand> _commands;

            public TestConnection()
            {
                _commands = new List<TestCommand>();
                _transactions = new List<DbTransaction>();
            }

            public override string ConnectionString { get; set; } = string.Empty;

            public override int ConnectionTimeout => 30;

            public override string Database => "TestDB";

            public override ConnectionState State => ConnectionState.Open;

            public override string DataSource => "My data source";

            public override string ServerVersion => "Test server version";

            public IEnumerable<TestCommand> Commands => _commands;

            public override void ChangeDatabase(string databaseName)
            {
            }

            public override void Close()
            {
            }

            public override void Open()
            {
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                var transaction = new TestTransaction(this);
                _transactions.Add(transaction);
                return transaction;
            }

            protected override DbCommand CreateDbCommand()
            {
                var command = new TestCommand(this, _transactions.LastOrDefault());
                _commands.Add(command);
                return command;
            }
        }

        public class TestCommand : DbCommand
        {
            private TestParameters _parameters;

            public TestCommand(DbConnection connection, DbTransaction transaction = null)
            {
                _parameters = new TestParameters();
                DbConnection = connection;
                DbTransaction = transaction;
            }

            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override bool DesignTimeVisible { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }

            protected override DbParameterCollection DbParameterCollection => _parameters;

            protected override DbTransaction DbTransaction { get; set; }

            public override void Cancel()
            {
            }

            public override int ExecuteNonQuery()
            {
                return 1;
            }

            public override object ExecuteScalar()
            {
                return null;
            }

            public override void Prepare()
            {
            }

            protected override DbParameter CreateDbParameter()
            {
                return new TestParameter();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }
        }

        public class TestParameter : DbParameter
        {
            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override int Size { get; set; }
            public override string SourceColumn { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override object Value { get; set; }
            public override void ResetDbType()
            {
            }
        }

        public class TestParameters : DbParameterCollection
        {
            private Dictionary<string, DbParameter> _params;

            public TestParameters()
            {
                _params = new Dictionary<string, DbParameter>();
            }

            public override int Count => _params.Count();

            public override object SyncRoot => _params;

            public override int Add(object value)
            {
                if (value is DbParameter parameter)
                {
                    _params.Add(parameter.ParameterName, parameter);
                    return 1;
                }

                return 0;
            }

            public override void AddRange(Array values)
            {
                foreach (var value in values)
                {
                    Add(value);
                }
            }

            public override void Clear() => _params.Clear();

            public override bool Contains(object value) => _params.ContainsValue(value as DbParameter);

            public override bool Contains(string value) => _params.ContainsKey(value);

            public override void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public override IEnumerator GetEnumerator() => _params.Values.GetEnumerator();

            public override int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public override int IndexOf(string parameterName)
            {
                throw new NotImplementedException();
            }

            public override void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public override void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public override void RemoveAt(string parameterName)
            {
                throw new NotImplementedException();
            }

            protected override DbParameter GetParameter(int index)
            {
                throw new NotImplementedException();
            }

            protected override DbParameter GetParameter(string parameterName) => _params[parameterName];

            protected override void SetParameter(int index, DbParameter value)
            {
                throw new NotImplementedException();
            }

            protected override void SetParameter(string parameterName, DbParameter value)
            {
                _params[parameterName] = value;
            }
        }

        public class TestTransaction : DbTransaction
        {
            private readonly TestConnection _connection;

            public TestTransaction(TestConnection connection)
            {
                _connection = connection;
            }

            public override IsolationLevel IsolationLevel => IsolationLevel.Chaos;

            protected override DbConnection DbConnection => _connection;

            public override void Commit()
            {
            }

            public override void Rollback()
            {
            }
        }
    }
}

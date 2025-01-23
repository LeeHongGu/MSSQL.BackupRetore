using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Moq;
using MSSQL.BackupRestore.Configurations;
using NUnit.Framework;

namespace MSSQL.BackupRestore.UnitTest
{
    /// <summary>
    /// NUnit�� Moq�� Ȱ���� ServerWrapper ���� �׽�Ʈ Ŭ�����Դϴ�.
    /// </summary>
    [TestFixture]
    public class ServerWrapperTests
    {
        private Server _server;
        private ServerWrapper _serverWrapper;
        private const string TestDatabaseName = "TestDB";

        [SetUp]
        public void Setup()
        {
            // ���� ������ ���� (���� �Ǵ� ���� ����)
            _server = new Server("localhost");
            _serverWrapper = new ServerWrapper(_server);

            // �׽�Ʈ ȯ�� ����: �׽�Ʈ �����ͺ��̽��� ������ ����
            if (!_server.Databases.Contains(TestDatabaseName))
            {
                var testDatabase = new Database(_server, TestDatabaseName);
                testDatabase.Create();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // �׽�Ʈ �� ����: �׽�Ʈ �����ͺ��̽� ����
            if (_server.Databases.Contains(TestDatabaseName))
            {
                var testDatabase = _server.Databases[TestDatabaseName];
                testDatabase.Drop();
            }
        }

        /// <summary>
        /// ������ null�� �� ArgumentNullException�� �߻��ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenServerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerWrapper(null));
        }

        /// <summary>
        /// �����ͺ��̽��� ������ �� ContainsDatabase�� true�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void ContainsDatabase_ShouldReturnTrue_WhenDatabaseExists()
        {
            // Act
            var result = _serverWrapper.ContainsDatabase(TestDatabaseName);

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// �����ͺ��̽��� �������� ���� �� ContainsDatabase�� false�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void ContainsDatabase_ShouldReturnFalse_WhenDatabaseDoesNotExist()
        {
            // Act
            var result = _serverWrapper.ContainsDatabase("NonExistentDB");

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// �����ͺ��̽��� ������ �� GetDatabase�� ��Ȯ�� Database ��ü�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void GetDatabase_ShouldReturnDatabase_WhenDatabaseExists()
        {
            // Act
            var database = _serverWrapper.GetDatabase(TestDatabaseName);

            // Assert
            Assert.That(database, Is.Not.Null);
            Assert.That(database.Name, Is.EqualTo(TestDatabaseName));
        }

        /// <summary>
        /// �����ͺ��̽��� �������� ���� �� GetDatabase�� null�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void GetDatabase_ShouldReturnNull_WhenDatabaseDoesNotExist()
        {
            // Act
            var database = _serverWrapper.GetDatabase("NonExistentDB");

            // Assert
            Assert.That(database, Is.Null);
        }

        /// <summary>
        /// ���� �޽��� �ڵ鷯�� ���������� ��ϵǴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void AddServerMessageHandler_ShouldAttachHandler_WithRealServer()
        {
            // Arrange
            var serverWrapper = new ServerWrapper(_server);

            var eventTriggered = false;

            void Handler(ServerMessageEventArgs e)
            {
                eventTriggered = true;
                Console.WriteLine($"Server Message: {e.Error.Message}");
            }

            serverWrapper.AddServerMessageHandler(Handler);

            // Act: ��ȿ���� ���� SQL ����
            try
            {
                _server.ConnectionContext.ExecuteNonQuery("INVALID SQL QUERY");
            }
            catch
            {
                // ���ܸ� �����մϴ�. �̺�Ʈ�� Ʈ���ŵǴ����� Ȯ��.
            }

            // Assert
            Assert.That(eventTriggered, Is.True);
        }

        /// <summary>
        /// AddServerMessageHandler �޼��忡 null �ڵ鷯�� �����ϸ� ���ܰ� �߻��ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void AddServerMessageHandler_ShouldThrowArgumentNullException_WhenHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _serverWrapper.AddServerMessageHandler(null));
        }
    }
}
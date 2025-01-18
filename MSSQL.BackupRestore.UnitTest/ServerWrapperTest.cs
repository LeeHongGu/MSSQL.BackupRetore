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
        private Mock<Server> _mockServer;
        private ServerWrapper _serverWrapper;

        [SetUp]
        public void Setup()
        {
            // Moq�� ����Ͽ� Server ��ü�� Mocking
            _mockServer = new Mock<Server>();
            _serverWrapper = new ServerWrapper(_mockServer.Object);
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
            var mockDatabases = new Mock<DatabaseCollection>(_mockServer.Object);
            mockDatabases.Setup(db => db.Contains("TestDB")).Returns(true);

            _mockServer.Setup(s => s.Databases).Returns(mockDatabases.Object);

            var result = _serverWrapper.ContainsDatabase("TestDB");

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// �����ͺ��̽��� �������� ���� �� ContainsDatabase�� false�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void ContainsDatabase_ShouldReturnFalse_WhenDatabaseDoesNotExist()
        {
            var mockDatabases = new Mock<DatabaseCollection>(_mockServer.Object);
            mockDatabases.Setup(db => db.Contains("NonExistentDB")).Returns(false);

            _mockServer.Setup(s => s.Databases).Returns(mockDatabases.Object);

            var result = _serverWrapper.ContainsDatabase("NonExistentDB");

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// �����ͺ��̽��� ������ �� GetDatabase�� ��Ȯ�� Database ��ü�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void GetDatabase_ShouldReturnDatabase_WhenDatabaseExists()
        {
            var mockDatabase = new Mock<Database>(_mockServer.Object, "TestDB");
            var mockDatabases = new Mock<DatabaseCollection>(_mockServer.Object);
            mockDatabases.Setup(db => db["TestDB"]).Returns(mockDatabase.Object);

            _mockServer.Setup(s => s.Databases).Returns(mockDatabases.Object);

            var result = _serverWrapper.GetDatabase("TestDB");

            Assert.That(result, Is.EqualTo(mockDatabase.Object));
        }

        /// <summary>
        /// �����ͺ��̽��� �������� ���� �� GetDatabase�� null�� ��ȯ�ϴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void GetDatabase_ShouldReturnNull_WhenDatabaseDoesNotExist()
        {
            var mockDatabases = new Mock<DatabaseCollection>(_mockServer.Object);
            mockDatabases.Setup(db => db["NonExistentDB"]).Returns((Database)null);

            _mockServer.Setup(s => s.Databases).Returns(mockDatabases.Object);

            var result = _serverWrapper.GetDatabase("NonExistentDB");

            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// ���� �޽��� �ڵ鷯�� ���������� ��ϵǴ��� �׽�Ʈ�մϴ�.
        /// </summary>
        [Test]
        public void AddServerMessageHandler_ShouldAttachHandler()
        {
            var mockConnectionContext = new Mock<ServerConnection>();
            _mockServer.Setup(s => s.ConnectionContext).Returns(mockConnectionContext.Object);

            var eventTriggered = false;

            void Handler(ServerMessageEventArgs e) => eventTriggered = true;

            _serverWrapper.AddServerMessageHandler(Handler);

            // �̺�Ʈ �߻� �ùķ��̼�
            var sqlErrorCollection = Activator.CreateInstance(typeof(SqlError), true) as SqlError;
            mockConnectionContext.Raise(m => m.ServerMessage += null, new ServerMessageEventArgs(sqlErrorCollection));

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
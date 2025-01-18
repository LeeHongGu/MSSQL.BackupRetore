using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Moq;
using MSSQL.BackupRestore.Configurations;
using NUnit.Framework;

namespace MSSQL.BackupRestore.UnitTest
{
    /// <summary>
    /// NUnit과 Moq를 활용한 ServerWrapper 단위 테스트 클래스입니다.
    /// </summary>
    [TestFixture]
    public class ServerWrapperTests
    {
        private Mock<Server> _mockServer;
        private ServerWrapper _serverWrapper;

        [SetUp]
        public void Setup()
        {
            // Moq를 사용하여 Server 객체를 Mocking
            _mockServer = new Mock<Server>();
            _serverWrapper = new ServerWrapper(_mockServer.Object);
        }

        /// <summary>
        /// 서버가 null일 때 ArgumentNullException이 발생하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenServerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerWrapper(null));
        }

        /// <summary>
        /// 데이터베이스가 존재할 때 ContainsDatabase가 true를 반환하는지 테스트합니다.
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
        /// 데이터베이스가 존재하지 않을 때 ContainsDatabase가 false를 반환하는지 테스트합니다.
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
        /// 데이터베이스가 존재할 때 GetDatabase가 정확한 Database 객체를 반환하는지 테스트합니다.
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
        /// 데이터베이스가 존재하지 않을 때 GetDatabase가 null을 반환하는지 테스트합니다.
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
        /// 서버 메시지 핸들러가 정상적으로 등록되는지 테스트합니다.
        /// </summary>
        [Test]
        public void AddServerMessageHandler_ShouldAttachHandler()
        {
            var mockConnectionContext = new Mock<ServerConnection>();
            _mockServer.Setup(s => s.ConnectionContext).Returns(mockConnectionContext.Object);

            var eventTriggered = false;

            void Handler(ServerMessageEventArgs e) => eventTriggered = true;

            _serverWrapper.AddServerMessageHandler(Handler);

            // 이벤트 발생 시뮬레이션
            var sqlErrorCollection = Activator.CreateInstance(typeof(SqlError), true) as SqlError;
            mockConnectionContext.Raise(m => m.ServerMessage += null, new ServerMessageEventArgs(sqlErrorCollection));

            Assert.That(eventTriggered, Is.True);
        }

        /// <summary>
        /// AddServerMessageHandler 메서드에 null 핸들러를 전달하면 예외가 발생하는지 테스트합니다.
        /// </summary>
        [Test]
        public void AddServerMessageHandler_ShouldThrowArgumentNullException_WhenHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _serverWrapper.AddServerMessageHandler(null));
        }
    }
}
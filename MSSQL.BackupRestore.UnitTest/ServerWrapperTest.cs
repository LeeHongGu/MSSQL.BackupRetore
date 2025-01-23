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
        private Server _server;
        private ServerWrapper _serverWrapper;
        private const string TestDatabaseName = "TestDB";

        [SetUp]
        public void Setup()
        {
            // 실제 서버에 연결 (로컬 또는 원격 서버)
            _server = new Server("localhost");
            _serverWrapper = new ServerWrapper(_server);

            // 테스트 환경 설정: 테스트 데이터베이스가 없으면 생성
            if (!_server.Databases.Contains(TestDatabaseName))
            {
                var testDatabase = new Database(_server, TestDatabaseName);
                testDatabase.Create();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 테스트 후 정리: 테스트 데이터베이스 삭제
            if (_server.Databases.Contains(TestDatabaseName))
            {
                var testDatabase = _server.Databases[TestDatabaseName];
                testDatabase.Drop();
            }
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
            // Act
            var result = _serverWrapper.ContainsDatabase(TestDatabaseName);

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// 데이터베이스가 존재하지 않을 때 ContainsDatabase가 false를 반환하는지 테스트합니다.
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
        /// 데이터베이스가 존재할 때 GetDatabase가 정확한 Database 객체를 반환하는지 테스트합니다.
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
        /// 데이터베이스가 존재하지 않을 때 GetDatabase가 null을 반환하는지 테스트합니다.
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
        /// 서버 메시지 핸들러가 정상적으로 등록되는지 테스트합니다.
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

            // Act: 유효하지 않은 SQL 실행
            try
            {
                _server.ConnectionContext.ExecuteNonQuery("INVALID SQL QUERY");
            }
            catch
            {
                // 예외를 무시합니다. 이벤트가 트리거되는지만 확인.
            }

            // Assert
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
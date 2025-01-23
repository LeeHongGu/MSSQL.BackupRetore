using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.UnitTest
{
    /// <summary>
    /// Integration tests for the <see cref="DatabaseWrapper"/> class using a real SQL Server instance.
    /// </summary>
    [TestFixture]
    public class DatabaseWrapperTests
    {
        private const string TestDatabaseName = "TestDB_RecoveryJob";
        private Server _server;
        private Database _database;
        private DatabaseWrapper _databaseWrapper;

        [SetUp]
        public void Setup()
        {
            // 실제 SQL Server에 연결 (로컬 또는 네트워크 서버)
            _server = new Server("localhost"); 

            // 테스트 데이터베이스가 이미 존재한다면 삭제
            if (_server.Databases.Contains(TestDatabaseName))
            {
                var existingDb = _server.Databases[TestDatabaseName];
                existingDb.Drop();
            }

            // 테스트 데이터베이스 생성
            _database = new Database(_server, TestDatabaseName);
            _database.Create();

            // DatabaseWrapper 초기화
            _databaseWrapper = new DatabaseWrapper(_database);
        }

        [TearDown]
        public void TearDown()
        {
            // 테스트가 끝난 후 데이터베이스 삭제
            if (_server.Databases.Contains(TestDatabaseName))
            {
                var existingDb = _server.Databases[TestDatabaseName];
                existingDb.Drop();
            }
        }

        /// <summary>
        /// 생성자가 null Database를 전달받았을 때 ArgumentNullException을 발생시키는지 테스트합니다.
        /// </summary>
        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenDatabaseIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseWrapper(null));
        }

        /// <summary>
        /// Name 속성이 올바른 데이터베이스 이름을 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Name_ShouldReturnCorrectDatabaseName()
        {
            // Act
            var name = _databaseWrapper.Name;

            // Assert
            Assert.That(name, Is.EqualTo(TestDatabaseName));
        }

        /// <summary>
        /// Create 메서드가 데이터베이스를 생성하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Create_ShouldCreateDatabase()
        {
            // Arrange
            var newDbName = "TestDB_Create";
            var newDb = new Database(_server, newDbName);
            var wrapper = new DatabaseWrapper(newDb);

            // Act
            wrapper.Create();

            // Assert
            Assert.That(_server.Databases.Contains(newDbName), Is.True);

            // Cleanup
            newDb.Drop();
        }

        /// <summary>
        /// Drop 메서드가 데이터베이스를 삭제하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Drop_ShouldDropDatabase()
        {
            // Arrange
            var dropDbName = "TestDB_Drop";
            var dropDb = new Database(_server, dropDbName);
            dropDb.Create();
            var wrapper = new DatabaseWrapper(dropDb);

            // Act
            wrapper.Drop();

            // Assert
            Assert.That(_server.Databases.Contains(dropDbName), Is.False);
        }

        /// <summary>
        /// Exists 메서드가 데이터베이스 존재 여부를 정확히 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Exists_ShouldReturnTrue_WhenDatabaseExists()
        {
            // Act
            var exists = _databaseWrapper.Exists();

            // Assert
            Assert.That(exists, Is.True);
        }

        /// <summary>
        /// Exists 메서드가 데이터베이스가 존재하지 않을 경우 false를 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        public void Exists_ShouldReturnFalse_WhenDatabaseDoesNotExist()
        {
            // Arrange
            var nonExistentDb = new Database(_server, "NonExistentDB");
            var wrapper = new DatabaseWrapper(nonExistentDb);

            // Act
            var exists = wrapper.Exists();

            // Assert
            Assert.That(exists, Is.False);
        }

        /// <summary>
        /// GetDatabase 메서드가 래핑된 Database 객체를 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        public void GetDatabase_ShouldReturnUnderlyingDatabase()
        {
            // Act
            var result = _databaseWrapper.GetDatabase();

            // Assert
            Assert.That(result, Is.EqualTo(_database));
        }
    }
}

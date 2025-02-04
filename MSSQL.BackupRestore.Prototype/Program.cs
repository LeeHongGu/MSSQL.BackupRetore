using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Interfaces;
using MSSQL.BackupRestore.Prototype.Interfaces;
using MSSQL.BackupRestore.Prototype.Jobs;
using MSSQL.BackupRestore.Prototype.Options;
using MSSQL.BackupRestore.Works.BackupWorks;
using MSSQL.BackupRestore.Works.RestoreWorks;

namespace MSSQL.BackupRestore.Prototype
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // HostBuilder를 사용하여 DI, 설정, 로깅을 구성합니다.
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // 현재 디렉터리를 기준으로 설정 파일 로드
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 설정 옵션을 등록
                    services.Configure<BackupOptions>(hostContext.Configuration.GetSection("Backup"));
                    services.Configure<SqlServerOptions>(hostContext.Configuration.GetSection("SqlServer"));

                    // 백업 작업 서비스를 DI에 등록 (Transient나 Scoped, Singleton 선택 가능)
                    services.AddTransient<IBackupJob, BackupJob>();

                    // 만약 라이브러리에서 ILoggerFactory가 필요하다면, Microsoft.Extensions.Logging.ILoggerFactory는 기본으로 등록됩니다.
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    // 필요에 따라 다른 로깅 프로바이더도 추가 가능 (예: 파일 로깅)
                })
                .Build();

            // DI로부터 백업 작업 서비스를 가져와 실행
            var backupJob = host.Services.GetRequiredService<IBackupJob>();
            await backupJob.ExecuteAsync();
        }
    }
}

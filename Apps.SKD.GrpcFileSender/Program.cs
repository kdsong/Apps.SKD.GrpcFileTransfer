using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SKD.Utility;

namespace Apps.SKD.GrpcFileSender;


internal class Program
{
    static void Main(string[] args)
    {
        // appsettings handling
        // decide which service objects are created and runned after reading JSON configuration file.
        IConfigurationBuilder builder = new ConfigurationBuilder();
        buildConfig(builder);

        IConfiguration tempConfig = builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();


        // creating app_data folder and its child folders, get log file name from them
        FolderConfigHelper folderConfigHelper = new FolderConfigHelper(tempConfig);
        string logFilePath = folderConfigHelper.LogFileName;


        // creating logger object
        int keepDays = tempConfig.GetValue<int>("LogRetentionDays");
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: keepDays)
            .CreateLogger();
        SeriHelper.Info("MAIN", "------------------------------------------------------");
        SeriHelper.Info("MAIN", "Apps.SKD.Gateway Program Started");
        SeriHelper.Info("MAIN", "------------------------------------------------------");


        // registering service objects with default configuration
        SeriHelper.Info("MAIN", "Registering service objects");
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                //services.AddTransient<IServiceGrpcFileSender, ServiceGrpcFileSender>();
            })
            .UseSerilog()
            .Build();


        // creating and running service objects
        SeriHelper.Info("MAIN", "Creating and running service objects");
        SeriHelper.Info("MAIN", "");
        createServiceGrpcFileSender(tempConfig, host.Services);


        // running host object and entering wait status
        host.Run();
    }
    static private void buildConfig(IConfigurationBuilder builder)
    {
        // EXE가 위치한 폴더에서 appsettings.json 파일을 찾을 후 읽
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();
    }
    static private void createServiceGrpcFileSender(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        bool isCreateMainMenu = configuration.GetSection("ServiceGrpcFileSender Configuration").GetValue<bool>("IsCreateService");
        bool isRunMainMenu = configuration.GetSection("ServiceGrpcFileSender Configuration").GetValue<bool>("IsRunService");
        bool isShowMenuMainMenu = configuration.GetSection("ServiceGrpcFileSender Configuration").GetValue<bool>("IsShowMenu");

        if (isCreateMainMenu)
        {
            SeriHelper.Info("MAIN", "Creating ServiceGrpcFileSender object");
            //var ServiceGrpcFileSender = ActivatorUtilities.CreateInstance<ServiceGrpcFileSender>(serviceProvider);
            //if (isRunMainMenu)
            //{
            //    SeriHelper.Info("MAIN", "Running ServiceGrpcFileSender object");
            //    ServiceGrpcFileSender.Run();
            //}
            //if (isShowMenuMainMenu)
            //{
            //    SeriHelper.Info("MAIN", "Show Menu of ServiceGrpcFileSender object");
            //    ServiceGrpcFileSender.ShowMenu();
            //}
            SeriHelper.Info("MAIN", "");
        }
        Thread.Sleep(500);
    }

}

using Apps.SKD.GrpcFileReceiver.Services;
using Serilog;
using SKD.Utility;

namespace Apps.SKD.GrpcFileReceiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // appsettings handling
            // decide which service objects are created and runned after reading JSON configuration file.
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            buildConfig(configBuilder);

            IConfiguration tempConfig = configBuilder.SetBasePath(Directory.GetCurrentDirectory())
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
                .ReadFrom.Configuration(configBuilder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: keepDays)
                .CreateLogger();
            SeriHelper.Info("MAIN", "------------------------------------------------------");
            SeriHelper.Info("MAIN", "Apps.SKD.GrpcFileReceiver Program Started");
            SeriHelper.Info("MAIN", "------------------------------------------------------");


            SeriHelper.Info("MAIN", "Building WebApplication");
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            SeriHelper.Info("MAIN", "Add gRPC service");
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            SeriHelper.Info("MAIN", "Configure the HTTP request pipeline.");
            SeriHelper.Info("MAIN", "Map GreeterService to service");
            app.MapGrpcService<GreeterService>();
            app.MapGrpcService<ServiceGrpcFileReceiver>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
        static private void buildConfig(IConfigurationBuilder builder)
        {
            // EXE가 위치한 폴더에서 appsettings.json 파일을 찾을 후 읽
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
        static private void createServiceGrpcFileReceiver(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            bool isCreateMainMenu = configuration.GetSection("ServiceGrpcFileReceiver Configuration").GetValue<bool>("IsCreateService");
            bool isRunMainMenu = configuration.GetSection("ServiceGrpcFileReceiver Configuration").GetValue<bool>("IsRunService");
            bool isShowMenuMainMenu = configuration.GetSection("ServiceGrpcFileReceiver Configuration").GetValue<bool>("IsShowMenu");

            if (isCreateMainMenu)
            {
                SeriHelper.Info("MAIN", "Creating ServiceGrpcFileReceiver object");
                //var ServiceGrpcFileReceiver = ActivatorUtilities.CreateInstance<ServiceGrpcFileReceiver>(serviceProvider);
                //if (isRunMainMenu)
                //{
                //    SeriHelper.Info("MAIN", "Running ServiceGrpcFileReceiver object");
                //    ServiceGrpcFileReceiver.Run();
                //}
                //if (isShowMenuMainMenu)
                //{
                //    SeriHelper.Info("MAIN", "Show Menu of ServiceGrpcFileReceiver object");
                //    ServiceGrpcFileReceiver.ShowMenu();
                //}
                SeriHelper.Info("MAIN", "");
            }
            Thread.Sleep(500);
        }
    }
}
## 1. REQUIREMENT ANALYSIS
- Add serilog functionality to project
- Modify appsettings.json to add default folder defintion
- Modify appsettings.json to change default log setting to serilog setting
<br><br>


<!----------------------------------------------------------------------------------->
## 2. WORK NOTE

### 2.1. ORIGINAL CODE

The following code is original code which is created by default.

```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddGrpc();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<GreeterService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
```
<br>


### 2.2. MODIFIED CODE

```csharp
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
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
```

The following code is helper function which is used to upper Main function.

```csharp
static private void buildConfig(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables();
}        
```


<br><br>

<!----------------------------------------------------------------------------------->
## 3. REMAIN PROBLEM
- nothing

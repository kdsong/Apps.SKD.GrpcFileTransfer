## 1. REQUIREMENT ANALYSIS

- add serilog functionality to client console project
- add dependency injection functionality to client console project 
<br>


<!----------------------------------------------------------------------------------->
## 2. WORK NOTE

### 2.1. Add SKD.Utility NuGet Package

Add SKD.Utility NuGet package which supports miscellaneous features to progress this work.

![Image](https://github.com/user-attachments/assets/64ac0dec-8fca-4440-9a6f-ac89350a7d24)

![Image](https://github.com/user-attachments/assets/b337c270-1319-4b20-9a7d-51bc98f96595)

<br>


### 2.2. Add appsettings.json

Add appsetting.json to project and define the settings information as show below.

![Image](https://github.com/user-attachments/assets/b82cd12e-78dd-4512-ad86-92569d14e123)

![Image](https://github.com/user-attachments/assets/65075bb8-e1a0-468d-b341-df69c1884e26)

![Image](https://github.com/user-attachments/assets/7ff4aa03-3c43-44b1-9009-dbbb8bf0ddd5)

<br>



### 2.3. Change Program.cs File

Modify Program.cs file to support serilog, dependency injection functionality as show below.

```csharp
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
    SeriHelper.Info("MAIN", "Apps.SKD.GrpcFileSender Program Started");
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
```

Below is the screen where the program ran and the default data folder that was automatically created.

![Image](https://github.com/user-attachments/assets/273e9840-692f-48fb-859e-744001eb7d26)

![Image](https://github.com/user-attachments/assets/f0bd068c-20d8-44f0-bcc1-fcd27e96734f)

<br>


<!----------------------------------------------------------------------------------->
## 3. REMAIN PROBLEM
- nothing

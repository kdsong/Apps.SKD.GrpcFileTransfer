using Grpc.Core;
using System.IO;
using System.Threading.Tasks;
using Apps.SKD.GrpcFileTransfer.Protos;
using SKD.Utility;

namespace Apps.SKD.GrpcFileReceiver.Services;

public class ServiceGrpcFileReceiver : GrpcFileService.GrpcFileServiceBase
{
    public ServiceGrpcFileReceiver(ILogger<ServiceGrpcFileReceiver> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _serviceConfig = config.GetSection("ServiceGrpcFileReceiver Configuration");
    }



    //---------------------------------------------------------------------
    // [[0]] typedef, enum, member variables
    //---------------------------------------------------------------------
    private const string _logClassName = "SGRFR";
    private readonly ILogger<ServiceGrpcFileReceiver> _logger;
    private readonly IConfiguration _config;
    private IConfiguration? _serviceConfig;



    //---------------------------------------------------------------------
    // [[2]] Method
    //---------------------------------------------------------------------
    public override async Task<UploadStatus> UploadFile(
        IAsyncStreamReader<FileUploadRequest> requestStream,
        ServerCallContext context)
    {
        string fileName = null;
        string fullPath = null;
        FileStream outputFile = null;

        if (_serviceConfig == null)
        {
            SeriHelper.Error(_logClassName, "UPLOAD FILE\tService configuration is null");
            return new UploadStatus { Status = "Service configuration is null" };
        }

        string? receiveFolder = _serviceConfig.GetValue<string>("WatchingFolder");
        if (string.IsNullOrEmpty(receiveFolder))
        {
            SeriHelper.Error(_logClassName, "UPLOAD FILE\tReading folder is null or empty");
            return new UploadStatus { Status = "Service configuration is null" };
        }

        //string receiveFolder = @"c:\APP_DATA\SKD\GrpcFileReceiver\UPLOAD\";

        try
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;

                if (fileName == null)
                {
                    fileName = request.FileName;
                    SeriHelper.Info(_logClassName, $"UPLOAD FILE\tFILE NAME\t{fileName}");

                    fullPath = Path.Combine(receiveFolder, fileName);
                    outputFile = File.Create(fullPath);
                }

                if (request.Content.Length > 0)
                {
                    await outputFile.WriteAsync(request.Content.ToByteArray());
                }
            }
            //SeriHelper.Info(_logClassName, $"UPLOAD FILE\tFOLDER NAME\t{receiveFolder}");
            SeriHelper.Info(_logClassName, $"UPLOAD FILE\tWRITE FILE\t{fileName}");

            return new UploadStatus { Status = "File uploaded successfully" };
        }
        finally
        {
            if (outputFile != null)
            {
                await outputFile.DisposeAsync();
            }
        }
    }

}

using Grpc.Core;
using System.IO;
using System.Threading.Tasks;
using Apps.SKD.GrpcFileReceiver.Protos;

namespace Apps.SKD.GrpcFileReceiver.Services;

public class ServiceGrpcFileReceiver : GrpcFileService.GrpcFileServiceBase
{
    private readonly ILogger<ServiceGrpcFileReceiver> _logger;
    
    public ServiceGrpcFileReceiver(ILogger<ServiceGrpcFileReceiver> logger)
    {
        _logger = logger;
    }
    public override async Task<UploadStatus> UploadFile(
        IAsyncStreamReader<FileUploadRequest> requestStream,
        ServerCallContext context)
    {
        string fileName = null;
        FileStream outputFile = null;

        try
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;

                if (fileName == null)
                {
                    fileName = request.FileName;
                    outputFile = File.Create(fileName);
                }

                if (request.Content.Length > 0)
                {
                    await outputFile.WriteAsync(request.Content.ToByteArray());
                }
            }

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

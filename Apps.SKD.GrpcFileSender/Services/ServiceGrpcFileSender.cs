using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SKD.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.SKD.GrpcFileSender.Protos;
using System.Text.RegularExpressions;

namespace Apps.SKD.GrpcFileSender.Services;

public class ServiceGrpcFileSender : IServiceGrpcFileSender
{
    public ServiceGrpcFileSender(ILogger<ServiceGrpcFileSender> log, IConfiguration config)
    {
        _log = log;
        _mainConfig = config;
        _serviceConfig = config.GetSection("ServiceGrpcFileSender Configuration");
        _autoResetEvent = new AutoResetEvent(false);
        _sendingFiles = new List<string>();
    }


    //---------------------------------------------------------------------
    // [[0]] typedef, enum, member variables
    //---------------------------------------------------------------------
    private const string _logClassName = "SGRFS";

    private readonly ILogger<ServiceGrpcFileSender> _log;
    private readonly IConfiguration _mainConfig;
    private IConfiguration? _serviceConfig;
    private AutoResetEvent? _autoResetEvent;
    private Timer? _timerRun;
    private Timer? _timerReader;
    private Timer? _timerSender;

    private List<string> _sendingFiles;
    private bool _isSending = false;


    //---------------------------------------------------------------------
    // [[1]] Property
    //---------------------------------------------------------------------



    //---------------------------------------------------------------------
    // [[2]] Method
    //---------------------------------------------------------------------
    public void Run()
    {
        SeriHelper.Info(_logClassName, "ServiceGrpcFileSender.Run()");

        int dueTime = 3000;
        int interval = 100;

        if (_serviceConfig == null)
        {
            SeriHelper.Error(_logClassName, "_serviceConfig is null");
        }
        else
        {
            dueTime = _serviceConfig.GetValue<int>("RunTimerDueTimeMS");
            interval = _serviceConfig.GetValue<int>("RunTimerIntervalMS");
        }

        _timerRun = new Timer(onTimerRun, _autoResetEvent, dueTime, interval);
    }
    public void ShowMenu()
    {
        SeriHelper.Info(_logClassName, "ServiceGrpcFileSender.showMenu()");
        ConsoleKeyInfo consoleKeyInfo;

        do
        {
            Console.Clear();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Apps.SKD.GrpcFileSenderServiceGrpcFileSender Menu");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("1. Reserved");
            Console.WriteLine("2. Reserved");
            Console.WriteLine("3. Reserved");
            Console.WriteLine("A. Reserved");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Q. Quit");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("");
            consoleKeyInfo = Console.ReadKey();


            switch (consoleKeyInfo.KeyChar)
            {
                case '1':
                    Console.WriteLine("Reserved...");
                    break;

                case '2':
                    Console.WriteLine("Reserved...");
                    break;

                case '3':
                    Console.WriteLine("Reserved...");
                    break;

                case 'A':
                case 'a':
                    Console.WriteLine("Reserved...");
                    break;

                case 'Q':
                case 'q':
                    Environment.Exit(0);
                    break;

                default:
                    break;
            }

        }
        while (consoleKeyInfo.KeyChar != 'Q' && consoleKeyInfo.KeyChar != 'q');
    }



    //---------------------------------------------------------------------
    // [[3]] User-defined functions
    //---------------------------------------------------------------------



    //---------------------------------------------------------------------
    // [[4]] Event Handler, Message Handler, Callback Functions
    //---------------------------------------------------------------------
    private void onTimerRun(Object? stateInfo)
    {
        SeriHelper.Info("SGRFS", $"ServiceMeasure\tonTimerRun");
        if (_timerRun == null)
            return;

        // First stop and remove the RUN timer, as we only need to run it once initially.
        _timerRun.Change(Timeout.Infinite, Timeout.Infinite);
        _timerRun.Dispose();


        // create a new timer for the reading process of 'WatchingFolder'
        int readingDueTime = 1000;
        int readingInterval = 5000;

        if (_serviceConfig == null)
        {
            SeriHelper.Error(_logClassName, "_serviceConfig is null");
        }
        else
        {
            readingInterval = _serviceConfig.GetValue<int>("ReadingIntervalMS");
        }

        _timerReader = new Timer(onTimerReading, _autoResetEvent, readingDueTime, readingInterval);


        // create a new timer for sending file process
        int sendingDueTime = 3000;
        int sendingInterval = 3000;

        if (_serviceConfig == null)
        {
            SeriHelper.Error(_logClassName, "_serviceConfig is null");
        }
        else
        {
            sendingInterval = _serviceConfig.GetValue<int>("SendingIntervalMS");
        }

        _timerSender = new Timer(onTimerSending, _autoResetEvent, sendingDueTime, sendingInterval);
    }
    private void onTimerReading(Object? stateInfo)
    {
        try
        {
            // if there is previous working files in list, return
            if (_sendingFiles.Count > 0)
            {
                SeriHelper.Info(_logClassName, $"TIMER READING\tEXIST\tPREVIOUS FILES\t{_sendingFiles.Count}");
                return;
            }
            // if service config is null, it cannot know where to read files
            if (_serviceConfig == null)
            {
                SeriHelper.Error(_logClassName, "_serviceConfig is null");
                return;
            }

            // get the folder path from service config, and read files from it
            string? readingFolder = _serviceConfig.GetValue<string>("WatchingFolder");
            if (string.IsNullOrEmpty(readingFolder))
            {
                SeriHelper.Error(_logClassName, "Reading folder is null or empty");
                return;
            }

            string[] newFiles = Directory.GetFiles(readingFolder);
            if (newFiles.Length > 0)
            {
                _sendingFiles.AddRange(newFiles);
                SeriHelper.Info(_logClassName, $"TIMER READING\tREADING FILES\tCOUNT\t{_sendingFiles.Count}");
            }
            else
            {
                SeriHelper.Info(_logClassName, $"TIMER READING\tREADING FILES\tCOUNT\t{_sendingFiles.Count}");
            }
        }
        catch (Exception ex)
        {
            SeriHelper.Error(_logClassName, $"Apps.SKD.GrpcFileSenderServiceGrpcFileSender.onTimerReading()");
            SeriHelper.Error(_logClassName, $"--> {ex.Message}");
        }
        finally
        {

        }
    }
    private async void onTimerSending(Object? stateInfo)
    {
        // if there is no file in the list, return
        if (_sendingFiles.Count == 0)
        {
            SeriHelper.Verbose(_logClassName, $"TIMER SENDING\tNO FILES");
            return;
        }

        // if it is already running, return
        if (_isSending == true)
        {
            SeriHelper.Info(_logClassName, $"TIMER SENDING\tALREADY RUNNING");
            return;
        }



        try
        {
            _isSending = true;
            string sendingFileFullName = _sendingFiles[0];

            if (string.IsNullOrEmpty(sendingFileFullName))
            {
                SeriHelper.Error(_logClassName, "TIMER SENDING\tSending file is null or empty");
                return;
            }
            if (!File.Exists(sendingFileFullName))
            {
                SeriHelper.Error(_logClassName, "TIMER SENDING\tSending file does not exist");
                return;
            }
            
            string sendingFileName = Path.GetFileName(sendingFileFullName);
            string? serverAddress = _serviceConfig?.GetValue<string>("ServerAddress");
            if (string.IsNullOrEmpty(serverAddress))
            {
                SeriHelper.Error(_logClassName, "TIMER SENDING\tServer address is null or empty");
                return;
            }

            var channel = GrpcChannel.ForAddress(serverAddress);
            var client = new GrpcFileService.GrpcFileServiceClient(channel);

            using (var call = client.UploadFile())
            {
                string returnStatus = string.Empty;

                SeriHelper.Info(_logClassName, $"TIMER SENDING\tBEGIN SEND");

                // Send the file name first 
                await call.RequestStream.WriteAsync(new FileUploadRequest
                {
                    FileName = Path.GetFileName(sendingFileFullName)
                });

                // Send the file content in chunks
                using (var fileStream = File.OpenRead(sendingFileFullName))
                {
                    var buffer = new byte[64 * 1024]; // 64 KB buffer
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await call.RequestStream.WriteAsync(new FileUploadRequest
                        {
                            Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
                        });
                    }

                    await call.RequestStream.CompleteAsync();
                    var response = await call;
                    returnStatus = response.Status;
                    SeriHelper.Info(_logClassName, $"TIMER SENDING\tEND SEND\t{returnStatus}");
                }

                if (returnStatus == "File uploaded successfully")
                {
                    // Remove the file from the list and delete it
                    _sendingFiles.RemoveAt(0);
                    File.Delete(sendingFileFullName);
                    SeriHelper.Info(_logClassName, $"TIMER SENDING\tFILE DELETED\t{sendingFileName}");
                }
                else
                {
                    SeriHelper.Error(_logClassName, $"TIMER SENDING\tFILE NOT UPLOADED\t{sendingFileName}");
                }
            }

            
        }
        catch (RpcException rpcEx)
        {
            SeriHelper.Error(_logClassName, $"Apps.SKD.GrpcFileSender.Services.ServiceGrpcFileSend.onTimerSending(RpcException)");
            SeriHelper.Error(_logClassName, $"--> {rpcEx.Message}");
        }
        catch (IOException ioEx)
        {
            SeriHelper.Error(_logClassName, $"Apps.SKD.GrpcFileSender.Services.ServiceGrpcFileSend.onTimerSending(IOException)");
            SeriHelper.Error(_logClassName, $"--> {ioEx.Message}");
        }
        catch (Exception ex)
        {
            SeriHelper.Error(_logClassName, $"Apps.SKD.GrpcFileSender.Services.ServiceGrpcFileSend.onTimerSending(Exception)");
            SeriHelper.Error(_logClassName, $"--> {ex.Message}");
        }
        finally
        {
            _isSending = false;
        }
    }



    //---------------------------------------------------------------------
    // [[5]] Override functions
    //---------------------------------------------------------------------
}

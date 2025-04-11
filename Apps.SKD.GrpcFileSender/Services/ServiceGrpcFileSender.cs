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

namespace Apps.SKD.GrpcFileSender.Services;

public class ServiceGrpcFileSender
{
    public ServiceGrpcFileSender(ILogger<ServiceGrpcFileSender> log, IConfiguration config)
    {
        _log = log;
        _mainConfig = config;
        _serviceConfig = config.GetSection("ServiceGrpcFileSender Configuration");
        _autoResetEvent = new AutoResetEvent(false);

        FolderConfigHelper folderConfigHelper = new FolderConfigHelper(_mainConfig);
        _tempPath = folderConfigHelper.TempFolder;
    }


    //---------------------------------------------------------------------
    // [[0]] typedef, enum, member variables
    //---------------------------------------------------------------------
    private readonly ILogger<ServiceGrpcFileSender> _log;
    private readonly IConfiguration _mainConfig;
    private IConfiguration? _serviceConfig;
    private AutoResetEvent? _autoResetEvent;
    private Timer? _timerRun;
    private Timer? _timerMeasurement;
    private int _count = 0;

    // SOME OTHERS....
    private string _tempPath = @"";



    //---------------------------------------------------------------------
    // [[1]] Property
    //---------------------------------------------------------------------



    //---------------------------------------------------------------------
    // [[2]] Method
    //---------------------------------------------------------------------
    public void Run()
    {
        SeriHelper.Info("SGVLM", "ServiceGrpcFileSender.Run()");

        int dueTime = _mainConfig.GetValue<int>("Measurement Run Timer DueTime (ms)");
        int interval = _mainConfig.GetValue<int>("Measurement Run Timer Interval (ms)");
        _timerRun = new Timer(onTimerRun, _autoResetEvent, dueTime, interval);
    }
    public void ShowMenu()
    {
        SeriHelper.Info("SGVLM", "ServiceGrpcFileSender.showMenu()");
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
    private void onTimerRun(Object stateInfo)
    {
        SeriHelper.Info("SSEVR", $"ServiceMeasure\tonTimerRun");
        if (_timerRun == null)
            return;

        // 최초에 한 번만 실행하면 되므로 먼저 RUN 타이머를 중지시키고 제거한다.
        _timerRun.Change(Timeout.Infinite, Timeout.Infinite);
        _timerRun.Dispose();


        // 최초 실행시에 동작하는 코드를 작성한다.
        // Run 작업을 수행할 때 오래 걸리는 시간이 있을 경우를 대비해서
        // 타이머를 별도로 생성하고 핸들러 함수에서 최초 실행과 관련되는 작업을 한다. 
        int dueTime = _mainConfig.GetValue<int>("Measurement Timer DueTime (ms)");
        int interval = _mainConfig.GetValue<int>("Measurement Timer Interval (ms)");
        _timerMeasurement = new Timer(onTimerMeasurement, _autoResetEvent, dueTime, interval);
    }
    private void onTimerMeasurement(Object? stateInfo)
    {
        try
        {
            if (_timerMeasurement == null)
                return;
            if (_serviceConfig == null)
                return;


            // 최초에 수행하는 작업을 한 번만 수행하면 되므로 타이머를 중지시키고 제거한다.
            SeriHelper.Info("SGVLM", $"ServiceGrpcFileSender\tonTimerMeasurement\t{_count++}");
            _timerMeasurement.Change(Timeout.Infinite, Timeout.Infinite);
            _timerMeasurement.Dispose();
        }
        catch (Exception ex)
        {
            SeriHelper.Error("SGVLM", $"Apps.SKD.GrpcFileSenderServiceGrpcFileSender.onTimerMeasurement()");
            SeriHelper.Error("SGVLM", $"--> {ex.Message}");
        }
        finally
        {

        }
    }



    //---------------------------------------------------------------------
    // [[5]] Override functions
    //---------------------------------------------------------------------
}

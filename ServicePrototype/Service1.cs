using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServicePrototype
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class TestService : ServiceBase
    {
        private int eventId = 1;
        private bool _polling = false;

        public TestService()
        {
            InitializeComponent();
          
            try
            {
                eventLog1 = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("MySource"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "MySource", "MyNewLog");
                }
                eventLog1.Source = "MySource";
                eventLog1.Log = "MyNewLog";

            }
            catch
            {

            }
        }
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            //the timer kicks off an aynschronous routine
            //the routine sets a variable indicating it has started.  When the asynch routine is done
            //it sets it to false.  This prevents the timer from kicking off this asynch process before
            //the last one was completed
        
            if(!_polling)
            {
                PollingOperation();
            }
        }

        private int GetFileCount()
        {
            string message = "Polling Operation " + eventId.ToString();
            eventId++;
            eventLog1.WriteEntry(message);
            return 1;

        }
        private async void PollingOperation()
        {
            try
            {
               
                //always run these types of process, which can be long running as
                //asynchronous  

                _polling = true;
                Task<int> getFileCount = new Task<int>(GetFileCount);

                getFileCount.Start();

                int fileCount = await getFileCount;

            }catch
            {

            }finally
            {
                _polling = false;
            }
        }

        protected override void OnStart(string[] args)
        {
            //Services typically use a polling type operation
            //this is started in OnStart however it must be implemented elsewhere
            //the event cannot stay in a continuous loop

            eventLog1.WriteEntry("In OnStart");

            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RemoteServant
{
    public partial class Service1 : ServiceBase
    {
        //TODO:Use Directory watcher rather than using Hard Coded Interval to Run Commands.
        //TODO:LOG Version on Each Rebuild
        //Service Timer: Everytime after each interval command will be executed. 
        //Code Refresh Timer (Improve Myself): Everytime after this interval the Service will Stop Itself (OOOhhhhh, Weireeed!!!),
                                                                            //Rebuild Itself
                                                                            //Start itself Again (WTH...Howww!!!)
         System.Timers.Timer servicetimer,CodeRefershTimer;
        public Service1()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            servicetimer = new System.Timers.Timer(10 * 1000);
            CodeRefershTimer = new System.Timers.Timer(1*60 * 1000);
            servicetimer.Elapsed += new ElapsedEventHandler(ExecuteAllBatchCommands);
            CodeRefershTimer.Elapsed += new ElapsedEventHandler(ReBuildandRestartService);
            servicetimer.Enabled = true;
            CodeRefershTimer.Enabled = true;

            
          WritetoLog("Service Started! New Version Another");
        }
        protected override void OnStop()
        {
            WritetoLog("Service Stopped! ");
        }
        private void ExecuteAllBatchCommands(object sender, ElapsedEventArgs e)
        {
            string[] BatchCommands = GetAllCommandsFromFile();
            foreach (string command in BatchCommands)
            {
                ExecuteCommand(command);
            }

        }
        private string [] GetAllCommandsFromFile()
        {
            // System.Diagnostics.Debugger.Launch();
            //Assuming Each line has one command
            string path = @"D:\PersonelSVN\trunk\RemoteServant\Files\Commands.txt";
            if (!File.Exists(path))
                File.Create(path);
            string[] Lines = File.ReadAllLines(path);
            return Lines;
        }    
        void ExecuteCommand(string command)
        {
            WritetoFile("D:\\PersonelSVN\\trunk\\RemoteServant\\Files\\serviceouput.txt", string.Format("*******Command Execution Started: {0}*********", command));
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", String.Format(" /c \"{0} >> D:\\PersonelSVN\\trunk\\RemoteServant\\Files\\serviceouput.txt\" ", command));


            WritetoLog(processInfo.Arguments);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            process = Process.Start(processInfo);
            process.WaitForExit();
            exitCode = process.ExitCode;
            process.Close();

            WritetoFile("D:\\PersonelSVN\\trunk\\RemoteServant\\Files\\serviceouput.txt", "********************************Command Execution Ended******************");
        }
        public void WritetoLog(string msg)
        {
            try
            {
                string path = @"D:\PersonelSVN\trunk\RemoteServant\Files\ServiceLog.txt";

                if (!File.Exists(path))
                {

                    File.Create(path);
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(System.DateTime.Now + " -- " + msg);
                    
                }


            }
            catch (Exception exp)
            {
                string path = @"D:\PersonelSVN\trunk\RemoteServant\Files\ErrorLog.txt";
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(System.DateTime.Now + " -- " + exp.Message);
                    sw.Close();
                }
            }
        }
        public void WritetoFile(string path, string msg)
        {
            try
            {
                

                if (!File.Exists(path))
                {

                    File.Create(path);
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(System.DateTime.Now + " -- " + msg);

                }


            }
            catch (Exception exp)
            {
                path = @"D:\PersonelSVN\trunk\RemoteServant\Files\ErrorLog.txt";
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(System.DateTime.Now + " -- " + exp.Message);
                    sw.Close();
                }
            }
        }
        private void ReBuildandRestartService(object sender, ElapsedEventArgs e)
        {
            ExecuteCommand("call net stop RemoteServant&\"%programfiles(x86)%\\Microsoft Visual Studio 14.0\\VC\\bin\\vcvars32.bat\"&msbuild \"D:\\PersonelSVN\\trunk\\RemoteServant\\RemoteServant.sln\"&timeout /t 10&net start RemoteServant");
        }
        
    }
}

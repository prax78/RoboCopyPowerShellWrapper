using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace PowerShellWrapperForRoboCopy
{

    [Cmdlet(VerbsCommon.Copy, "PowerShellWrapperForRoboCopy")]
    public class PowerShellWrapperForRoboCopy:PSCmdlet
    {

            [Parameter(Mandatory = true, Position = 0, HelpMessage = "Provide Source For RoboCopy")]
            [ValidateNotNullOrEmpty]
            public  string Source { get; set; }

            [Parameter(Mandatory = true, Position = 1, HelpMessage = "Provide Destination For RoboCopy")]
            [ValidateNotNullOrEmpty]
            public  string Destination { get; set; }

            [Parameter(Mandatory = true, Position = 2, HelpMessage = "Provide Arguements  For RoboCopy within quotes")]
            [ValidateNotNullOrEmpty]


            public  string RoboCopyArguements { get; set; }
    
          
                static double sourcefileze = 0.0;
          
                static int sourcefilecount = 0;
                static int destfilecount = 0;
                static int PID = 0;
                static Stopwatch elptimeWatch = new Stopwatch();
      
            static double eta = 0.0;

            static double etaFPS = 0.0;
  

            static double destfilesize = 0.0;

       

        protected override void ProcessRecord()
            {
            elptimeWatch.Start();
            string arguements = $"{Source} {Destination} {RoboCopyArguements}";
            Console.CancelKeyPress += (s, e) => { if (!Process.GetProcessById(PID).HasExited && PID != 0) { Process.GetProcessById(PID).Kill(); } };

                if (Directory.Exists(Destination))
                {
                    RunRoboCopyFirstTime(Source, Destination,arguements);
                    while (Process.GetProcesses().Where(x => x.Id == PID).Any())
                    {
                        WriteProgress(ProgressBar(100, Source, Destination, PID));

                    }
                elptimeWatch.Stop();

                Console.WriteLine($"Elapsed Time {TimeSpan.FromSeconds(elptimeWatch.Elapsed.TotalSeconds)}");
                }
               
            sourcefileze = 0.0;
            destfilesize = 0.0;
            sourcefilecount = 0;
            destfilecount = 0;
            PID = 0;
            eta = 0.0;
          
            etaFPS = 0.0;
            
   
        }

            static void RunRoboCopyFirstTime(string source, string destination,string arguements)
            {
               foreach(var files in Directory.GetFiles(source,"*",SearchOption.AllDirectories))
                {
                    var srcfileinfo = new FileInfo(files);
                    sourcefileze += srcfileinfo.Length / 1000000;
              


                }

            sourcefilecount = Directory.GetFiles(source, "*", SearchOption.AllDirectories).Count();

               PID = RunRoboCopy(arguements);

            }


            static int RunRoboCopy(string args)
            {
            
                using (Process robo = new Process())
                {
                    robo.StartInfo.FileName = "robocopy.exe";
                    robo.StartInfo.Arguments = args;
                    robo.StartInfo.UseShellExecute = false;
                    robo.StartInfo.CreateNoWindow = true;
                    robo.Start();
                    return robo.Id;
                }
         

            }
                    
        static ProgressRecord ProgressBar(int actId,string source,string destination, int pID)
            {
            destfilesize = 0.0;
          
           try { destfilecount = Directory.GetFiles(destination, "*", SearchOption.AllDirectories).Count(); } catch { };

            try { foreach (var files in Directory.GetFiles(destination, "*", SearchOption.AllDirectories)){ var finfo = new FileInfo(files); destfilesize += Convert.ToDouble(finfo.Length) /1000000;  } } catch { }
            
              double percentprogress = 0.0;

           
                etaFPS =destfilecount/ elptimeWatch.Elapsed.TotalSeconds;
                if (etaFPS != 0)
                {

                    eta = (sourcefilecount - destfilecount) / etaFPS;
                   
                }

          
            ProgressRecord pRecProg = new ProgressRecord(actId, "Copy-PowerShellWrapperForRoboCopy", $"RoboCopy In Progres[PID]:{pID} ");
                pRecProg.RecordType = ProgressRecordType.Processing;
                percentprogress = (double)destfilecount / (double)sourcefilecount * 100;
                pRecProg.PercentComplete = Convert.ToInt32(percentprogress);
                pRecProg.CurrentOperation = $"Copying.. {percentprogress.ToString("0.00") }% || Bkp Speed {etaFPS.ToString("0.0")} Mbps || ETA {TimeSpan.FromSeconds(eta)} ";
            pRecProg.StatusDescription = $"Copied {destfilecount} of {sourcefilecount}..Source: {source} Destination: {destination} PID: {pID}  Elapsed Time {TimeSpan.FromSeconds(elptimeWatch.Elapsed.TotalSeconds)}";
            return pRecProg;
            }
               
    }
}

    

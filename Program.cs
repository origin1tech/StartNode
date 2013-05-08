using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace StartNode
{
    class Program
    {
        static void Main(string[] args)
        {               

            string environment, nodePath, appDir, appStart, appPath;
            bool min, appFound, allowOverrides;
            string[] defaultApps;

            try
            {
                XElement config = XElement.Load(@"config.xml");
                environment = config.Element("mode").Value;
                nodePath = config.Element("nodepath").Value;
                appDir = config.Element("appdir").Value;
                appStart = config.Element("appstart").Value;
                min = Convert.ToBoolean(config.Element("minimize").Value);
                allowOverrides = Convert.ToBoolean(config.Element("allowoverrides").Value);
                defaultApps = config.Element("defaultapps").Value.Split(',');
                appFound = true;

                // if overrides allowed try current directory if can't find specified.
                if (allowOverrides && !Directory.Exists(appDir))
                {
                    Console.WriteLine("Directory not found, overriding to current.");
                    appDir = Environment.CurrentDirectory.ToString();
                }

                if (!File.Exists(appDir + @"\" + appStart))
                {
                    appFound = false;
                    if (allowOverrides)
                    {
                        Console.WriteLine("Application not found, checking overrides.");
                    
                        foreach (var s in defaultApps)
                        {
                            if (File.Exists(appDir + @"\" + s))
                            {
                                appFound = true;
                                appStart = s;
                                Console.WriteLine("Application found at " + appDir + @"\" + s);
                                break;
                            }
                        }
                    }                
                }              

                // set the application startup path.
                appPath = appDir + @"\" + appStart;        
          
                if (!appFound)
                {
                    MessageBox.Show("Unable to locate Application path located at " + appPath, "Error - Path Not Found", MessageBoxButtons.OK);
                    Environment.Exit(0);
                }                

                if (!File.Exists(nodePath))
                {
                    MessageBox.Show("Unable to locate Node executable located at " + nodePath, "Error - Node Not Found", MessageBoxButtons.OK);
                    Environment.Exit(0);
                }
              
                Launch(environment, nodePath, appPath, min);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error - Configuration Failure", MessageBoxButtons.OK);              
            }              
        }

     
        static void Launch(string env, string nodePath, string appDir, bool min)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = nodePath;
            startInfo.Arguments = appDir;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;          
     
            if(min)
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if(!string.IsNullOrEmpty(env))
                startInfo.EnvironmentVariables.Add("NODE_ENV", env);

            Console.WriteLine("Starting Node...");
            Console.WriteLine(Environment.NewLine);

            try
            {
                using (var p = Process.Start(startInfo))
                {                    
                    p.Exited += UnhandledProcessException;
                    p.WaitForExit();
                }              
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error - Process Start Failure", MessageBoxButtons.OK);
            }       
        }

        static void UnhandledProcessException(object sender, EventArgs e)
        {
            var p = (Process)sender;

            if (p != null)
                Console.WriteLine("Node.exe exited with code: {0} ", p.ExitCode);
            else
                Console.WriteLine("Node.exe has exited.");

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            Environment.Exit(1);
        }

    }
}

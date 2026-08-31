using System;

namespace QuantarisHost
{
    class Launcher
    {
        static void Main(string[] args)
        {
            string port = args.Length > 0 ? args[0] : "8080";
            string appPath = args.Length > 1 ? args[1] : Environment.CurrentDirectory;

            var server = new Server();
            server.Start(port, appPath);
        }
    }
}

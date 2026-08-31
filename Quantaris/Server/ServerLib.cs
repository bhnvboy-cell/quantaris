using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace QuantarisHost
{
    // This class lives in the web app's Bin directory so ApplicationHost can find it
    public class HostContext : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null; // Live forever
        }
    }

    public class Server
    {
        private HttpListener _listener;
        private string _port;
        private string _appPath;

        public void Start(string port, string appPath)
        {
            _port = port;
            _appPath = Path.GetFullPath(appPath);

            Console.WriteLine("=======================================");
            Console.WriteLine(" Quantaris Standalone Server");
            Console.WriteLine("=======================================");
            Console.WriteLine(" App: " + _appPath);
            Console.WriteLine(" URL: http://localhost:" + _port + "/");
            Console.WriteLine("=======================================");
            Console.WriteLine(" Press Ctrl+C to stop.\n");

            // Initialize ASP.NET application host
            ApplicationHost.CreateApplicationHost(typeof(HostContext), "/", _appPath);

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port + "/");
            _listener.Start();

            Console.WriteLine(" Server ready! Browse to:");
            Console.WriteLine(" http://localhost:" + _port + "/Setup.aspx");
            Console.WriteLine(" http://localhost:" + _port + "/Account/Login.aspx?tenant=DEMO");
            Console.WriteLine("");

            while (true)
            {
                try
                {
                    var ctx = _listener.GetContext();
                    ProcessRequest(ctx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server error: " + ex.Message);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext ctx)
        {
            try
            {
                string path = ctx.Request.Url.AbsolutePath;
                string query = ctx.Request.Url.Query ?? "";
                if (query.StartsWith("?")) query = query.Substring(1);

                Console.WriteLine(" " + ctx.Request.HttpMethod + " " + path);

                using (var sw = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
                {
                    var wr = new SimpleWorkerRequest(path, query, sw);
                    typeof(HttpRuntime).GetMethod("ProcessRequest",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Static)
                        .Invoke(null, new object[] { wr });
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Request error: " + ex.Message);
                try
                {
                    byte[] err = Encoding.UTF8.GetBytes(
                        "<html><body><h2>Error</h2><pre>" +
                        System.Net.WebUtility.HtmlEncode(ex.ToString()) +
                        "</pre></body></html>");
                    ctx.Response.StatusCode = 500;
                    ctx.Response.OutputStream.Write(err, 0, err.Length);
                    ctx.Response.OutputStream.Close();
                }
                catch { }
            }
        }
    }
}

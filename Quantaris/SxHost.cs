using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace SxHost
{
    public class AppHost : MarshalByRefObject
    {
        public override object InitializeLifetimeService() { return null; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string port = args.Length > 0 ? args[0] : "8080";
            string appPath = Path.GetFullPath(args.Length > 1 ? args[1] : Environment.CurrentDirectory);

            Console.WriteLine("=======================================");
            Console.WriteLine(" Quantaris Server");
            Console.WriteLine("=======================================");
            Console.WriteLine(" Path: " + appPath);
            Console.WriteLine(" URL:  http://localhost:" + port + "/");
            Console.WriteLine("=======================================\n");

            var host = ApplicationHost.CreateApplicationHost(typeof(AppHost), "/", appPath);

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();

            Console.WriteLine(" Ready!\n");

            while (true)
            {
                try
                {
                    var ctx = listener.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;
                    string query = ctx.Request.Url.Query ?? "";
                    if (query.StartsWith("?")) query = query.Substring(1);

                    Console.WriteLine(" " + ctx.Request.HttpMethod + " " + path);

                    using (var sw = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
                    {
                        var wr = new SimpleWorkerRequest(path, query, sw);
                        typeof(HttpRuntime).GetMethod("ProcessRequest",
                            BindingFlags.NonPublic | BindingFlags.Static)
                            .Invoke(null, new object[] { wr });
                        sw.Flush();
                    }
                }
                catch (HttpListenerException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error: " + ex.Message);
                }
            }
        }
    }
}

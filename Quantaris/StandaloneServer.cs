using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace SxHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string port = args.Length > 0 ? args[0] : "8080";
            string appPath = Path.GetFullPath(args.Length > 1 ? args[1] : Environment.CurrentDirectory);

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (e.Name.StartsWith("SxHost")) return Assembly.GetExecutingAssembly();
                if (e.Name.StartsWith("Npgsql"))
                {
                    string dll = Path.Combine(appPath, "Bin", "Npgsql.dll");
                    return File.Exists(dll) ? Assembly.LoadFrom(dll) : null;
                }
                return null;
            };

            try
            {
                // Create app host in the ASP.NET AppDomain
                var host = (RequestProcessor)ApplicationHost.CreateApplicationHost(
                    typeof(RequestProcessor), "/", appPath);

                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + port + "/");
                listener.Start();
                Console.WriteLine("Ready on :" + port);

                while (true)
                {
                    try
                    {
                        var ctx = listener.GetContext();
                        string path = ctx.Request.Url.AbsolutePath;
                        string query = ctx.Request.Url.Query.TrimStart('?');
                        string method = ctx.Request.HttpMethod;
                        Console.WriteLine(" " + method + " " + path);

                        // Read POST body if present
                        byte[] entityBody = null;
                        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && ctx.Request.HasEntityBody)
                        {
                            using (var ms = new MemoryStream())
                            {
                                ctx.Request.InputStream.CopyTo(ms);
                                entityBody = ms.ToArray();
                            }
                        }

                                        // Build a Hashtable of incoming request headers
                        var requestHeaders = new Hashtable();
                        foreach (string key in ctx.Request.Headers.AllKeys)
                            requestHeaders[key] = ctx.Request.Headers[key];

                        // Process the request inside the ASP.NET AppDomain
                        byte[] body = host.ProcessRequestInAppDomain(path, query, method, entityBody, requestHeaders);

                        ctx.Response.StatusCode = host.StatusCode;
                        ctx.Response.StatusDescription = host.StatusDescription;

                        // Forward all response headers
                        if (host.ResponseHeaders != null)
                        {
                            foreach (DictionaryEntry h in host.ResponseHeaders)
                            {
                                string key = h.Key.ToString();
                                string val = h.Value.ToString();
                                if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                                    ctx.Response.ContentType = val;
                                else if (!string.Equals(key, "Content-Length", StringComparison.OrdinalIgnoreCase))
                                    ctx.Response.Headers[key] = val;
                            }
                        }

                        if (body != null && body.Length > 0)
                        {
                            ctx.Response.ContentLength64 = body.Length;
                            ctx.Response.OutputStream.Write(body, 0, body.Length);
                        }
                        else
                        {
                            ctx.Response.ContentLength64 = 0;
                        }
                        ctx.Response.OutputStream.Close();
                    }
                    catch (HttpListenerException) { break; }
                    catch (Exception ex)
                    {
                        Console.WriteLine("REQ ERR: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL: " + ex);
                Console.ReadLine();
            }
        }
    }

    // This class runs INSIDE the ASP.NET application AppDomain
    public class RequestProcessor : MarshalByRefObject
    {
        private static string _appPath;

        public int StatusCode { get; private set; }
        public string StatusDescription { get; private set; }
        public string ContentType { get; private set; }
        public Hashtable ResponseHeaders { get; private set; }

        public override object InitializeLifetimeService() { return null; }

        public RequestProcessor()
        {
            _appPath = AppDomain.CurrentDomain.GetData(".appPath") as string;
            if (_appPath == null) _appPath = Environment.CurrentDirectory;

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string name = e.Name.Split(',')[0];
                string[] searchPaths = {
                    Path.Combine(_appPath, "Bin"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".nuget", "packages")
                };

                // Search Bin folder first
                foreach (var dir in searchPaths)
                {
                    if (!Directory.Exists(dir)) continue;
                    var files = Directory.GetFiles(dir, name + ".dll", SearchOption.AllDirectories);
                    foreach (var f in files)
                    {
                        try { return Assembly.LoadFrom(f); } catch { }
                    }
                }
                return null;
            };
        }

        public byte[] ProcessRequestInAppDomain(string page, string query, string verb, byte[] entityBody = null, Hashtable requestHeaders = null)
        {
            try
            {
                var wr = new CustomWorkerRequest(page, query, verb, entityBody, requestHeaders);
                HttpRuntime.ProcessRequest(wr);
                wr.CloseResponse();

                StatusCode = wr.StatusCode;
                StatusDescription = wr.StatusDescription;
                ContentType = wr.ContentType;
                ResponseHeaders = wr.ResponseHeaders;

                return wr.GetBodyBytes();
            }
            catch (Exception ex)
            {
                StatusCode = 500;
                StatusDescription = "Error";
                ContentType = "text/html; charset=utf-8";
                return Encoding.UTF8.GetBytes("<pre>" + ex.ToString() + "</pre>");
            }
        }
    }

    public class CustomWorkerRequest : HttpWorkerRequest
    {
        private string _page, _query, _verb;
        private int _statusCode = 200;
        private string _statusDesc = "OK";
        private string _contentType = "text/html; charset=utf-8";
        private MemoryStream _responseBody = new MemoryStream();
        private Hashtable _responseHeaders = new Hashtable();
        private Hashtable _requestHeaders;
        private byte[] _entityBody;
        private int _entityBodyPos = 0;

        public int StatusCode { get { return _statusCode; } }
        public string StatusDescription { get { return _statusDesc; } }
        public string ContentType { get { return _contentType; } }
        public Hashtable ResponseHeaders { get { return _responseHeaders; } }

        private static readonly string _appPhysDir;
        private static readonly string _appVirtDir = "/";

        static CustomWorkerRequest()
        {
            _appPhysDir = AppDomain.CurrentDomain.GetData(".appPath") as string;
            if (_appPhysDir == null) _appPhysDir = Environment.CurrentDirectory;
            if (!_appPhysDir.EndsWith("\\")) _appPhysDir += "\\";
        }

        public CustomWorkerRequest(string page, string query, string verb, byte[] entityBody = null, Hashtable requestHeaders = null)
        {
            _page = page ?? "/";
            _query = query ?? "";
            _verb = verb ?? "GET";
            _entityBody = entityBody;
            _requestHeaders = requestHeaders;
        }

        public byte[] GetBodyBytes() { return _responseBody.ToArray(); }
        public void CloseResponse() { _responseBody.Flush(); }

        public override string GetUriPath() { return _page; }
        public override string GetQueryString() { return _query; }
        public override string GetRawUrl() { return _page + (_query.Length > 0 ? "?" + _query : ""); }
        public override string GetHttpVerbName() { return _verb; }
        public override string GetHttpVersion() { return "HTTP/1.1"; }
        public override string GetRemoteAddress() { return "127.0.0.1"; }
        public override string GetRemoteName() { return "localhost"; }
        public override int GetRemotePort() { return 0; }
        public override string GetLocalAddress() { return "127.0.0.1"; }
        public override int GetLocalPort() { return 8080; }
        public override string GetAppPath() { return _appVirtDir; }
        public override string GetAppPathTranslated() { return _appPhysDir; }
        public override string GetFilePath() { return _page; }
        public override string GetFilePathTranslated()
        {
            string p = _page.Replace('/', '\\');
            if (p.StartsWith("\\")) p = p.Substring(1);
            return Path.Combine(_appPhysDir, p);
        }
        public override string GetPathInfo() { return ""; }
        public override string GetServerName() { return "localhost"; }

        public override byte[] GetPreloadedEntityBody() { return _entityBody; }
        public override bool IsEntireEntityBodyIsPreloaded() { return true; }
        public override int ReadEntityBody(byte[] buffer, int offset, int size) { return 0; }
        public override int GetTotalEntityBodyLength() { return _entityBody != null ? _entityBody.Length : 0; }

        public override string GetServerVariable(string name)
        {
            switch (name)
            {
                case "SERVER_PROTOCOL": return "HTTP/1.1";
                case "SERVER_SOFTWARE": return "SxHost/1.0";
                case "APPL_PHYSICAL_PATH": return _appPhysDir;
                case "APPL_VIRTUAL_PATH": return _appVirtDir;
                case "REMOTE_ADDR": return "127.0.0.1";
                case "REMOTE_HOST": return "localhost";
                case "LOCAL_ADDR": return "127.0.0.1";
                case "SERVER_PORT": return "8080";
                case "HTTP_HOST": return "localhost:8080";
                default: return "";
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            _statusCode = statusCode;
            _statusDesc = statusDescription;
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            string name = HttpWorkerRequest.GetKnownResponseHeaderName(index);
            _responseHeaders[name] = value;
            if (string.Equals(name, "Content-Type", StringComparison.OrdinalIgnoreCase))
                _contentType = value;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            _responseHeaders[name] = value;
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (data != null && length > 0)
                _responseBody.Write(data, 0, length);
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            if (length > 0)
            {
                byte[] buf = new byte[length];
                Marshal.Copy(data, buf, 0, length);
                _responseBody.Write(buf, 0, length);
            }
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (File.Exists(filename))
            {
                byte[] data = File.ReadAllBytes(filename);
                long end = Math.Min(offset + length, data.Length);
                int len = (int)(end - offset);
                if (len > 0)
                    _responseBody.Write(data, (int)offset, len);
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) { }

        public override void FlushResponse(bool finalFlush) { _responseBody.Flush(); }
        public override void EndOfRequest() { _responseBody.Flush(); }

        public override string MapPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath) || virtualPath == "/")
                return _appPhysDir;
            string rel = virtualPath.Replace('/', '\\');
            if (rel.StartsWith("\\")) rel = rel.Substring(1);
            return Path.GetFullPath(Path.Combine(_appPhysDir, rel));
        }

        public override string GetKnownRequestHeader(int index)
        {
            string name = HttpWorkerRequest.GetKnownRequestHeaderName(index);
            if (name != null && _requestHeaders != null && _requestHeaders.ContainsKey(name))
                return _requestHeaders[name] as string;
            if (index == HttpWorkerRequest.HeaderContentType && _entityBody != null && _entityBody.Length > 0)
                return "application/x-www-form-urlencoded";
            if (index == HttpWorkerRequest.HeaderContentLength && _entityBody != null)
                return _entityBody.Length.ToString();
            return null;
        }
        public override string GetUnknownRequestHeader(string name)
        {
            if (_requestHeaders != null && name != null && _requestHeaders.ContainsKey(name))
                return _requestHeaders[name] as string;
            return null;
        }
        public override string[][] GetUnknownRequestHeaders()
        {
            if (_requestHeaders == null) return null;
            var list = new List<string[]>();
            foreach (DictionaryEntry de in _requestHeaders)
                list.Add(new string[] { de.Key.ToString(), de.Value.ToString() });
            return list.ToArray();
        }
    }
}

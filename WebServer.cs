using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PUBGLiteBackendWV
{
    public static class WebServer
    {
        public static TcpListener tcp;
        public static bool _exit = false;
        public static readonly object _sync = new object();
        public static int clientCounter = 0;

        public static void Start()
        {
            Log.WriteLine("WEBS", "Starting server...");
            new Thread(tMain).Start();
        }

        public static void Stop()
        {
            if (tcp != null)
                tcp.Stop();
            lock (_sync)
            {
                _exit = true;
            }
        }

        static void tMain(object obj)
        {
            try
            {
                tcp = new TcpListener(IPAddress.Any, 80);
                tcp.Start();
                Log.WriteLine("WEBS", "Server is listening");
                while (true)
                {
                    lock (_sync)
                    {
                        if (_exit)
                            return;
                    }
                    TcpClient client = tcp.AcceptTcpClient();
                    new Thread(tClient).Start(client);
                }
            }
            catch
            {
                Log.WriteLine("WEBS", "Server stopped");
            }
        }

        static void tClient(object obj)
        {
            int ID = 0;
            try
            {
                lock (_sync)
                {
                    ID = clientCounter++;
                }
                TcpClient client = (TcpClient)obj;
                NetworkStream ns = client.GetStream();
                ns.ReadTimeout = 10000;
                byte[] buff = new byte[4096];
                int readBytes = 0;
                while (readBytes == 0)
                    readBytes = ns.Read(buff, 0, buff.Length);
                string request = Encoding.UTF8.GetString(buff, 0, readBytes);
                ProcessRequest(request, ns, ID);
            }
            catch
            {
                Log.WriteLine("WEBS", "(" + ID + ") Client crashed");
            }
        }

        static void ProcessRequest(string req, Stream s, int ID)
        {
            StringReader sr = new StringReader(req);
            string line = null;
            while((line = sr.ReadLine()) != null)
                if (line.StartsWith("GET"))
                {
                    ProcessGET(line, s, ID);
                    break;
                }
                else if (line.StartsWith("POST"))
                {
                    ProcessPOST(line, s, ID);
                    break;
                }
        }

        static void ProcessPOST(string req, Stream s, int ID)
        {
            Log.WriteLine("WEBS", "(" + ID + ") " + req);
            string file = req.Split(' ')[1];
            if (file.Contains("?"))
                file = file.Split('?')[0];
            file = "fs" + file.Replace("/", "\\");
            if (File.Exists(file))
            {
                DeliverFile(s, file);
                return;
            }
            else
                Log.WriteLine("WEBS", "File not found! " + file);
            string res = "HTTP/1.1 301 Moved Permanently\r\n\r\n";
            byte[] data = Encoding.UTF8.GetBytes(res);
            s.Write(data, 0, data.Length);
            s.Close();
        }

        static void ProcessGET(string req, Stream s, int ID)
        {
            Log.WriteLine("WEBS", "(" + ID + ") " + req);
            string file = req.Split(' ')[1];
            if (file.Contains("?"))
                file = file.Split('?')[0];
            file = "fs" + file.Replace("/", "\\");
            if (File.Exists(file))
            {
                DeliverFile(s, file);
                return;
            }
            else
                Log.WriteLine("WEBS", "File not found! " + file);
            string res = "HTTP/1.1 301 Moved Permanently\r\n\r\n";
            byte[] data = Encoding.UTF8.GetBytes(res);
            s.Write(data, 0, data.Length);
            s.Close();
        }


        static void DeliverFile(Stream s, string file)
        {
            string f = file.ToLower();
            if (f.EndsWith("png"))
            {
                DeliverRaw(s, file, "image/png");
                return;
            }
            if (f.EndsWith("svg"))
            {
                DeliverRaw(s, file, "image/svg+xml");
                return;
            }
            if (f.EndsWith("css"))
            {
                DeliverRaw(s, file, "text/css");
                return;
            }            	
            if (f.EndsWith("html"))
            {
                DeliverRaw(s, file, "text/html; charset=utf-8");
                return;
            }
            if (f.EndsWith("js"))
            {
                DeliverRaw(s, file, "application/javascript; charset=UTF-8");
                return;
            }
            List<string> props;
            switch (file)
            {
                case "fs\\auth\\oidc":
                    props = new List<string>(){
                        "sessionId=s%3Apy_eNWh08LrVTW-Oq4Zp8I5VjVu4iBqQ.8lQ%2B3S%2FaRKNr%2BaCHnrkai%2BpkZIqqXEgSH5haCdFE9cw; Domain=accounts.pubg.com; Path=/; HttpOnly",
                        "Location: /oidc/auth?client_id=26995eff-42b5-4606-8f2f-4fce296ecd2a&prompt=login&redirect_uri=http%3A%2F%2Flocahost%2Fauth%2Foidc%2Fcallback&response_type=code&scope=openid+email+address+created_at&state=2b96d7e5-640f-4f02-a883-9bdd9c93cb1f&ui_locales=de\n\r",
                    };
                    DeliverHttp(s, file, props, 302);
                    break;
                case "fs\\oidc\\auth":
                    props = new List<string>(){
                        "Set-Cookie: _interaction=Bp_slJlpM0Ot-RmNxgczV; path=/oidc/interaction/Bp_slJlpM0Ot-RmNxgczV; expires=Wed, 31 Mar 2021 18:13:35 GMT; samesite=lax; httponly",
                        "Set-Cookie: _interaction.sig=_PqyZedOEk14s5LIF-kJetKvs3o; path=/oidc/interaction/Bp_slJlpM0Ot-RmNxgczV; expires=Wed, 31 Mar 2021 18:13:35 GMT; samesite=lax; httponly",
                        "Set-Cookie: _interaction_resume=Bp_slJlpM0Ot-RmNxgczV; path=/oidc/auth/Bp_slJlpM0Ot-RmNxgczV; expires=Wed, 31 Mar 2021 18:13:35 GMT; samesite=lax; httponly",
                        "Set-Cookie: _interaction_resume.sig=S8Mqk9BySjr2H-_E1f8hIHonXsM; path=/oidc/auth/Bp_slJlpM0Ot-RmNxgczV; expires=Wed, 31 Mar 2021 18:13:35 GMT; samesite=lax; httponly",
                        "Location: /oidc/interaction/login"
                    };
                    DeliverHttp(s, file, props, 302);
                    break;
                case "fs\\oidc\\interaction\\login":
                    props = new List<string>(){
                        "Set-Cookie: _icl_current_language=de; Domain=localhost; Path=/",
                        "Set-Cookie: sessionId=s%3AamKKOl-MtArBy8funNFPreW3V93wsraC.e7WzyghDiJZ85fymc48omULdbfOxZmal%2Fh%2FPNjYwaXo; Domain=localhost; Path=/; HttpOnly",
                        "ETag: W/\"5283-teOcgE0v0glUt71UtwQSmvmBjDQ\"",
                    };
                    DeliverHttp(s, file, props);
                    break;
                case "fs\\auth\\local":
                    props = new List<string>(){
                        "Set-Cookie: XSRF-TOKEN=; Domain=localhost; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT",
                        "Set-Cookie: XSRF-TOKEN=zxzQLZ3d-08AaBs5_b8q-jfQwmChypH5zTQ4; Domain=localhost; Path=/",
                        "Set-Cookie: pubg_logged_in=1; Domain=localhost; Path=/; Secure",
                        "Set-Cookie: _gtmUID=globalaccount.61fd58ba-ad8b-409c-91b8-8ad83ba45b40; Domain=localhost; Path=/",
                        "Set-Cookie: linking-email=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT",
                        "Set-Cookie: oidc-message=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT",
                        "Set-Cookie: sessionId=s%3AamKKOl-MtArBy8funNFPreW3V93wsraC.e7WzyghDiJZ85fymc48omULdbfOxZmal%2Fh%2FPNjYwaXo; Domain=localhost; Path=/; HttpOnly",
                        "Location: /oidc/interaction/login2"
                    };
                    DeliverHttp(s, file, props, 302);
                    break;
                case "fs\\oidc\\interaction\\login2":
                    props = new List<string>(){                        
                        "Set-Cookie: sessionId=s%3AamKKOl-MtArBy8funNFPreW3V93wsraC.e7WzyghDiJZ85fymc48omULdbfOxZmal%2Fh%2FPNjYwaXo; Domain=localhost; Path=/; HttpOnly",
                        "Location: /oidc/auth2",
                    };
                    DeliverHttp(s, file, props, 302);
                    break;
                case "fs\\oidc\\auth2":
                    props = new List<string>(){                        
                        "Set-Cookie: _interaction_resume=; path=/oidc/auth2; expires=Thu, 01 Jan 1970 00:00:00 GMT; samesite=lax; httponly",
                        "Set-Cookie: _interaction_resume.sig=MdIALq1kalYWu6cGCaoCgaemCsw; path=/oidc/auth2; expires=Thu, 01 Jan 1970 00:00:00 GMT; samesite=lax; httponly",
                        "Set-Cookie: _session=Fw8OtZJjhgzbmwhAVaG6Z; path=/; samesite=none; secure; httponly",
                        "Set-Cookie: _session.sig=56JPtY98dGFDZYsejcnkzLWXzWs; path=/; samesite=none; httponly",
                        "Set-Cookie: _session.legacy=Fw8OtZJjhgzbmwhAVaG6Z; path=/; httponly",
                        "Set-Cookie: _session.legacy.sig=tQeGeSj6SrmszG499SEtx04VT-I; path=/; httponly",
                        "Set-Cookie: sessionId=s%3AamKKOl-MtArBy8funNFPreW3V93wsraC.e7WzyghDiJZ85fymc48omULdbfOxZmal%2Fh%2FPNjYwaXo; Domain=localhost; Path=/; HttpOnly",
                        "Location: /auth/callback?code=GL3a0fw3y4Dog5PQ-qdkcvOorBg9_cigUzh00WOr9gK&state=2b96d7e5-640f-4f02-a883-9bdd9c93cb1f",
                    };
                    DeliverHttp(s, file, props, 302);
                    break;
                case "fs\\auth\\callback":
                    props = new List<string>(){                        
                        "Set-Cookie: lpc_sessionId=QFOMF4Z5RS7XELLGFYCQNZVUH5FVOAKFMRZQQAOS7URBGRSVJL3Q; Path=/; Domain=localhost; Max-Age=3600; HttpOnly",
                    };
                    DeliverHttp(s, file, props);
                    break;
                case "fs\\litepc\\verify":
                    props = new List<string>(){                        
                        "Set-Cookie: lpc_sessionId=QFOMF4Z5RS7XELLGFYCQNZVUH5FVOAKFMRZQQAOS7URBGRSVJL3Q; Path=/; Domain=localhost; Max-Age=3600; HttpOnly",
                    };
                    DeliverHttp(s, file, props);
                    break;
                case "fs\\api\\UserService\\gcoin":
                case "fs\\api\\UserService\\verify":
                case "fs\\api\\UserService\\checkIsValidIp":
                case "fs\\api\\CommonService\\getBanners":
                case "fs\\api\\CommonService\\getMaintenance":
                case "fs\\api\\CommonService\\getNotice":
                    DeliverRaw(s, file, "application/json; charset=utf-8");
                    break;
                default:
                    DeliverRaw(s, file);
                    break;
            }
        }
        static void DeliverRaw(Stream s, string file, string type = "binary/octet-stream")
        {
            byte[] data = File.ReadAllBytes(file);
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 200 OK\r\n");
            sb.Append("Content-Type: " + type + "\r\n");
            sb.Append("Content-Length: " + data.Length + "\r\n\r\n");
            MemoryStream m = new MemoryStream();
            byte[] data2 = Encoding.UTF8.GetBytes(sb.ToString());
            m.Write(data2, 0, data2.Length);
            m.Write(data, 0, data.Length);
            byte[] buff = m.ToArray();
            s.Write(buff, 0, buff.Length);
            s.Flush();
            s.Close();
        }

        static void DeliverHttp(Stream s, string file, List<string> extra = null, int code = 200)
        {
            byte[] data = File.ReadAllBytes(file);
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 " + code + " OK\r\n");
            sb.Append("Content-Type: text/html; charset=utf-8\r\n");
            if (extra != null)
                foreach (string line in extra)
                    sb.Append(line + "\r\n");
            sb.Append("Content-Length: " + data.Length + "\r\n\r\n");
            MemoryStream m = new MemoryStream();
            byte[] data2 = Encoding.UTF8.GetBytes(sb.ToString());
            m.Write(data2, 0, data2.Length);
            m.Write(data, 0, data.Length);
            byte[] buff = m.ToArray();
            s.Write(buff, 0, buff.Length);
            s.Flush();
            s.Close();
        }
    }
}

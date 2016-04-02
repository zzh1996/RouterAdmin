using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace RouterAdmin
{
    class Router
    {
        private Cookie cookie;

        public Router()
        {
            string password = System.IO.File.ReadAllText(@"password.txt");
            string auth = System.Uri.EscapeDataString("Basic " + Base64Encode("admin:" + password));
            cookie = new Cookie("Authorization", auth, "/", "192.168.1.1");
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Get(string url)
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create("http://192.168.1.1/" + url);
            rq.CookieContainer = new CookieContainer();
            rq.CookieContainer.Add(cookie);
            rq.Referer = "http://192.168.1.1/";
            rq.Timeout = 5000;
            WebResponse response=null;
            try
            {
                response = rq.GetResponse();
            }
            catch (WebException)
            {
                throw new Exception("连接路由器超时！");
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }

        public List<string> GetClientList()
        {
            string url = "userRpm/AssignedIpAddrListRpm.htm";
            string html = Get(url);
            List<string> results = new List<string>();
            foreach (string line in html.Split('\n'))
            {
                if (line.StartsWith("0")) break;
                if (line.StartsWith("\""))
                {
                    string data = line.Substring(1, line.Length - 3);
                    results.Add(data);
                }
            }
            return results;
        }

        public List<string> GetTraffic()
        {
            string url = "userRpm/SystemStatisticRpm.htm?contType=1&sortType=1&autoRefresh=2&Refresh=%CB%A2+%D0%C2&Num_per_page=100&Goto_page=1";
            string html = Get(url);
            List<string> results = new List<string>();
            foreach (string line in html.Split('\n'))
            {
                if (line.StartsWith("0,0")) break;
                if (!line.StartsWith("<") && !line.StartsWith("v"))
                {
                    foreach (string elem in line.Split(','))
                    {
                        if (elem.StartsWith("\""))
                        {
                            results.Add(elem.Substring(1, elem.Length - 2));
                        }
                        else if (elem != " ")
                        {
                            results.Add(elem);
                        }
                    }
                }
            }
            return results;
        }

        public void LimitOn()
        {
            string url = "userRpm/QoSCfgRpm.htm?QoSCtrl=1&userWanType=0&down_bandWidth=4096&up_bandWidth=512&Save=%B1%A3%20%B4%E6";
            Get(url);
        }

        public void LimitOff()
        {
            string url = "userRpm/QoSCfgRpm.htm?QoSCtrl=0&userWanType=0&down_bandWidth=4096&up_bandWidth=512&Save=%B1%A3%20%B4%E6";
            Get(url);
        }

        public bool GetLimitStatus()
        {
            string url = "userRpm/QoSCfgRpm.htm";
            string html = Get(url);
            return html.Split('\n')[2][0] == '1';
        }

        public void ChangePassword(string phonenum,string password) {
            string url = "userRpm/PPPoECfgRpm.htm?wan=0&wantype=2&acc=" + phonenum + "&psw=" + password + "&confirm=" + password + "&specialDial=0&SecType=0&sta_ip=0.0.0.0&sta_mask=0.0.0.0&linktype=1&waittime=5&Connect=%C1%AC+%BD%D3";
            Get(url);
        }

        public string GetPhone() {
            string url = "userRpm/PPPoECfgRpm.htm";
            string html = Get(url);
            string line=html.Split('\n')[16];
            return line.Substring(1, line.Length - 3);
        }

        public string GetDialStatus() {
            string[] statusstrings=new string[] {
                "未连接",
                "已连接",
                "正在连接...",
                "用户名或密码验证失败",
                "服务器无响应",
                "未知原因失败",
                "WAN口未连接！"
            };
            string url= "userRpm/PPPoECfgRpm.htm";
            string html = Get(url);
            int status = int.Parse(html.Split('\n')[35][0].ToString());
            return statusstrings[status];
        }
    }
}

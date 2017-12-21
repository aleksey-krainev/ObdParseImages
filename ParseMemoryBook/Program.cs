using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ParseMemoryBook
{
    class Program
    {
        static async Task<HtmlNodeCollection> GoGetLink(int page)
        {
            HttpClient hc = new HttpClient();
            
            HttpResponseMessage result = await hc.GetAsync($"https://www.obd-memorial.ru/html/info.htm?id=404048806&page=${page}");

            Stream stream = await result.Content.ReadAsStreamAsync();

            HtmlDocument doc = new HtmlDocument();

            doc.Load(stream);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//img");//the parameter is use xpath see: https://www.w3schools.com/xml/xml_xpath.asp 

            return links;
        }

        private class InfoResponse
        {
            public string entity { get; set; }

            public string header { get; set; }

            public string id { get; set; }

            public string id_page { get; set;}

            public string img { get; set; }

            public string countPages { get; set; }
        }


        static async Task<InfoResponse> GoGetInfo(string id, int page)
        {
            HttpClient hc = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", id),
                new KeyValuePair<string, string>("pagenum", page.ToString()),
                new KeyValuePair<string, string>("callback", ""),

            });

            var result = await hc.PostAsync($"https://www.obd-memorial.ru/memorial/info", content);

            var str = await result.Content.ReadAsStringAsync();

            str = str.TrimStart('(');
            str = str.TrimEnd(')');
            
            var model = str.ParseJson<InfoResponse>(null);
            return model;
        }

        private string CalculateHash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();

            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < hash.Length; j++)
            {
                sb.Append(hash[j].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        static void Main(string[] args)
        {
            string id = "404048806";

            int page = 1;

            Program program = new Program();


            for (page = 1; page < 817; page++)
            {

                Task<InfoResponse> task = GoGetInfo(id, page);

                task.Wait();

                var infoModel = task.Result;

                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(infoModel.img);

                HtmlNodeCollection imgs = doc.DocumentNode.SelectNodes("//img");

                HtmlNode img;
                if (imgs != null)
                {
                    img = imgs.FirstOrDefault();
                }
                else
                {
                    imgs = doc.DocumentNode.SelectNodes("//i");
                    img = imgs.FirstOrDefault();
                }

                var path = img.Attributes.First(x => x.Name == "src").Value;

                var strToCalculateHash = infoModel.id_page +
                                         "db76xdlrtxcxcghn7yusxjcdxsbtq1hnicnaspohh5tzbtgqjixzc5nmhybeh";

                var hash = program.CalculateHash(strToCalculateHash);

                var url =
                    $@"https://www.obd-memorial.ru/memorial/fullimage?id1={hash}&id={infoModel.id_page}&path={path}";


                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(url), $@"c:\temp\ivanovo\{infoModel.id_page}.png");

                }

                Console.WriteLine($"{page}, {infoModel.entity}");
            }
            Console.WriteLine("Hello World!");
        }
    }
}
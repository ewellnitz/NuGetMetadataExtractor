using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Xml;

namespace PackageParser
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument document = new XmlDocument();
            document.Load("input.xml");
            var nodes = document.SelectNodes("//packages/package");

            var client = new HttpClient();

            
            
            foreach (XmlNode node in nodes)
            {
                var id = node.Attributes["id"].Value;
                var version = node.Attributes["version"].Value;
                var url = $"https://api.nuget.org/v3/registration3/{id.ToLower()}/{version}.json";
                var response = client.GetAsync(url).Result;
                var description = "";
                var licenseUrl = "";
                var projectUrl = "";
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.WriteLine($"Can't find {id}");
                }
                else
                {

                    //deserialize response into a JObject
                    var result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    var catalog = result["catalogEntry"];

                    response = client.GetAsync(catalog.ToString()).Result;
                    result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    description = result["description"]?.ToString();
                    licenseUrl = result["licenseUrl"]?.ToString();
                    projectUrl = result["projectUrl"]?.ToString();
                }

                Console.WriteLine($"{id}|{version}|{description}|{licenseUrl}|{projectUrl}".Replace("\r", " ").Replace("\n", " "));
            }
        }
    }
}

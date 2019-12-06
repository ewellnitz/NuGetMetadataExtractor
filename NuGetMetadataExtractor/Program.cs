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
            if (args.Length < 1)
            {
                Console.WriteLine("usage: NuGetMetaDataExtractor <inputFile> [outputFile]");
                return;
            }



            XmlDocument document = new XmlDocument();
            document.Load(args[0]);
            var nodes = document.SelectNodes("//packages/package");

            var client = new HttpClient();
            JArray array = new JArray();

            foreach (XmlNode node in nodes)
            {
                
                var id = node.Attributes["id"].Value;
                var version = node.Attributes["version"].Value;
                //Console.WriteLine($"Processing package {id} {version}...");
                var url = $"https://api.nuget.org/v3/registration3/{id.ToLower()}/{version}.json";
                var response = client.GetAsync(url).Result;
                var description = "";
                var licenseUrl = "";
                var projectUrl = "";
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //Console.WriteLine($"Error: Can't find registration: {url}");
                }
                else
                {

                    //deserialize response into a JObject
                    var result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    var catalog = result["catalogEntry"];

                    response = client.GetAsync(catalog.ToString()).Result;
                    result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    array.Add(result);
                    //Console.WriteLine("\t" + result);
                    description = result["description"]?.ToString();
                    licenseUrl = result["licenseUrl"]?.ToString();
                    projectUrl = result["projectUrl"]?.ToString();
                }
                //Console.WriteLine($"{id}|{version}|{description}|{licenseUrl}|{projectUrl}".Replace("\r", " ").Replace("\n", " "));
            }
            Console.WriteLine(array);

        }
    }
}

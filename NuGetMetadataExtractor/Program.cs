using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;

namespace PackageParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string fileName = args.Length > 0 ? args[0] : "packages.config";

            //Console.WriteLine("usage: NuGetMetaDataExtractor [inputFile]");

            var document = new XmlDocument();
            document.Load(fileName);
            XmlNodeList nodes = document.SelectNodes("//packages/package");

            var client = new HttpClient();
            var array = new JArray();

            Parallel.ForEach(nodes.OfType<XmlNode>(), (node) =>
            {
                string id = node.Attributes["id"].Value;
                string version = node.Attributes["version"].Value;
                //Console.WriteLine($"Processing package {id} {version}...");
                string url = $"https://api.nuget.org/v3/registration3/{id.ToLower()}/{version}.json";
                HttpResponseMessage response = client.GetAsync(url).Result;
                //string description = "";
                //string licenseUrl = "";
                //string projectUrl = "";
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //Console.WriteLine($"Error: Can't find registration: {url}");
                }
                else
                {

                    //deserialize response into a JObject
                    JObject result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    JToken catalog = result["catalogEntry"];

                    response = client.GetAsync(catalog.ToString()).Result;
                    result = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                    lock (array)
                    {
                        array.Add(result);
                    }
                    //Console.WriteLine("\t" + result);
                    //description = result["description"]?.ToString();
                    //licenseUrl = result["licenseUrl"]?.ToString();
                    //projectUrl = result["projectUrl"]?.ToString();
                }
                //Console.WriteLine($"{id}|{version}|{description}|{licenseUrl}|{projectUrl}".Replace("\r", " ").Replace("\n", " "));
            });
            Console.WriteLine(array);
        }
    }
}

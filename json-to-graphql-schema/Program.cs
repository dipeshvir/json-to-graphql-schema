using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Authentication.ExtendedProtection;

namespace json_to_graphql_schema
{
    class Program
    {
        static Dictionary<string, List<string>> schema = new Dictionary<string, List<string>>();
        static string jsonString;
        static JObject data;
        static void Main(string[] args)
        {
            try
            {
                string filePath = "{PATH TO THE JSON FILE}";
                data = JObject.Parse(File.ReadAllText(filePath));

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading the JSON file {e.Message}");
                throw e;
            }

            string output = string.Empty;
            try
            {
                foreach (var x in data)
                {
                    string key = x.Key;
                    JToken jToken = x.Value;
                    string valueType = ProcessValue(key, jToken);
                    if (!schema.ContainsKey("Root"))
                    {
                        schema.Add("Root", new List<string>());
                    }
                    List<string> rootList = schema["Root"];
                    rootList.Add(CreateKeyValPair(key, valueType));
                    schema["Root"] = rootList;

                }

                foreach (var key in schema)
                {
                    output = output + $"type {key.Key} {"{"}" + Environment.NewLine;
                    foreach (var item in key.Value)
                    {

                        output = output + item;
                        output += Environment.NewLine;
                    }
                    output += "}";
                    output += Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while converting json to GraphQl schema {e.Message}");
                throw e;
            }
            Console.WriteLine(output);
        }

        private static void ReadArguments(string[] args)
        {
            if (args.Length == 0) throw new Exception("Invalid Arguments");

        }

        private static string CreateKeyValPair(string key, string valueType)
        {
            return $"{key} : {valueType}";
        }

        private static string ProcessValue(string key, JToken jToken)
        {

            string finalkey = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key.ToLower());
            if (jToken.Type == JTokenType.Integer)
            {
                return "Int";
            }
            else if (jToken.Type == JTokenType.String)
            {
                return "String";
            }
            else if (jToken.Type == JTokenType.Boolean)
            {
                return "Boolean";
            }
            else if (jToken.Type == JTokenType.Float)
            {
                return "Float";
            }
            else if (jToken.Type == JTokenType.Array)
            {
                if (!jToken.HasValues) return $"[{"String"}]";
                string itemType = ProcessValue(key, jToken.First);
                return $"[{itemType}]";
            }
            else if (jToken.Type == JTokenType.Object)
            {
                string stringjToken = JsonConvert.SerializeObject(jToken);
                foreach (var x in JObject.Parse(stringjToken))
                {
                    string k = x.Key;
                    JToken j = x.Value;
                    string valueType = ProcessValue(k, j);
                    
                    if (!schema.ContainsKey(finalkey))
                    {
                        schema.Add(finalkey, new List<string>());
                    }
                    List<string> list = schema[finalkey];
                    list.Add(CreateKeyValPair(k, valueType));
                    schema[finalkey] = list;
                }
                return finalkey;
            }

            return null;
        }
    }
}

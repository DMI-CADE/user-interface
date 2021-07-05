using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dmicade
{
    public class DmicAppData
    {
        public string Name { get; }

        public string DisplayName { get; private set; }
        public string AltDisplayName { get; private set; }
        public SortedDictionary<string, string> Info { get; private set; }
        public string MoreInfoText { get; private set; }
        public string[] GameFormats { get; private set; }
        public string[] DemoVideoUrls { get; private set; }
        
        // Media
        public string LogoPath { get; private set; }
        public string[] ImagePaths { get; private set; }
        public string[] Videos { get; private set; }
        

        public DmicAppData(string name)
        {
            Name = name;
        }

        public void LoadPreviewConfigs(string dmicAppsPath)
        {
            string jsonText =
                File.ReadAllText(Path.Combine(dmicAppsPath, Name, @"PreviewMedia\preview_config.json"));
            
            Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);
            
            // Texts
            DisplayName =    jsonData.ContainsKey("displayName")    ? (string) jsonData["displayName"]    : Name; //TODO prettify default name
            AltDisplayName = jsonData.ContainsKey("altDisplayName") ? (string) jsonData["altDisplayName"] : null;
            MoreInfoText =   jsonData.ContainsKey("moreInfoText")   ? (string) jsonData["moreInfoText"]   : null;

            if (jsonData.ContainsKey("gameFormats"))
            {
                GameFormats = ((JArray) jsonData["gameFormats"]).ToObject<string[]>();
            }

            // Media
            if (jsonData.ContainsKey("media"))
            {
                Dictionary<string, object> mediaData =
                    ((JObject) jsonData["media"]).ToObject<Dictionary<string, object>>();

                foreach (var o in mediaData.Keys)
                {
                    Debug.Log(o);
                }

                LogoPath =   mediaData.ContainsKey("logo")   ? (string)   mediaData["logo"]  : null;
                ImagePaths = mediaData.ContainsKey("images")
                    ? ((JArray) mediaData["images"]).ToObject<string[]>()
                    : null;
                Videos =  mediaData.ContainsKey("videos") 
                    ? ((JArray) mediaData["videos"]).ToObject<string[]>()
                    : null;
            }

            foreach (var s in ImagePaths)
            {
                Debug.Log("---ValueType: " + s.GetType() + "\nValue: " + s);
            }

            foreach (var key in jsonData.Keys)
            {
                Debug.Log("Key: " + key + "\nValueType: " + jsonData[key].GetType() + "\nValue: " + jsonData[key]);
            }
        }

        public void GetLogo()
        {
            
        }
    }
}
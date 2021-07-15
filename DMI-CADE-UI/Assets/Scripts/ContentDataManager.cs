using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Dmicade
{
    public class ContentDataManager : MonoBehaviour
    {
        public static ContentDataManager Instance { get; private set; }
        
        public string DmicAppsProjectRelativePath;
        
        private Dictionary<string, DmicAppData> AppData = new Dictionary<string, DmicAppData>();

        public int AppAmount => AppData.Count;
        public string[] AppNames => AppData.Keys.ToArray();

        /// <summary>Returns app data from the pool.</summary>
        /// <param name="appId">the name of the app.</param>
        /// <returns>The app data.</returns>
        public DmicAppData GetApp(string appId)
        {
            return AppData[appId];
        }

        private void Awake()
        {
            Instance = this;

            //Debug.Log($"datapath: {Application.dataPath}");
            //Debug.Log($"DmicAppsProjectRelativePath '{DmicAppsProjectRelativePath}'");
            // Get path to repo.
            string dmicAppsLocation = Application.dataPath.Replace(@"DMI-CADE-UI/Assets", "");
            if (DmicAppsProjectRelativePath.Length > 0)
            {
                string customPath = Path.Combine(dmicAppsLocation, DmicAppsProjectRelativePath);
                bool customPathIsValid = Directory.Exists(customPath);
                dmicAppsLocation = customPathIsValid ? customPath : Path.Combine(dmicAppsLocation, @"dmic-apps");
                if (customPathIsValid) 
                    Debug.LogWarning($"Using custom DMIC-Apps location: {customPath}");
                else
                    Debug.LogWarning($"Custom DMIC-Apps location not found: {customPath}");
            }
            else
            {
                dmicAppsLocation = Path.Combine(dmicAppsLocation, @"dmic-apps");
            }

            Debug.Log($"Path: {dmicAppsLocation}");
            LoadAppData(dmicAppsLocation);
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="appsLocation"></param>
        private void LoadAppData(string appsLocation)
        {
            string[] appFolders = new string[0];
            
            try
            {
                appFolders = Directory.GetDirectories(appsLocation);
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is DirectoryNotFoundException)
                {
                    Debug.LogWarning("App path not correctly configured, can not load app data:\n" + e);
                }
                else
                    throw;
            }
            
            foreach (string appFolder in appFolders)
            {
                string appName = appFolder.Substring(appFolder.LastIndexOf('\\') + 1);
                try
                {
                    AppData.Add(appName, DmicAppData.CreateFromJson(appName, appsLocation));
                } catch (DirectoryNotFoundException dirNotFoundException)
                {
                    Debug.LogWarning("Could not load: " + appName + 
                                     "\nReason: " + dirNotFoundException.Message);

                    break;
                }

                AppData[appName].LoadLogoSprite(appsLocation); // TODO load async
            }
        }
    }
}

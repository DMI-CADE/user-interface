using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dmicade
{
    public class ContentDataManager : MonoBehaviour
    {
        [SerializeField] private string DmicAppsLocation;
        
        private Dictionary<string, DmicAppData> AppData = new Dictionary<string, DmicAppData>();

        public int AppAmount => AppData.Count;
        public string[] AppNames => AppData.Keys.ToArray();
        

        private void Awake()
        {
            LoadAppData(DmicAppsLocation);
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="appsLocation"></param>
        private void LoadAppData(string appsLocation)
        {
            string[] appFolders = Directory.GetDirectories(appsLocation);
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

                AppData[appName].LoadLogoImage(appsLocation); // TODO load async
                // Debug.Log("Configured: " + AppData[appName].Name);
            }

            //_rawImage = imageObject.GetComponent<RawImage>();
            //_rawImage.texture = AppData["example-app"].logoTexture;
            //imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(_rawImage.texture.width, _rawImage.texture.height);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dmicade
{
    public class ContentDataManager : MonoBehaviour
    {
        
        SortedList<string, DmicAppData> AppData = new SortedList<string, DmicAppData>();

        // Start is called before the first frame update
        void Start()
        {
            
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

                AppData[appName].LoadLogoImage(appsLocation);
                // Debug.Log("Configured: " + AppData[appName].Name);
            }

            //_rawImage = imageObject.GetComponent<RawImage>();
            //_rawImage.texture = AppData["example-app"].logoTexture;
            //imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(_rawImage.texture.width, _rawImage.texture.height);
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DmicAppData GetAppDataByIndex(int index)
        {
            return AppData.Values[index % AppData.Count];
        }
    }
}

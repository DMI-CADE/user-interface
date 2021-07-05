using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Dmicade
{
    public class ContentDataManager : MonoBehaviour
    {
        
        SortedList<string, DmicAppData> AppData = new SortedList<string, DmicAppData>();
        
        
        // Start is called before the first frame update
        void Start()
        {
            LoadAppData(@"C:\Users\BenKr\Documents\DMI-CADE\vm_shared_folder\dmic-apps");
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void LoadAppData(string appsLocation)
        {
            string[] appFolders = Directory.GetDirectories(appsLocation);
            foreach (string appFolder in appFolders)
            {
                string appName = appFolder.Substring(appFolder.LastIndexOf('\\') + 1);
                AppData[appName] = new DmicAppData(appName);
                Debug.Log(appName);
            }

            //DmicAppData appData = new DmicAppData("example-app");
            //appData.LoadPreviewConfigs(appsLocation);

            DmicAppDataSimple testAppData = DmicAppDataSimple.CreateFromJson(appsLocation, "example-app");
            Debug.Log(testAppData);
        }

        public DmicAppData GetAppDataByIndex(int index)
        {
            return AppData.Values[index % AppData.Count];
        }
    }
}

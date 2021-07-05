using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dmicade
{
    [Serializable]
    public class DmicAppDataSimple
    {
        public string name;

        public string displayName;

        public string altDisplayName;

        public string[] gameFormats;
        
        // Media
        public string logo;
        public string[] images;
        public string[] videos;
        
        //Info
        public string genre;
        public string developer;
        public string publisher;
        public string release;

        public string moreInfoText;
        
        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="dmicAppsLocation"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DmicAppDataSimple CreateFromJson(string dmicAppsLocation, string name)
        {
            string jsonText =
                File.ReadAllText(Path.Combine(dmicAppsLocation, name, @"PreviewMedia\preview_config_simple.json"));

            DmicAppDataSimple newAppData = JsonUtility.FromJson<DmicAppDataSimple>(jsonText);
            newAppData.name = name;
            
            return newAppData;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Name: " + name +
                   "\nDisplayName: " + displayName +
                   "\nAltDisplayName: " + altDisplayName +
                   "\nGameFormats: [" + String.Join(", ", gameFormats) + "]" +
                   "\nLogo path: " + logo + 
                   "\nImages: [" + String.Join(", ", images) + "]" +
                   "\nVideos: [" + String.Join(", ", videos) + "]" +
                   "\nGenre: " + genre + 
                   "\nDeveloper: " + developer + 
                   "\nPublisher: " + publisher + 
                   "\nRelease: " + release + 
                   "\nMoreInfoText: " +  moreInfoText;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <returns></returns>
        public string GetRandomVideo()
        {
            int videoCnt = videos.Length;
            string randomVideo = "";

            if (videoCnt > 0)
            {
                randomVideo = videos[Random.Range(0, videoCnt)];
            }

            return randomVideo;
        }
    }
}

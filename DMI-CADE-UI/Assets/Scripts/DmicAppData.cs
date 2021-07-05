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
    public class DmicAppData
    {
        public string Name { get; private set; }

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

        public Texture2D logoTexture = new Texture2D(2, 2);

        /// <summary>
        /// Creates a new DmicAppData instance with values from its preview config.</summary>
        /// <para>Requires an correctly configured preview config in the apps folder under:
        /// <paramref name="dmicAppsLocation"/>/<paramref name="name"/>/PreviewMedia/preview_config.json</para>
        /// <para>Uses Unity's JsonUtility for serialization from the json file:
        /// <seealso cref="https://docs.unity3d.com/Manual/JSONSerialization.html"/></para>
        /// <param name="name">the apps name.</param>
        /// <param name="dmicAppsLocation">the dmic apps folder location.</param>
        /// <returns></returns>
        public static DmicAppData CreateFromJson(string name, string dmicAppsLocation)
        {
            string jsonText =
                File.ReadAllText(Path.Combine(dmicAppsLocation, name, @"PreviewMedia/preview_config.json"));

            DmicAppData newAppData = JsonUtility.FromJson<DmicAppData>(jsonText);
            newAppData.Name = name;
            
            return newAppData;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="appsLocation"></param>
        /// <param name="imageName"></param>
        private Texture2D LoadImage(string appsLocation, string imageName)
        {
            string imagePath = Path.Combine(appsLocation, Name, "PreviewMedia", imageName);
            Texture2D tex2D = new Texture2D(1, 1);

            byte[] byteArray = File.ReadAllBytes(imagePath);
            bool isLoaded = tex2D.LoadImage(byteArray);
            if (!isLoaded)
            {
                Debug.LogWarning("Could not load image: " + imagePath);
            }

            return tex2D;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="appsLocation"></param>
        public void LoadLogoImage(string appsLocation)
        {
            if (logo == null) return;

            logoTexture = LoadImage(appsLocation, logo);
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

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Name: " + Name +
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
    }
}

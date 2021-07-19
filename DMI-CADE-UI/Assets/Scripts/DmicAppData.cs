using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dmicade
{
    public enum GameMode { None, SingleP, Coop, SingleAndCoop, Vs, VsCpu }
    
    [Serializable]
    public class DmicAppData
    {
        public string Name { get; private set; }

        public string displayName;
        public string altDisplayName;
        public string[] gameFormats = new string[0];
        public GameMode[] parsedGameModes = new GameMode[0];

        // Media
        public string logo;
        public string[] images = new string[0];
        public Sprite[] previewSprites = new Sprite[0];
        public string[] videos = new string[0];
        
        //Info
        public string genre;
        public string developer;
        public string publisher;
        public string release;

        public string moreInfoText;

        public string infoFormatted;
        public string descriptorFormatted;
        
        //Images
        public Sprite logoSprite;
        private const int PixelsPerUnit = 100;
        //private Texture2D _logoTexture;

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
            
            newAppData.SetVideoPaths(dmicAppsLocation);
            newAppData.SetGameModes();
            
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
        public void LoadLogoSprite(string appsLocation)
        {
            if (logo == null) return;

            var logoTexture = LoadImage(appsLocation, logo);
            
            logoSprite = Sprite.Create(logoTexture,
                new Rect(0, 0, logoTexture.width, logoTexture.height), 
                new Vector2(0, 0), 
                PixelsPerUnit);
        }

        public void LoadPreviewImages(string appsLocation)
        {
            List<Sprite> sprites = new List<Sprite>();
            
            foreach (string imageName in images)
            {
                try
                {
                    var imageTexture = LoadImage(appsLocation, imageName);
                    if (imageTexture == null) continue;
                    
                    var newSprite = Sprite.Create(imageTexture,
                        new Rect(0, 0, imageTexture.width, imageTexture.height), 
                        new Vector2(0, 0), 
                        PixelsPerUnit);
                    
                    sprites.Add(newSprite);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e);
                }
            }

            previewSprites = sprites.ToArray();
        }

        /// Use only when <see cref="Name"/> is set correctly. TODO doc
        public void SetVideoPaths(string dmicAppsLocation)
        {
            if (videos == null) return;
            
            ArrayList videoUris = new ArrayList();
            for (int i = 0; i < videos.Length; i++)
            {
                if (videos[i].Length != 0)
                    videoUris.Add(Path.Combine(dmicAppsLocation, Name, @"PreviewMedia", videos[i]));
            }

            videos = (string[]) videoUris.ToArray(typeof(string));
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

        public void SetDescriptorFormatted(string desc) => descriptorFormatted = desc;
        public void SetInfoFormatted(string info) => infoFormatted = info;

        public void SetGameModes()
        {
            List<GameMode> modes = new List<GameMode>(1);
            
            foreach (var modeStr in gameFormats)
            {
                switch (modeStr)
                {
                    case "1p":
                        if(gameFormats.Contains("coop"))
                            modes.Add(GameMode.SingleAndCoop);
                        else
                            modes.Add(GameMode.SingleP);
                        break;
                    case "coop":
                        if(!gameFormats.Contains("1p"))
                            modes.Add(GameMode.Coop);
                        break;
                    case "vs":
                        modes.Add(GameMode.Vs);
                        break;
                    case "vs_cpu":
                        modes.Add(GameMode.VsCpu);
                        break;
                    
                    default:
                        Debug.LogWarning($"Could not recognize game mode: {modeStr}");
                        break;
                }
            }

            parsedGameModes = modes.ToArray();
        }
        
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

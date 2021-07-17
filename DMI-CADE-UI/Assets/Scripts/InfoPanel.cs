using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Dmicade
{
    public class InfoPanel : MonoBehaviour
    {
        public TextMeshProUGUI headerTextSingleLine;
        public VideoPlayer previewVideoPlayer;
        public RawImage videoDisplay;
        public TextMeshProUGUI textInfoDescriptor;
        public TextMeshProUGUI textInfoData;
        public ModeInfo modeInfo; 
        
        public int availableDataCharacters = 17;

        ///<summary>Updates the header of the info panel.</summary> TODO doc
        public void SetTitle(string title, string altTitle=null)
        {
            if (altTitle == null)
            {
                headerTextSingleLine.text = title.ToUpper();
            }
        }

        public void SetVideoUri(string videoUri)
        { 
            previewVideoPlayer.Stop();
            previewVideoPlayer.url = videoUri;
        }

        public void PlayVideo() => previewVideoPlayer.Play();

        public void StopVideo() => previewVideoPlayer.Stop();

        public void SetGameModes(GameMode[] modeConfig) => modeInfo.UpdateModeInfo(modeConfig);
        
        public void ShowNextGameMode() => modeInfo.ShowNextMode();

        ///<summary>Updates the info displayed in the bottom panel.</summary> TODO doc
        public void SetBaseInfo(DmicAppData app)
        {
            StringBuilder descriptorSb = new StringBuilder(app.descriptorFormatted);
            StringBuilder infoDataSb = new StringBuilder(app.infoFormatted);
            
            if (app.descriptorFormatted == null || app.descriptorFormatted == null)
            {
                string[] descriptor = {"Genre:", "Developer:", "Publisher:", "Release:"};
                string[] infoData = {app.genre, app.developer, app.publisher, app.release};
                for (int i = 0; i < descriptor.Length; i++)
                {
                    if (infoData[i] == null) continue;
                    
                    if (infoData[i].Length > availableDataCharacters)
                    {
                        // TODO accurate/word based line break counting
                        //Debug.Log($"{descriptor[i]}: {infoData[i].Length / availableDataCharacters + 1}");
                        descriptorSb.Append(descriptor[i]+"\n");
                        descriptorSb.Insert(descriptorSb.Length - 1, "\n", infoData[i].Length / availableDataCharacters);
                        
                        infoDataSb.AppendFormat("{0}\n", infoData[i]);
                    }
                    else
                    {
                        descriptorSb.AppendFormat("{0}\n", descriptor[i]);
                        infoDataSb.AppendFormat("{0}\n", infoData[i]);
                    }
                }

                // Cache formatted strings in data object.
                app.SetDescriptorFormatted(descriptorSb.ToString());
                app.SetInfoFormatted(infoDataSb.ToString());
            }

            textInfoDescriptor.text = app.descriptorFormatted;
            textInfoData.text = app.infoFormatted;
        }
    }
}

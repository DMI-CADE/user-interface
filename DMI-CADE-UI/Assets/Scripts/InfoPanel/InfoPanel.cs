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
        public GameObject videoDisplay;
        public RectTransform previewImage;
        public GameObject noMediaAvailableIndicator;
        public TextMeshProUGUI textInfoDescriptor;
        public TextMeshProUGUI textInfoData;
        public ModeInfo modeInfo;
        public BorderedContainer moreInfoIndicator;
        
        public int availableDescriptorCharacters = 10;
        public int availableDataCharacters = 16;

        void Awake()
        {
            previewImage.gameObject.GetComponent<Image>().preserveAspect = true;
        }

        ///<summary>Updates the header of the info panel.</summary> TODO doc
        public void SetTitle(string title, string altTitle=null)
        {
            if (altTitle == null)
            {
                headerTextSingleLine.text = title.ToUpper();
            }
        }

        public void ShowNoMediaAvailable() => noMediaAvailableIndicator.SetActive(true);
        
        public void HideNoMediaAvailable() => noMediaAvailableIndicator.SetActive(false);

        public void ShowPreviewImage() => previewImage.gameObject.SetActive(true);

        public void HidePreviewImage() => previewImage.gameObject.SetActive(false);

        public void SetVideoUri(string videoUri)
        { 
            previewVideoPlayer.Stop();
            previewVideoPlayer.url = videoUri;
        }

        public void PlayVideo()
        {
            videoDisplay.SetActive(true);
            previewVideoPlayer.Play();
        }

        public void StopVideo()
        {
            videoDisplay.SetActive(false);
            previewVideoPlayer.Stop();
        }

        public void SetGameModes(GameMode[] modeConfig) => modeInfo.UpdateModeInfo(modeConfig);
        
        public void ShowNextGameMode() => modeInfo.ShowNextMode();

        public void DeactivateGameModes() => modeInfo.DeactivateAllModes();

        public void ShowMoreInfoIndicator() => moreInfoIndicator.Open();

        public void HideMoreInfoIndicator() => moreInfoIndicator.Close();

        ///<summary>Updates the info displayed in the bottom panel.</summary> TODO doc
        public void SetBaseInfo(DmicAppData app)
        {
            StringBuilder descriptorSb = new StringBuilder(app.descriptorFormatted);
            StringBuilder infoDataSb = new StringBuilder(app.infoFormatted);

            if (app.descriptorFormatted == null || app.infoFormatted == null)
            {
                string[] descriptors = {app.info1[0], app.info2[0], app.info3[0], app.info4[0]};
                string[] infoData = {app.info1[1], app.info2[1], app.info3[1], app.info4[1]};

                for (int i = 0; i < descriptors.Length; i++)
                {
                    if (descriptors[i] == null || descriptors[i].Length == 0 || infoData[i] == null || infoData[i].Length == 0)
                        continue;

                    // Cut of descriptors if to long and add ':' if missing.
                    descriptors[i] = descriptors[i].Length - 1 <= availableDescriptorCharacters ? descriptors[i] : descriptors[i].Substring(0, availableDescriptorCharacters);
                    descriptors[i] = descriptors[i][descriptors[i].Length - 1] == ':' ? descriptors[i] : descriptors[i] + ':';

                    //Debug.Log($"{descriptors[i]}: {infoData[i].Length / availableDataCharacters}");
                    int approximateAdditionalLinebreaks = infoData[i].Length / availableDataCharacters;
                    descriptorSb.Append(descriptors[i]+"\n");
                    descriptorSb.Insert(descriptorSb.Length - 1, "\n", approximateAdditionalLinebreaks);

                    infoDataSb.AppendFormat("{0}\n", infoData[i]);
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

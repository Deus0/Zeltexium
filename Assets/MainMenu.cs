using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

namespace Zeltex.Guis
{
    /// <summary>
    /// The gui for main menu
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public Button AdsButton;
        public string placementId = "rewardedVideo";
#if UNITY_IOS
        private string gameId = "1648973";
#elif UNITY_ANDROID
        private string gameId = "1648973";
#endif

        public void OnEnable()
        {
            GameManager.Get().EndGame();    // incase it was canceled.
#if !(UNITY_ANDROID || UNITY_EDITOR)
            AdsButton.gameObject.SetActive(false);
#endif
        }

        public void PushAdventureButton()
        {
            GameManager.Get().BeginGame();
        }

        public void PushResourcesButton()
        {
            GameManager.Get().BeginResourcesEditing();
        }

        public void PushSettingsButton()
        {
            GameManager.Get().BeginSettings();
        }

        public void ShowAds()
        {
            ShowOptions options = new ShowOptions();
            options.resultCallback = HandleShowResult;

            Advertisement.Show(placementId, options);
        }

        void HandleShowResult(ShowResult result)
        {
            if (result == ShowResult.Finished)
            {
                Debug.Log("Video completed - Offer a reward to the player");

            }
            else if (result == ShowResult.Skipped)
            {
                Debug.LogWarning("Video was skipped - Do NOT reward the player");

            }
            else if (result == ShowResult.Failed)
            {
                Debug.LogError("Video failed to show");
            }
        }
    }

}
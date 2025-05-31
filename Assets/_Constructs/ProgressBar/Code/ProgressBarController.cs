using System;
using Anarchy.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.ProgressBar
{
    public class ProgressBarController : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;

        private void OnEnable()
        {
            ConstructBindings.Send_ProgressBarData_ShowProgressBar?.AddListener(OnShowProgressBar);
            ConstructBindings.Send_ProgressBarData_CloseProgressBar?.AddListener(OnCloseProgressBar);
            ConstructBindings.Send_ProgressBarData_UpdateProgress?.AddListener(OnUpdateProgress);
        }

        void OnDisable()
        {
            ConstructBindings.Send_ProgressBarData_ShowProgressBar?.RemoveListener(OnShowProgressBar);
            ConstructBindings.Send_ProgressBarData_CloseProgressBar?.RemoveListener(OnCloseProgressBar);
            ConstructBindings.Send_ProgressBarData_UpdateProgress?.RemoveListener(OnUpdateProgress);
        }

        private void OnShowProgressBar(int startingValue, int maxValue)
        {
            progressBar.value =  startingValue;
            progressBar.maxValue = maxValue;
            UpdateText();
            container.SetActive(true);
        }
        
        private void OnCloseProgressBar()
        {
            container.SetActive(false);
        }
        
        private void OnUpdateProgress(int updateValue)
        {
            progressBar.value =  updateValue;
            container.SetActive(true);
            UpdateText();
        }

        private void UpdateText()
        {
            var percentage = (progressBar.value / progressBar.maxValue) * 100;
            progressText.text = $"{percentage:N0}%";
        }
    }
}

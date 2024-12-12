using System;
using NameChangeSimulator.Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Shared
{
    public class Keyword : MonoBehaviour
    {
        public KeywordType keywordType = KeywordType.None;

        [SerializeField] private TMP_Text inputField;
        [SerializeField] private Image checkField;
        
        private RectTransform _rectTransform;
        
        private void OnValidate()
        {
            gameObject.name = keywordType.ToString();
        }

        private void OnEnable()
        {
            if (TryGetComponent(out TMP_Text foundField))
            {
                inputField = foundField;
            }

            if (TryGetComponent(out Image foundCheckField))
            {
                checkField = foundCheckField;
            }
        }

        public void Submit(string text)
        {
            if (inputField != null)
            {
                inputField.text = text;
            }
        }

        public void Submit(bool check)
        {
            if (checkField != null)
            {
                checkField.enabled = check;
            }
        }
    }
}

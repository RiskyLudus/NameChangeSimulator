using Anarchy.Shared;
using NameChangeSimulator.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NameChangeSimulator.Constructs.Dialogue.InputBox
{
    public class NameInputBoxController : MonoBehaviour
    {
        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private static readonly int CloseTrigger = Animator.StringToHash("Close");

		[SerializeField] private string formKeyword = "Dead Name Input"; // or "New Name Input"
		[SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputField firstNameInputField, middleNameInputField, lastNameInputField;
        [SerializeField] private Animator nameInputAnimator;
        [SerializeField] private Animator[] animators;
        [SerializeField] private string triggerName;
        [SerializeField] private Sprite textureSwapImage;
        [SerializeField] private GameObject button;

        private bool _goToNextRunning = false; // I hate doing stuff like this for animation control but ah well -Risky
        private bool _open = false;
        
        public void DisplayNameInputWindow()
        {
            Debug.Log("<color=lightblue>[INPUT]</color>Showing name input window");
            ResetBox();
            nameInputAnimator.SetTrigger(OpenTrigger);
        }

		public void SubmitInput() {
			// Guard: need first & last to proceed
			if (string.IsNullOrEmpty(firstNameInputField.text) || string.IsNullOrEmpty(lastNameInputField.text)) {
				Debug.LogWarning($"[NAME_UI][SUBMIT] blocked — missing required fields. first='{firstNameInputField.text}', last='{lastNameInputField.text}'");
				return;
			}

			string payload = $"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}";
			Debug.Log($"<color=lightblue>[SUBMIT]</color> name input keyword='{formKeyword}' payload='{payload}'");

			// Push to the form filler immediately (we also resend in GoToNext as a belt-and-suspenders)
			ConstructBindings.Send_FormDataFillerData_Submit?.Invoke(formKeyword, payload);

			button.SetActive(false);
			AudioManager.Instance.PlayUIConfirm_SFX();
			var ph = middleNameInputField.placeholder != null ? middleNameInputField.placeholder.GetComponent<TMP_Text>() : null;
			if (ph != null)
				ph.enabled = false;

			foreach (var animator in animators) {
				animator.SetTrigger(triggerName);
				ConstructBindings.Send_ScreenBlockerData_ToggleScreenBlocker?.Invoke(true);
			}
		}


		public void GoToNext() {
			if (_goToNextRunning)
				return;
			_goToNextRunning = true;

			string payload = $"{firstNameInputField.text}~{middleNameInputField.text}~{lastNameInputField.text}";
			Debug.Log($"[NAME_UI][NEXT] keyword='{formKeyword}' payload='{payload}'");

			// Resend to ensure the very latest text reaches the filler
			ConstructBindings.Send_FormDataFillerData_Submit?.Invoke(formKeyword, payload);

			// Advance the dialogue flow (you were already passing the payload here)
			dialogueController.GoToNext(payload);

			ConstructBindings.Send_ScreenBlockerData_ToggleScreenBlocker?.Invoke(false);
			nameInputAnimator.SetTrigger(CloseTrigger);
		}


		public void Close()
        {
            if (_open)
            {
                nameInputAnimator.SetTrigger(CloseTrigger);
            }
        }

        public void ResetBox()
        {
            firstNameInputField.gameObject.SetActive(true);
            middleNameInputField.gameObject.SetActive(true);
            lastNameInputField.gameObject.SetActive(true);
            button.SetActive(true);
            _goToNextRunning = false;
            firstNameInputField.text = string.Empty;
            middleNameInputField.text = string.Empty;
            lastNameInputField.text = string.Empty;
        }

        public void TurnOffInputFields()
        {
            firstNameInputField.gameObject.SetActive(false);
            middleNameInputField.gameObject.SetActive(false);
            lastNameInputField.gameObject.SetActive(false);
        }

        public void ChangeTexture()
        {
            firstNameInputField.GetComponent<TMP_InputField>().enabled = false;
            firstNameInputField.GetComponent<Image>().sprite = textureSwapImage;
            
            middleNameInputField.GetComponent<TMP_InputField>().enabled = false;
            middleNameInputField.GetComponent<Image>().sprite = textureSwapImage;
            
            lastNameInputField.GetComponent<TMP_InputField>().enabled = false;
            lastNameInputField.GetComponent<Image>().sprite = textureSwapImage;
        }
        
        public void ToggleOpenStateOn()
        {
            _open = true;
        }

        public void ToggleOpenStateOff()
        {
            _open = false;
        }
    }
}

using NameChangeSimulator.Constructs.Dialogue.InputBox;
using NameChangeSimulator.Shared;
using UnityEngine;
using UnityEngine.UI;

public class NewNameGlowAnimationEventController : MonoBehaviour
{
    public NameInputBoxController nameInputBoxController;

    public void PlayGlowSound()
    {
        AudioManager.Instance.PlayGlow_SFX();
    }
    
    public void OnGlowComplete()
    {
        nameInputBoxController.GoToNext();
    }

    public void ChangeToGlowTexture()
    {
        nameInputBoxController.ChangeTexture();
    }
}

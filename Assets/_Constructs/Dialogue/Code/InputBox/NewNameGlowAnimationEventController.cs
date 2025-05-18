using NameChangeSimulator.Constructs.Dialogue.InputBox;
using NameChangeSimulator.Shared;
using UnityEngine;

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
}

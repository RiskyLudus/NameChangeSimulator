using NameChangeSimulator.Constructs.Dialogue.InputBox;
using NameChangeSimulator.Shared;
using UnityEngine;

public class DeadNameBurnAnimationEventController : MonoBehaviour
{
    public NameInputBoxController nameInputBoxController;

    public void PlayBurnSound()
    {
        AudioManager.Instance.PlayBurn_SFX();
    }
    
    public void OnBurnComplete()
    {
        nameInputBoxController.GoToNext();
    }

    public void BurnAwayTextFields()
    {
        nameInputBoxController.TurnOffInputFields();
    }
}
using UnityEngine;

namespace NameChangeSimulator.Constructs.Credits
{
    public class CreditsController : MonoBehaviour
    {
        [SerializeField] private GameObject container;

        public void Show()
        {
            container.SetActive(true);
        }

        public void Close()
        {
            container.SetActive(false);
        }

        public void OpenSocialLink(string link)
        {
            Application.OpenURL(link);
        }
    }
}

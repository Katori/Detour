using UnityEngine;
using UnityEngine.UI;

namespace Detour.Examples.Client
{
    public class UIController : MonoBehaviour
    {
        internal static UIController Instance { get; private set; }

        [SerializeField]
        private GameObject ConnectionPanel;

        [SerializeField]
        private InputField NameInput;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Connect()
        {
            DetourExampleConnection.Instance.Connect(NameInput.text);
        }

        internal void HideConnectionUI()
        {
            ConnectionPanel.SetActive(false);
        }

        internal void ShowConnectionUI()
        {
            ConnectionPanel.SetActive(true);
        }
    }
}
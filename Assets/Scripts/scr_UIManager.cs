using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class scr_UIManager : NetworkBehaviour
{

    [SerializeField] public TMP_InputField nombreInputField;

    [SerializeField] private TextMeshProUGUI gameInfoTexto;

    [SerializeField] Button hostButton;

    [SerializeField] Button clientButton;

    [SerializeField] GameObject menu;

    void Start()
    {
        hostButton.onClick.AddListener(hostFuncion);
        clientButton.onClick.AddListener(clienteFuncion);
    }

    void hostFuncion()
    {
        NetworkManager.Singleton.StartHost();
        menu.SetActive(false);
        gameInfoTexto.gameObject.SetActive(true);
        gameInfoTexto.text = "Press enter to start";
    }

    void clienteFuncion()
    {
        NetworkManager.Singleton.StartClient();
        menu.SetActive(false);
        gameInfoTexto.gameObject.SetActive(true);
        gameInfoTexto.text = "Waiting for host...";
    }

}

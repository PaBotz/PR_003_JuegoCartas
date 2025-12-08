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

    [Header("Config del Mapa")]
    [SerializeField] private TMP_InputField anchoInput;
    [SerializeField] private TMP_InputField altoInput;
    [SerializeField] private GameObject configMapaPanel;

    private scr_Generator generator;

    void Start()
    {
        hostButton.onClick.AddListener(hostFuncion);
        clientButton.onClick.AddListener(clienteFuncion);

        generator = FindFirstObjectByType<scr_Generator>();

        if (configMapaPanel != null)
            configMapaPanel.SetActive(false);
    }

    void hostFuncion()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Ya estás conectado. No puedes crear otro host.");
            MostrarMensajeError("Ya estás conectado a una sesión");
            return;
        }

        bool logroIniciar = NetworkManager.Singleton.StartHost();

        if (logroIniciar)
        {
            menu.SetActive(false);
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = "Esperando jugadores...\nConfigura el mapa y presiona ENTER para comenzar";

            configMapaPanel.SetActive(true);

            anchoInput.text = generator.mapSize.x.ToString();
            altoInput.text = generator.mapSize.y.ToString();

            anchoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
            altoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
        }
    } // ✅ Cerrar hostFuncion aquí

    void clienteFuncion()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Ya estás conectado.");
            MostrarMensajeError("Ya estás conectado a una sesión");
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        bool success = NetworkManager.Singleton.StartClient();

        if (success)
        {
            menu.SetActive(false);
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = "Conectado! Esperando que el host inicie...";
        }
    }

    void ActualizarTamanoMapa(string valor)
    {
        if (anchoInput != null && altoInput != null && generator != null)
        {
            if (float.TryParse(anchoInput.text, out float ancho) &&
                float.TryParse(altoInput.text, out float alto))
            {
                if (ancho > 0 && alto > 0 && ancho % 2 == 0 && alto % 2 == 0)
                {
                    generator.CambiarTamanoMapa(ancho, alto);
                }
            }
        }
    }

    public void OcultarConfigPanel()
    {
        configMapaPanel.SetActive(false);
        gameInfoTexto.text = "¡Partida iniciada! Busca las parejas";
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;

            menu.SetActive(true);
            gameInfoTexto.gameObject.SetActive(false);
            if (configMapaPanel != null)
                configMapaPanel.SetActive(false);
            MostrarMensajeError("Desconectado del servidor");
        }
    }

    private void MostrarMensajeError(string mensaje)
    {
        gameInfoTexto.gameObject.SetActive(true);
        gameInfoTexto.text = mensaje;
        gameInfoTexto.color = Color.red;

        Invoke(nameof(OcultarMensajeError), 3f);
    }

    private void OcultarMensajeError()
    {
        if (menu.activeSelf)
        {
            gameInfoTexto.gameObject.SetActive(false);
            gameInfoTexto.color = Color.white;
        }
    }
}
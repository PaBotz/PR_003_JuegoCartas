using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la interfaz de usuario.
/// NO es NetworkBehaviour - la UI es local para cada cliente.
/// </summary>
public class scr_UIManager : MonoBehaviour
{
    [Header("Menú Principal")]
    [SerializeField] public TMP_InputField nombreInputField;
    [SerializeField] private TextMeshProUGUI gameInfoTexto;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject menu;

    [Header("Config del Mapa")]
    [SerializeField] private TMP_InputField anchoInput;
    [SerializeField] private TMP_InputField altoInput;
    [SerializeField] private GameObject configMapaPanel;

    private scr_Generator generator;

    void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(HostFuncion);
        if (clientButton != null) clientButton.onClick.AddListener(ClienteFuncion);

        generator = FindFirstObjectByType<scr_Generator>();

        if (configMapaPanel != null)
            configMapaPanel.SetActive(false);
    }

    void HostFuncion()
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

            if (gameInfoTexto != null)
            {
                gameInfoTexto.gameObject.SetActive(true);
                gameInfoTexto.text = "Esperando jugadores...\nConfigura el mapa y presiona ENTER para comenzar";
            }

            if (configMapaPanel != null)
            {
                configMapaPanel.SetActive(true);

                if (generator != null)
                {
                    if (anchoInput != null) anchoInput.text = generator.mapSize.x.ToString();
                    if (altoInput != null) altoInput.text = generator.mapSize.y.ToString();
                }

                if (anchoInput != null) anchoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
                if (altoInput != null) altoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
            }
        }
    }

    void ClienteFuncion()
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

            if (gameInfoTexto != null)
            {
                gameInfoTexto.gameObject.SetActive(true);
                gameInfoTexto.text = "Conectado! Esperando que el host inicie...";
            }
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

    /// <summary>
    /// Llamado por el Generator cuando inicia la partida.
    /// </summary>
    public void OcultarConfigPanel()
    {
        if (configMapaPanel != null) configMapaPanel.SetActive(false);
        if (gameInfoTexto != null) gameInfoTexto.text = "¡Partida iniciada! Busca las parejas";
    }

    /// <summary>
    /// Llamado por el Generator via ClientRpc.
    /// </summary>
    public void MostrarPartidaIniciada()
    {
        if (configMapaPanel != null) configMapaPanel.SetActive(false);
        if (gameInfoTexto != null)
        {
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = "¡Partida iniciada! Busca las parejas";
            gameInfoTexto.color = Color.white;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;

            if (menu != null) menu.SetActive(true);
            if (gameInfoTexto != null) gameInfoTexto.gameObject.SetActive(false);
            if (configMapaPanel != null) configMapaPanel.SetActive(false);

            MostrarMensajeError("Desconectado del servidor");
        }
    }

    private void MostrarMensajeError(string mensaje)
    {
        if (gameInfoTexto != null)
        {
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = mensaje;
            gameInfoTexto.color = Color.red;
            Invoke(nameof(OcultarMensajeError), 3f);
        }
    }

    private void OcultarMensajeError()
    {
        if (menu != null && menu.activeSelf && gameInfoTexto != null)
        {
            gameInfoTexto.gameObject.SetActive(false);
            gameInfoTexto.color = Color.white;
        }
    }
}
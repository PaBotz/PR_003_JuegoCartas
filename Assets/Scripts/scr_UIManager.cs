using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja toda la interfaz de usuario: menú principal, configuración,
/// display de puntajes durante el juego, y pantalla de victoria.
/// </summary>
public class scr_UIManager : NetworkBehaviour
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

    [Header("HUD del Juego")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI puntajesTexto;
    [SerializeField] private TextMeshProUGUI paresTexto;

    [Header("Pantalla de Victoria")]
    [SerializeField] private GameObject victoriaPanel;
    [SerializeField] private TextMeshProUGUI victoriaTexto;
    [SerializeField] private TextMeshProUGUI resultadosTexto;
    [SerializeField] private Button volverMenuButton;
    [SerializeField] private Button reiniciarButton;

    // Referencias
    private scr_Generator generator;
    private scr_ScoreSystem scoreSystem;
    private scr_MatchManager matchManager;

    // Cache de nombres de jugadores
    private Dictionary<ulong, string> nombresJugadores = new Dictionary<ulong, string>();

    void Start()
    {
        // Configurar botones del menú
        if (hostButton != null) hostButton.onClick.AddListener(HostFuncion);
        if (clientButton != null) clientButton.onClick.AddListener(ClienteFuncion);
        if (volverMenuButton != null) volverMenuButton.onClick.AddListener(VolverAlMenu);
        if (reiniciarButton != null) reiniciarButton.onClick.AddListener(ReiniciarPartida);

        // Buscar referencias
        generator = FindFirstObjectByType<scr_Generator>();

        // Estado inicial de paneles
        if (configMapaPanel != null) configMapaPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoriaPanel != null) victoriaPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Buscar y suscribirse al ScoreSystem
        scoreSystem = scr_ScoreSystem.Instance;
        if (scoreSystem == null)
        {
            scoreSystem = FindFirstObjectByType<scr_ScoreSystem>();
        }

        if (scoreSystem != null)
        {
            scoreSystem.OnPuntajeCambiado += OnPuntajeCambiado;
            scoreSystem.OnJuegoTerminado += OnJuegoTerminado;
        }

        // Buscar MatchManager
        matchManager = FindFirstObjectByType<scr_MatchManager>();
        if (matchManager != null)
        {
            matchManager.OnPartidaTerminada += OnPartidaTerminada;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (scoreSystem != null)
        {
            scoreSystem.OnPuntajeCambiado -= OnPuntajeCambiado;
            scoreSystem.OnJuegoTerminado -= OnJuegoTerminado;
        }

        if (matchManager != null)
        {
            matchManager.OnPartidaTerminada -= OnPartidaTerminada;
        }
    }

    #region Menú Principal

    private void HostFuncion()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            MostrarMensajeError("Ya estás conectado a una sesión");
            return;
        }

        bool logroIniciar = NetworkManager.Singleton.StartHost();

        if (logroIniciar)
        {
            menu.SetActive(false);
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = "Esperando jugadores...\nConfigura el mapa y presiona ENTER";

            if (configMapaPanel != null)
            {
                configMapaPanel.SetActive(true);

                if (generator != null)
                {
                    anchoInput.text = generator.mapSize.x.ToString();
                    altoInput.text = generator.mapSize.y.ToString();
                }

                anchoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
                altoInput.onValueChanged.AddListener(ActualizarTamanoMapa);
            }
        }
    }

    private void ClienteFuncion()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            MostrarMensajeError("Ya estás conectado a una sesión");
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        bool success = NetworkManager.Singleton.StartClient();

        if (success)
        {
            menu.SetActive(false);
            gameInfoTexto.gameObject.SetActive(true);
            gameInfoTexto.text = "Conectando...\nEsperando que el host inicie";
        }
    }

    private void ActualizarTamanoMapa(string valor)
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

    #endregion

    #region Durante el Juego

    public void OcultarConfigPanel()
    {
        if (configMapaPanel != null) configMapaPanel.SetActive(false);
        if (gameInfoTexto != null) gameInfoTexto.gameObject.SetActive(false);

        MostrarHUD();
    }

    public void MostrarPartidaIniciada()
    {
        if (configMapaPanel != null) configMapaPanel.SetActive(false);
        if (gameInfoTexto != null) gameInfoTexto.gameObject.SetActive(false);

        MostrarHUD();
    }

    private void MostrarHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
            ActualizarDisplayPuntajes();
        }
    }

    private void OnPuntajeCambiado(ulong clientId, int nuevoPuntaje)
    {
        ActualizarDisplayPuntajes();
    }

    private void ActualizarDisplayPuntajes()
    {
        if (puntajesTexto == null || scoreSystem == null) return;

        // Actualizar cache de nombres
        ActualizarNombresJugadores();

        // Construir texto de puntajes
        string texto = "";
        var puntajes = scoreSystem.ObtenerTodosPuntajes();

        foreach (var kvp in puntajes)
        {
            string nombre = ObtenerNombreJugador(kvp.Key);
            texto += $"{nombre}: {kvp.Value} pts\n";
        }

        puntajesTexto.text = texto;

        // Actualizar pares encontrados
        if (paresTexto != null && matchManager != null)
        {
            paresTexto.text = $"Pares: {matchManager.ObtenerParesEncontrados()}/{matchManager.ObtenerTotalPares()}";
        }
    }

    private void ActualizarNombresJugadores()
    {
        var jugadores = FindObjectsByType<scr_PlayerName>(FindObjectsSortMode.None);
        foreach (var jugador in jugadores)
        {
            nombresJugadores[jugador.OwnerClientId] = jugador.GetPlayerName();
        }
    }

    private string ObtenerNombreJugador(ulong clientId)
    {
        if (nombresJugadores.TryGetValue(clientId, out string nombre))
        {
            return nombre;
        }
        return $"Jugador {clientId}";
    }

    #endregion

    #region Fin del Juego

    private void OnPartidaTerminada()
    {
        Debug.Log("[UIManager] Partida terminada, esperando resultados...");
    }

    private void OnJuegoTerminado(ulong ganadorId, string nombreGanador, int puntaje)
    {
        Debug.Log($"[UIManager] Mostrando pantalla de victoria: {nombreGanador}");

        if (hudPanel != null) hudPanel.SetActive(false);

        if (victoriaPanel != null)
        {
            victoriaPanel.SetActive(true);

            if (victoriaTexto != null)
            {
                if (nombreGanador == "¡EMPATE!")
                {
                    victoriaTexto.text = "¡EMPATE!";
                }
                else
                {
                    victoriaTexto.text = $"¡{nombreGanador} GANA!";
                }
            }

            // Mostrar todos los resultados
            if (resultadosTexto != null && scoreSystem != null)
            {
                string resultados = "RESULTADOS FINALES\n\n";
                var puntajes = scoreSystem.ObtenerTodosPuntajes();

                // Ordenar por puntaje (mayor a menor)
                var listaOrdenada = new List<KeyValuePair<ulong, int>>(puntajes);
                listaOrdenada.Sort((a, b) => b.Value.CompareTo(a.Value));

                int posicion = 1;
                foreach (var kvp in listaOrdenada)
                {
                    string nombre = ObtenerNombreJugador(kvp.Key);
                    resultados += $"{posicion}. {nombre}: {kvp.Value} puntos\n";
                    posicion++;
                }

                resultadosTexto.text = resultados;
            }
        }
    }

    private void VolverAlMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Resetear UI
        if (victoriaPanel != null) victoriaPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (menu != null) menu.SetActive(true);
    }

    private void ReiniciarPartida()
    {
        // Solo el host puede reiniciar
        if (!IsServer) return;

        // TODO: Implementar reinicio de partida
        // Por ahora, volver al menú
        VolverAlMenu();
    }

    #endregion

    #region Utilidades

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;

            if (menu != null) menu.SetActive(true);
            if (gameInfoTexto != null) gameInfoTexto.gameObject.SetActive(false);
            if (configMapaPanel != null) configMapaPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (victoriaPanel != null) victoriaPanel.SetActive(false);

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

    #endregion
}
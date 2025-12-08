using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Genera el mapa de cartas y gestiona el mazo de sprites.
/// Inicializa el MatchManager con el total de pares a encontrar.
/// </summary>
public class scr_Generator : NetworkBehaviour
{
    [Header("Configuración del Mapa")]
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX = 2f;
    public float spacingY = 2f;

    [Header("Cámara")]
    public Camera mainCamera;
    public float paddingCamara = 50f;

    [Header("Sprites")]
    public List<Sprite> sprites_Disponibles;
    public List<Sprite> mazo;

    [Header("Referencias (Opcional - se buscan automáticamente)")]
    [SerializeField] private GameObject uiManagerObject;

    // Control de estado
    private bool partidaIniciada = false;
    private Vector2 mapSizeTemporal;

    // Referencias
    private scr_MatchManager matchManager;
    private scr_ScoreSystem scoreSystem;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnHostStartedFuncion;
        }

        mapSizeTemporal = mapSize;
        mazo = new List<Sprite>();

        // Buscar referencias
        matchManager = FindFirstObjectByType<scr_MatchManager>();
        scoreSystem = FindFirstObjectByType<scr_ScoreSystem>();
    }

    private void Update()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("[Generator] Iniciando partida...");
                partidaIniciada = true;
                IniciarPartida();
            }
        }
    }

    private void OnHostStartedFuncion()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[Generator] Host iniciado → Esperando jugadores...");
            Debug.Log("[Generator] Presiona ENTER cuando todos estén listos");

            // Registrar al host en el sistema de puntaje
            if (scoreSystem == null)
            {
                scoreSystem = FindFirstObjectByType<scr_ScoreSystem>();
            }

            if (scoreSystem != null)
            {
                scoreSystem.RegistrarJugador(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void IniciarPartida()
    {
        mapSize = mapSizeTemporal;
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];

        // Generar el mapa y mazo
        GenerarMazo();
        GenerarMapa();
        CentrarCamara();

        // Calcular total de pares e informar al MatchManager
        int totalPares = (int)(mapSize.x * mapSize.y) / 2;

        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<scr_MatchManager>();
        }

        if (matchManager != null)
        {
            matchManager.InicializarTotalPares(totalPares);
        }

        Debug.Log($"[Generator] Mapa generado: {mapSize.x}x{mapSize.y} ({totalPares} pares)");

        // Ocultar panel de configuración usando SendMessage para evitar dependencia de tipo
        OcultarUIConfig();

        // Notificar a los clientes que la partida comenzó
        NotificarInicioPartidaClientRpc();
    }

    private void OcultarUIConfig()
    {
        // Buscar el UIManager sin depender del tipo directamente
        if (uiManagerObject != null)
        {
            uiManagerObject.SendMessage("OcultarConfigPanel", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            // Buscar por nombre si no está asignado
            GameObject uiObj = GameObject.Find("UIManager");
            if (uiObj != null)
            {
                uiObj.SendMessage("OcultarConfigPanel", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    [ClientRpc]
    private void NotificarInicioPartidaClientRpc()
    {
        Debug.Log("[Generator][Cliente] ¡Partida iniciada!");

        // Notificar UI usando SendMessage
        if (uiManagerObject != null)
        {
            uiManagerObject.SendMessage("MostrarPartidaIniciada", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            GameObject uiObj = GameObject.Find("UIManager");
            if (uiObj != null)
            {
                uiObj.SendMessage("MostrarPartidaIniciada", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// Método público para cambiar el tamaño desde el UI
    /// </summary>
    public void CambiarTamanoMapa(float ancho, float alto)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            mapSizeTemporal = new Vector2(ancho, alto);
            Debug.Log($"[Generator] Tamaño del mapa: {ancho}x{alto}");
        }
    }

    private void GenerarMazo()
    {
        mazo.Clear();

        int espacioDisponible = (int)(mapSize.x * mapSize.y);
        int cartasRequeridas = espacioDisponible / 2;
        int max = Mathf.Min(sprites_Disponibles.Count, cartasRequeridas);

        for (int i = 0; i < max; i++)
        {
            var carta = sprites_Disponibles[i];
            mazo.Add(carta);
            mazo.Add(carta); // Cada sprite se añade dos veces (par)
        }

        Debug.Log($"[Generator] Mazo creado con {mazo.Count} cartas ({max} pares)");
    }

    private void GenerarMapa()
    {
        float offsetX = ((mapSize.x - 1) * spacingX) / 2f;
        float offsetY = ((mapSize.y - 1) * spacingY) / 2f;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2 tilePosition = new Vector2(
                    (x * spacingX) - offsetX,
                    (y * spacingY) - offsetY
                );

                GameObject cloneCard = Instantiate(tile, tilePosition, Quaternion.identity);

                NetworkObject netObj = cloneCard.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }

                tilesTransform[x, y] = cloneCard.transform;
            }
        }
    }

    private void CentrarCamara()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return;

        Vector3 camaraPos = mainCamera.transform.position;
        camaraPos.x = 0;
        camaraPos.y = 0;
        mainCamera.transform.position = camaraPos;

        float anchoMapa = (mapSize.x - 1) * spacingX;
        float altoMapa = (mapSize.y - 1) * spacingY;
        float altoCamaraNecesario = (altoMapa / 2f) + paddingCamara;
        float anchoCamaraNecesario = (anchoMapa / 2f) + paddingCamara;
        float aspectRatio = mainCamera.aspect;

        mainCamera.orthographicSize = Mathf.Max(altoCamaraNecesario, anchoCamaraNecesario / aspectRatio);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnHostStartedFuncion;
        }
    }
}
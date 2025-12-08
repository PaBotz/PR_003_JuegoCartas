using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Genera el mapa de cartas.
/// </summary>
public class scr_Generator : NetworkBehaviour
{
    [Header("Configuración del Mapa")]
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX, spacingY;

    [Header("Cámara")]
    public Camera mainCamera;
    public float paddingCamara = 50f;

    [Header("Sprites")]
    public List<Sprite> sprites_Disponibles;
    public List<Sprite> mazo;

    private bool partidaIniciada = false;
    private Vector2 mapSizeTemporal;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnHostStartedFuncion;
        }

        mapSizeTemporal = mapSize;
        mazo = new List<Sprite>();
    }

    private void Update()
    {
        // Solo el host puede iniciar la partida
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Iniciando partida...");
                partidaIniciada = true;
                IniciarPartida();
            }
        }
    }

    private void OnHostStartedFuncion()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host iniciado → Esperando jugadores...");
            Debug.Log("Presiona ENTER cuando todos estén listos para generar el mapa");
        }
    }

    private void IniciarPartida()
    {
        mapSize = mapSizeTemporal;
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];

        GenerarMazo();
        GenerarMapa();
        CentrarCamara();

        // Calcular total de pares e informar al MatchManager
        int totalPares = (int)(mapSize.x * mapSize.y) / 2;
        scr_MatchManager matchManager = FindFirstObjectByType<scr_MatchManager>();
        if (matchManager != null)
        {
            matchManager.InicializarPartida(totalPares);
        }

        Debug.Log($"Mapa generado: {mapSize.x}x{mapSize.y} ({totalPares} pares)");

        // Ocultar panel de configuración usando SendMessage (evita dependencia de tipo)
        OcultarUIConfig();

        // Notificar a todos los clientes
        NotificarInicioClientRpc();
    }

    private void OcultarUIConfig()
    {
        GameObject uiObj = GameObject.Find("UIManager");
        if (uiObj != null)
        {
            uiObj.SendMessage("OcultarConfigPanel", SendMessageOptions.DontRequireReceiver);
        }
    }

    [ClientRpc]
    private void NotificarInicioClientRpc()
    {
        Debug.Log("[Cliente] ¡Partida iniciada!");

        // Actualizar UI en cada cliente
        GameObject uiObj = GameObject.Find("UIManager");
        if (uiObj != null)
        {
            uiObj.SendMessage("MostrarPartidaIniciada", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void CambiarTamanoMapa(float ancho, float alto)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            mapSizeTemporal = new Vector2(ancho, alto);
            Debug.Log($"Tamaño del mapa cambiado a: {ancho}x{alto}");
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
            mazo.Add(carta);
        }

        Debug.Log($"Mazo creado con {mazo.Count} cartas");
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

                // Spawnear en red
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
        if (mainCamera == null) mainCamera = Camera.main;
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
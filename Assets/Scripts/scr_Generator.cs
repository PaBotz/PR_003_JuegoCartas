using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class scr_Generator : NetworkBehaviour
{
    public Transform[,] tilesTransform;
    public Vector2 mapSize; // Tamaño por defecto
    public GameObject tile;
    public float spacingX, spacingY;

    [Header("Camara")]
    public Camera mainCamera;
    public float paddingCamara = 50f;

    public List<Sprite> sprites_Disponibles;
    public List<Sprite> mazo;

    private bool partidaIniciada = false;

    private Vector2 mapSizeTemporal;

    // ✅ AÑADIR: Referencia al UIManager
    private scr_UIManager uiManager;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnHostStartedFuncion;
        }

        mapSizeTemporal = mapSize;

        // ✅ AÑADIR: Buscar el UIManager
        uiManager = FindFirstObjectByType<scr_UIManager>();
    }

    private void Update()
    {
        // ✅ Solo el host puede iniciar la partida
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("🎮 Iniciando partida...");
                partidaIniciada = true;
                IniciarPartida();
            }
        }
    }

    private void OnHostStartedFuncion()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("✔ Host iniciado → Esperando jugadores...");
            Debug.Log("⌨️ Presiona ENTER cuando todos estén listos para generar el mapa");

            // ✅ NO generar nada aún, esperar a que presione ENTER
        }
    }

    private void IniciarPartida()
    {
        mapSize = mapSizeTemporal;

        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];

        generatorMap();
        gestionDeLista();
        centrarCamara();

        Debug.Log($"✅ Mapa generado: {mapSize.x}x{mapSize.y}");

        // ✅ Buscar UIManager si no lo tenemos
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<scr_UIManager>();
        }

        if (uiManager != null)
        {
            uiManager.OcultarConfigPanel();
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró el UIManager en la escena");
        }
    }

    // ✅ AÑADIR: Método público para cambiar el tamaño desde el UI
    public void CambiarTamanoMapa(float ancho, float alto)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && !partidaIniciada)
        {
            mapSizeTemporal = new Vector2(ancho, alto);
            Debug.Log($"📐 Tamaño del mapa cambiado a: {ancho}x{alto}");
        }
    }

    private void generatorMap()
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

                // ✅ Spawnear en red - TODOS verán el MISMO objeto sincronizado
                NetworkObject netObj = cloneCard.GetComponent<NetworkObject>();
                netObj.Spawn();

                tilesTransform[x, y] = cloneCard.transform;
            }
        }
    }

    private void gestionDeLista()
    {
        int espacioDisponible = (int)(mapSize.x * mapSize.y);
        int cartasRequeridas = espacioDisponible / 2;
        int max = Mathf.Min(sprites_Disponibles.Count, cartasRequeridas);

        for (int i = 0; i < max; i++)
        {
            var carta = sprites_Disponibles[i];
            mazo.Add(carta);
            mazo.Add(carta);
        }

        // ✅ NO necesitamos sincronizar el mazo
        // Solo el servidor lo usa para asignar sprites
    }

    private void centrarCamara()
    {
        if (mainCamera == null) mainCamera = Camera.main;

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

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnHostStartedFuncion;
        }
    }
}
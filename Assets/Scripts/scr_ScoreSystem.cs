using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sistema de puntaje sincronizado en red.
/// Maneja los puntos de cada jugador y notifica cambios.
/// </summary>
public class scr_ScoreSystem : NetworkBehaviour
{
    public static scr_ScoreSystem Instance { get; private set; }

    // Diccionario local para UI - se actualiza via eventos
    private Dictionary<ulong, int> puntajesLocales = new Dictionary<ulong, int>();

    // Evento que se dispara cuando cambia el puntaje de cualquier jugador
    public System.Action<ulong, int> OnPuntajeCambiado;

    // Evento que se dispara cuando hay un ganador
    public System.Action<ulong, string, int> OnJuegoTerminado; // clientId, nombre, puntaje

    private void Awake()
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // El servidor escucha cuando se conectan jugadores
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            // Inicializar puntaje del nuevo jugador
            if (!puntajesLocales.ContainsKey(clientId))
            {
                puntajesLocales[clientId] = 0;
                Debug.Log($"[ScoreSystem] Jugador {clientId} registrado con 0 puntos");
            }
        }
    }

    /// <summary>
    /// Registra un jugador manualmente (útil para el host)
    /// </summary>
    public void RegistrarJugador(ulong clientId)
    {
        if (!IsServer) return;

        if (!puntajesLocales.ContainsKey(clientId))
        {
            puntajesLocales[clientId] = 0;
            Debug.Log($"[ScoreSystem] Jugador {clientId} registrado manualmente");
        }
    }

    /// <summary>
    /// Añade puntos a un jugador específico. Solo el servidor puede llamar esto.
    /// </summary>
    public void AgregarPuntos(ulong clientId, int puntos)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[ScoreSystem] Solo el servidor puede agregar puntos");
            return;
        }

        if (!puntajesLocales.ContainsKey(clientId))
        {
            puntajesLocales[clientId] = 0;
        }

        puntajesLocales[clientId] += puntos;
        int nuevoPuntaje = puntajesLocales[clientId];

        Debug.Log($"[ScoreSystem] Jugador {clientId} ahora tiene {nuevoPuntaje} puntos");

        // Notificar a TODOS los clientes del cambio
        ActualizarPuntajeClientRpc(clientId, nuevoPuntaje);
    }

    [ClientRpc]
    private void ActualizarPuntajeClientRpc(ulong clientId, int nuevoPuntaje)
    {
        puntajesLocales[clientId] = nuevoPuntaje;
        OnPuntajeCambiado?.Invoke(clientId, nuevoPuntaje);
        Debug.Log($"[ScoreSystem][Cliente] Puntaje actualizado - Jugador {clientId}: {nuevoPuntaje}");
    }

    /// <summary>
    /// Obtiene el puntaje de un jugador
    /// </summary>
    public int ObtenerPuntaje(ulong clientId)
    {
        return puntajesLocales.TryGetValue(clientId, out int puntaje) ? puntaje : 0;
    }

    /// <summary>
    /// Obtiene todos los puntajes actuales
    /// </summary>
    public Dictionary<ulong, int> ObtenerTodosPuntajes()
    {
        return new Dictionary<ulong, int>(puntajesLocales);
    }

    /// <summary>
    /// Determina el ganador y notifica a todos. Solo el servidor llama esto.
    /// </summary>
    public void DeterminarGanador()
    {
        if (!IsServer) return;

        ulong ganadorId = 0;
        int maxPuntaje = -1;
        bool empate = false;

        foreach (var kvp in puntajesLocales)
        {
            if (kvp.Value > maxPuntaje)
            {
                maxPuntaje = kvp.Value;
                ganadorId = kvp.Key;
                empate = false;
            }
            else if (kvp.Value == maxPuntaje)
            {
                empate = true;
            }
        }

        // Buscar el nombre del ganador
        string nombreGanador = "Jugador " + ganadorId;

        // Buscar en los objetos de jugador
        foreach (var player in FindObjectsByType<scr_PlayerName>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == ganadorId)
            {
                nombreGanador = player.GetPlayerName();
                break;
            }
        }

        if (empate)
        {
            nombreGanador = "¡EMPATE!";
        }

        Debug.Log($"[ScoreSystem] Juego terminado. Ganador: {nombreGanador} con {maxPuntaje} puntos");

        // Notificar a todos los clientes
        NotificarGanadorClientRpc(ganadorId, nombreGanador, maxPuntaje, empate);
    }

    [ClientRpc]
    private void NotificarGanadorClientRpc(ulong ganadorId, string nombreGanador, int puntaje, bool esEmpate)
    {
        if (esEmpate)
        {
            OnJuegoTerminado?.Invoke(0, "¡EMPATE!", puntaje);
        }
        else
        {
            OnJuegoTerminado?.Invoke(ganadorId, nombreGanador, puntaje);
        }
    }

    /// <summary>
    /// Reinicia todos los puntajes
    /// </summary>
    public void ReiniciarPuntajes()
    {
        if (!IsServer) return;

        List<ulong> keys = new List<ulong>(puntajesLocales.Keys);
        foreach (ulong key in keys)
        {
            puntajesLocales[key] = 0;
            ActualizarPuntajeClientRpc(key, 0);
        }

        Debug.Log("[ScoreSystem] Puntajes reiniciados");
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class scr_ScoreSystem : NetworkBehaviour
{
    public static scr_ScoreSystem Instance { get; private set; }

    // Almacenamiento local de puntajes
    private Dictionary<ulong, int> puntajes = new Dictionary<ulong, int>();

    // Eventos
    public System.Action<ulong, int> OnPuntajeCambiado;
    public System.Action<ulong, string, int> OnJuegoTerminado;

    private void Awake()
    {
        // Singleton
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
            RegistrarJugador(clientId);
        }
    }

    public void RegistrarJugador(ulong clientId)
    {
        if (!IsServer) return;

        if (!puntajes.ContainsKey(clientId))
        {
            puntajes[clientId] = 0;
            Debug.Log($"Jugador {clientId} registrado con 0 puntos");

            // Notificar a todos los clientes
            ActualizarPuntajeClientRpc(clientId, 0);
        }
    }


    public void AgregarPuntos(ulong clientId, int puntos)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Solo el servidor puede agregar puntos");
            return;
        }

        if (!puntajes.ContainsKey(clientId))
        {
            puntajes[clientId] = 0;
        }

        puntajes[clientId] += puntos;
        int nuevoPuntaje = puntajes[clientId];

        Debug.Log($"Jugador {clientId} ahora tiene {nuevoPuntaje} puntos (+{puntos})");

        // Sincronizar con todos los clientes
        ActualizarPuntajeClientRpc(clientId, nuevoPuntaje);
    }

    [ClientRpc]
    private void ActualizarPuntajeClientRpc(ulong clientId, int puntaje)
    {
        puntajes[clientId] = puntaje;
        OnPuntajeCambiado?.Invoke(clientId, puntaje);
        Debug.Log($"[Cliente] Puntaje actualizado - Jugador {clientId}: {puntaje}");
    }

    public int ObtenerPuntaje(ulong clientId)
    {
        return puntajes.TryGetValue(clientId, out int puntaje) ? puntaje : 0;
    }


    public Dictionary<ulong, int> ObtenerTodosPuntajes()
    {
        return new Dictionary<ulong, int>(puntajes);
    }

    public void DeterminarGanador()
    {
        if (!IsServer) return;

        ulong ganadorId = 0;
        int maxPuntaje = -1;
        bool hayEmpate = false;

        // Buscar el puntaje más alto
        foreach (var kvp in puntajes)
        {
            if (kvp.Value > maxPuntaje)
            {
                maxPuntaje = kvp.Value;
                ganadorId = kvp.Key;
                hayEmpate = false;
            }
            else if (kvp.Value == maxPuntaje && maxPuntaje > 0)
            {
                hayEmpate = true;
            }
        }

        // Buscar el nombre del ganador
        string nombreGanador = "Jugador " + ganadorId;

        if (!hayEmpate)
        {
            scr_PlayerName[] jugadores = FindObjectsByType<scr_PlayerName>(FindObjectsSortMode.None);
            foreach (var jugador in jugadores)
            {
                if (jugador.OwnerClientId == ganadorId)
                {
                    nombreGanador = jugador.GetPlayerName();
                    break;
                }
            }
        }
        else
        {
            nombreGanador = "¡EMPATE!";
        }

        Debug.Log($"GANADOR: {nombreGanador} con {maxPuntaje} puntos");

        // Notificar a todos los clientes
        NotificarGanadorClientRpc(ganadorId, nombreGanador, maxPuntaje, hayEmpate);
    }

    [ClientRpc]
    private void NotificarGanadorClientRpc(ulong ganadorId, string nombre, int puntaje, bool empate)
    {
        if (empate)
        {
            OnJuegoTerminado?.Invoke(0, "¡EMPATE!", puntaje);
        }
        else
        {
            OnJuegoTerminado?.Invoke(ganadorId, nombre, puntaje);
        }

        Debug.Log($"[Cliente] Juego terminado - {nombre}: {puntaje} puntos");
    }


    public void ReiniciarPuntajes()
    {
        if (!IsServer) return;

        List<ulong> keys = new List<ulong>(puntajes.Keys);
        foreach (ulong key in keys)
        {
            puntajes[key] = 0;
            ActualizarPuntajeClientRpc(key, 0);
        }

        Debug.Log("Puntajes reiniciados");
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gestiona las comparaciones de cartas, asigna puntos al jugador correcto,
/// y detecta cuando termina la partida.
/// </summary>
public class scr_MatchManager : NetworkBehaviour
{
    [Header("Listas de Control")]
    public List<GameObject> cartasHijoActivas_List;
    public List<scr_SpriteCard> scriptsCartas_List;

    [Header("Configuración de Tiempos")]
    public float tiempoParaVerCartas = 1.5f;

    [Header("Puntos por Match")]
    [SerializeField] private int puntosPorMatch = 10;

    // Control interno
    private bool estaComparando = false;
    private scr_SpriteCard cartaEnEspera_01, cartaEnEspera_02;

    // Guardamos qué jugador activó cada carta
    private ulong jugadorCarta_01;
    private ulong jugadorCarta_02;

    // Referencias
    private scr_ScoreSystem scoreSystem;
    private scr_Generator generator;

    // Contadores sincronizados
    private NetworkVariable<int> paresEncontradosNet = new NetworkVariable<int>(0);
    private NetworkVariable<int> totalParesNet = new NetworkVariable<int>(0);

    // Evento para cuando termina el juego
    public System.Action OnPartidaTerminada;

    private void Start()
    {
        cartasHijoActivas_List = new List<GameObject>();
        scriptsCartas_List = new List<scr_SpriteCard>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Buscar referencias
        scoreSystem = scr_ScoreSystem.Instance;
        if (scoreSystem == null)
        {
            scoreSystem = FindFirstObjectByType<scr_ScoreSystem>();
        }

        generator = FindFirstObjectByType<scr_Generator>();

        // Suscribirse a cambios en los pares encontrados
        paresEncontradosNet.OnValueChanged += OnParesEncontradosCambiado;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        paresEncontradosNet.OnValueChanged -= OnParesEncontradosCambiado;
    }

    /// <summary>
    /// El Generator llama esto cuando se crean las cartas para saber cuántos pares hay
    /// </summary>
    public void InicializarTotalPares(int total)
    {
        if (!IsServer) return;

        totalParesNet.Value = total;
        paresEncontradosNet.Value = 0;
        Debug.Log($"[MatchManager] Total de pares a encontrar: {total}");
    }

    private void OnParesEncontradosCambiado(int anterior, int nuevo)
    {
        Debug.Log($"[MatchManager] Pares encontrados: {nuevo}/{totalParesNet.Value}");

        // Verificar si terminó el juego
        if (nuevo >= totalParesNet.Value && totalParesNet.Value > 0)
        {
            Debug.Log("[MatchManager] ¡Todos los pares encontrados!");
            OnPartidaTerminada?.Invoke();

            if (IsServer && scoreSystem != null)
            {
                scoreSystem.DeterminarGanador();
            }
        }
    }

    public bool PuedeActivarCarta()
    {
        if (estaComparando || cartasHijoActivas_List.Count >= 2)
        {
            Debug.Log("[MatchManager] No se puede activar carta: límite alcanzado o comparando");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Registra una carta activada junto con el ID del jugador que la activó
    /// </summary>
    public void RegistrarCartaActivada(GameObject spriteCard_Active, ulong clientIdJugador)
    {
        if (!IsServer) return;

        Debug.Log($"[MatchManager] Carta activada por jugador {clientIdJugador}. Total activas: {scriptsCartas_List.Count + 1}");

        cartasHijoActivas_List.Add(spriteCard_Active);
        scr_SpriteCard spriteScript = spriteCard_Active.GetComponent<scr_SpriteCard>();
        scriptsCartas_List.Add(spriteScript);

        // Guardar quién activó esta carta (primera o segunda)
        if (scriptsCartas_List.Count == 1)
        {
            jugadorCarta_01 = clientIdJugador;
        }
        else if (scriptsCartas_List.Count == 2)
        {
            jugadorCarta_02 = clientIdJugador;
        }

        // Si hay 2 cartas, comparar
        if (cartasHijoActivas_List.Count >= 2)
        {
            StartCoroutine(CompararCartas());
        }
    }

    private IEnumerator CompararCartas()
    {
        estaComparando = true;

        Debug.Log("[MatchManager] Comprobando matches...");

        // Pequeña espera para que los sprites se sincronicen
        yield return new WaitForSeconds(0.1f);

        Sprite sprite_01 = scriptsCartas_List[0].ObtenerSprite();
        Sprite sprite_02 = scriptsCartas_List[1].ObtenerSprite();

        Debug.Log($"[MatchManager] Comparando: {(sprite_01 != null ? sprite_01.name : "null")} con {(sprite_02 != null ? sprite_02.name : "null")}");

        if (sprite_01 != null && sprite_02 != null && sprite_01 == sprite_02)
        {
            Debug.Log("[MatchManager] ¡MATCH ENCONTRADO!");

            // Guardar referencias antes de limpiar las listas
            cartaEnEspera_01 = scriptsCartas_List[0];
            cartaEnEspera_02 = scriptsCartas_List[1];

            // Determinar quién se lleva los puntos
            ulong jugadorGanadorPuntos = jugadorCarta_02;

            // Asignar puntos
            if (scoreSystem != null)
            {
                scoreSystem.AgregarPuntos(jugadorGanadorPuntos, puntosPorMatch);
                Debug.Log($"[MatchManager] Puntos asignados a jugador {jugadorGanadorPuntos}");
            }

            // Buscar el objeto que tiene el Animator (subiendo por la jerarquía)
            Animator anim_01 = BuscarAnimatorEnJerarquia(cartaEnEspera_01.transform);
            Animator anim_02 = BuscarAnimatorEnJerarquia(cartaEnEspera_02.transform);

            if (anim_01 != null) anim_01.SetTrigger("Match_Trigger");
            if (anim_02 != null) anim_02.SetTrigger("Match_Trigger");

            // Notificar match a los clientes para efectos visuales
            NotificarMatchClientRpc(jugadorGanadorPuntos, puntosPorMatch);
        }
        else
        {
            Debug.Log("[MatchManager] NO son match. Mostrando cartas un momento...");

            yield return new WaitForSeconds(tiempoParaVerCartas);

            Debug.Log("[MatchManager] Ocultando cartas...");

            // Guardar referencias antes de limpiar
            scr_SpriteCard script_01 = scriptsCartas_List[0];
            scr_SpriteCard script_02 = scriptsCartas_List[1];

            // Limpiar listas
            scriptsCartas_List.Clear();
            cartasHijoActivas_List.Clear();

            // Buscar scr_Card subiendo por la jerarquía y usar sus métodos de red
            scr_Card cardScript_01 = BuscarScriptCardEnJerarquia(script_01.transform);
            scr_Card cardScript_02 = BuscarScriptCardEnJerarquia(script_02.transform);

            // Ocultar sprites y resetear cartas usando los métodos del padre
            if (cardScript_01 != null) cardScript_01.OcultarSpriteEnRed();
            if (cardScript_02 != null) cardScript_02.OcultarSpriteEnRed();

            estaComparando = false;
            Debug.Log("[MatchManager] Comparación terminada. Listas limpiadas.");
        }
    }

    /// <summary>
    /// Busca un Animator subiendo por la jerarquía del transform
    /// </summary>
    private Animator BuscarAnimatorEnJerarquia(Transform desde)
    {
        Transform actual = desde;
        while (actual != null)
        {
            Animator anim = actual.GetComponent<Animator>();
            if (anim != null) return anim;
            actual = actual.parent;
        }
        return null;
    }

    /// <summary>
    /// Busca scr_Card subiendo por la jerarquía del transform
    /// </summary>
    private scr_Card BuscarScriptCardEnJerarquia(Transform desde)
    {
        Transform actual = desde;
        while (actual != null)
        {
            scr_Card card = actual.GetComponent<scr_Card>();
            if (card != null) return card;
            actual = actual.parent;
        }
        return null;
    }

    [ClientRpc]
    private void NotificarMatchClientRpc(ulong jugadorId, int puntos)
    {
        Debug.Log($"[MatchManager][Cliente] Match! Jugador {jugadorId} ganó {puntos} puntos");
        // Aquí podrías añadir efectos de sonido o partículas
    }

    /// <summary>
    /// Llamado por Animation Event cuando termina la animación de match
    /// </summary>
    public void EliminarCartasMatch()
    {
        if (!IsServer) return;

        Debug.Log("[MatchManager] Animation Event: Eliminando cartas con match");

        // Limpiar listas
        scriptsCartas_List.Clear();
        cartasHijoActivas_List.Clear();

        // Desactivar las cartas usando scr_Card
        if (cartaEnEspera_01 != null)
        {
            scr_Card card_01 = BuscarScriptCardEnJerarquia(cartaEnEspera_01.transform);
            if (card_01 != null) card_01.DesactivarCartaCompleta();
        }

        if (cartaEnEspera_02 != null)
        {
            scr_Card card_02 = BuscarScriptCardEnJerarquia(cartaEnEspera_02.transform);
            if (card_02 != null) card_02.DesactivarCartaCompleta();
        }

        // Incrementar contador de pares (esto dispara la verificación de fin de juego)
        paresEncontradosNet.Value++;

        // Limpiar referencias
        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;

        estaComparando = false;
        Debug.Log("[MatchManager] Eliminación completada. Sistema listo para nuevas cartas.");
    }

    /// <summary>
    /// Reinicia todo el sistema para una nueva partida
    /// </summary>
    public void ReiniciarPartida()
    {
        if (!IsServer) return;

        cartasHijoActivas_List.Clear();
        scriptsCartas_List.Clear();
        paresEncontradosNet.Value = 0;
        estaComparando = false;
        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;

        Debug.Log("[MatchManager] Sistema reiniciado para nueva partida");
    }

    // Getters públicos para UI
    public int ObtenerParesEncontrados() => paresEncontradosNet.Value;
    public int ObtenerTotalPares() => totalParesNet.Value;
}
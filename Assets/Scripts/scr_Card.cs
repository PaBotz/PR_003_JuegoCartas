using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Control de la carta. Maneja colisiones con jugadores, activación de cartas,
/// y toda la sincronización de red (incluyendo la del hijo SpriteCard).
/// Este es el ÚNICO NetworkObject de la carta.
/// </summary>
public class scr_Card : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject SpriteCard;
    [SerializeField] private scr_SpriteCard myscr_SpriteCard;

    [Header("Referencias Externas (se buscan automáticamente)")]
    [SerializeField] private GameObject myMatchManager;
    private scr_MatchManager myscr_MatchManager;

    [Header("Estado")]
    public bool card_Collision;
    public bool card_Active;

    // Variable de red para sincronizar el estado activo
    private NetworkVariable<bool> cardActiveNet = new NetworkVariable<bool>(false);

    // Variable de red para sincronizar el índice del sprite
    private NetworkVariable<int> spriteIndexNet = new NetworkVariable<int>(-1);

    void Start()
    {
        InicializarReferencias();
        card_Collision = false;
        card_Active = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (myscr_SpriteCard == null)
        {
            InicializarReferencias();
        }

        // Ocultar sprite al inicio
        if (myscr_SpriteCard != null)
        {
            myscr_SpriteCard.OcultarSprite();
        }

        // Suscribirse a cambios de estado
        cardActiveNet.OnValueChanged += OnCardActiveChanged;
        spriteIndexNet.OnValueChanged += OnSpriteIndexChanged;

        // Si ya tiene un sprite asignado (reconexión), aplicarlo
        if (spriteIndexNet.Value >= 0)
        {
            myscr_SpriteCard?.AplicarSprite(spriteIndexNet.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        cardActiveNet.OnValueChanged -= OnCardActiveChanged;
        spriteIndexNet.OnValueChanged -= OnSpriteIndexChanged;
    }

    private void OnCardActiveChanged(bool anterior, bool nuevo)
    {
        card_Active = nuevo;

        if (myscr_SpriteCard != null)
        {
            if (nuevo)
                myscr_SpriteCard.MostrarSprite();
            else
                myscr_SpriteCard.OcultarSprite();
        }
    }

    private void OnSpriteIndexChanged(int anterior, int nuevo)
    {
        if (nuevo >= 0 && myscr_SpriteCard != null)
        {
            myscr_SpriteCard.AplicarSprite(nuevo);
        }
    }

    private void InicializarReferencias()
    {
        if (myMatchManager == null)
        {
            myMatchManager = GameObject.Find("MatchManager");
        }

        if (myscr_SpriteCard == null)
        {
            // Buscar en hijos si no está asignado
            if (SpriteCard != null)
            {
                myscr_SpriteCard = SpriteCard.GetComponent<scr_SpriteCard>();
            }
            else
            {
                myscr_SpriteCard = GetComponentInChildren<scr_SpriteCard>();
            }
        }

        if (myscr_MatchManager == null && myMatchManager != null)
        {
            myscr_MatchManager = myMatchManager.GetComponent<scr_MatchManager>();
        }
    }

    void Update()
    {
        cardActive_Funcion();
    }

    private void cardActive_Funcion()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.U) && card_Collision && !card_Active)
        {
            SolicitarActivacionServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SolicitarActivacionServerRpc(RpcParams rpcParams = default)
    {
        ulong clientIdJugador = rpcParams.Receive.SenderClientId;

        if (myscr_MatchManager == null)
        {
            InicializarReferencias();
        }

        if (myscr_MatchManager != null && !myscr_MatchManager.PuedeActivarCarta())
        {
            Debug.Log($"[Card] Jugador {clientIdJugador} no puede activar carta en este momento");
            return;
        }

        if (cardActiveNet.Value)
        {
            Debug.Log("[Card] La carta ya está activa");
            return;
        }

        Debug.Log($"[Card] Carta activada por jugador {clientIdJugador}");

        // Marcar como activa (esto sincroniza a todos los clientes)
        cardActiveNet.Value = true;

        // Activar la carta en el SpriteCard (esto asigna sprite si no tiene)
        if (myscr_SpriteCard != null)
        {
            myscr_SpriteCard.CartaActivada(clientIdJugador);
        }
    }

    /// <summary>
    /// Llamado por SpriteCard para sincronizar el índice del sprite a todos los clientes
    /// </summary>
    public void SincronizarSpriteIndex(int indice)
    {
        if (!IsServer) return;

        spriteIndexNet.Value = indice;
    }

    /// <summary>
    /// Resetea el estado de la carta. Solo el servidor puede llamar esto.
    /// </summary>
    public void ResetCardActiveRed()
    {
        if (!IsServer) return;

        cardActiveNet.Value = false;
    }

    /// <summary>
    /// Desactiva toda la carta (el objeto raíz). Solo servidor.
    /// </summary>
    public void DesactivarCartaCompleta()
    {
        if (!IsServer) return;

        DesactivarCartaClientRpc();
    }

    [ClientRpc]
    private void DesactivarCartaClientRpc()
    {
        // Buscar el objeto raíz subiendo por la jerarquía
        Transform raiz = transform;
        while (raiz.parent != null)
        {
            raiz = raiz.parent;
        }
        raiz.gameObject.SetActive(false);
    }

    /// <summary>
    /// Oculta el sprite en todos los clientes (para cuando no hay match)
    /// </summary>
    public void OcultarSpriteEnRed()
    {
        if (!IsServer) return;

        cardActiveNet.Value = false;
        OcultarSpriteClientRpc();
    }

    [ClientRpc]
    private void OcultarSpriteClientRpc()
    {
        if (myscr_SpriteCard != null)
        {
            myscr_SpriteCard.OcultarSprite();
        }
        card_Active = false;
    }

    // Método legacy para compatibilidad
    public void ResetCardActive()
    {
        if (IsServer)
        {
            ResetCardActiveRed();
        }
        else
        {
            card_Active = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            card_Collision = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            card_Collision = false;
        }
    }
}
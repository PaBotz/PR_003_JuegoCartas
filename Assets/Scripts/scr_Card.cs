using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Control de la carta. Maneja colisiones, activación y TODA la sincronización de red.
/// Este es el ÚNICO script con NetworkBehaviour en la carta.
/// </summary>
public class scr_Card : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject SpriteCard;
    [SerializeField] private scr_SpriteCard myscr_SpriteCard;
    [SerializeField] private scr_MatchManager myscr_MatchManager;

    [Header("Estado")]
    public bool card_Collision;
    public bool card_Active;

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
    }

    private void InicializarReferencias()
    {
        // Buscar MatchManager
        if (myscr_MatchManager == null)
        {
            GameObject mmObj = GameObject.Find("MatchManager");
            if (mmObj != null)
            {
                myscr_MatchManager = mmObj.GetComponent<scr_MatchManager>();
            }
        }

        // Buscar SpriteCard en los hijos
        if (myscr_SpriteCard == null)
        {
            if (SpriteCard != null)
            {
                myscr_SpriteCard = SpriteCard.GetComponent<scr_SpriteCard>();
            }
            else
            {
                myscr_SpriteCard = GetComponentInChildren<scr_SpriteCard>();
            }
        }
    }

    void Update()
    {
        CardActive_Funcion();
    }

    private void CardActive_Funcion()
    {
        // Solo el owner puede activar cartas con input
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.U) && card_Collision && !card_Active)
        {
            // Verificar localmente primero (opcional, para feedback rápido)
            if (myscr_MatchManager != null && !myscr_MatchManager.PuedeActivarCarta())
            {
                Debug.Log("No se puede activar más cartas en este momento");
                return;
            }

            ActivarCartaServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void ActivarCartaServerRpc()
    {
        // Verificar en el servidor
        if (myscr_MatchManager == null)
        {
            InicializarReferencias();
        }

        if (myscr_MatchManager != null && !myscr_MatchManager.PuedeActivarCarta())
        {
            Debug.Log("[Server] No se puede activar más cartas");
            return;
        }

        if (card_Active)
        {
            Debug.Log("[Server] La carta ya está activa");
            return;
        }

        Debug.Log("[Server] Carta activada");

        // Activar la carta y obtener el índice del sprite
        int indiceSprite = myscr_SpriteCard.CartaActivada();

        // Sincronizar a todos los clientes
        MostrarCartaClientRpc(indiceSprite);
    }

    [ClientRpc]
    void MostrarCartaClientRpc(int indiceSprite)
    {
        // Aplicar sprite si se asignó uno nuevo
        if (indiceSprite >= 0)
        {
            myscr_SpriteCard.AplicarSprite(indiceSprite);
        }

        // Mostrar el sprite
        myscr_SpriteCard.MostrarSprite();
        card_Active = true;
    }

    /// <summary>
    /// Oculta el sprite en todos los clientes (cuando NO hay match).
    /// Solo el servidor puede llamar esto.
    /// </summary>
    public void OcultarSpriteEnRed()
    {
        if (!IsServer) return;
        OcultarSpriteClientRpc();
    }

    [ClientRpc]
    void OcultarSpriteClientRpc()
    {
        if (myscr_SpriteCard != null)
        {
            myscr_SpriteCard.OcultarSprite();
        }
        card_Active = false;
        Debug.Log("[Cliente] Carta ocultada");
    }

    /// <summary>
    /// Desactiva toda la carta en todos los clientes (cuando HAY match).
    /// Solo el servidor puede llamar esto.
    /// </summary>
    public void DesactivarCartaEnRed()
    {
        if (!IsServer) return;
        DesactivarCartaClientRpc();
    }

    [ClientRpc]
    void DesactivarCartaClientRpc()
    {
        // Buscar el objeto raíz y desactivarlo
        Transform raiz = transform;
        while (raiz.parent != null)
        {
            raiz = raiz.parent;
        }
        raiz.gameObject.SetActive(false);
        Debug.Log("[Cliente] Carta desactivada completamente");
    }

    /// <summary>
    /// Resetea el estado de la carta en todos los clientes.
    /// Solo el servidor puede llamar esto.
    /// </summary>
    public void ResetCardActiveEnRed()
    {
        if (!IsServer) return;
        ResetCardActiveClientRpc();
    }

    [ClientRpc]
    void ResetCardActiveClientRpc()
    {
        card_Active = false;
    }

    // Método original para compatibilidad (ahora llama a la versión de red)
    public void ResetCardActive()
    {
        card_Active = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D - Entrando");
            card_Collision = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D - Saliendo");
            card_Collision = false;
        }
    }
}
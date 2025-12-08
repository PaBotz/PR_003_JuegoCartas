using UnityEngine;
using Unity.Netcode;

public class scr_Card : NetworkBehaviour
{
    private GameObject Player;
    public scr_PlayerMovements my_scrPlayer;

    [SerializeField] private GameObject SpriteCard;
    [SerializeField] private scr_SpriteCard myscr_SpriteCard;
    [SerializeField] private GameObject myMatchManager;
    [SerializeField] private scr_MatchManager myscr_MatchManager;
    [Header("PRUEBA")]
    public bool card_Collision, card_Active;

    void Start()
    {
        InicializarReferencias();
        card_Collision = false;
        card_Active = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ✅ Asegurarse de que las referencias estén inicializadas
        if (myscr_SpriteCard == null)
        {
            InicializarReferencias();
        }

        // ✅ Ahora sí ocultar el sprite
        if (myscr_SpriteCard != null)
        {
            myscr_SpriteCard.OcultarSprite();
        }
    }

    // ✅ NUEVO: Método para inicializar referencias
    private void InicializarReferencias()
    {
        if (myMatchManager == null)
        {
            myMatchManager = GameObject.Find("MatchManager");
        }

        if (my_scrPlayer == null)
        {
            my_scrPlayer = GetComponent<scr_PlayerMovements>();
        }

        if (myscr_SpriteCard == null && SpriteCard != null)
        {
            myscr_SpriteCard = SpriteCard.GetComponent<scr_SpriteCard>();
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
            if (!myscr_MatchManager.PuedeActivarCarta())
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
        Debug.Log("Estoy active");
        MostrarCartaClientRpc();
        myscr_SpriteCard.CartaActivada();
    }

    [ClientRpc]
    void MostrarCartaClientRpc()
    {
        // ✅ CAMBIO: Mostrar el sprite en vez de activar el GameObject
        myscr_SpriteCard.MostrarSprite();
        card_Active = true;
    }

    void eliminarMatchCartas_MatchManager()
    {
        myscr_MatchManager.EliminarCartasMatch();
    }

    public void ResetCardActive()
    {
        card_Active = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D");
            card_Collision = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D");
            card_Collision = false;
        }


    }
}
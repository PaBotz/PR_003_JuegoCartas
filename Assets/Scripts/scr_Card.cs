using UnityEngine;


public class scr_Card : MonoBehaviour
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
        Player = GameObject.Find("Player");
        myMatchManager= GameObject.Find("MatchManager");
        my_scrPlayer = Player.GetComponent<scr_PlayerMovements>(); //Por el network AHora debo encontrar un modo de solucionarlo
        myscr_SpriteCard = SpriteCard.GetComponent<scr_SpriteCard>();
        myscr_MatchManager = myMatchManager.GetComponent<scr_MatchManager>();
        card_Collision = false;
        SpriteCard.SetActive(false);
        card_Active = false;

    }


    void Update()
    {
        cardActive_Funcion();

        
        
    }


    private void cardActive_Funcion()
    {
        
        if (Input.GetKeyDown(KeyCode.U) && card_Collision && !card_Active)
        {
            if (!myscr_MatchManager.PuedeActivarCarta())
            {
                Debug.Log("No se puede activar más cartas en este momento");
                return; // Bloquear la activación
            }
            Debug.Log("Estoy active");
            SpriteCard.SetActive(true);
            card_Active = true;
            myscr_SpriteCard.CartaActivada();

        }
       

      /*  else if (Input.GetKeyDown(KeyCode.U) && card_Collision && card_Active)
        {
            Debug.Log("Estoy desactive");
            SpriteCard.SetActive(false);
            card_Active = false;
        } */
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
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D");
            card_Collision = true;
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D");
            card_Collision = false;
        }
    }



}


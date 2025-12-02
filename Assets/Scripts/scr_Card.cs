using UnityEngine;


public class scr_Card : MonoBehaviour
{

    private GameObject Player;
    public scr_PlayerMovements my_scrPlayer;
    
    [SerializeField] GameObject SpriteCard;
    public scr_SpriteCard myscr_SpriteCard;

    [Header("PRUEBA")]
    public bool card_Collision, card_Active;


    void Start()
    {
        Player = GameObject.Find("Player");
        my_scrPlayer = Player.GetComponent<scr_PlayerMovements>();
        myscr_SpriteCard = SpriteCard.GetComponent<scr_SpriteCard>();
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
            Debug.Log("Estoy active");
            SpriteCard.SetActive(true);
            card_Active = true;
            //myscr_SpriteCard.CartaActivada();

        }
        else if (Input.GetKeyDown(KeyCode.U) && card_Collision && card_Active)
        {
            Debug.Log("Estoy desactive");
            SpriteCard.SetActive(false);
            card_Active = false;
        }
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


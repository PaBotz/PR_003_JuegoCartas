using UnityEngine;


public class scr_Card : MonoBehaviour
{

    private GameObject Player;
    public scr_PlayerMovements my_scrPlayer;
    
    [SerializeField] GameObject SpriteCard;

    [Header("PRUEBA")]
    public bool card_Active;

    void Start()
    {
        Player = GameObject.Find("Player");
        my_scrPlayer = Player.GetComponent<scr_PlayerMovements>();
        card_Active = false;
        SpriteCard.SetActive(false);

      

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U) && card_Active == true )
        {
            SpriteCard.SetActive(true);
        }
     

    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision 2D");
            card_Active = true;
        }
    }


}


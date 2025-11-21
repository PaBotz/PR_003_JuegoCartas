using UnityEngine;

public class scr_PlayerMovements : MonoBehaviour
{
    [Header("Parametros")]
    [SerializeField] int velocidad;



    private Rigidbody2D rb;
    private float moveInputV, moveInputH;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();


    }

    void Update()
    {
        myMove();

    }

    void myMove()
    {
        moveInputV = Input.GetAxisRaw("Vertical");
        moveInputH = Input.GetAxisRaw("Horizontal");
        Vector2 movimiento = new Vector2(moveInputH, moveInputV);
        rb.MovePosition(rb.position + movimiento.normalized * velocidad * Time.deltaTime);
    }







}

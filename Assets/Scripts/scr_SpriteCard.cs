using UnityEngine;

public class scr_SpriteCard : MonoBehaviour
{
    //Sprite aleatotio
    [Header("Sprites disponibles")]
    public Sprite[] sprites;
    public string sortingLayerName = "Front_Card";
    public int orderInLayer = 1;

    private SpriteRenderer sr;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }


    void Start()
    {
        //Sprite aleatorios
        SetRandomSprite();
        //ApplyRenderOptions();

    }

    void SetRandomSprite()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No hay sprites asignados en " + gameObject.name);
            return;
        }

        int randomIndex = Random.Range(0, sprites.Length); //Coloca un numero aleatorio de la cantidad de sprites que tengamos
        sr.sprite = sprites[randomIndex]; //Lo setea
    }

    /*void ApplyRenderOptions()
    {
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = orderInLayer;
    }*/


    void Update()
    {
        
    }
}

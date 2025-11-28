using UnityEngine;
using System.Collections.Generic;

public class scr_SpriteCard : MonoBehaviour
{
    //Sprite aleatotio
    [Header("Sprites disponibles")]
    public List<Sprite> sprites;
   //public List<GameObject> pares_List;
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
        //SetRandomSprite();
        //ApplyRenderOptions();

    }

    void SetRandomSprite()
    {
        /*if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No hay sprites asignados en " + gameObject.name);
            return;
        }*/

        /*int randomIndex = Random.Range(0, sprites.Length); //Coloca un numero aleatorio de la cantidad de sprites que tengamos
        sr.sprite = sprites.splice(randomIndex,1)[0];*/ //Lo setea
        //Buscar los objetos con el index actual en la escena
        //Si hay 2 objetos que ya lo poseen que vuelva a hacer el randomIndex 
        
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

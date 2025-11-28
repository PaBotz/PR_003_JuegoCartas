using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scr_Generator : MonoBehaviour
{
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX,spacingY;
   

    public List<Sprite> sprites_Disponibles; //Todas las opciones posibles de cartas
    public List<Sprite> mazo; //Mazo con las opciones segun el tamaño de 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Matriz/array.
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];
        generatorMap();
        gestionDeLista();
        
    }

    private void generatorMap()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                Vector2 tilePosition = new Vector2(x * spacingX,y * spacingY);
                GameObject cloneCard = Instantiate(tile,tilePosition,Quaternion.identity);
                tilesTransform[x,y]= cloneCard.transform;
            }
        }
    }

    private void gestionDeLista()
    {
        int espacioDisponible = (int)(mapSize.x * mapSize.y);
        int cartasRequeridas = espacioDisponible / 2; // Int con la mitad de espacios de cartas para luego meter los duplicados

        int max = Mathf.Min(sprites_Disponibles.Count, cartasRequeridas); //Coge el valor más pequeño; Esto nos ayuda en caso de que algun array quede más grande que el otro (que no nos pasemos)
                                                                          

        for (int i=0;i<max; i++)
        {
            Debug.Log("max: " + max);
            var carta = sprites_Disponibles[i]; //Añade el sprite del número de indice actual.
            mazo.Add(carta);
            mazo.Add(carta); 

        }
    }
  

}

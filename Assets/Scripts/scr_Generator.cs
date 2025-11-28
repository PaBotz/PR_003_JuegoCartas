using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scr_Generator : MonoBehaviour
{
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX,spacingY;

    public List<Sprite> sprites;

    public List<Sprite> mazo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        for(int i=0;i<sprites.Count;i++)
        {
            var carta = sprites[i];
            mazo.Add(carta);
            mazo.Add(carta); 

        }
    }
  



}

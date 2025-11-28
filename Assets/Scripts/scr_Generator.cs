using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scr_Generator : MonoBehaviour
{
    public Transform[,] tilesTransform;
    public Sprite[] tilesSprite; //La carta por atras
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX,spacingY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Matriz/array.
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];
        generatorMap();
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

  



}

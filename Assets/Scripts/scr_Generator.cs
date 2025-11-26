using UnityEngine;
using System.Collections;

public class scr_Generator : MonoBehaviour
{
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Matriz/array.
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];
        StartCoroutine(nameof(generatorMap));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void generatorMap() // Este sería el uso correcto, pero hasta no acabar el tuto toca con IEnumarator
    IEnumerator generatorMap()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                Vector2 tilePosition = new Vector2(x + 0.5f,y + 0.5f);
                GameObject cloneCard = Instantiate(tile,tilePosition,Quaternion.Euler(90,0,0));
                tilesTransform[x,y]= cloneCard.transform;
                yield return cloneCard;
            }
        }
    }
}

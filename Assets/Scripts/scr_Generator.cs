using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scr_Generator : MonoBehaviour
{
    public Transform[,] tilesTransform;
    public Vector2 mapSize;
    public GameObject tile;
    public float spacingX,spacingY;


    [Header("Camara")]
    public Camera mainCamera; // Arrastra tu cámara aquí o déjalo en null para usar Camera.main
    public float paddingCamara = 50f; // Espacio extra alrededor del mapa


    public List<Sprite> sprites_Disponibles; //Todas las opciones posibles de cartas
    public List<Sprite> mazo; //Mazo con las opciones segun el tamaño de 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
        tilesTransform = new Transform[(int)mapSize.x, (int)mapSize.y];
        generatorMap();
        gestionDeLista();

        centrarCamara();

        
    }

    private void generatorMap()
    {
        float offsetX = ((mapSize.x - 1) * spacingX) / 2f; //****CUANTO DA ESTO?****
        float offsetY = ((mapSize.y - 1) * spacingY) / 2f;

        for (int x = 0; x < mapSize.x; x++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                
                Vector2 tilePosition = new Vector2(  //Vector2 tilePosition = new Vector2(x * spacingX,y * spacingY); //Sin restar offset
                   (x * spacingX) - offsetX,         //Esto no es un JSON, lo pongo así para que se vea más claro
                   (y * spacingY) - offsetY
               );
                GameObject cloneCard = Instantiate(tile,tilePosition,Quaternion.identity); //Quaternion Identiti equivale a una rotacion 0; Vector3(0,0,0);
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
  private void centrarCamara()
    {
        // Centrar la cámara en el origen (donde está el mapa)
        //Es un poco inecesario, pues la camara ya está centrada, pero es una buena praxis
        Vector3 camaraPos = mainCamera.transform.position;
        camaraPos.x = 0;
        camaraPos.y = 0;
        mainCamera.transform.position = camaraPos;

        // Calcular el tamaño total del mapa
        float anchoMapa = (mapSize.x - 1) * spacingX; //Se resta uno para que quede centrado, si no quedaría desfasado a la derecha
        float altoMapa = (mapSize.y - 1) * spacingY;  //Explicacion: mapSize.y - 1 = el numero de espacios entre cartas Ejemplo C | C | C (2 espacios y 3 cartas)

        // Calcular el tamaño de cámara necesario
        // Orthographic size es la MITAD de la altura visible
        float altoCamaraNecesario = (altoMapa / 2f) + paddingCamara;
        float anchoCamaraNecesario = (anchoMapa / 2f) + paddingCamara;

        // Ajustar según el aspect ratio de la cámara
        float aspectRatio = mainCamera.aspect; // ancho/alto
        float sizePorAltura = altoCamaraNecesario; //Refencia
        float sizePorAnchura = anchoCamaraNecesario / aspectRatio; //Referencia

        // Usar el mayor de los dos para que todo quepa
        mainCamera.orthographicSize = Mathf.Max(sizePorAltura, sizePorAnchura);

        Debug.Log($"Cámara ajustada. Size: {mainCamera.orthographicSize}, Aspect: {aspectRatio}");

    }

}

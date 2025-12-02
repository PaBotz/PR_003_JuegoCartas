using UnityEngine;
using System.Collections.Generic;

public class scr_SpriteCard : MonoBehaviour
{
    //Sprite aleatotio
    [Header("Sprites disponibles")]

    //public List<GameObject> pares_List;
    public string sortingLayerName = "Front_Card";
    public int orderInLayer = 1;

    private SpriteRenderer sr;
    private Sprite sprite_Propio;

    public scr_Generator myGenerator;
    public scr_MatchManager myMatchManager;

    public GameObject cartaPadre;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        myGenerator = FindFirstObjectByType<scr_Generator>();
        myMatchManager = FindFirstObjectByType<scr_MatchManager>();
    }


    void Start() //Se activa solo una vez en toda la vida del gameobject al igual que el awake.
    {
        SetRandomSprite();
        CartaActivada();

    }
    private void OnEnable() //Este se activa cada vez que (valga la redundancia) se activa el gameobject
    {

    }


    void SetRandomSprite()
    {
        int randomIndex = Random.Range(0, myGenerator.mazo.Count); //Coloca un numero aleatorio de la cantidad de sprites que tengamos
        sr.sprite = myGenerator.mazo[randomIndex];
        sprite_Propio = sr.sprite;
        myGenerator.mazo.RemoveAt(randomIndex);

    }

    public Sprite ObtenerSprite()
    {
        return sprite_Propio;
    }

    public void DesactivarPadre()
    {
        cartaPadre.SetActive(false);
    }

    public void CartaActivada()
    {

        myMatchManager.RegistrarCartaActivada(this.gameObject);


    }
}

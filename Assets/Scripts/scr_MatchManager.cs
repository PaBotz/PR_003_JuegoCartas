using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class scr_MatchManager : MonoBehaviour
{
    //public List<Sprite> myList_spriteCards;//Lista que contendrá solo los sprites 
    public List<GameObject> cartasHijo_Activas; //Lista que contendrá las cartas front activa. Si alguna se desactiva la sacaremos de la lista
    public List<scr_SpriteCard> scripts_Cartas;
  
    public int paresEncontrados;


    //AUN  NO LO USO
    [Header("Configuración de Tiempos")]
    public float tiempoParaVerCartas = 1.5f;
    public float tiempoAnimacionMatch = 1f;

    //No necesitamos start() O Upadate() ya que estas funciones solo se ejecutan cuando se llaman desde afuera


    private void Update()
    {
        Debug.Log("cartasHijo_Activas.count: "+cartasHijo_Activas.Count);
    }

    public void RegistrarCartaActivada(GameObject spriteCard_Active)
    {
        // Añadir a las listas
        cartasHijo_Activas.Add(spriteCard_Active);
        scripts_Cartas.Add(spriteCard_Active.GetComponent<scr_SpriteCard>());

        Debug.Log("Carta activada. Total activas: " + cartasHijo_Activas.Count);
       
        // Si hay 2 o más cartas activadas, comparar
        if (cartasHijo_Activas.Count >= 2)
        {
            ComprobarMatches();
        }
    }

    private void ComprobarMatches()
    {
        Debug.Log("Comprobando matches...");

        // Comparar todas las cartas activadas entre sí
        for (int i = 0; i < scripts_Cartas.Count; i++)   
        {                                                                                
            for (int j = i + 1; j < scripts_Cartas.Count; j++)//Este doble for sirve para cpmparar todos los objetos dentro de la lista por si mismos, coge i y lo compara por todos los j´s
            {
            
                if (scripts_Cartas[i].ObtenerSprite() == scripts_Cartas[j].ObtenerSprite()) //Compara los sprites de cada carta entre ellos
                {
                    Debug.Log("¡MATCH ENCONTRADO!");

                    //Desactivacion
                    scr_SpriteCard script1 = scripts_Cartas[i];
                    scr_SpriteCard script2 = scripts_Cartas[j];


                    //Remover de los lists
                    scripts_Cartas.RemoveAt(j);
                    scripts_Cartas.RemoveAt(i);
                    cartasHijo_Activas.RemoveAt(j);
                    cartasHijo_Activas.RemoveAt(i);

                    script1.DesactivarPadre();
                    script2.DesactivarPadre();

                    paresEncontrados++;
                    Debug.Log("Pares encontrados: " + paresEncontrados);

                    // Volver a comprobar por si hay más matches
                    if (cartasHijo_Activas.Count >= 2)
                    {
                        ComprobarMatches();
                    }

                    return; // Salir después de encontrar un match
                }
               /* else if (scripts_Cartas[i].ObtenerSprite() != scripts_Cartas[j].ObtenerSprite())
                {
                    GameObject carta_sprite1 = cartasHijo_Activas[i];
                    GameObject carta_sprite2 = cartasHijo_Activas[j];

                    scripts_Cartas.RemoveAt(j);
                    scripts_Cartas.RemoveAt(i);
                    cartasHijo_Activas.RemoveAt(j);
                    cartasHijo_Activas.RemoveAt(i);

                    carta_sprite1.SetActive(false);
                    carta_sprite2.SetActive(false);


                }*/
            }
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class scr_MatchManager : MonoBehaviour
{
    //public List<Sprite> myList_spriteCards;//Lista que contendr� solo los sprites 
    public List<GameObject> cartasHijoActivas_List; //Lista que contendr� las cartas front activa. Si alguna se desactiva la sacaremos de la lista
    public List<scr_SpriteCard> scriptsCartas_List; //lo ponemos con los scripts para saltarnos el meter todo el tiempo el "GetComponent<scr_SpriteCard>()", osea, nos saltamos ese paso.
  
    public int paresEncontrados;


    //AUN  NO LO USO
    [Header("Configuracion de Tiempos")]
    public float tiempoParaVerCartas = 1.5f;
    private bool estaComparando = false; //Sirve como interruptor para saber si ya hay dos cartas en juego, si las hay, se empieza a comparar, por lo que ya no se pueden activar nuevas cartas
    private scr_SpriteCard cartaEnEspera_01, cartaEnEspera_02; //Referencias de las cartas que estan esperando

    //No necesitamos start() O Upadate() ya que estas funciones solo se ejecutan cuando se llaman desde afuera


    private void Update()
    {
      
    }

    public bool PuedeActivarCarta()//Estafuncion es llamada por la carta padre para saber si puede activar las siguientes cartas tras presionar la "U" o se espera.
    {
        if (estaComparando || cartasHijoActivas_List.Count >= 2)
        {
            Debug.Log("No se puede activar carta: limite alcanzado o comparando");
            return false;
        }
        return true;
    }


    public void RegistrarCartaActivada(GameObject spriteCard_Active)
    {
        /*  if (estaComparando)
          {
              Debug.LogWarning("Ya hay una comparacion en proceso, espera...");
              return;
          }

          if (cartasHijoActivas_List.Count >= 2) //Con la funcion "PuedeActivarCarta()"  tanto este if como el anterior deber�an de quedar obsoletos, pues al no poderse activar la carta hijo, no deber�a agregarse nada a ninguna lista.
          {
              Debug.LogWarning("Ya hay 2 cartas activas, no se pueden activar mas");
              return;
          }
        */

        Debug.Log("el scriptsCartas_List.Count: " + scriptsCartas_List.Count);

        // A�adir a las listas
        cartasHijoActivas_List.Add(spriteCard_Active);
        scriptsCartas_List.Add(spriteCard_Active.GetComponent<scr_SpriteCard>());

        Debug.Log("Carta activada. Total activas: " + cartasHijoActivas_List.Count);
       
        // Si hay 2 o mas cartas activadas, comparar
        if (cartasHijoActivas_List.Count >= 2)
        {
            StartCoroutine(CompararCartas());
        }
    }
    
    private IEnumerator CompararCartas() //Se hace la comparacion de las cartas, si es match se activa una animacion en el animator y luego esa animacion activa la funcion de eliminar cartas match(),
                                         //si no es match, se reinicia todo
    {
        estaComparando = true;

        Debug.Log("Comprobando matches...");

        Sprite sprite_01 = scriptsCartas_List[0].ObtenerSprite(); //Referenciamos en una variable el sprite de las dos cartas abiertas.
        Sprite sprite_02 = scriptsCartas_List[1].ObtenerSprite(); //A diferenciade la otra funcion de bucle doble donde teniamos todaas las cartas activas y habia que recorrerlas y compararlas todas.
                                                                  //aqui solo tendremos en la lista las cartas activas, las cuales solo podr�n ser 2.

        //Debug.Log($"Comparando: {sprite_01.name} con {sprite_02.name}"); // $ = Interpolacion de cadenas. Es lo mismo que poner "Debug.Log("Comparando: " + sprite1.name + " con " + sprite2.name);"
                                                                         // Pero es m�s limpio a la vista, osea mas facil de leer
        Debug.Log("Comparando: " + sprite_01 + " con " + sprite_02);

        if (sprite_01 == sprite_02)
        {
            Debug.Log("MATCH ENCONTRADO!");

            cartaEnEspera_01 = scriptsCartas_List[0];//Referenciamos los scripts de las cartas que tenemos en la lista
            cartaEnEspera_02 = scriptsCartas_List[1];

            GameObject cartaPadre_01 = cartaEnEspera_01.transform.parent.gameObject; //Con esta linea le estamos pidiendo que nos de el gameobject del padre del objeto que tenga este script
            GameObject cartaPadre_02 = cartaEnEspera_02.transform.parent.gameObject; //Lo hacemos con la intencion de ingresar a su animacion para desaparcer los pares

            Animator anim_01 = cartaPadre_01.GetComponent<Animator>();
            Animator anim_02 = cartaPadre_02.GetComponent<Animator>();
            /* if (anim1 != null)
             {
                 anim1.SetTrigger("Match");
                 Debug.Log("Animaci�n activada en carta 1");
             }

             if (anim2 != null)
             {
                 anim2.SetTrigger("Match");
                 Debug.Log("Animaci�n activada en carta 2");
             }
            */

            anim_01.SetTrigger("Match_Trigger");
            anim_02.SetTrigger("Match_Trigger");


        }
        else
        {
            Debug.Log("NO son match. Mostrando cartas un momento...");
            yield return new WaitForSeconds(tiempoParaVerCartas);
            Debug.Log("Ocultando cartas...");

            scr_SpriteCard script_01 = scriptsCartas_List[0];
            scr_SpriteCard script_02 = scriptsCartas_List[1];  //Preguntar a chatGPT �No seria mejor hacer esto con las referencias que ya tenemos:  cartaEnEspera_01 y 02
            GameObject hijo_01 = cartasHijoActivas_List[0];
            GameObject hijo_02 = cartasHijoActivas_List[1];

            scriptsCartas_List.Clear();
            cartasHijoActivas_List.Clear();

            hijo_01.SetActive(false);
            hijo_02.SetActive(false);

            hijo_01.transform.parent.GetComponent<scr_Card>().ResetCardActive();
            hijo_02.transform.parent.GetComponent<scr_Card>().ResetCardActive();

            /*GameObject cartaPadre_01 = cartaEnEspera_01.transform.parent.gameObject; //Con esta linea le estamos pidiendo que nos de el gameobject del padre del objeto que tenga este script
            GameObject cartaPadre_02 = cartaEnEspera_02.transform.parent.gameObject; //Lo hacemos con la intencion de ingresar a su animacion para desaparcer los pares

            cartaPadre_01.GetComponent<scr_Card>().card_Active = false;
            cartaPadre_02.GetComponent<scr_Card>().card_Active = false;

            Debug.Log("CartaPadre01: " + cartaPadre_01.GetComponent<scr_Card>().card_Active);
            Debug.Log("CartaPadre02: " + cartaPadre_02.GetComponent<scr_Card>().card_Active); 
            */

            estaComparando = false;
            Debug.Log("Comparacion terminada. Listas limpiadas.");

            
        }
    }
    
    public void EliminarCartasMatch()
    {
        Debug.Log("Animation Event: Eliminando cartas con match");

        scriptsCartas_List.Clear();
        cartasHijoActivas_List.Clear();

        cartaEnEspera_01.DesactivarPadre();
        cartaEnEspera_02.DesactivarPadre();

        paresEncontrados++;
        Debug.Log("Pares encontrados total: " + paresEncontrados);

        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;

        estaComparando = false;
        Debug.Log("Eliminaci�n completada. Sistema listo para nuevas cartas.");
    }

    #region ReiniciarContado
    //Aun no se para que es
    public void ReiniciarContador() //Por si algo
    {
        cartasHijoActivas_List.Clear();
        scriptsCartas_List.Clear();
        paresEncontrados = 0;
        estaComparando = false;
        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;
    }
    #endregion

    #region MatchAnterior
    /* private void ComprobarMatches()
     {

         Debug.Log("Comprobando matches...");

         // Comparar todas las cartas activadas entre s�
         for (int i = 0; i < scriptsCartas_List.Count; i++)
         {
             for (int j = i + 1; j < scriptsCartas_List.Count; j++)//Este doble for sirve para cpmparar todos los objetos dentro de la lista por si mismos, coge i y lo compara por todos los j�s
             {

                 if (scriptsCartas_List[i].ObtenerSprite() == scriptsCartas_List[j].ObtenerSprite()) //Compara los sprites de cada carta entre ellos
                 {
                     Debug.Log("�MATCH ENCONTRADO!");

                     //Desactivacion
                     scr_SpriteCard script_01 = scriptsCartas_List[i];
                     scr_SpriteCard script_02 = scriptsCartas_List[j];


                     //Remover de los lists
                     scriptsCartas_List.RemoveAt(j);
                     scriptsCartas_List.RemoveAt(i);
                     cartasHijoActivas_List.RemoveAt(j);
                     cartasHijoActivas_List.RemoveAt(i);

                     script_01.DesactivarPadre();
                     script_02.DesactivarPadre();

                     paresEncontrados++;
                     Debug.Log("Pares encontrados: " + paresEncontrados);

                     // Volver a comprobar por si hay m�s matches
                     if (cartasHijoActivas_List.Count >= 2)
                     {
                         ComprobarMatches();
                     }

                     return; // Salir despu�s de encontrar un match
                 }

             }
         }
     }*/
    #endregion

}

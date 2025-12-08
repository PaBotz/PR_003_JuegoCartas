using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona las comparaciones de cartas.
/// NO es NetworkBehaviour - solo corre en el servidor.
/// </summary>
public class scr_MatchManager : MonoBehaviour
{
    [Header("Listas de Control")]
    public List<GameObject> cartasHijoActivas_List;
    public List<scr_SpriteCard> scriptsCartas_List;

    [Header("EstadÃ­sticas")]
    public int paresEncontrados;
    public int totalPares; // Se calcula cuando inicia la partida

    [Header("Configuracion de Tiempos")]
    public float tiempoParaVerCartas = 1.5f;

    private bool estaComparando = false;
    private scr_SpriteCard cartaEnEspera_01, cartaEnEspera_02;

    private void Start()
    {
        cartasHijoActivas_List = new List<GameObject>();
        scriptsCartas_List = new List<scr_SpriteCard>();
    }

    /// <summary>
    /// Llamado por el Generator cuando se crean las cartas.
    /// </summary>
    public void InicializarPartida(int numPares)
    {
        totalPares = numPares;
        paresEncontrados = 0;
        cartasHijoActivas_List.Clear();
        scriptsCartas_List.Clear();
        estaComparando = false;
        Debug.Log($"[MatchManager] Partida iniciada. Total pares: {totalPares}");
    }

    public bool PuedeActivarCarta()
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
        Debug.Log("scriptsCartas_List.Count: " + scriptsCartas_List.Count);

        // AÃ±adir a las listas
        cartasHijoActivas_List.Add(spriteCard_Active);
        scriptsCartas_List.Add(spriteCard_Active.GetComponent<scr_SpriteCard>());

        Debug.Log("Carta activada. Total activas: " + cartasHijoActivas_List.Count);

        // Si hay 2 cartas activadas, comparar
        if (cartasHijoActivas_List.Count >= 2)
        {
            StartCoroutine(CompararCartas());
        }
    }

    private IEnumerator CompararCartas()
    {
        estaComparando = true;

        Debug.Log("Comprobando matches...");

        // PequeÃ±a espera para asegurar que los sprites se sincronizaron
        yield return new WaitForSeconds(0.1f);

        Sprite sprite_01 = scriptsCartas_List[0].ObtenerSprite();
        Sprite sprite_02 = scriptsCartas_List[1].ObtenerSprite();

        Debug.Log("Comparando: " + sprite_01 + " con " + sprite_02);

        if (sprite_01 == sprite_02)
        {
            Debug.Log("Â¡MATCH ENCONTRADO!");

            cartaEnEspera_01 = scriptsCartas_List[0];
            cartaEnEspera_02 = scriptsCartas_List[1];

            // Buscar el scr_Card (padre) para activar animaciÃ³n
            scr_Card card_01 = BuscarScriptCard(cartaEnEspera_01.transform);
            scr_Card card_02 = BuscarScriptCard(cartaEnEspera_02.transform);

            // Buscar Animator
            Animator anim_01 = BuscarAnimator(cartaEnEspera_01.transform);
            Animator anim_02 = BuscarAnimator(cartaEnEspera_02.transform);

            if (anim_01 != null) anim_01.SetTrigger("Match_Trigger");
            if (anim_02 != null) anim_02.SetTrigger("Match_Trigger");

            // La animaciÃ³n llamarÃ¡ a EliminarCartasMatch() via Animation Event
        }
        else
        {
            Debug.Log("NO son match. Mostrando cartas un momento...");
            yield return new WaitForSeconds(tiempoParaVerCartas);
            Debug.Log("Ocultando cartas...");

            // Guardar referencias antes de limpiar
            scr_SpriteCard script_01 = scriptsCartas_List[0];
            scr_SpriteCard script_02 = scriptsCartas_List[1];

            // Limpiar listas
            scriptsCartas_List.Clear();
            cartasHijoActivas_List.Clear();

            // IMPORTANTE: Usar los mÃ©todos de red de scr_Card para ocultar
            scr_Card card_01 = BuscarScriptCard(script_01.transform);
            scr_Card card_02 = BuscarScriptCard(script_02.transform);

            if (card_01 != null) card_01.OcultarSpriteEnRed();
            if (card_02 != null) card_02.OcultarSpriteEnRed();

            estaComparando = false;
            Debug.Log("ComparaciÃ³n terminada. Listas limpiadas.");
        }
    }

    /// <summary>
    /// Busca scr_Card subiendo por la jerarquÃ­a.
    /// </summary>
    private scr_Card BuscarScriptCard(Transform desde)
    {
        Transform actual = desde;
        while (actual != null)
        {
            scr_Card card = actual.GetComponent<scr_Card>();
            if (card != null) return card;
            actual = actual.parent;
        }
        return null;
    }

    /// <summary>
    /// Busca Animator subiendo por la jerarquÃ­a.
    /// </summary>
    private Animator BuscarAnimator(Transform desde)
    {
        Transform actual = desde;
        while (actual != null)
        {
            Animator anim = actual.GetComponent<Animator>();
            if (anim != null) return anim;
            actual = actual.parent;
        }
        return null;
    }

    /// <summary>
    /// Llamado por Animation Event cuando termina la animaciÃ³n de match.
    /// </summary>
    public void EliminarCartasMatch()
    {
        Debug.Log("Animation Event: Eliminando cartas con match");

        // Limpiar listas
        scriptsCartas_List.Clear();
        cartasHijoActivas_List.Clear();

        // IMPORTANTE: Usar los mÃ©todos de red de scr_Card para desactivar
        if (cartaEnEspera_01 != null)
        {
            scr_Card card_01 = BuscarScriptCard(cartaEnEspera_01.transform);
            if (card_01 != null) card_01.DesactivarCartaEnRed();
        }

        if (cartaEnEspera_02 != null)
        {
            scr_Card card_02 = BuscarScriptCard(cartaEnEspera_02.transform);
            if (card_02 != null) card_02.DesactivarCartaEnRed();
        }

        paresEncontrados++;
        Debug.Log("Pares encontrados total: " + paresEncontrados);

        // Verificar si terminÃ³ el juego
        if (totalPares > 0 && paresEncontrados >= totalPares)
        {
            Debug.Log("Â¡Â¡Â¡JUEGO TERMINADO!!!");
            // AquÃ­ puedes llamar a un evento o mostrar pantalla de victoria
        }

        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;

        estaComparando = false;
        Debug.Log("EliminaciÃ³n completada. Sistema listo para nuevas cartas.");
    }

    public void ReiniciarContador()
    {
        cartasHijoActivas_List.Clear();
        scriptsCartas_List.Clear();
        paresEncontrados = 0;
        estaComparando = false;
        cartaEnEspera_01 = null;
        cartaEnEspera_02 = null;
    }
}
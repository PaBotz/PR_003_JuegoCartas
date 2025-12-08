using UnityEngine;
using System.Collections.Generic;
// QUITAMOS: using Unity.Netcode; - Ya no es necesario

/// <summary>
/// Maneja el sprite frontal de la carta.
/// IMPORTANTE: Este script ya NO es NetworkBehaviour.
/// La sincronización la maneja el padre (scr_Card).
/// </summary>
public class scr_SpriteCard : MonoBehaviour  // CAMBIADO: Era NetworkBehaviour
{
    [Header("Sprites disponibles")]
    public string sortingLayerName = "Front_Card";
    public int orderInLayer = 1;

    private SpriteRenderer sr;
    private Sprite sprite_Propio;
    private bool spriteYaAsignado = false;

    [HideInInspector] public scr_Generator myGenerator;
    [HideInInspector] public scr_MatchManager myMatchManager;
    public GameObject cartaPadre;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        myGenerator = FindFirstObjectByType<scr_Generator>();
        myMatchManager = FindFirstObjectByType<scr_MatchManager>();

        // Configurar sorting layer
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;
        }
    }

    /// <summary>
    /// Asigna un sprite aleatorio. Solo llamar desde el servidor.
    /// Retorna el índice del sprite para sincronizar.
    /// </summary>
    public int SetRandomSprite()
    {
        if (myGenerator == null || myGenerator.mazo == null || myGenerator.mazo.Count == 0)
        {
            Debug.LogError("[SpriteCard] No hay sprites disponibles en el mazo");
            return -1;
        }

        int randomIndex = Random.Range(0, myGenerator.mazo.Count);
        Sprite spriteElegido = myGenerator.mazo[randomIndex];
        int indiceSprite = myGenerator.sprites_Disponibles.IndexOf(spriteElegido);
        myGenerator.mazo.RemoveAt(randomIndex);

        spriteYaAsignado = true;

        // Aplicar sprite localmente (en el servidor)
        AplicarSprite(indiceSprite);

        return indiceSprite;
    }

    /// <summary>
    /// Aplica un sprite por su índice. 
    /// Llamado por scr_Card en todos los clientes.
    /// </summary>
    public void AplicarSprite(int indiceSprite)
    {
        if (myGenerator == null)
        {
            myGenerator = FindFirstObjectByType<scr_Generator>();
        }

        if (myGenerator != null && indiceSprite >= 0 && indiceSprite < myGenerator.sprites_Disponibles.Count)
        {
            sr.sprite = myGenerator.sprites_Disponibles[indiceSprite];
            sprite_Propio = sr.sprite;
            spriteYaAsignado = true;
            Debug.Log($"[SpriteCard] Sprite asignado: {sprite_Propio.name}");
        }
    }

    public Sprite ObtenerSprite()
    {
        return sprite_Propio;
    }

    /// <summary>
    /// Desactiva el objeto raíz de la carta.
    /// </summary>
    public void DesactivarPadre()
    {
        if (cartaPadre != null)
        {
            cartaPadre.SetActive(false);
            return;
        }

        // Buscar el objeto raíz subiendo por la jerarquía
        Transform raiz = transform;
        while (raiz.parent != null)
        {
            raiz = raiz.parent;
        }
        raiz.gameObject.SetActive(false);
    }

    /// <summary>
    /// Llamado cuando el jugador activa esta carta.
    /// Retorna el índice del sprite si se asignó uno nuevo, -1 si ya tenía.
    /// </summary>
    public int CartaActivada()
    {
        int indiceSprite = -1;

        if (!spriteYaAsignado)
        {
            indiceSprite = SetRandomSprite();
        }

        // Registrar en MatchManager
        if (myMatchManager != null)
        {
            myMatchManager.RegistrarCartaActivada(this.gameObject);
        }
        else
        {
            Debug.LogError("[SpriteCard] No se encontró MatchManager");
        }

        return indiceSprite;
    }

    public void OcultarSprite()
    {
        if (sr != null)
        {
            sr.enabled = false;
        }
    }

    public void MostrarSprite()
    {
        if (sr != null)
        {
            sr.enabled = true;
        }
    }

    public bool TieneSprite()
    {
        return spriteYaAsignado;
    }
}
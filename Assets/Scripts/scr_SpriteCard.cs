using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Maneja el sprite frontal de la carta (la imagen que se revela).
/// Este script NO necesita NetworkObject propio - usa el del padre.
/// La sincronización se hace a través del padre (scr_Card).
/// </summary>
public class scr_SpriteCard : MonoBehaviour
{
    [Header("Configuración de Render")]
    public string sortingLayerName = "Front_Card";
    public int orderInLayer = 1;

    // Componentes
    private SpriteRenderer sr;
    private Sprite sprite_Propio;
    private bool spriteYaAsignado = false;

    // Referencias (se asignan automáticamente)
    [HideInInspector] public scr_Generator myGenerator;
    [HideInInspector] public scr_MatchManager myMatchManager;
    [HideInInspector] public GameObject cartaPadre;

    // Referencia al script del padre para usar sus RPCs
    private scr_Card parentCard;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        myGenerator = FindFirstObjectByType<scr_Generator>();
        myMatchManager = FindFirstObjectByType<scr_MatchManager>();

        // Buscar el scr_Card en los padres
        parentCard = GetComponentInParent<scr_Card>();

        // Configurar sorting layer
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;
        }
    }

    /// <summary>
    /// Asigna un sprite aleatorio del mazo. Solo el servidor ejecuta esto.
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

        // Aplicar localmente en el servidor
        AplicarSprite(indiceSprite);

        return indiceSprite;
    }

    /// <summary>
    /// Aplica un sprite por su índice (llamado desde ClientRpc del padre)
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

    /// <summary>
    /// Devuelve el sprite de esta carta para comparaciones
    /// </summary>
    public Sprite ObtenerSprite()
    {
        return sprite_Propio;
    }

    /// <summary>
    /// Desactiva el objeto raíz completo (toda la carta)
    /// </summary>
    public void DesactivarPadre()
    {
        // Si cartaPadre está asignado en el inspector, usarlo
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
    /// Llamado cuando el jugador activa esta carta (desde scr_Card)
    /// </summary>
    public void CartaActivada(ulong clientIdJugador)
    {
        int indiceSprite = -1;

        if (!spriteYaAsignado)
        {
            indiceSprite = SetRandomSprite();
        }

        // Registrar en MatchManager con el ID del jugador
        if (myMatchManager != null)
        {
            myMatchManager.RegistrarCartaActivada(this.gameObject, clientIdJugador);
        }
        else
        {
            Debug.LogError("[SpriteCard] No se encontró MatchManager");
        }

        // Retornar el índice para que el padre lo sincronice
        if (parentCard != null && indiceSprite >= 0)
        {
            parentCard.SincronizarSpriteIndex(indiceSprite);
        }
    }

    /// <summary>
    /// Oculta el sprite localmente
    /// </summary>
    public void OcultarSprite()
    {
        if (sr != null)
        {
            sr.enabled = false;
        }
    }

    /// <summary>
    /// Muestra el sprite localmente
    /// </summary>
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
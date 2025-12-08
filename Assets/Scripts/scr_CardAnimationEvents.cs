using UnityEngine;

/// <summary>
/// Script auxiliar para manejar Animation Events.
/// Se coloca en el GameObject que tiene el Animator.
/// Notifica al MatchManager cuando termina la animación de match.
/// </summary>
public class scr_CardAnimationEvents : MonoBehaviour
{
    private scr_MatchManager matchManager;

    private void Start()
    {
        matchManager = FindFirstObjectByType<scr_MatchManager>();

        if (matchManager == null)
        {
            Debug.LogWarning("⚠️ No se encontró MatchManager");
        }
    }

    /// <summary>
    /// Llamado por Animation Event al final de la animación "Match"
    /// </summary>
    public void OnMatchAnimationEnd()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<scr_MatchManager>();
        }

        if (matchManager != null)
        {
            matchManager.EliminarCartasMatch();
            Debug.Log("🎬 Animation Event: Notificado al MatchManager");
        }
        else
        {
            Debug.LogError("❌ [AnimationEvent] No se encontró MatchManager");
        }
    }
}
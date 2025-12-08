using UnityEngine;

/// <summary>
/// Script auxiliar que se coloca en el objeto que tiene el Animator.
/// Sirve como intermediario para que el Animation Event pueda
/// notificar al MatchManager cuando termina la animación de match.
/// 
/// IMPORTANTE: Coloca este script en el mismo GameObject que tiene el Animator.
/// </summary>
public class scr_CardAnimationEvents : MonoBehaviour
{
    private scr_MatchManager matchManager;

    void Start()
    {
        matchManager = FindFirstObjectByType<scr_MatchManager>();
    }

    /// <summary>
    /// Este método es llamado por el Animation Event al final
    /// de la animación de "Match".
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
        }
        else
        {
            Debug.LogError("[CardAnimationEvents] No se encontró el MatchManager");
        }
    }
}
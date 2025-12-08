using UnityEngine;

public class scr_CardAnimationEvents : MonoBehaviour
{
    private scr_MatchManager matchManager;

    private void Start()
    {
        matchManager = FindFirstObjectByType<scr_MatchManager>();

        if (matchManager == null)
        {
            Debug.LogWarning("No se encontro MatchManager");
        }
    }

    public void OnMatchAnimationEnd()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<scr_MatchManager>();
        }

        if (matchManager != null)
        {
            matchManager.EliminarCartasMatch();
            Debug.Log("Animation Event: Notificado al MatchManager");
        }
        else
        {
            Debug.LogError("[AnimationEvent] No se encontró MatchManager");
        }
    }
}
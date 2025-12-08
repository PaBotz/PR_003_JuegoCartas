using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class scr_SpriteCard : NetworkBehaviour
{
    [Header("Sprites disponibles")]
    public string sortingLayerName = "Front_Card";
    public int orderInLayer = 1;
    private SpriteRenderer sr;
    private Sprite sprite_Propio;
    private bool spriteYaAsignado = false;
    public scr_Generator myGenerator;
    public scr_MatchManager myMatchManager;
    public GameObject cartaPadre;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        myGenerator = FindFirstObjectByType<scr_Generator>();
        myMatchManager = FindFirstObjectByType<scr_MatchManager>();
    }

    void SetRandomSprite()
    {
        if (!IsServer) return;

        int randomIndex = Random.Range(0, myGenerator.mazo.Count);
        Sprite spriteElegido = myGenerator.mazo[randomIndex];
        int indiceSprite = myGenerator.sprites_Disponibles.IndexOf(spriteElegido); // ✅ AQUÍ estaba el problema
        myGenerator.mazo.RemoveAt(randomIndex);

        spriteYaAsignado = true;

        // ✅ Mostrar el sprite a TODOS
        MostrarSpriteClientRpc(indiceSprite);
    }

    // ✅ AÑADIR: Todos ven el mismo sprite
    [ClientRpc]
    void MostrarSpriteClientRpc(int indiceSprite)
    {
        sr.sprite = myGenerator.sprites_Disponibles[indiceSprite];
        sprite_Propio = sr.sprite;
    }

    public Sprite ObtenerSprite()
    {
        return sprite_Propio;
    }

    public void DesactivarPadre()
    {
        if (!IsServer) return;

        // ✅ AÑADIR: Desactivar para TODOS
        DesactivarPadreClientRpc();
    }

    // ✅ AÑADIR: Todos ven que desaparece
    [ClientRpc]
    void DesactivarPadreClientRpc()
    {
        cartaPadre.SetActive(false);
    }

    public void CartaActivada()
    {
        if (!spriteYaAsignado)
        {
            SetRandomSprite();
        }
        myMatchManager.RegistrarCartaActivada(this.gameObject);
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
}
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class scr_PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerName;

    // NetworkVariable sincronizada automáticamente entre servidor y clientes
    public NetworkVariable<FixedString32Bytes> networkPlayerName =
        new NetworkVariable<FixedString32Bytes>("Default Name",
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public event Action<string> OnNameChanged;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string inputName = ObtenerNombreDelInput();
            networkPlayerName.Value = new FixedString32Bytes(inputName);
        }

        playerName.text = networkPlayerName.Value.ToString();
        networkPlayerName.OnValueChanged += NetworkPlayerName_OnValueChanged;

        OnNameChanged?.Invoke(networkPlayerName.Value.ToString());
    }

    /// <summary>
    /// Obtiene el nombre del InputField sin depender directamente del tipo scr_UIManager
    /// </summary>
    private string ObtenerNombreDelInput()
    {
        // Buscar el UIManager por nombre
        GameObject uiManagerObj = GameObject.Find("UIManager");

        if (uiManagerObj != null)
        {
            // Buscar el InputField en los hijos del UIManager
            TMP_InputField inputField = uiManagerObj.GetComponentInChildren<TMP_InputField>();

            if (inputField != null && !string.IsNullOrEmpty(inputField.text))
            {
                return inputField.text;
            }
        }

        // Fallback: buscar cualquier InputField con el tag o nombre específico
        GameObject inputObj = GameObject.Find("NombreInputField");
        if (inputObj != null)
        {
            TMP_InputField input = inputObj.GetComponent<TMP_InputField>();
            if (input != null && !string.IsNullOrEmpty(input.text))
            {
                return input.text;
            }
        }

        // Si no encuentra nada, usar nombre por defecto
        return "Jugador " + OwnerClientId;
    }

    private void NetworkPlayerName_OnValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerName.text = newValue.Value;
        OnNameChanged?.Invoke(newValue.Value);
    }

    public string GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }
}
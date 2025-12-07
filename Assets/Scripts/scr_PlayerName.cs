using System; // Event Action: 
using TMPro;
using Unity.Collections;
using Unity.Netcode; // Para todo aquello que utilice network
using UnityEngine;


public class scr_PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerName;
      
    public NetworkVariable<FixedString32Bytes> networkPlayerName =  //NetworkVariable = Es una variable sincronizada automáticamente ente el servidor(host) y todos los clientes. tambien es una forma de sincronizar valores sin udar RPCs Manualmente
    new NetworkVariable<FixedString32Bytes>("Default Name",         // FixedString32Bytes es simplemente un string limitado/fijo, lo cual es más seguro de enviar por red 
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); //Aquí indicamos quien puede leer el nombre de los demás y quien es el que tiene permisos para escribir el nombre                                



    public event Action<string> OnNameChanged;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string inputName = FindFirstObjectByType<scr_UIManager>()
                .nombreInputField.text;

            networkPlayerName.Value = new FixedString32Bytes(inputName); 

        } 

        playerName.text = networkPlayerName.Value.ToString();
        networkPlayerName.OnValueChanged += NetworkPlayerName_OnValueChanged; //OnValueChanged = Se utiliza para escuchar cuando una variable de tipo NetworkVariable ha cambiado por alguno de los players
                                                                              

        OnNameChanged?.Invoke(networkPlayerName.Value.ToString()); // ? = null-conditional operator: Lo que hace es activarse si onNameChanged no es null, si es null,pues no pasa nada; if (OnNameChanged != null)
                                                                                                                                                                                           //OnNameChanged.Invoke(newValue);

    }

    private void NetworkPlayerName_OnValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerName.text = newValue.Value;
        OnNameChanged?.Invoke(newValue.Value);
    }

    public String GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }

 

}

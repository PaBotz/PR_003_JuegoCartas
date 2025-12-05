using UnityEngine;
using TMPro;
using Unity.Collections;


public class scr_PlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshPro playerName;

    public NetworkVariable<FixedString32Bytes> networkPlayerName =
    new NetworkVariable<FixedString32Bytes>("Default Name",
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public event Action <string> OnNameChanged;

    
  

 

}

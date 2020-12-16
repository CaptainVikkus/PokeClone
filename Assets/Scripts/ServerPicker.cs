using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerPicker : MonoBehaviour
{
    private TMP_InputField ServerFieldText;

    private void Start()
    {
        ServerFieldText = GetComponent<TMP_InputField>();
    }

    public void SetIP()
    {
        NetworkManager.SetServerIP(ServerFieldText.text);
    }
}

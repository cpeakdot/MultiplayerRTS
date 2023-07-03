using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Button startGameButton;

    private void OnEnable() 
    {
        RTSNetworkManager.ClientOnConnected += HandleClientOnConnect;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
    }

    private void OnDisable() 
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientOnConnect;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
    }

    private void HandleClientOnConnect()
    {
        lobbyUI.SetActive(true);
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            // If you are the host.
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool isTrue)
    {
        startGameButton.gameObject.SetActive(isTrue);
    }
}

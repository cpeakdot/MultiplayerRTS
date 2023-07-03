using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public List<RTSPlayer> players { get; } = new();

    private bool isGameInProgress = false;

    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStartServer()
    {
        players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (players.Count < 2) { return; }

        isGameInProgress = true;

        ServerChangeScene("Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        players.Add(player);

        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        ));

        player.SetPartyOwner(players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Map_"))
        {
            GameOverHandler gameOverHandler = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandler.gameObject);

            foreach (RTSPlayer player in players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        players.Clear();
    }

    #endregion
}

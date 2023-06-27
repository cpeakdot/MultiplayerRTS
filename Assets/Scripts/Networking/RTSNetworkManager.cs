using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawner;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        
        player.SetTeamColor(new Color(
            Random.Range(0, 1),
            Random.Range(0, 1),
            Random.Range(0, 1)
        ));

        GameObject spawnerClone = Instantiate(
            unitSpawner,
            conn.identity.transform.position,
            conn.identity.transform.rotation
        );

        NetworkServer.Spawn(spawnerClone, conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Map_"))
        {
            GameOverHandler gameOverHandler = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandler.gameObject);
        }
    }
}

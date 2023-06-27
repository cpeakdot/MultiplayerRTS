using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int resourcesPerInterval = 10;
    private RTSPlayer player;
    private float interval = 1f;
    private float timer = 0f;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;
    }

   [ServerCallback]
    private void Update() 
    {
        
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            player.SetResources = resourcesPerInterval;
            timer += interval;
        }
    }    
}

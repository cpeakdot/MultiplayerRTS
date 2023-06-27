using System;
using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health;

    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawn;
    public static event Action<UnitBase> ServerOnBaseDespawn;


    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;

        ServerOnBaseSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;

        ServerOnBaseDespawn?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(this.gameObject);
    }

    #endregion

    #region Client

    #endregion
}

using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;
    private List<UnitBase> bases = new();

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawn += ServerHandleBaseSpawn;
        UnitBase.ServerOnBaseDespawn += ServerHandleBaseDeSpawn;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawn -= ServerHandleBaseSpawn;
        UnitBase.ServerOnBaseDespawn -= ServerHandleBaseDeSpawn;
    }

    [Server]
    private void ServerHandleBaseSpawn(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDeSpawn(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) { return; }

        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}

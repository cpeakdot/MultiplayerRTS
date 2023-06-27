using System;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHp = 100;

    [SyncVar(hook = nameof(HandleHealthUpdate))]
    private int currentHP;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHP = maxHp;
        UnitBase.ServerOnPlayerDie += ServerHandlerPlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlerPlayerDie;        
    }

    [Server]
    private void ServerHandlerPlayerDie(int playerId)
    {
        if (connectionToClient.connectionId != playerId) { return; }

        DealDamage(maxHp);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHP == 0) { return; }

        currentHP = Mathf.Max(currentHP - damageAmount, 0);

        if (currentHP != 0) { return; }

        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdate(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHp);
    }

    #endregion
}

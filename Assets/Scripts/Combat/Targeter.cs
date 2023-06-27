using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    [SerializeField] private Targetable target;

    public Targetable GetTarget => target;

    public override void OnStartServer()
    {
        GameOverHandler.ClientOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ClientOnGameOver -= ServerHandleGameOver;
    }

    private void ServerHandleGameOver(string winner)
    {
        ClearTarget();
    }

    [Command]
    public void CmdSetTarget(GameObject targetGO)
    {
        if (!targetGO.TryGetComponent(out Targetable targetable)) { return; }
        target = targetable;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

}

using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange = 10f;

    #region SERVER

    public override void OnStartServer()
    {
        GameOverHandler.ClientOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ClientOnGameOver -= ServerHandleGameOver;
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    } 

    [ServerCallback]
    private void Update() 
    {
        Targetable target = targeter.GetTarget;

        if(target != null)
        {
            if(Vector3.Distance(transform.position, target.transform.position) > chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        if (!agent.hasPath) { return; }

        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    private void ServerHandleGameOver(string winner)
    {
        agent.ResetPath();
    }

    #endregion

    #region CLIENT

    

    #endregion
}

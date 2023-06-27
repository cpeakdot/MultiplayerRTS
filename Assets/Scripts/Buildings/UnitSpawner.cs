using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private Health health;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;
    private float proggressImageVelocity;

    private void Update()
    {
        if(isServer)
        {
            ProduceUnit();
        }
        
        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region SERVER

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        int resources = player.GetResources;

        if (resources < unitPrefab.GetCost) { return; }

        queuedUnits++;

        player.SetResources = unitPrefab.GetCost * -1;
    }

    [Server]
    private void ProduceUnit()
    {
        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) { return; }

        GameObject unitInstance = Instantiate(
            unitPrefab.gameObject,
            spawnTransform.position,
            transform.rotation
        );

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnTransform.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnOffset + spawnTransform.position);

        queuedUnits--;

        unitTimer = 0f;
    }

    #endregion

    #region CLIENT

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOwned) { return; }

        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldValue, int newValue)
    {
        remainingUnitsText.text = newValue.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if(newProgress <unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref proggressImageVelocity,
                0.1f
            );
        }
    }

    #endregion
}

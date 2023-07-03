using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private LayerMask buildingBlockLayer;
    [SerializeField] private float buildingRange = 5f;
    [SerializeField] private List<Unit> myUnits = new();
    [SerializeField] private List<Building> myBuildings = new();

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public event Action<int> ClientOnResourcesUpdated;
    [SerializeField] private Color teamColor;
    public List<Unit> GetMyUnits => myUnits;
    public List<Building> GetMyBuildings => myBuildings;
    public int GetResources => resources;
    public int SetResources {[Server] set { resources += value; } }
    public Color GetTeamColor => teamColor;
    public Transform GetCameraTransform => cameraTransform;

    public bool GetIsPartyOwner => isPartyOwner;

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += UNIT_OnUnitSpawned;
        Unit.ServerOnUnitDespawned += UNIT_OnUnitDespawned;

        Building.ServerOnBuildingSpawn += ServerHandleOnBuildingSpawn;
        Building.ServerOnBuildingDespawn += ServerHandleOnBuildingDespawn;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= UNIT_OnUnitSpawned;
        Unit.ServerOnUnitDespawned -= UNIT_OnUnitDespawned;

        Building.ServerOnBuildingSpawn -= ServerHandleOnBuildingSpawn;
        Building.ServerOnBuildingDespawn -= ServerHandleOnBuildingDespawn;
    }

    [Server]
    public void SetPartyOwner(bool newValue)
    {
        isPartyOwner = newValue;
    }

    [Server]
    public void SetTeamColor(Color color)
    {
        teamColor = color;
    }

    private void ServerHandleOnBuildingSpawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }

    private void ServerHandleOnBuildingDespawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }

    private void UNIT_OnUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }

    private void UNIT_OnUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 position)
    {
        Building buildingToPlace = null;

        for (int i = 0; i < buildings.Length; i++)
        {
            if(buildings[i].GetId == buildingId)
            {
                buildingToPlace = buildings[i];
                break;
            }
        }
        
        if (buildingToPlace == null) { return; }

        if (resources < buildingToPlace.GetPrice) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, position)) { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResources = buildingToPlace.GetPrice * -1;
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }
        Unit.AuthorityOnUnitSpawned += UNIT_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned += UNIT_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawn += AuthorityHandleOnBuildingSpawn;
        Building.AuthorityOnBuildingDespawn += AuthorityHandleOnBuildingDespawn;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).players.Add(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).players.Remove(this);

        if (!isOwned) { return; }

        Unit.AuthorityOnUnitSpawned -= UNIT_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= UNIT_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawn -= AuthorityHandleOnBuildingSpawn;
        Building.AuthorityOnBuildingDespawn -= AuthorityHandleOnBuildingDespawn;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!isOwned) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleOnBuildingSpawn(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleOnBuildingDespawn(Building building)
    {
        myBuildings.Remove(building);
    }

    private void UNIT_AuthorityOnUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void UNIT_AuthorityOnUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void ClientHandleResourcesUpdated(int oldAmount, int newAmount)
    {
        ClientOnResourcesUpdated?.Invoke(newAmount);
    }

    #endregion

    public bool CanPlaceBuilding(BoxCollider buildingBoxCollider, Vector3 position)
    {
        if(Physics.CheckBox(
            position + buildingBoxCollider.center, 
            buildingBoxCollider.size / 2, 
            Quaternion.identity, 
            buildingBlockLayer))
        {
            return false;
        }

        for (int i = 0; i < myBuildings.Count; i++)
        {
            if(Vector3.Distance(myBuildings[i].transform.position, position) <= buildingRange)
            {
                return true;
            }
        }

        return false;
    }
}

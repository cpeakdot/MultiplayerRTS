using System;
using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview;
    [SerializeField] private Sprite icon;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    public static event Action<Building> ServerOnBuildingSpawn;
    public static event Action<Building> ServerOnBuildingDespawn;

    public static event Action<Building> AuthorityOnBuildingSpawn;
    public static event Action<Building> AuthorityOnBuildingDespawn;

    public Sprite GetIcon => icon;
    public int GetId => id;
    public int GetPrice => price;
    public GameObject GetBuildingPreview => buildingPreview;

    #region  Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawn?.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isOwned) { return; }
        AuthorityOnBuildingDespawn?.Invoke(this);
    }


    #endregion

}

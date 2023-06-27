using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class UnitCommandHandler : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler;
    [SerializeField] private LayerMask layerMask;

    private void Start() 
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy() 
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update() 
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        if(hit.collider.TryGetComponent(out Targetable targetable))
        {
            if(targetable.isOwned)
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(targetable);
            return;
        }

        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement.CmdMove(point);
        }
    }

    private void TryTarget(Targetable targetable)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter.CmdSetTarget(targetable.gameObject);
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }
}

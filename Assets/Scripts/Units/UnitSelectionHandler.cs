using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Mirror;
using System;

public class UnitSelectionHandler : MonoBehaviour 
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private RectTransform unitSelectionArea;

    private Vector2 dragStartPosition;
    private RTSPlayer player;
    public List<Unit> SelectedUnits { get; } = new();

    private void Start() 
    {
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy() 
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;    
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update() 
    {
        if(player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }  
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits.ToArray())
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);

        dragStartPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - dragStartPosition.x;
        float areaHeight = mousePosition.y - dragStartPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = dragStartPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            if (!unit.isOwned) { return; }

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);

        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits)
        {
            if (SelectedUnits.Contains(unit)) { continue; }

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x 
                && screenPosition.x < max.x 
                && screenPosition.y > min.y 
                && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderTickness = 10f;
    [SerializeField] private Vector2 screenXLimits;
    [SerializeField] private Vector2 screenZLimits;

    private Vector2 previousInput;

    private InputActions input;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        input = new InputActions();

        input.Player.MoveCamera.performed += SetPreviousInput;
        input.Player.MoveCamera.canceled += SetPreviousInput;

        input.Enable();
    }

    private void SetPreviousInput(InputAction.CallbackContext  ctx) 
    {
        previousInput = ctx.ReadValue<Vector2>();
    }

    [ClientCallback]
    private void Update() 
    {
        if (!isOwned || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderTickness)
            {
                cursorMovement.z += 1;
            }
            else if(cursorPosition.y <= screenBorderTickness)
            {
                cursorMovement.z -= 1;
            }

            if(cursorPosition.x >= Screen.width - screenBorderTickness)
            {
                cursorMovement.x += 1;
            }
            else if(cursorPosition.x <= screenBorderTickness)
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            pos += new Vector3(previousInput.x, 0, previousInput.y) * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;
    }

}

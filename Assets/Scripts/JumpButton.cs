using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this script directly to your UI Jump Button!
/// It automatically handles detecting when you hold down and let go of the button,
/// and tells the PlayerController to continuously jump.
/// </summary>
public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player != null)
        {
            player.PointerDownJump();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player != null)
        {
            player.PointerUpJump();
        }
    }
}

using UnityEngine;

public class DropdownList : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    private void OnEnable()
    {
        canvas.sortingLayerName = "Popups";
    }
}
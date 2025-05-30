using System;
using UnityEngine;

public class DropdownList : MonoBehaviour
{
    public string SortingLayerName = "Overlay";
    private Canvas canvas;

    private void OnEnable()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingLayerName = SortingLayerName;
    }
}
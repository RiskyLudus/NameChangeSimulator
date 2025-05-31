using System;
using UnityEngine;

public class DropdownList : MonoBehaviour
{
    private Canvas canvas;

    private void OnEnable()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingLayerName = "Popups";
    }
}
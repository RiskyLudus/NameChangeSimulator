using System;
using System.Collections.Generic;
using System.IO;
using Anarchy.Shared;
using iTextSharp.text.pdf;
using SkiaSharp;
using UnityEngine;
using UnityEngine.UI;

public class PDFViewerController : MonoBehaviour
{
    public List<Texture2D> pdfPages = new List<Texture2D>(); // Store PDF pages as Texture2D

    [SerializeField] private GameObject container;
    [SerializeField] private Transform layoutContainer;
    [SerializeField] private GameObject pdfImageTemplate;

    private int _totalPageCount = 0;

    private void OnEnable()
    {
        ConstructBindings.Send_PDFViewerData_Load?.AddListener(OnLoad);
    }

    private void OnDisable()
    {
        ConstructBindings.Send_PDFViewerData_Load?.RemoveListener(OnLoad);
    }

    /// <summary>
    /// Handles incoming raw PDF data.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF data as a byte array.</param>
    private void OnLoad(byte[] pdfBytes)
    {
        _totalPageCount = GetPDFPageCount(pdfBytes);
        Debug.Log($"Total pages in PDF: {_totalPageCount}");

        if (_totalPageCount > 0)
        {
            for (int i = 1; i <= _totalPageCount; i++) // PDF pages are 1-based
            {
                Debug.Log($"Rendering Page {i}");
                LoadPDFPageAsImage(pdfBytes, i);
                var image = Instantiate(pdfImageTemplate, layoutContainer);
                image.GetComponent<RawImage>().texture = pdfPages[i - 1];
                image.GetComponent<RawImage>().SetNativeSize();
            }
        }

        container.gameObject.SetActive(true);
    }

    /// <summary>
    /// Gets the total number of pages in a PDF from raw data.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF data as a byte array.</param>
    /// <returns>Total number of pages in the PDF.</returns>
    public int GetPDFPageCount(byte[] pdfBytes)
    {
        try
        {
            using (var pdfStream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(pdfStream))
            {
                return reader.NumberOfPages;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting PDF page count: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Renders a PDF page to an image using SkiaSharp and adds it to the Texture2D list.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF data as a byte array.</param>
    /// <param name="pageNumber">Page number to render.</param>
    private void LoadPDFPageAsImage(byte[] pdfBytes, int pageNumber)
    {
        // Render PDF to SKBitmap
        SKBitmap bitmap = RenderPDFPageToBitmap(pdfBytes, pageNumber);
        if (bitmap == null)
        {
            Debug.LogError($"Failed to render PDF page {pageNumber}.");
            return;
        }

        // Convert SKBitmap to Texture2D
        Texture2D texture = SKBitmapToTexture2D(bitmap);
        if (texture != null)
        {
            pdfPages.Add(texture);
            Debug.Log($"PDF Page {pageNumber} rendered successfully.");
        }
    }

    /// <summary>
    /// Renders a specific PDF page to an SKBitmap using SkiaSharp.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF data as a byte array.</param>
    /// <param name="pageNumber">Page number to render.</param>
    /// <returns>Rendered SKBitmap of the page.</returns>
    private SKBitmap RenderPDFPageToBitmap(byte[] pdfBytes, int pageNumber)
    {
        try
        {
            using (var pdfStream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(pdfStream))
            {
                if (pageNumber < 1 || pageNumber > reader.NumberOfPages)
                {
                    Debug.LogError($"Invalid page number {pageNumber}. Total pages: {reader.NumberOfPages}");
                    return null;
                }

                // SkiaSharp Canvas to render PDF
                SKBitmap bitmap = new SKBitmap(612, 792); // Standard PDF page size
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.White);

                // Render the page (placeholder)
                canvas.DrawText($"PDF Page {pageNumber}", 50, 50, new SKPaint { Color = SKColors.Black, TextSize = 24 });

                return bitmap;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error rendering PDF page {pageNumber}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts an SKBitmap to a Unity Texture2D.
    /// </summary>
    private Texture2D SKBitmapToTexture2D(SKBitmap skBitmap)
    {
        Texture2D texture = new Texture2D(skBitmap.Width, skBitmap.Height, TextureFormat.RGBA32, false);
        for (int y = 0; y < skBitmap.Height; y++)
        {
            for (int x = 0; x < skBitmap.Width; x++)
            {
                SKColor color = skBitmap.GetPixel(x, y);
                texture.SetPixel(x, skBitmap.Height - y - 1, new Color32(color.Red, color.Green, color.Blue, color.Alpha));
            }
        }
        texture.Apply();
        return texture;
    }
}

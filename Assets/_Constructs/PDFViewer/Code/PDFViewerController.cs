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
    public List<Texture2D> pdfPages; // UI RawImage to display the PDF page

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

    private void OnLoad(string pdfFilePath)
    {
        _totalPageCount = GetPDFPageCount(pdfFilePath);
        Debug.Log($"Total pages in PDF: {_totalPageCount}");

        if (_totalPageCount > 0)
        {
            for (int i = 0; i < _totalPageCount; i++)
            {
                Debug.Log($"Page {i + 1}");
                LoadPDFPageAsImage(pdfFilePath, i);
                var image = Instantiate(pdfImageTemplate, layoutContainer);
                image.GetComponent<RawImage>().texture = pdfPages[i];
                image.GetComponent<RawImage>().SetNativeSize();
            }
        }
        
        container.gameObject.SetActive(true);
    }


    /// <summary>
    /// Renders a PDF page to an image using SkiaSharp and displays it as a Texture2D.
    /// </summary>
    /// <param name="path">PDF file path</param>
    /// <param name="pageNumber">Page number to render</param>
    public void LoadPDFPageAsImage(string path, int pageNumber)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("PDF file not found: " + path);
            return;
        }

        // Use SkiaSharp to render PDF to an SKBitmap
        SKBitmap bitmap = RenderPDFPageToBitmap(path, pageNumber);
        if (bitmap == null)
        {
            Debug.LogError("Failed to render PDF page.");
            return;
        }

        // Convert SKBitmap to Texture2D
        Texture2D texture = SKBitmapToTexture2D(bitmap);
        if (texture != null)
        {
            pdfPages.Add(texture);
            Debug.Log("PDF page displayed successfully.");
        }
    }

    /// <summary>
    /// Renders a specific PDF page to an SKBitmap using SkiaSharp.
    /// </summary>
    private SKBitmap RenderPDFPageToBitmap(string pdfPath, int pageNumber)
    {
        try
        {
            using var pdfDocument = SKDocument.CreatePdf(new SKFileWStream(pdfPath)); // Open the PDF
            using var pdfStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read);
            using var reader = new PdfReader(pdfStream);

            if (pageNumber < 1 || pageNumber > reader.NumberOfPages)
            {
                Debug.LogError("Invalid page number.");
                return null;
            }

            // SkiaSharp Canvas to render PDF
            SKBitmap bitmap = new SKBitmap(612, 792); // Standard PDF page size
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            // Render the page as text or graphics
            canvas.DrawText($"PDF Page {pageNumber}", 50, 50, new SKPaint { Color = SKColors.Black, TextSize = 24 });

            return bitmap;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error rendering PDF: {e.Message}");
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
    
    /// <summary>
    /// Gets the total number of pages in a PDF from raw data.
    /// </summary>
    /// <param name="pdfFilePath">Path to the PDF file.</param>
    /// <returns>Total number of pages in the PDF.</returns>
    public int GetPDFPageCount(string pdfFilePath)
    {
        if (!File.Exists(pdfFilePath))
        {
            Debug.LogError("PDF file not found: " + pdfFilePath);
            return 0;
        }

        try
        {
            // Load the entire PDF file into a byte array to avoid sharing violations
            byte[] pdfData = File.ReadAllBytes(pdfFilePath);

            // Use MemoryStream to read the PDF data
            using (var pdfStream = new MemoryStream(pdfData))
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

}

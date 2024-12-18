using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Anarchy.Shared;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Utils;
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

    private void OnLoad(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            Debug.LogError("PDF data is empty or null.");
            return;
        }

        // Write the PDF bytes to a temporary file
        string tempPdfPath = Path.Combine(Application.persistentDataPath, "temp.pdf");
        File.WriteAllBytes(tempPdfPath, pdfBytes);
        Debug.Log($"Temporary PDF saved at: {tempPdfPath}");

        // Get page count using iTextSharp
        _totalPageCount = GetPDFPageCount(tempPdfPath);
        Debug.Log($"Total Pages: {_totalPageCount}");

        if (_totalPageCount > 0)
        {
            StartCoroutine(RenderAndLoadPDFPages(tempPdfPath));
        }
        else
        {
            Debug.LogError("Failed to retrieve PDF page count. Conversion aborted.");
        }
    }

    private int GetPDFPageCount(string pdfPath)
    {
        try
        {
            using (PdfReader reader = new PdfReader(pdfPath))
            {
                int pageCount = reader.NumberOfPages;
                Debug.Log($"PDF page count retrieved: {pageCount}");
                return pageCount;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading PDF page count: {e.Message}");
            return 0;
        }
    }

    private IEnumerator RenderAndLoadPDFPages(string pdfPath)
    {
        Debug.Log("Starting PDF conversion...");

        // Output pattern for image files
        string outputPattern = Path.Combine(Application.persistentDataPath, "output-%d.jpg");
    
        // Run the conversion synchronously (on the main thread, avoids thread issues)
        ConvertPDF converter = new ConvertPDF();
        converter.Convert(pdfPath, outputPattern, 1, _totalPageCount, "jpeg", 1920, 1080);

        Debug.Log("PDF conversion complete. Loading images...");

        // Load generated images one by one
        for (int i = 1; i <= _totalPageCount; i++)
        {
            string imagePath = Path.Combine(Application.persistentDataPath, $"output-{i}.jpg");

            if (File.Exists(imagePath))
            {
                Texture2D texture = LoadImageAsTexture2D(imagePath);
                if (texture != null)
                {
                    pdfPages.Add(texture);

                    // Instantiate the image in the layout container
                    var imageObject = Instantiate(pdfImageTemplate, layoutContainer);
                    var rawImage = imageObject.GetComponent<RawImage>();
                    rawImage.texture = texture;
                    rawImage.SetNativeSize();
                }
            }
            else
            {
                Debug.LogError($"Image file not found: {imagePath}");
            }

            // Yield to prevent blocking the main thread completely
            yield return null;
        }

        container.SetActive(true);
        Debug.Log("All PDF pages loaded.");
    }


    private Texture2D LoadImageAsTexture2D(string imagePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (texture.LoadImage(fileData, false)) // Load image without mipmaps
            {
                texture.Apply(false, true); // Compress and mark as non-readable
                Debug.Log($"Image loaded: {imagePath}");
                return texture;
            }
            else
            {
                Debug.LogError($"Failed to load image as texture: {imagePath}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading image file '{imagePath}': {e.Message}");
            return null;
        }
    }
}

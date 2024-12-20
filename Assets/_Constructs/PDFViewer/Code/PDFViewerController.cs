using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        OnLoadAsync(pdfBytes, Application.persistentDataPath);
    }

    private async void OnLoadAsync(byte[] pdfBytes, string persistentPath)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            Debug.LogError("PDF data is empty or null.");
            return;
        }

        // Write the PDF bytes to a temporary file
        string tempPdfPath = Path.Combine(persistentPath, "temp.pdf");
        await File.WriteAllBytesAsync(tempPdfPath, pdfBytes);
        Debug.Log($"Temporary PDF saved at: {tempPdfPath}");

        // Get page count using iTextSharp
        _totalPageCount = GetPDFPageCount(tempPdfPath);
        ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(0, _totalPageCount);
        Debug.Log($"Total Pages: {_totalPageCount}");

        if (_totalPageCount > 0)
        {
            await RenderAndLoadPDFPagesAsync(tempPdfPath, persistentPath);
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

    private async Task RenderAndLoadPDFPagesAsync(string pdfPath, string persistentPath)
    {
        Debug.Log("Starting PDF conversion...");

        // Output pattern for image files
        string outputPattern = Path.Combine(persistentPath, "output-%d.jpg");

        // Run the conversion on a separate thread to avoid blocking the main thread
        await Task.Run(() =>
        {
            ConvertPDF converter = new ConvertPDF();
            converter.Convert(pdfPath, outputPattern, 1, _totalPageCount, "jpeg", 1063, 1375);
        });

        Debug.Log("PDF conversion complete. Loading images...");

        // Load generated images one by one
        for (int i = 1; i <= _totalPageCount; i++)
        {
            string imagePath = Path.Combine(persistentPath, $"output-{i}.jpg");

            if (File.Exists(imagePath))
            {
                // Read the image data on a background thread
                byte[] imageData = await File.ReadAllBytesAsync(imagePath);
                
                ConstructBindings.Send_ProgressBarData_UpdateProgress?.Invoke(i);

                // Process the image creation and UI update on the main thread
                await LoadImageOnMainThread(imageData, imagePath);
            }
            else
            {
                Debug.LogError($"Image file not found: {imagePath}");
            }
        }

        container.SetActive(true);
        ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
        Debug.Log("All PDF pages loaded.");
    }

    private async Task LoadImageOnMainThread(byte[] imageData, string imagePath)
    {
        await Task.Yield(); // Ensure this runs on the main thread
        try
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (texture.LoadImage(imageData, false)) // Load image without mipmaps
            {
                texture.Apply(false, true); // Compress and mark as non-readable
                Debug.Log($"Image loaded: {imagePath}");

                pdfPages.Add(texture);
                
                // Instantiate the image in the layout container on the main thread
                CreateImageObject(texture);
            }
            else
            {
                Debug.LogError($"Failed to load image as texture: {imagePath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading image as texture: {e.Message}");
        }
    }

    private void CreateImageObject(Texture2D texture)
    {
        if (texture == null) return;

        var imageObject = Instantiate(pdfImageTemplate, layoutContainer);
        var rawImage = imageObject.GetComponent<RawImage>();
        rawImage.texture = texture;
    }
}

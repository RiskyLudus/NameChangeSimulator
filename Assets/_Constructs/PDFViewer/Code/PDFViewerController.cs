using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anarchy.Shared;
using iTextSharp.text.pdf;
using NameChangeSimulator.Shared.Utils;
using NameChangeSimulator.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PDFViewerController : MonoBehaviour
{
	private const string TEMP_PDF_FILE = "temp.pdf";
	public List<Texture2D> pdfPages = new List<Texture2D>(); // Store PDF pages as Texture2D
    [SerializeField] private GameObject container;
    [SerializeField] private RawImage prevPageImage, nextPageImage, mainPageImage;
    [SerializeField] private GameObject waitingText;
    [SerializeField] private GameObject celebrationObject;

    private int _totalPageCount = 0;
    private string _tempPDFPath;
    private int _currentPage = 0;

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
        waitingText.SetActive(true);
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
        _tempPDFPath = Path.Combine(persistentPath, TEMP_PDF_FILE);
        await File.WriteAllBytesAsync(_tempPDFPath, pdfBytes);
        Debug.Log($"Temporary PDF saved at: {_tempPDFPath}");

        // Get page count using iTextSharp
        _totalPageCount = GetPDFPageCount(_tempPDFPath);
        ConstructBindings.Send_ProgressBarData_ShowProgressBar?.Invoke(0, _totalPageCount);
        Debug.Log($"Total Pages: {_totalPageCount}");

        if (_totalPageCount > 0)
        {
            await RenderAndLoadPDFPagesAsync(_tempPDFPath, persistentPath);
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

        waitingText.SetActive(false);
        
        _currentPage = 0;
        LoadCarousel();
        
        container.SetActive(true);
        ConstructBindings.Send_ProgressBarData_CloseProgressBar?.Invoke();
        Debug.Log("All PDF pages loaded.");

        StartCoroutine(PlayCelebrationCoroutine());
    }

    private IEnumerator PlayCelebrationCoroutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        celebrationObject.SetActive(true);
        AudioManager.Instance.PlayCelebrate_SFX();
    }

    private void LoadCarousel()
    {
        if (_currentPage == pdfPages.Count)
        {
            _currentPage = 0;
            
        } else if (_currentPage < 0)
        {
            _currentPage = pdfPages.Count - 1;
        }
        
        int nextPage = _currentPage + 1;
        if (nextPage >= pdfPages.Count)
        {
            nextPage = 0;
        }
        
        int previousPage = _currentPage - 1;
        if (previousPage < 0)
        {
            previousPage = pdfPages.Count - 1;
        }
        
        
        prevPageImage.texture = pdfPages[previousPage];
        nextPageImage.texture = pdfPages[nextPage];
        mainPageImage.texture = pdfPages[_currentPage];
    }

    private async Task LoadImageOnMainThread(byte[] imageData, string imagePath)
    {
        AudioManager.Instance.PlayForm_SFX();
        await Task.Yield(); // Ensure this runs on the main thread
        try
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (texture.LoadImage(imageData, false)) // Load image without mipmaps
            {
                texture.Apply(false, true); // Compress and mark as non-readable
                Debug.Log($"Image loaded: {imagePath}");

                pdfPages.Add(texture);
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

    public void Save()
    {
        try
        {
            // Get the path to the desktop (works on Windows, Mac, etc.)
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

            // Decide on a filename for the saved PDF
            string newFileName = "NameChangeForm.pdf";
            string newFilePath = Path.Combine(desktopPath, newFileName);

            // Make sure we actually have a PDF to save
            if (!File.Exists(_tempPDFPath))
            {
                Debug.LogError("No PDF file found at _tempPDFPath to save!");
                return;
            }

            // Copy the file to Desktop, overwriting if the file already exists
            File.Copy(_tempPDFPath, newFilePath, true);
            Debug.Log($"PDF saved to Desktop: {newFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save PDF to Desktop: {e.Message}");
        }
        
        ConstructBindings.Send_DialogueData_Load?.Invoke("Ending");
        container.SetActive(false);
    }

    public void Redo()
    {
        SceneManager.LoadScene(0);
    }

    public void NextPage()
    {
        _currentPage++;
        LoadCarousel();
    }

    public void PreviousPage()
    {
        _currentPage--;
        LoadCarousel();
    }
}

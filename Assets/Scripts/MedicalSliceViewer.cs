using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla UNA vista (axial, coronal o sagital) de un órgano.
/// Carga todas las imágenes JPG de una carpeta dentro de Resources
/// y permite recorrerlas con un Slider.
///
/// SETUP EN UNITY:
/// 1. Crea un GameObject (ej. "AxialViewer") dentro de tu Canvas.
/// 2. Agrégale este script.
/// 3. Arrastra el RawImage donde se mostrará el corte, y el Slider que lo controla.
/// 4. Las imágenes deben estar en: Assets/Resources/DICOM/<organo>/<vista>/
///    Ejemplo: Assets/Resources/DICOM/estomago/axial/estomago_axial_0000.jpg
/// </summary>
public class MedicalSliceViewer : MonoBehaviour
{
    [Header("Referencias UI")]
    public RawImage displayImage;
    public Slider slider;
    public Text sliceLabel; // opcional, muestra "Corte 12 / 80"

    [Header("Config")]
    [Tooltip("Nombre de la vista: debe coincidir con el nombre de la subcarpeta (axial, coronal, sagital)")]
    public string viewName = "axial";

    private List<Texture2D> slices = new List<Texture2D>();
    private string currentOrgan = "";

    /// <summary>
    /// Llama a esto (desde el botón del órgano, o desde un manager) para cargar
    /// las imágenes de esa vista para el órgano indicado.
    /// organName debe ser el nombre exacto de la carpeta, ej: "estomago", "higado", "pancreas", "vesicula"
    /// </summary>
    public void LoadOrgan(string organName)
    {
        currentOrgan = organName;
        string resourcePath = $"DICOM/{organName}/{viewName}";

        // Carga TODAS las texturas de esa carpeta (Unity ordena alfabéticamente,
        // por eso el nombre de archivo con ceros a la izquierda, ej. _0000, _0001, es importante)
        Texture2D[] loaded = Resources.LoadAll<Texture2D>(resourcePath);

        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"[MedicalSliceViewer] No se encontraron imágenes en Resources/{resourcePath}. " +
                              $"Verifica que la carpeta exista y las imágenes estén ahí.");
            slices.Clear();
            if (displayImage != null) displayImage.texture = null;
            if (slider != null) { slider.maxValue = 0; slider.value = 0; }
            UpdateLabel(0, 0);
            return;
        }

        // Unity a veces no respeta el orden de archivo al cargar con LoadAll,
        // así que ordenamos manualmente por nombre para asegurar la secuencia correcta.
        slices = loaded.OrderBy(t => t.name).ToList();

        if (slider != null)
        {
            slider.wholeNumbers = true;
            slider.minValue = 0;
            slider.maxValue = slices.Count - 1;
            slider.value = 0;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        ShowSlice(0);
    }

    private void OnSliderChanged(float value)
    {
        ShowSlice(Mathf.RoundToInt(value));
    }

    private void ShowSlice(int index)
    {
        if (slices.Count == 0) return;
        index = Mathf.Clamp(index, 0, slices.Count - 1);

        if (displayImage != null)
            displayImage.texture = slices[index];

        UpdateLabel(index, slices.Count);
    }

    private void UpdateLabel(int index, int total)
    {
        if (sliceLabel != null)
            sliceLabel.text = total > 0 ? $"Corte {index + 1} / {total}" : "Sin imágenes";
    }
}

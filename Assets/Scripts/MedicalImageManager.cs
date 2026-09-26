using UnityEngine;

/// <summary>
/// Conecta la selección de un órgano (desde un botón) con los 3 visores
/// (axial, coronal, sagital) para que carguen las imágenes de ese órgano.
///
/// SETUP EN UNITY:
/// 1. Crea un GameObject vacío (ej. "MedicalImageManager") y agrégale este script.
/// 2. Arrastra los 3 objetos que tienen el script MedicalSliceViewer
///    (el de axial, el de coronal, el de sagital) a los 3 campos de abajo.
/// 3. En cada botón de órgano (Boton_Estomago, Boton_Higado, etc.), en su evento
///    OnClick (además de la llamada que ya tienes para mostrar/ocultar el modelo 3D),
///    agrega una llamada extra a: MedicalImageManager.SelectOrgan("estomago")
///    (o "higado", "pancreas", "vesicula" según el botón).
/// </summary>
public class MedicalImageManager : MonoBehaviour
{
    [Header("Visores (arrastra los 3 GameObjects con MedicalSliceViewer)")]
    public MedicalSliceViewer axialViewer;
    public MedicalSliceViewer coronalViewer;
    public MedicalSliceViewer sagitalViewer;

    /// <summary>
    /// Llama a este método desde el botón del órgano (ej: "estomago", "higado", "pancreas", "vesicula")
    /// </summary>
    public void SelectOrgan(string organName)
    {
        if (axialViewer != null) axialViewer.LoadOrgan(organName);
        if (coronalViewer != null) coronalViewer.LoadOrgan(organName);
        if (sagitalViewer != null) sagitalViewer.LoadOrgan(organName);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja mostrar/ocultar modelos 3D (organos o segmentaciones).
/// Arrastra este script a un GameObject vacio (ej. "GameManager")
/// y asigna la lista "models" en el Inspector.
/// </summary>
public class ModelVisibilityManager : MonoBehaviour
{
    [Tooltip("Lista de modelos (organos, arterias, segmentaciones, etc). Por ahora pueden ser cubos/esferas de prueba.")]
    public List<GameObject> models = new List<GameObject>();

    [Tooltip("Si esta activo, al mostrar 'ShowOnly' se ocultan automaticamente los demas.")]
    public bool exclusiveMode = false;

    [Header("Focus (agrandar al enfocar)")]
    [Tooltip("Cuanto se agranda el modelo enfocado respecto a su tamano original. 1.5 = 50% mas grande.")]
    public float focusScaleMultiplier = 1.5f;

    // Guarda el scale original de cada modelo para poder restaurarlo despues de un Focus.
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private int focusedIndex = -1;

    private void Start()
    {
        // Guarda los tamanos originales antes de tocar nada.
        foreach (var model in models)
        {
            if (model != null && !originalScales.ContainsKey(model))
            {
                originalScales[model] = model.transform.localScale;
            }
        }

        // Opcional: arranca con todo visible. Cambia si prefieres arrancar vacio.
        ShowAll();
    }

    /// <summary>
    /// Prende/apaga un modelo especifico segun su indice en la lista.
    /// Usar esto en el OnClick() de un boton MRTK.
    /// </summary>
    public void ToggleModel(int index)
    {
        if (!IsValidIndex(index)) return;

        GameObject model = models[index];
        model.SetActive(!model.activeSelf);
    }

    /// <summary>
    /// Muestra un modelo especifico. Si exclusiveMode esta activo,
    /// oculta todos los demas primero (util para segmentaciones que
    /// se ven de una en una).
    /// </summary>
    public void ShowOnly(int index)
    {
        if (!IsValidIndex(index)) return;

        if (exclusiveMode)
        {
            HideAll();
        }

        models[index].SetActive(true);
    }

    /// <summary>
    /// Oculta un modelo especifico.
    /// </summary>
    public void HideModel(int index)
    {
        if (!IsValidIndex(index)) return;
        models[index].SetActive(false);
    }

    /// <summary>
    /// Muestra todos los modelos de la lista.
    /// </summary>
    public void ShowAll()
    {
        foreach (var model in models)
        {
            if (model != null) model.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta todos los modelos de la lista.
    /// </summary>
    public void HideAll()
    {
        foreach (var model in models)
        {
            if (model != null) model.SetActive(false);
        }
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= models.Count)
        {
            Debug.LogWarning($"ModelVisibilityManager: indice {index} fuera de rango (lista tiene {models.Count} modelos).");
            return false;
        }
        return true;
    }

    /// <summary>
    /// "El doctor quiere ver esta parte": oculta los demas modelos y agranda
    /// el seleccionado usando focusScaleMultiplier. Usar esto en el OnClick()
    /// de un boton en vez de ToggleModel cuando quieras este comportamiento.
    /// </summary>
    public void FocusModel(int index)
    {
        if (!IsValidIndex(index)) return;

        // Si ya estaba enfocado este mismo modelo, un segundo click quita el foco
        // (vuelve a mostrar todos en su tamano original).
        if (focusedIndex == index)
        {
            ClearFocus();
            return;
        }

        // Restaura el tamano del modelo enfocado anteriormente, si habia uno.
        if (focusedIndex != -1)
        {
            RestoreOriginalScale(focusedIndex);
        }

        HideAll();

        GameObject model = models[index];
        model.SetActive(true);

        if (originalScales.TryGetValue(model, out Vector3 baseScale))
        {
            model.transform.localScale = baseScale * focusScaleMultiplier;
        }

        focusedIndex = index;
    }

    /// <summary>
    /// Quita el foco actual: vuelve a mostrar todos los modelos y restaura
    /// el tamano original del que estaba agrandado.
    /// </summary>
    public void ClearFocus()
    {
        if (focusedIndex != -1)
        {
            RestoreOriginalScale(focusedIndex);
            focusedIndex = -1;
        }

        ShowAll();
    }

    private void RestoreOriginalScale(int index)
    {
        if (!IsValidIndex(index)) return;

        GameObject model = models[index];
        if (model != null && originalScales.TryGetValue(model, out Vector3 baseScale))
        {
            model.transform.localScale = baseScale;
        }
    }
}
using UnityEngine;

public class SurgicalCutManager : MonoBehaviour
{
    [Header("SISTEMA DE CORTE")]

    public CrossSectionPlaneSync planeSync;

    public Transform planeTransform;


    [Header("RENDERERS DEL SISTEMA")]

    [Tooltip("Renderer que genera la superficie interna del corte.")]
    public Renderer capRenderer;

    [Tooltip("Renderer del borde visual del plano.")]
    public Renderer borderRenderer;


    [Header("MANIPULADOR")]

    [Tooltip("Normalmente HandleCorte.")]
    public GameObject handleObject;


    [Header("OPCIONES")]

    public bool keepBorderVisibleWhenCutOff = true;


    private bool planeVisualVisible = false;


    private Vector3 initialLocalPosition;

    private Quaternion initialLocalRotation;

    private Vector3 initialLocalScale;


    // ==========================================
    // INICIALIZACION
    // ==========================================

    private void Awake()
    {
        if (
            planeTransform == null &&
            planeSync != null
        )
        {
            planeTransform =
                planeSync.transform;
        }


        if (planeTransform != null)
        {
            initialLocalPosition =
                planeTransform.localPosition;

            initialLocalRotation =
                planeTransform.localRotation;

            initialLocalScale =
                planeTransform.localScale;
        }
    }


    private void Start()
    {
        planeVisualVisible = false;
        UpdatePlaneVisuals();
    }


    // ==========================================
    // CUT
    // ==========================================

    public void EnableCut()
    {
        SetCut(true);
    }


    public void DisableCut()
    {
        SetCut(false);
    }


    public void ToggleCut()
    {
        if (planeSync == null)
        {
            return;
        }

        SetCut(
            !planeSync.IsCutEnabled
        );
    }


    public void SetCut(bool enabled)
    {
        if (planeSync != null)
        {
            planeSync.SetCutEnabled(
                enabled
            );
        }


        // El CAP solo debe existir
        // cuando el corte esta activo.

        if (capRenderer != null)
        {
            capRenderer.enabled =
                enabled;
        }


        UpdatePlaneVisuals();
    }


    // ==========================================
    // TARGETS
    // ==========================================

    public void TargetAll()
    {
        if (planeSync == null)
        {
            return;
        }

        planeSync.SelectAllOrgans();
    }


    public void TargetStomach()
    {
        SelectTarget(0);
    }


    public void TargetLiver()
    {
        SelectTarget(1);
    }


    public void TargetPancreas()
    {
        SelectTarget(2);
    }


    public void TargetGallbladder()
    {
        SelectTarget(3);
    }


    public void SelectTarget(int index)
    {
        if (planeSync == null)
        {
            return;
        }

        planeSync.SelectOrgan(
            index
        );
    }


    // ==========================================
    // SURGICAL VIEW
    // ==========================================

    public void EnableSurgicalView()
    {
        if (planeSync != null)
        {
            planeSync.EnableSurgicalView();
        }
    }


    public void DisableSurgicalView()
    {
        if (planeSync != null)
        {
            planeSync.DisableSurgicalView();
        }
    }


    public void ToggleSurgicalView()
    {
        if (planeSync != null)
        {
            planeSync.ToggleSurgicalView();
        }
    }


    // ==========================================
    // SHOW / HIDE PLANE
    // ==========================================

    public void ShowPlaneVisual()
    {
        planeVisualVisible = true;

        UpdatePlaneVisuals();
    }


    public void HidePlaneVisual()
    {
        planeVisualVisible = false;

        UpdatePlaneVisuals();
    }


    public void TogglePlaneVisual()
    {
        planeVisualVisible =
            !planeVisualVisible;

        UpdatePlaneVisuals();
    }


    private void UpdatePlaneVisuals()
    {
        bool cutCurrentlyEnabled =
            planeSync != null &&
            planeSync.IsCutEnabled;


        // BorderPlano
        if (borderRenderer != null)
        {
            borderRenderer.enabled =
                planeVisualVisible &&
                (
                    cutCurrentlyEnabled ||
                    keepBorderVisibleWhenCutOff
                );
        }


        // HandleCorte
        if (handleObject != null)
        {
            handleObject.SetActive(
                planeVisualVisible
            );
        }
    }


    // ==========================================
    // RESET
    // ==========================================

    public void ResetPlane()
    {
        if (planeTransform == null)
        {
            return;
        }


        planeTransform.localPosition =
            initialLocalPosition;


        planeTransform.localRotation =
            initialLocalRotation;


        planeTransform.localScale =
            initialLocalScale;
    }
}
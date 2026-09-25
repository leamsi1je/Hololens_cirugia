using UnityEngine;

public class CrossSectionPlaneSync : MonoBehaviour
{
    [Header("ORGANOS")]
    [Tooltip("Orden recomendado: 0 Estomago, 1 Higado, 2 Pancreas, 3 Vesicula")]
    public Renderer[] organRenderers;


    [Header("CORTE")]
    [SerializeField]
    private bool cutEnabled = true;

    [SerializeField]
    private bool cutOnlySelected = false;

    [SerializeField]
    private int selectedOrganIndex = -1;


    [Header("VISTA QUIRURGICA")]
    [SerializeField]
    private bool surgicalViewEnabled = false;

    [Range(0f, 1f)]
    [SerializeField]
    private float surgicalGrayStrength = 0.90f;


    private MaterialPropertyBlock propertyBlock;

    private bool dirty = true;


    // ==========================================
    // SHADER PROPERTY IDs
    // ==========================================

    private static readonly int PlanePosID =
        Shader.PropertyToID("_PlanePos");

    private static readonly int PlaneNormalID =
        Shader.PropertyToID("_PlaneNormal");

    private static readonly int CutEnabledID =
        Shader.PropertyToID("_CutEnabled");

    private static readonly int SurgicalModeID =
        Shader.PropertyToID("_SurgicalMode");

    private static readonly int SurgicalGrayStrengthID =
        Shader.PropertyToID("_SurgicalGrayStrength");


    // ==========================================
    // PROPIEDADES PUBLICAS
    // ==========================================

    public bool IsCutEnabled
    {
        get { return cutEnabled; }
    }

    public bool IsSurgicalViewEnabled
    {
        get { return surgicalViewEnabled; }
    }

    public bool IsCutOnlySelected
    {
        get { return cutOnlySelected; }
    }

    public int SelectedOrganIndex
    {
        get { return selectedOrganIndex; }
    }


    // ==========================================
    // INICIALIZACION
    // ==========================================

    private void Awake()
    {
        EnsurePropertyBlock();

        dirty = true;

        ApplyProperties();
    }


    private void OnEnable()
    {
        EnsurePropertyBlock();

        dirty = true;

        ApplyProperties();
    }


    private void EnsurePropertyBlock()
    {
        if (propertyBlock == null)
        {
            propertyBlock =
                new MaterialPropertyBlock();
        }
    }


    // ==========================================
    // UPDATE OPTIMIZADO
    // ==========================================

    private void LateUpdate()
    {
        // Solo actualizamos shaders cuando:
        // 1. el plano se movio/roto
        // 2. algun estado cambio

        if (
            transform.hasChanged ||
            dirty
        )
        {
            ApplyProperties();

            transform.hasChanged = false;

            dirty = false;
        }
    }


    // ==========================================
    // CONTROL GENERAL DEL CORTE
    // ==========================================

    public void SetCutEnabled(bool enabled)
    {
        cutEnabled = enabled;

        dirty = true;

        ApplyProperties();
    }


    public void EnableCut()
    {
        SetCutEnabled(true);
    }


    public void DisableCut()
    {
        SetCutEnabled(false);
    }


    public void ToggleCut()
    {
        SetCutEnabled(
            !cutEnabled
        );
    }


    // ==========================================
    // TARGET
    // ==========================================

    public void SelectOrgan(int index)
    {
        if (
            organRenderers == null ||
            index < 0 ||
            index >= organRenderers.Length
        )
        {
            Debug.LogWarning(
                "CrossSectionPlaneSync: indice de organo invalido."
            );

            return;
        }

        selectedOrganIndex = index;

        // Al seleccionar un organo,
        // solamente ese organo se corta.
        cutOnlySelected = true;

        dirty = true;

        ApplyProperties();
    }


    public void SelectAllOrgans()
    {
        selectedOrganIndex = -1;

        cutOnlySelected = false;

        dirty = true;

        ApplyProperties();
    }


    public void SetCutOnlySelected(bool enabled)
    {
        cutOnlySelected = enabled;

        dirty = true;

        ApplyProperties();
    }


    // ==========================================
    // SURGICAL VIEW
    // ==========================================

    public void SetSurgicalView(bool enabled)
    {
        surgicalViewEnabled = enabled;

        dirty = true;

        ApplyProperties();
    }


    public void EnableSurgicalView()
    {
        SetSurgicalView(true);
    }


    public void DisableSurgicalView()
    {
        SetSurgicalView(false);
    }


    public void ToggleSurgicalView()
    {
        SetSurgicalView(
            !surgicalViewEnabled
        );
    }


    // ==========================================
    // APLICAR PROPIEDADES
    // ==========================================

    private void ApplyProperties()
    {
        EnsurePropertyBlock();

        if (organRenderers == null)
        {
            return;
        }


        Vector3 planePosition =
            transform.position;

        Vector3 planeNormal =
            transform.up.normalized;


        bool validSelection =
            selectedOrganIndex >= 0 &&
            selectedOrganIndex < organRenderers.Length;


        for (
            int i = 0;
            i < organRenderers.Length;
            i++
        )
        {
            Renderer rend =
                organRenderers[i];

            if (rend == null)
            {
                continue;
            }


            // --------------------------------
            // ¿ESTE ORGANO SE CORTA?
            // --------------------------------

            bool thisOrganCutEnabled =
                cutEnabled &&
                (
                    !cutOnlySelected ||
                    (
                        validSelection &&
                        i == selectedOrganIndex
                    )
                );


            // --------------------------------
            // ¿ESTE ORGANO VA EN GRIS?
            // --------------------------------

            bool grayThisOrgan =
                surgicalViewEnabled &&
                validSelection &&
                i != selectedOrganIndex;


            propertyBlock.Clear();

            rend.GetPropertyBlock(
                propertyBlock
            );


            // --------------------------------
            // PLANO
            // --------------------------------

            propertyBlock.SetVector(
                PlanePosID,
                new Vector4(
                    planePosition.x,
                    planePosition.y,
                    planePosition.z,
                    0f
                )
            );


            propertyBlock.SetVector(
                PlaneNormalID,
                new Vector4(
                    planeNormal.x,
                    planeNormal.y,
                    planeNormal.z,
                    0f
                )
            );


            // --------------------------------
            // CUT
            // --------------------------------

            propertyBlock.SetFloat(
                CutEnabledID,
                thisOrganCutEnabled
                    ? 1f
                    : 0f
            );


            // --------------------------------
            // SURGICAL VIEW
            // --------------------------------

            propertyBlock.SetFloat(
                SurgicalModeID,
                grayThisOrgan
                    ? 1f
                    : 0f
            );


            propertyBlock.SetFloat(
                SurgicalGrayStrengthID,
                surgicalGrayStrength
            );


            rend.SetPropertyBlock(
                propertyBlock
            );
        }


        dirty = false;
    }
}
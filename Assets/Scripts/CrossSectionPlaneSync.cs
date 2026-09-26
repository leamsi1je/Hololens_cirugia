using UnityEngine;

// Colocar este script en el objeto "PlanoDeCorte".
// A diferencia de la version anterior, aqui apuntamos al Renderer (no al Material)
// para obtener siempre la copia real que MRTK esta usando en pantalla.
public class CrossSectionPlaneSync : MonoBehaviour
{
    [Tooltip("Renderers de los organos a cortar (arrastra el objeto que tiene el Mesh Renderer, ej. 'estomago')")]
    public Renderer[] organRenderers;

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        foreach (var rend in organRenderers)
        {
            if (rend == null) continue;
            // renderer.material devuelve/crea la instancia real que se esta dibujando
            rend.material.SetVector("_PlanePos", new Vector4(pos.x, pos.y, pos.z, 0));
            rend.material.SetVector("_PlaneNormal", new Vector4(normal.x, normal.y, normal.z, 0));
        }
    }
}


using UnityEngine;

public class MeasurementManager : MonoBehaviour
{
    public bool modoMedicion = false;

    private bool esperandoPuntoA = true;

    private Vector3 puntoA;
    private Vector3 puntoB;

    public GameObject estomago;

    private GameObject marcadorA;
    private GameObject marcadorB;
    private GameObject objetoLinea;

    public void AlternarMedicion()
    {
        modoMedicion = !modoMedicion;

        if (modoMedicion)
        {
            Debug.Log("MODO MEDICION ACTIVADO");

            esperandoPuntoA = true;

            BorrarMedicionAnterior();
        }
        else
        {
            Debug.Log("MODO MEDICION DESACTIVADO");
        }
    }

    void Update()
    {
        if (!modoMedicion)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            DetectarPunto();
        }
    }

    void DetectarPunto()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] impactos = Physics.RaycastAll(ray);

        foreach (RaycastHit impacto in impactos)
        {
            MeshCollider meshCollider = impacto.collider as MeshCollider;

            if (meshCollider == null)
                continue;

            if (impacto.collider.gameObject != estomago)
                continue;

            if (esperandoPuntoA)
            {
                puntoA = impacto.point;

                marcadorA = CrearMarcador(
                    puntoA,
                    "Punto_A",
                    Color.green
                );

                esperandoPuntoA = false;

                Debug.Log("PUNTO A GUARDADO: " + puntoA);
            }
            else
            {
                puntoB = impacto.point;

                marcadorB = CrearMarcador(
                    puntoB,
                    "Punto_B",
                    Color.red
                );

                DibujarLinea();

                float distancia = Vector3.Distance(puntoA, puntoB);

                Debug.Log("PUNTO B GUARDADO: " + puntoB);
                Debug.Log("DISTANCIA UNITY: " + distancia);

                esperandoPuntoA = true;
            }

            break;
        }
    }

    GameObject CrearMarcador(
        Vector3 posicion,
        string nombre,
        Color color
    )
    {
        GameObject marcador =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        marcador.name = nombre;

        marcador.transform.position = posicion;

        marcador.transform.localScale =
            Vector3.one * 0.01f;

        Renderer renderer =
            marcador.GetComponent<Renderer>();

        renderer.material.color = color;

        Collider collider =
            marcador.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        return marcador;
    }

    void DibujarLinea()
    {
        objetoLinea = new GameObject("Linea_Medicion");

        LineRenderer linea =
            objetoLinea.AddComponent<LineRenderer>();

        linea.positionCount = 2;

        linea.SetPosition(0, puntoA);
        linea.SetPosition(1, puntoB);

        linea.startWidth = 0.003f;
        linea.endWidth = 0.003f;

        linea.material =
            new Material(Shader.Find("Sprites/Default"));

        linea.startColor = Color.yellow;
        linea.endColor = Color.yellow;
    }

    void BorrarMedicionAnterior()
    {
        if (marcadorA != null)
            Destroy(marcadorA);

        if (marcadorB != null)
            Destroy(marcadorB);

        if (objetoLinea != null)
            Destroy(objetoLinea);

        marcadorA = null;
        marcadorB = null;
        objetoLinea = null;
    }
}
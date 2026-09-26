using UnityEngine;

public class Boton_organo : MonoBehaviour
{
    [Tooltip("Arrastra aquí el órgano que este botón debe controlar")]
    public GameObject organoAsociado;

    // Se ejecuta al hacer clic en el objeto (requiere un Collider)
    private void OnMouseDown()
    {
        if (organoAsociado != null)
        {
            // Alterna entre activo e inactivo
            organoAsociado.SetActive(!organoAsociado.activeSelf);
        }
        else
        {
            Debug.LogWarning("Falta asignar el órgano al botón: " + gameObject.name);
        }
    }
}

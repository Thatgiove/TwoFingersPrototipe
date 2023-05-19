using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 20f;     // Velocità di movimento della telecamera
    public float rotationSpeed = 20f; // Velocità di rotazione della telecamera
    private float distance;
    public bool isRotating { get; set; }

    private void Start()
    {
        distance = Vector3.Distance(transform.position, target.position);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(2))
        {
            Cursor.visible = false;
            isRotating = true;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            Cursor.visible = true;
            isRotating = false;
        }

        if (isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X");

            // Ruota la telecamera intorno all'asse Y
            transform.RotateAround(target.position, Vector3.up, mouseX * rotationSpeed);
        }

        // Aggiorna la posizione della telecamera mantenendo la stessa distanza dal giocatore
        transform.position = target.position - transform.forward * distance;

        // Fai in modo che la telecamera guardi sempre in direzione del giocatore
        transform.LookAt(target);
    }
}

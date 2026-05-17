using UnityEngine;
using UnityEngine.InputSystem;


public class MenuCameraMovement : MonoBehaviour
{
    [Header("Movimiento Rat�n")]
    [SerializeField] private float _moveAmount = 0.1f; // Cu�nto se mueve la c�mara con el rat�n
    [SerializeField] private float _smoothSpeed = 5f; // Velocidad de suavizado del movimiento

    [Header("Oscilaci�n Idle")]
    [SerializeField] private float _idleAmount = 0.03f; // Cu�nto se mueve la c�mara autom�ticamente
    [SerializeField] private float _idleSpeed = 0.5f; // Velocidad de la oscilaci�n autom�tica

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        //movimiento del rat�n normalizado para que el centro de la pantalla sea (0,0) y los bordes sean (-1,1)
        float mouseX = (mousePosition.x / Screen.width - 0.5f) * 2f;
        float mouseY = (mousePosition.y / Screen.height - 0.5f) * 2f;

        // Movimiento por rat�n
        Vector3 mouseOffset = new Vector3(
            mouseX * _moveAmount,
            mouseY * _moveAmount * 0.5f,
            0f
        );

        // oscilaci�n autom�tica (idle)
        Vector3 idleOffset = new Vector3(
            Mathf.Sin(Time.time * _idleSpeed) * _idleAmount, // movimiento horizontal oscilante
            Mathf.Cos(Time.time * _idleSpeed * 0.8f) * _idleAmount, // movimiento vertical oscilante, con una frecuencia diferente para evitar patrones repetitivos
            0f
        );

        //se suma el movimiento del rat�n y la oscilaci�n autom�tica al punto inicial para obtener la posici�n objetivo
        Vector3 targetPosition = initialPosition + mouseOffset + idleOffset;

        //se interpola suavemente entre la posici�n actual y la posici�n objetivo para crear un movimiento fluido
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * _smoothSpeed
        );
    }
}


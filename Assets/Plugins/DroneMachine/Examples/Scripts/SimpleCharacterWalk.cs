using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterWalk : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _acceleration = 1f;

    private CharacterController _characterController;
    private float _currentSpeed;
    private Vector3 _lastMoveVector;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _currentSpeed = 0;
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 moveVector = (transform.right*x + transform.forward*z).normalized;

        if (moveVector.sqrMagnitude > 0)
        {
            _currentSpeed += _acceleration*Time.deltaTime;

            if (_currentSpeed > _walkSpeed)
            {
                _currentSpeed = _walkSpeed;
            }

            _lastMoveVector = moveVector;
        }
        else
        {
            _currentSpeed -= _acceleration*Time.deltaTime;

            if (_currentSpeed < 0)
            {
                _currentSpeed = 0;
            }
        }

        _characterController.Move(_lastMoveVector*_currentSpeed*Time.deltaTime);
    }
}

using System.Numerics;
using System.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    public float Speed = 5f;
    public float JumpHeight = 2f;
    public float Gravity = -9.81f;
    public float GroundDistance = 0.2f;
    public float DashDistance = 5f;
    public LayerMask Ground;
    public Vector3 Drag;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded = true;
    private Transform _groundChecker;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _groundChecker = transform.GetChild( 0 );
    }

    // Update is called once per frame
    void Update()
    {
        /* Handles all of our input */
        PlayerInput();
    }

    /* Controlls the player input */
    void PlayerInput()
    {
        /* Check if are in ground */
        _isGrounded = Physics.CheckSphere( _groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore );
        if ( _isGrounded && _velocity.y < 0 )
            _velocity.y = 0f;

        /* Move our character to all directions */
        Vector3 move = new Vector3( Input.GetAxis( "Vertical" ) * +0.5f, 0, Input.GetAxis( "Horizontal" ) * -0.5f );
        _controller.Move( move * Time.deltaTime * Speed );
        if ( move != Vector3.zero )
            transform.forward = move;

        /* Jump the player that obeys the gravity */
        if ( Input.GetButtonDown( "Jump" ) && _isGrounded )
            _velocity.y += Mathf.Sqrt( JumpHeight * -2f * Gravity );

        /* Make the player run */
        if ( /*Input.GetButtonDown( "Dash" )*/ Input.GetKey( KeyCode.Z ) )
            _velocity += Vector3.Scale( transform.forward, DashDistance * new Vector3( ( Mathf.Log( 1f / ( Time.deltaTime * Drag.x + 1 ) ) / -Time.deltaTime ), 0, ( Mathf.Log( 1f / ( Time.deltaTime * Drag.z + 1 ) ) / -Time.deltaTime ) ) );

        _velocity.y += Gravity * Time.deltaTime;

        _velocity.x /= 1 + Drag.x * Time.deltaTime;
        _velocity.y /= 1 + Drag.y * Time.deltaTime;
        _velocity.z /= 1 + Drag.z * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);
    }

    void OnCollisionEnter( Collision collision )
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMover : MonoBehaviour
{

    [Tooltip("How fast the player moves.")]
    public float MovementSpeed = 7.0f;
    [Tooltip("Units per second acceleration")]
    public float AccelRate = 20.0f;
    [Tooltip("Units per second deceleration")]
    public float DecelRate = 20.0f;
    [Tooltip("Acceleration the player has in mid-air")]
    public float AirborneAccel = 5.0f;
    [Tooltip("The velocity applied to the player when the jump button is pressed")]
    public float JumpSpeed = 7.0f;
    [Tooltip("Extra units added to the player's fudge height")]
    // Extra units added to the player's fudge height... if you're rocketting off
    // ramps or feeling too loosely attached to the ground, increase this.
    // If you're being yanked down to stuff too far beneath you, lower this.
    // Thid can't be modified during runtime
    public float FudgeExtra = 0.5f;
    [Tooltip("Maximum slope the player can walk up")]
    public float MaximumSlope = 45.0f;

    private bool _isGrounded = false;
    public bool IsGrounded { get => _isGrounded; }

    //Unity Components
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    // Temp vars
    private float _inputX;
    private float _inputY;
    private Vector2 _movementInput;
    private Vector3 _movementVector;

    // Acceleration or deceleration
    private float _acceleration;

    /*
     * Keep track of falling
     */
    private bool _isFalling;
    public bool IsFalling { get => _isFalling; }

    private float _fallSpeed;
    public float FallSpeed { get => _fallSpeed; }

    /*
     * Jump state var:
     * 0 = hit ground since last jump, can jump if grounded = true
     * 1 = jump button pressed, try to jump during fixedupdate
     * 2 = jump force applied, waiting to leave the ground
     * 3 = jump was successful, haven't hit the ground yet (this state is to ignore fudging)
    */
    private byte _jumpState;

    // Average normal of the ground i'm standing on
    private Vector3 _groundNormal;
    public Vector3 GroundNormal { get => _groundNormal; }

    // If we're touching a dynamic object, don't prevent idle sliding
    private bool _touchingDynamic;

    // Was i grounded last frame? used for fudging
    private bool _groundedLastFrame;

    // The objects i'm colliding with
    private List<GameObject> _collisions;

    // All of the collision contact points
    private Dictionary<int, ContactPoint[]> _contactPoints;

    /*
     * Temporary calculations
     */
    private float _halfPlayerHeight;
    private float _fudgeCheck;
    private float _bottomCapsuleSphereOrigin; // transform.position.y - this variable = the y coord for the origin of the capsule's bottom sphere
    private float _capsuleRadius;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _movementVector = Vector3.zero;

        _isGrounded = false;
        _groundNormal = Vector3.zero;
        _touchingDynamic = false;
        _groundedLastFrame = false;

        _collisions = new List<GameObject>();
        _contactPoints = new Dictionary<int, ContactPoint[]>();

        // do our calculations so we don't have to do them every frame
        Debug.Log(_capsuleCollider);
        _halfPlayerHeight = _capsuleCollider.height * 0.5f;
        _fudgeCheck = _halfPlayerHeight + FudgeExtra;
        _bottomCapsuleSphereOrigin = _halfPlayerHeight - _capsuleCollider.radius;
        _capsuleRadius = _capsuleCollider.radius;

        PhysicMaterial controllerMat = new PhysicMaterial();
        controllerMat.bounciness = 0.0f;
        controllerMat.dynamicFriction = 0.0f;
        controllerMat.staticFriction = 0.0f;
        controllerMat.bounceCombine = PhysicMaterialCombine.Minimum;
        controllerMat.frictionCombine = PhysicMaterialCombine.Minimum;
        _capsuleCollider.material = controllerMat;

        // just in case this wasn't set in the inspector
        _rigidbody.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // check if we're grounded
        RaycastHit hit;
        _isGrounded = false;
        _groundNormal = Vector3.zero;

        foreach (ContactPoint[] contacts in _contactPoints.Values)
        {
            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].point.y <= _rigidbody.position.y - _bottomCapsuleSphereOrigin &&
                    Physics.Raycast(contacts[i].point + Vector3.up, Vector3.down, out hit, 1.1f, ~0) &&
                    Vector3.Angle(hit.normal, Vector3.up) <= MaximumSlope)
                {
                    _isGrounded = true;
                    _groundNormal += hit.normal;

                }
            }
        }

        if (_isGrounded)
        {
            // average the summed normals
            _groundNormal.Normalize();

            if (_jumpState == 3)
                _jumpState = 0;
        }
        else if (_jumpState == 2)
            _jumpState = 3;

        // get player input
        _inputX = _movementInput.x;
        _inputY = _movementInput.y;

        // limit the length to 1.0f
        float length = 0;

        if (_isGrounded && _jumpState != 3)
        {
            if (_isFalling)
            {
                // we just landed from a fall
                _isFalling = false;
                this.DoFallDamage(Mathf.Abs(_fallSpeed));
            }

            // align our movement vectors with the ground normal (ground normal = up)
            Vector3 newForward = transform.forward;
            Vector3.OrthoNormalize(ref _groundNormal, ref newForward);

            Vector3 targetSpeed = Vector3.Cross(_groundNormal, newForward) * _inputX * MovementSpeed +
                newForward * _inputY * MovementSpeed;

            length = targetSpeed.magnitude;
            float difference = length - _rigidbody.velocity.magnitude;

            // avoid divide by zero
            if (Mathf.Approximately(difference, 0.0f))
                _movementVector = Vector3.zero;

            else
            {
                // determine if we should accelerate or decelerate
                if (difference > 0.0f)
                    _acceleration = Mathf.Min(AccelRate * Time.deltaTime, difference);

                else
                    _acceleration = Mathf.Max(-DecelRate * Time.deltaTime, difference);

                // normalize the difference vector and store it in movement
                difference = 1.0f / difference;
                _movementVector = (targetSpeed - _rigidbody.velocity) * difference * _acceleration;
            }

            if (_jumpState == 1)
            {
                // jump button was pressed, do jump  
                _movementVector.y = JumpSpeed - _rigidbody.velocity.y;
                _jumpState = 2;
            }
            else if (!_touchingDynamic && Mathf.Approximately(_inputX + _inputY, 0.0f) && _jumpState < 2)
                // prevent sliding by countering gravity... this may be dangerous
                _movementVector.y -= Physics.gravity.y * Time.deltaTime;

            _rigidbody.AddForce(_movementVector, ForceMode.VelocityChange);
            _groundedLastFrame = true;
        }
        else
        {
            // not grounded, so check if we need to fudge and do air accel

            // fudging
            if (_groundedLastFrame && _jumpState != 3 && !_isFalling)
            {
                // see if there's a surface we can stand on beneath us within fudgeCheck range
                if (Physics.Raycast(transform.position, Vector3.down, out hit, _fudgeCheck +
                    (_rigidbody.velocity.magnitude * Time.deltaTime), ~0) &&
                    Vector3.Angle(hit.normal, Vector3.up) <= MaximumSlope)
                {
                    _groundedLastFrame = true;

                    // catches jump attempts that would have been missed if we weren't fudging
                    if (_jumpState == 1)
                    {
                        _movementVector.y += JumpSpeed;
                        _jumpState = 2;
                        return;
                    }

                    // we can't go straight down, so do another raycast for the exact distance towards the surface
                    // i tried doing exsec and excsc to avoid doing another raycast, but my math sucks and it failed
                    // horribly. if anyone else knows a reasonable way to implement a simple trig function to bypass
                    // this raycast, please contribute to the thread!
                    if (Physics.Raycast(new Vector3(transform.position.x,
                        transform.position.y - _bottomCapsuleSphereOrigin,
                        transform.position.z), -hit.normal, out hit, hit.distance, ~0))
                    {
                        _rigidbody.AddForce(hit.normal * -hit.distance, ForceMode.VelocityChange);
                        return; // skip air accel because we should be grounded
                    }
                }
            }

            // if we're here, we're not fudging so we're defintiely airborne
            // thus, if falling isn't set, set it
            if (!_isFalling)
                _isFalling = true;

            _fallSpeed = _rigidbody.velocity.y;

            // air accel
            if (!Mathf.Approximately(_inputX + _inputY, 0.0f))
            {
                // note, this will probably malfunction if you set the air accel too high...
                // this code should be rewritten if you intend to do so

                // get direction vector
                _movementVector = transform.TransformDirection(new Vector3(_inputX * AirborneAccel * Time.deltaTime,
                    0.0f, _inputY * AirborneAccel * Time.deltaTime));

                // add up our accel to the current velocity to check if it's too fast
                float a = _movementVector.x + _rigidbody.velocity.x;
                float b = _movementVector.z + _rigidbody.velocity.z;

                // check if our new velocity will be too fast
                length = Mathf.Sqrt(a * a + b * b);
                if (length > 0.0f)
                {
                    if (length > MovementSpeed)
                    {
                        // normalize the new movement vector
                        length = 1.0f / Mathf.Sqrt(_movementVector.x * _movementVector.x +
                            _movementVector.z * _movementVector.z);
                        _movementVector.x *= length;
                        _movementVector.z *= length;

                        // normalize our current velocity (before accel)
                        length = 1.0f / Mathf.Sqrt(_rigidbody.velocity.x * _rigidbody.velocity.x +
                            _rigidbody.velocity.z * _rigidbody.velocity.z);
                        Vector3 rigidbodyDirection = new Vector3(_rigidbody.velocity.x * length, 0.0f,
                            _rigidbody.velocity.z * length);

                        // dot product of accel unit vector and velocity unit vector, clamped above 0 and inverted (1-x)
                        length = (1.0f - Mathf.Max(_movementVector.x * rigidbodyDirection.x +
                            _movementVector.z * rigidbodyDirection.z, 0.0f)) * AirborneAccel * Time.deltaTime;
                        _movementVector.x *= length;
                        _movementVector.z *= length;
                    }

                    // and finally, add our force
                    _rigidbody.AddForce(new Vector3(_movementVector.x, 0.0f, _movementVector.z),
                        ForceMode.VelocityChange);
                }
            }

            _groundedLastFrame = false;
        }
    }

    void DoFallDamage(float fallSpeed) // fallSpeed will be positive
    {
        // do your fall logic here using fallSpeed to determine how hard we hit the ground
        Debug.Log("Hit the ground at " + fallSpeed.ToString() + " units per second");
    }

    void OnCollisionEnter(Collision collision)
    {
        // keep track of collision objects and contact points
        _collisions.Add(collision.gameObject);
        _contactPoints.Add(collision.gameObject.GetInstanceID(), collision.contacts);

        // check if this object is dynamic
        if (!collision.gameObject.isStatic)
            _touchingDynamic = true;

        // reset the jump state if able
        if (_jumpState == 3)
            _jumpState = 0;
    }

    void OnCollisionStay(Collision collision)
    {
        // update contact points
        _contactPoints[collision.gameObject.GetInstanceID()] = collision.contacts;
    }

    void OnCollisionExit(Collision collision)
    {
        _touchingDynamic = false;

        // remove this collision and its associated contact points from the list
        // don't break from the list once we find it because we might somehow have duplicate entries,
        // and we need to recheck groundedOnDynamic anyways
        for (int i = 0; i < _collisions.Count; i++)
        {
            if (_collisions[i] == collision.gameObject)
                _collisions.RemoveAt(i--);

            else if (!_collisions[i].isStatic)
                _touchingDynamic = true;
        }

        _contactPoints.Remove(collision.gameObject.GetInstanceID());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_groundedLastFrame)
            _jumpState = 1;
    }
}


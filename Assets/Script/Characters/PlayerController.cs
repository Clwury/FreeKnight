using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Run,
        Attack,
        
    }

    public Transform _transform;
    public float speed;
    public float slideSpeed;
    public float jumpForce;
    public Vector2 bottomOffset;
    public LayerMask groundLayerMask;
    public LayerMask shelfLayerMask;
    public Vector2 climbPointOffset;
    
    private GameObject _attackTarget;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Collider2D _collider2D;
    private float _xVelocity;
    private float _scaleX;
    private bool _isRun;
    private bool _onGround;
    private bool _isAttack;
    private bool _canMove;
    private bool _isSlide;
    private bool _isClimb;
    private bool _isAction;
    private bool _wallSide;
    private bool _headWallCollision;
    private bool _headShelfCollision;
    private bool _footCollision;
    private bool _headTopCollision;
    private Vector3 _currentPos;
    private Vector3 _slideDirection;

    // Start is called before the first frame update
    void Start()
    {
        _canMove = true;
        _rb = GetComponentInParent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        
        _scaleX = _transform.localScale.x;
        _rb.gravityScale = 3;
        groundLayerMask = 1 << 6 | 1 << 7;
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            _xVelocity = Input.GetAxis("Horizontal");
            _animator.SetFloat("speed", Mathf.Abs(_xVelocity));
            if (_xVelocity > 0 && !_isSlide)
            {
                _transform.localScale = new Vector3(1, 1, 1);
            }
            else if (_xVelocity < 0 && !_isSlide)
            {
                _transform.localScale = new Vector3(-1, 1, 1);
            }
            _scaleX = _transform.localScale.x;
            if (_isSlide)
            {
                _rb.velocity = new Vector2(_slideDirection.x * slideSpeed, _rb.velocity.y);
            }
            else
            {
                _rb.velocity = new Vector2(_xVelocity * speed, _rb.velocity.y);
            }
            // Debug.Log(_rb.velocity.y);
            _animator.SetFloat("VerticalVelocity", _rb.velocity.y);
        }

        RayCheck();
        if (Input.GetButtonDown("Jump"))
        {
            if (_onGround)
            {
                // Climb wall TODO: Trigger可能无法重置
                if (_wallSide && !_headWallCollision)
                {
                    _animator.SetTrigger("Climb");
                }
                else
                {
                    _animator.SetTrigger("Jump");
                    _rb.velocity = new Vector2(_rb.velocity.x, 0);
                    _rb.velocity += Vector2.up * jumpForce;
                }
            }
        }

        if (Input.GetButtonDown("Fire1") && !_isAttack) // TODO: 考虑简化参数
        {
            _animator.SetTrigger("Attack");
        }

        if (Input.GetButtonDown("Slide") && !_isSlide)
        {
            if (_onGround && Mathf.Abs(_xVelocity) > 0.1f && !_isAction)
            {
                _slideDirection = _transform.localScale;
                _animator.SetTrigger("Slide"); // 动画不执行Trigger不能重置
            }
        }
    }

    void RayCheck()
    {
        _onGround = Physics2D.Raycast((Vector2)_transform.position - _scaleX * bottomOffset, Vector2.down, 0.1f, groundLayerMask);
        _animator.SetBool("onGround", _onGround);
        Debug.DrawRay(_transform.position + new Vector3(-_scaleX * bottomOffset.x, bottomOffset.y, 0), Vector3.down * 0.1f, _onGround ? Color.red : Color.green);

        _wallSide = Physics2D.OverlapCircle((Vector2)_transform.position + new Vector2(0.2f * _scaleX, 0.9f), 0.1f, groundLayerMask);

        _headWallCollision = Physics2D.OverlapCircle((Vector2)_transform.position + new Vector2(0.2f * _scaleX, 1.2f),
            0.1f, groundLayerMask);
        Collider2D headShelfCollider = Physics2D.OverlapCircle((Vector2)_transform.position + new Vector2(0.2f * _scaleX, 1.2f), 0.1f,
            shelfLayerMask);
        _headShelfCollision = headShelfCollider;
        _footCollision = Physics2D.OverlapCircle((Vector2)_transform.position + new Vector2(0.2f * _scaleX, 0), 0.1f, groundLayerMask);

        // head top
        _headTopCollision = Physics2D.Raycast((Vector2)_transform.position + new Vector2(-_scaleX * 0.16f, 1.2f),
            Vector2.up, 0.15f, groundLayerMask);
        Debug.DrawRay(_transform.position + new Vector3(-_scaleX * 0.16f, 1.2f, 0), Vector3.up * 0.15f, _headTopCollision ? Color.red : Color.green);
        if (!_isClimb && !_onGround && _headShelfCollision && !_footCollision && !_headTopCollision)
        {
            _isClimb = true;
            _rb.velocity = new Vector2(0, 0);
            Transform climbPointPos = headShelfCollider.transform.GetChild(headShelfCollider.transform.childCount - 1);
            _transform.position = climbPointPos.position + new Vector3(-_scaleX * climbPointOffset.x, climbPointOffset.y, 0);
            _animator.SetTrigger("Climb"); //TODO:Trigger可能无法重置
        }
    }

    void StartAttack()
    {
        _isAction = _isAttack = true;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _canMove = false;
    }

    void EndAttack()
    {
        _isAction = _isAttack = false;
        _canMove = true;
    }

    void StartSlide()
    {
        _isAction = _isSlide = true;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        if (capsuleCollider2D != null)
        {
            capsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
        }
    }

    void EndSlide()
    {
        _isAction = _isSlide = false;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        if (capsuleCollider2D != null)
        {
            capsuleCollider2D.direction = CapsuleDirection2D.Vertical;
        }
    }

    void StartClimb()
    {
        _isAction = _isClimb = true;
        _canMove = false;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _rb.bodyType = RigidbodyType2D.Static;
        _collider2D.enabled = false;
        _currentPos = _transform.position;
    }

    void EndClimb()
    {
        _isAction = _isClimb = false;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        Vector2 offset = capsuleCollider2D.offset;
        Vector3 size = capsuleCollider2D.size;
        _transform.position = _currentPos + new Vector3(_scaleX * offset.x, offset.y, 0) +
                              new Vector3(_scaleX * (size.x / 2 - 0.1f), -size.y / 2, 0);
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _collider2D.enabled = true;
        _canMove = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = Vector3.right * 0.5f;
        // Gizmos.DrawRay(_transform.position + new Vector3(0.1f, 0.8f, 0), direction);
        Gizmos.DrawWireSphere(_transform.position + new Vector3(0.2f * _scaleX, 0.9f, 0), 0.1f);
        Gizmos.DrawWireSphere(_transform.position + new Vector3(0.2f * _scaleX, 1.2f, 0), 0.1f);
        Gizmos.DrawWireSphere(_transform.position + new Vector3(0.2f * _scaleX, 0, 0), 0.1f);
    }
}

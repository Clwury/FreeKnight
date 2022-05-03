using System;
using System.Collections;
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
    public float rollSpeed;
    public float jumpForce;
    public Vector2 bottomOffset;
    public LayerMask groundLayerMask;
    public LayerMask shelfLayerMask;
    public Vector2 climbPointOffset;

    public float maxHp;
    public float hp;
    public float defendTime;
    public event Action<float, float> UpdateHealthBar;

    private GameObject _attackTarget;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Collider2D _collider2D;
    private SpriteRenderer _spriteRenderer;
    private float _xVelocity;
    private float _scaleX;
    private bool _isRun;
    private bool _onGround;
    // private bool _isAttack;
    private bool _canMove;
    private bool _isSlide;
    private bool _isRoll;
    // private bool _isClimb;
    private bool _isAction;
    private bool _wallSide;
    private bool _headWallCollision;
    private bool _headShelfCollision;
    private bool _footCollision;
    private bool _headTopCollision;
    private Vector3 _currentPos;
    private Vector3 _slideDirection;
    private Vector3 _rollDirection;
    private bool _isDefend;

    private void Awake()
    {
        _canMove = true;
        _rb = GetComponentInParent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        hp = maxHp;
        _scaleX = _transform.localScale.x;
        _rb.gravityScale = 3;
        groundLayerMask = 1 << 6 | 1 << 7;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            _xVelocity = Input.GetAxis("Horizontal");
            _animator.SetFloat("speed", Mathf.Abs(_xVelocity));
            if (_xVelocity > 0 && !_isSlide && !_isRoll)
            {
                _transform.localScale = new Vector3(1, 1, 1);
            }
            else if (_xVelocity < 0 && !_isSlide && !_isRoll)
            {
                _transform.localScale = new Vector3(-1, 1, 1);
            }
            _scaleX = _transform.localScale.x;
            if (_isSlide)
            {
                _rb.velocity = new Vector2(_slideDirection.x * slideSpeed, _rb.velocity.y);
            }
            else if (_isRoll)
            {
                _rb.velocity = new Vector2(_rollDirection.x * rollSpeed, _rb.velocity.y);
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
            if (_onGround && !_isAction)
            {
                // Climb wall
                if (_wallSide && !_headWallCollision)
                {
                    _isAction = true;
                    _canMove = false;
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

        if (Input.GetButtonDown("Fire1") && !_isAction)
        {
            _isAction = true;
            _canMove = false;
            _animator.SetTrigger("Attack");
        }

        if (Input.GetButtonDown("Slide") && !_isAction)
        {
            if (_onGround && Mathf.Abs(_xVelocity) > 0.1f)
            {
                _isAction = true;
                _slideDirection = _transform.localScale;
                _animator.SetTrigger("Slide"); // 动画不执行Trigger不能重置
            }
        }

        if (Input.GetButtonDown("Roll") && !_isAction)
        {
            if (_onGround)
            {
                _isAction = true;
                _rollDirection = _transform.localScale;
                _animator.SetTrigger("Roll");
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
        if (!_isAction && !_onGround && _headShelfCollision && !_footCollision && !_headTopCollision)
        {
            _isAction = true;
            _canMove = false;
            _rb.velocity = new Vector2(0, 0);
            Transform climbPointPos = headShelfCollider.transform.GetChild(headShelfCollider.transform.childCount - 1);
            _transform.position = climbPointPos.position + new Vector3(-_scaleX * climbPointOffset.x, climbPointOffset.y, 0);
            _animator.SetTrigger("Climb");
        }
    }

    void StartAttack()
    {
        // if (_onGround) _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    void EndAttack()
    {
        _isAction = false;
        _canMove = true;
    }

    void StartSlide()
    {
        _isSlide = true;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        if (capsuleCollider2D != null)
        {
            capsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
        }
        gameObject.layer = LayerMask.NameToLayer("Flashing");
    }

    void EndSlide()
    {
        _isAction = _isSlide = false;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        if (capsuleCollider2D != null)
        {
            capsuleCollider2D.direction = CapsuleDirection2D.Vertical;
        }
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    void StartClimb()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _rb.bodyType = RigidbodyType2D.Static;
        _collider2D.enabled = false;
        _currentPos = _transform.position;
    }

    void EndClimb()
    {
        _isAction = false;
        _canMove = true;
        CapsuleCollider2D capsuleCollider2D = _collider2D as CapsuleCollider2D;
        Vector2 offset = capsuleCollider2D.offset;
        Vector3 size = capsuleCollider2D.size;
        _transform.position = _currentPos + new Vector3(_scaleX * offset.x, offset.y, 0) +
                              new Vector3(_scaleX * (size.x / 2 - 0.1f), -size.y / 2, 0);
        _collider2D.enabled = true;
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void StartRoll()
    {
        _isRoll = true;
        gameObject.layer = LayerMask.NameToLayer("Flashing");
    }

    void EndRoll()
    {
        _isAction = _isRoll = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public void TakeDamage(int amount)
    {
        if (!_isDefend)
        {
            hp -= amount;
            UpdateHealthBar?.Invoke(Mathf.Max(0, hp), maxHp);
            StartCoroutine(DefendFlash());
        }
    }

    IEnumerator DefendFlash()
    {
        _isDefend = true;
        float flashInterval = 0;
        bool spriteVisual = true;
        while (defendTime > 0)
        {
            defendTime -= Time.deltaTime;
            flashInterval += Time.deltaTime;
            if (flashInterval >= 0.15f)
            {
                _spriteRenderer.enabled = spriteVisual = !spriteVisual;
                flashInterval = 0;
            }

            yield return null;
        }

        if (defendTime <= 0)
        {
            _spriteRenderer.enabled = true;
            defendTime = 1f;
            _isDefend = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        
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

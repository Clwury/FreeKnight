using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WormController : MonoBehaviour
{
    public enum WormState
    {
        Idle,
        Walk,
        Dash,
        BeHit,
        FireBall,
        PowerFireBall,
        Death
    }
    
    // 近身距离
    public float closeDistance;
    // 攻击概率
    public float attackChance;
    // 行走速度
    public float walkSpeed;
    // 冲击距离
    public float dashDistance;
    // 冲击时间
    public float dashTime;
    // 冲击CD
    public float dashCd;
    public float maxHp;
    public GameObject fireBallPrefab;
    public GameObject player;
    public event Action<float, float> UpdateHealthBar;

    public WormState _state;
    private Animator _animator;
    private float _hp;
    private bool _isDead;
    // 是否二阶段
    
    private bool _powerState;
    private bool _isIdle;
    private bool _isWalk;
    private bool _isDash;
    private bool _isAttack;
    // private bool _isAttack;
    private Vector3 _currentPos;
    private float _dashDuration;
    private float _dashLastTime;
    private PlayerController _playerController;

    private void Awake()
    {
        _isIdle = true;
        _state = WormState.Idle;
        _hp = maxHp;
        _playerController = player.GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SwitchState();
        SwitchAnimation();
    }

    public void SwitchState()
    {
        switch (_state)
        {
            case WormState.Idle:
                float value = Random.Range(0, 1f);
                _isIdle = false;
                // Debug.Log(value);
                if (value >= attackChance)
                {
                    _state = _powerState ? WormState.PowerFireBall : WormState.FireBall;
                }
                else
                {
                    _state = WormState.Walk;
                }
                break;
            case WormState.Walk:
                Walk();
                break;
            case WormState.Dash:
                DashSkill();
                break;
            case WormState.FireBall:
                FireBallSkill();
                break;
            case WormState.PowerFireBall:
                PowerFireBallSkill();
                break;
            case WormState.BeHit:
                BeHit();
                break;
            case WormState.Death:
                Invoke(nameof(BeDead), 0.8f);
                break;
        }
    }

    public void SwitchAnimation()
    {
        _animator.SetBool("isIdle", _isIdle);
        _animator.SetBool("isWalk", _isWalk);
        _animator.SetBool("isDash", _isDash);
        _animator.SetBool("isDead", _isDead);
    }

    public void LookAtPlayer()
    {
        Vector3 distance = player.transform.position - transform.position;
        transform.localScale = new Vector3(distance.x < 0 ? -1 : 1, 1, 1);
    }

    public void Walk()
    {
        // Debug.Log("walk...");
        _isWalk = true;
        LookAtPlayer();
        // 与player距离小于近身距离
        if (Mathf.Abs(player.transform.position.x - transform.position.x) <= closeDistance)
        {
            if (_dashLastTime + dashCd < Time.time)
            {
                _isWalk = false;
                _state = WormState.Dash;
                _dashLastTime = Time.time;
                _currentPos = transform.position;
            }
            else
            {
                _isWalk = false;
                _isIdle = true;
                _state = WormState.Idle;
            }
        }
        else
        {
            transform.position += new Vector3(transform.localScale.x * Time.deltaTime * walkSpeed, 0, 0);
        }
    }
    
    public void DashSkill()
    {
        // Debug.Log("Dash...");
        _isDash = true;
        LookAtPlayer();
        // Debug.Log(_dashDuration);
        if (_dashDuration >= dashTime)
        {
            _isDash = false;
            _dashDuration = 0;
            _state = _powerState ? WormState.PowerFireBall : WormState.FireBall;
            return;
        }
        // Debug.Log($"{_currentPos}, {_currentPos + new Vector3(transform.localScale.x * dashDistance, 0, 0)}, {transform.position}");
        Vector3 dashPos = _currentPos + new Vector3(transform.localScale.x * dashDistance, 0, 0);
        if (dashPos.x <= 2f || dashPos.x >= 24f)
        {
            // dashPos = new Vector3(2f, dashPos.y, dashPos.z);
            _isDash = false;
            _dashDuration = 0;
            _isIdle = true;
            _state = WormState.Idle;
            return;
        }
        transform.position = Vector3.Lerp(_currentPos, dashPos, _dashDuration / dashTime);
        _dashDuration += Time.deltaTime;
    }

    public void FireBallSkill()
    {
        if (!_isAttack)
        {
            _isAttack = true;
            LookAtPlayer();
            _animator.SetTrigger("Attack");
        }
    }

    public void EndFireBallSkill()
    {
        _isAttack = false;
        _isIdle = true;
        _state = WormState.Idle;
    }

    public void PowerFireBallSkill()
    {
        
    }

    public void SprayFireBall()
    {
        if (_powerState)
        {
            
        }
        else
        {
            StartCoroutine(InitFireBall());
        }
    }

    IEnumerator InitFireBall()
    {
        Vector3 firePoint = transform.GetChild(0).position;
        int fireBallCount = 3;
        while (fireBallCount > 0)
        {
            Instantiate(fireBallPrefab, firePoint, Quaternion.identity, transform);
            fireBallCount--;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void BeHit()
    {
        
    }

    public void BeDead()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        _hp -= amount;
        UpdateHealthBar?.Invoke(Mathf.Max(_hp, 0), maxHp);
        if (_hp <= 0)
        {
            _isDead = true;
            _state = WormState.Death;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_isDash && col.gameObject.CompareTag("Player"))
        {
            _playerController.TakeDamage(Random.Range(2, 9));
        }
    }
}

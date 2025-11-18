using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;
public class Player : AbstractCharacter
{
    private float attackTimer = 0f, dashTimer = 0f, jumpBoostTimer = 0f;
    private Vector2 moveDirection = Vector2.zero, shootDirection = Vector2.zero, dashDirection = Vector2.left, attackDirection = Vector2.right;
    private bool dashAvailable = true, isDashing = false, isJumping = false;
    
    [NonSerialized] public static bool isAlive = true;
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction dashAction;
    private InputAction jumpAction;

    [Header("References")]
    // [SerializeField] public ProjectileBehavior projectileItem;
    // [SerializeField] public SoundEffectPlayer noiseMaker;
    [SerializeField] protected Rigidbody2D rb;
    // [SerializeField] protected Animator animator;
    // [SerializeField] public PlayerInputActions playerControls;
    // [SerializeField] public HealthBarUI healthBar;
    // [SerializeField] public HealthBarUI xpBar;
    // [SerializeField] public HealthBarUI dashBar;

    [Header("Extra Player Items")]
    [SerializeField] public float dashCooldown = 0.27f;
    [SerializeField] public float dashLength = 0.35f;
    [SerializeField] private float jumpBoostForce = 20f;     // small continuous force
    [SerializeField] private float maxJumpBoostTime = 0.15f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;

        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();
        jumpAction = InputSystem.actions.FindAction("Jump");
        jumpAction.Enable();
        dashAction = InputSystem.actions.FindAction("Dash");
        dashAction.Enable();
        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.Enable();

        moveAction.performed += MovePerformed;
        moveAction.canceled += MoveCancelled;
        dashAction.performed += DashPerformed;
        dashAction.canceled += DashCancelled;
        attackAction.performed += AttackPerformed;
        attackAction.canceled += AttackCancelled;
        jumpAction.performed += JumpPerformed;
        jumpAction.canceled += JumpCancelled;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        attackAction.Disable();
        dashAction.Disable();
        jumpAction.Disable();
    }

    void Update()
    {
        if (!isAlive) return;
        UpdateStandardTimers();
    }
    private void FixedUpdate()
    {
        if (!isAlive) return;
        UpdateFixedTimers();
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(getVelocity().x, rb.linearVelocity.y);
        }
        if (isJumping && jumpBoostTimer < maxJumpBoostTime)
        {
            rb.AddForce(Vector2.up * jumpBoostForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    public override void DamageEffects()
    {
        if (health <= 0)
        {
            Suicide();
        }
    }
    private void Suicide()
    {
        rb.linearVelocity = Vector2.zero;
        OnDisable();
        // animator.SetBool("isDead", true);
        if (isAlive)
        {
            // noiseMaker.PlayLongSound(deathSound, 1.2f);
            var time = 500;
            Invoke("CompleteDeath", time);
        }
        //Destroy(gameObject);
    }
    private void CompleteDeath()
    {
        isAlive = false;
    }
    public Vector2 getVelocity()
    {
        return new Vector2(moveDirection.x * GetEffectiveSpeed(), rb.linearVelocityY);
    }
    private void MovePerformed(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
        dashDirection = moveDirection;
        Debug.Log("Moving here: " + moveDirection);
        // animator.SetBool("isWalking", true);
    }
    private void MoveCancelled(InputAction.CallbackContext context)
    {
        moveDirection = Vector2.zero;
        // animator.SetBool("isWalking", false);
    }

    private void DashPerformed(InputAction.CallbackContext context)
    {
        if (dashAvailable && (dashTimer > dashCooldown))
        {
            Vector2 attackInput = attackAction.ReadValue<Vector2>();
            if (attackInput != Vector2.zero)
            {
                return; //no dashing while attacking
            }

            attackAction.Disable();
            dashAvailable = false;
            isDashing = true;
            dashTimer = -100f;
            float dashSpeed = speed * 4;
            Vector2 dashVelocity = new Vector2(dashDirection.x * dashSpeed, 0);
            rb.linearVelocityX = dashVelocity.x;
            Invoke("EndDash", dashLength);

            //animator.SetBool("isWalking", true);
        }
    }
    private void EndDash()
    {
        attackAction.Enable();
        dashTimer = 0f;
        isDashing = false;
        rb.linearVelocityX = getVelocity().x;
    }
    private void DashCancelled(InputAction.CallbackContext context)
    {
        dashAvailable = true;
    }

    private void AttackPerformed(InputAction.CallbackContext context)
    {
        if (attackTimer > GetEffectiveFireRate())
        {
            attackTimer = 0f;
            attackDirection = moveDirection;
            Debug.Log("Attacking!: " + attackDirection);
        }
        
    }
    private void AttackCancelled(InputAction.CallbackContext context)
    {
        return;
    }
    private void JumpPerformed(InputAction.CallbackContext context)
    {
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            isJumping = true;
            jumpBoostTimer = 0f;
        }
        
    }
    private void JumpCancelled(InputAction.CallbackContext context)
    {
        isJumping = false;
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.33f);
        }
    }
    
    private void UpdateStandardTimers()
    {
        attackTimer += Time.deltaTime;
        dashTimer += Time.deltaTime;
    }
    private void UpdateFixedTimers()
    {
        jumpBoostTimer += Time.fixedDeltaTime;
    }
}

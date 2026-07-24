using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlimeMovement : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("跳跃")]
    [SerializeField] private float jumpForce = 8f;

    [Header("组件")]
    [SerializeField] private SpriteRenderer slimeRenderer;

    private Rigidbody2D rb;
    private Collider2D slimeCollider;
    private Animator animator;
    private PhysicsMaterial2D noFrictionMaterial;
    private float moveInput;
    private bool jumpQueued;

    private readonly List<ContactPoint2D> contacts = new();

    private static readonly int JumpParameter = Animator.StringToHash("Jump");
    private static readonly int GroundedParameter = Animator.StringToHash("Grounded");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        // 墙面的摩擦力可能抵消重力，导致角色按住方向键时贴在墙上。
        // 使用零摩擦材质，保留碰撞体原本的弹性设置。
        float bounciness = slimeCollider.sharedMaterial == null
            ? 0f
            : slimeCollider.sharedMaterial.bounciness;

        noFrictionMaterial = new PhysicsMaterial2D("Slime No Friction")
        {
            friction = 0f,
            bounciness = bounciness,
            hideFlags = HideFlags.HideAndDontSave
        };
        slimeCollider.sharedMaterial = noFrictionMaterial;

        if (slimeRenderer == null)
        {
            slimeRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        moveInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        // 原始图片朝左
        if (moveInput < 0f)
        {
            slimeRenderer.flipX = false;
        }
        else if (moveInput > 0f)
        {
            slimeRenderer.flipX = true;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        RefreshContacts();
        bool isGrounded = rb.linearVelocity.y <= 0.1f && HasGroundContact();

        if (jumpQueued && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;

            if (animator != null)
            {
                animator.SetTrigger(JumpParameter);
            }
        }

        jumpQueued = false;

        float horizontalVelocity = moveInput * moveSpeed;

        // 在空中接触墙面时，不再持续向墙内施加速度。
        // 玩家仍然可以立即按反方向键离开墙面。
        if (!isGrounded && IsPushingIntoWall(moveInput))
        {
            horizontalVelocity = 0f;
        }

        rb.linearVelocity = new Vector2(
            horizontalVelocity,
            rb.linearVelocity.y
        );

        if (animator != null)
        {
            animator.SetBool(GroundedParameter, isGrounded);
        }
    }

    private void RefreshContacts()
    {
        contacts.Clear();
        slimeCollider.GetContacts(contacts);
    }

    private bool HasGroundContact()
    {
        foreach (ContactPoint2D contact in contacts)
        {
            // 接触面的法线朝上，说明史莱姆脚下有地面。
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPushingIntoWall(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
        {
            return false;
        }

        foreach (ContactPoint2D contact in contacts)
        {
            bool wallOnLeft = contact.normal.x > 0.5f;
            bool wallOnRight = contact.normal.x < -0.5f;

            if ((direction < 0f && wallOnLeft) ||
                (direction > 0f && wallOnRight))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        if (noFrictionMaterial != null)
        {
            Destroy(noFrictionMaterial);
        }
    }
}

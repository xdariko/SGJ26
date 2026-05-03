using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDetailsSO playerDetails;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Player player;

    private Vector2 moveDirection;
    private Vector2 lastNonZeroDirection = Vector2.down;

    private bool isDashing = false;
    private bool canDash = true;

    private float dashCooldownTimer;

    private Vector2 dashStartPos;
    private Vector2 dashTargetPos;
    private float dashElapsed;

    public event System.Action<Vector2> OnMove;
    public Vector2 LastMoveDirection => lastNonZeroDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerController: Duplicate instance detected, destroying this one");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        ProcessInputs();
        HandleTimers();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing)
            PerformDash();
        else
            Move();
    }

    private void ProcessInputs()
    {
        if (Keyboard.current == null)
            return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveX -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveX += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveY -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveY += 1f;

        moveDirection = new Vector2(moveX, moveY).normalized;

        if (moveDirection != Vector2.zero)
            lastNonZeroDirection = moveDirection;

        bool dashInput =
            Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current.rightShiftKey.wasPressedThisFrame;

        if (
            dashInput &&
            canDash &&
            !isDashing &&
            moveDirection != Vector2.zero &&
            player.CurrentStamina >= playerDetails.DashStaminaCost
        )
        {
            TryDash();
        }
    }

    private void Move()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            rb.linearVelocity = moveDirection * playerDetails.MoveSpeed;
            lastNonZeroDirection = moveDirection;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        OnMove?.Invoke(moveDirection != Vector2.zero ? moveDirection : lastNonZeroDirection);
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f || isDashing;
        animator.SetBool("IsMoving", isMoving);

        // Бега больше нет, поэтому скорость анимации всегда обычная.
        animator.speed = 1f;
    }

    private void HandleTimers()
    {
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0f)
                canDash = true;
        }
    }

    private void TryDash()
    {
        if (!canDash || isDashing || player.CurrentStamina < playerDetails.DashStaminaCost)
            return;

        Vector2 direction = lastNonZeroDirection;
        float maxDashDistance = playerDetails.DashDistance;

        Vector2 rayStart = (Vector2)transform.position + direction * 0.1f;
        float availableDistance = maxDashDistance;

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, direction, maxDashDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                availableDistance = hit.distance - 0.1f;
                break;
            }
        }

        if (availableDistance <= 0.05f)
            return;

        if (player.TryUseStamina(playerDetails.DashStaminaCost))
            StartDash(direction, availableDistance);
    }

    private void StartDash(Vector2 direction, float dashDistance)
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = playerDetails.DashCooldown;

        IgnoreEnemyCollisions(true);

        dashStartPos = rb.position;
        dashTargetPos = dashStartPos + direction * dashDistance;
        dashElapsed = 0f;
    }

    private void PerformDash()
    {
        dashElapsed += Time.fixedDeltaTime;

        float t = Mathf.Clamp01(dashElapsed / playerDetails.DashDuration);
        Vector2 newPos = Vector2.Lerp(dashStartPos, dashTargetPos, t);

        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, dashTargetPos) < 0.01f || dashElapsed >= playerDetails.DashDuration)
            EndDash();
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        IgnoreEnemyCollisions(false);
    }

    private void IgnoreEnemyCollisions(bool ignore)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider == null)
            return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Collider2D[] cols = enemy.GetComponents<Collider2D>();

            for (int i = 0; i < cols.Length; i++)
            {
                Collider2D col = cols[i];

                if (col != null && col.enabled && !col.isTrigger)
                    Physics2D.IgnoreCollision(playerCollider, col, ignore);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing)
            return;

        if (collision.gameObject.CompareTag("Wall"))
            EndDash();
    }

    public Vector2 GetMouseDirection()
    {
        if (Mouse.current == null)
        {
            Debug.LogWarning("PlayerController: Mouse.current is null!");
            return Vector2.right;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = ((Vector2)worldMousePos - (Vector2)transform.position).normalized;

        return direction;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            OnMove = null;
        }
    }
}
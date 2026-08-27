using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3f;
    public SpriteRenderer spriteRenderer;

    [Header("男角 Sprite")]
    public Sprite[] maleWalkDown;
    public Sprite[] maleWalkUp;
    public Sprite[] maleWalkLeft;
    public Sprite[] maleWalkRight;
    public Sprite[] maleIdleDown;
    public Sprite[] maleIdleUp;
    public Sprite[] maleIdleLeft;
    public Sprite[] maleIdleRight;

    [Header("女角 Sprite")]
    public Sprite[] femaleWalkDown;
    public Sprite[] femaleWalkUp;
    public Sprite[] femaleWalkLeft;
    public Sprite[] femaleWalkRight;
    public Sprite[] femaleIdleDown;
    public Sprite[] femaleIdleUp;
    public Sprite[] femaleIdleLeft;
    public Sprite[] femaleIdleRight;
    private Vector2 moveInput;
    private float animTimer = 0f;
    private int animFrame = 0;
    private float animSpeed = 0.15f;
    private Vector2 lastDirection = Vector2.down;
    private bool isMoving = false;
    private Rigidbody2D rb;

    private Sprite[] walkDown;
    private Sprite[] walkUp;
    private Sprite[] walkLeft;
    private Sprite[] walkRight;
    private Sprite[] idleDown;
    private Sprite[] idleUp;
    private Sprite[] idleLeft;
    private Sprite[] idleRight;

    [Header("坐下動畫")]
    public Sprite[] maleSitDown;
    public Sprite[] femaleSitDown;
    private Sprite[] sitDown;
    private bool isSittingDown = false;
    private bool isSeated = false;
    private bool movementLocked = false;
    private bool inputLocked = false;
    public float bongFrameSpeed = 0.1f;
    public Sprite[] femaleBongFrames;
    public Sprite[] maleBongFrames;
    public bool isPlayingBong = false;
    private bool lockVertical = false;
    private bool infiniteHorizontal = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        int selected = PlayerPrefs.GetInt("SelectedCharacter", 0);

        if (selected == 0)
        {
            walkDown = maleWalkDown;
            walkUp = maleWalkUp;
            walkLeft = maleWalkLeft;
            walkRight = maleWalkRight;
            idleDown = maleIdleDown;
            idleUp = maleIdleUp;
            idleLeft = maleIdleLeft;
            idleRight = maleIdleRight;
            sitDown = maleSitDown;
        }
        else
        {
            walkDown = femaleWalkDown;
            walkUp = femaleWalkUp;
            walkLeft = femaleWalkLeft;
            walkRight = femaleWalkRight;
            idleDown = femaleIdleDown;
            idleUp = femaleIdleUp;
            idleLeft = femaleIdleLeft;
            idleRight = femaleIdleRight;
            sitDown = femaleSitDown;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleAnimation();
    }

    void HandleMovement()
    {
        if (inputLocked)
            return;

        if (movementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
            return;
        }
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = Vector2.zero;

        if (!lockVertical)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                moveInput.y = 1f;
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                moveInput.y = -1f;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput.x = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            moveInput.x = 1f;
        }

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        bool wasMoving = isMoving;
        isMoving = moveInput.magnitude > 0f;

        if (wasMoving != isMoving)
        {
            animFrame = 0;
            animTimer = 0f;
        }

        if (isMoving)
        {
            rb.linearVelocity = moveInput * moveSpeed;

            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                lastDirection = moveInput.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = moveInput.y > 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        Vector2 clampedPos = rb.position;
        if (!infiniteHorizontal)
        {
            clampedPos.x = Mathf.Clamp(clampedPos.x, -30f, 30f);
            clampedPos.y = Mathf.Clamp(clampedPos.y, -18f, 18f);
            rb.position = clampedPos;
        }

    }

    void HandleAnimation()
    {
        if (isSittingDown || isSeated || isPlayingBong) return;

        Sprite[] anim = GetCurrentAnimation();
        if (anim == null || anim.Length == 0) return;

        if (!isMoving)
        {
            Sprite[] idleAnim = GetCurrentIdleAnimation();
            if (idleAnim != null && idleAnim.Length > 0)
            {
                if (animFrame >= idleAnim.Length)
                    animFrame = 0;

                animTimer += Time.deltaTime;
                if (animTimer >= animSpeed * 2f)
                {
                    animTimer = 0f;
                    animFrame = (animFrame + 1) % idleAnim.Length;
                }
                spriteRenderer.sprite = idleAnim[animFrame];
            }
        }
        else
        {
            if (animFrame >= anim.Length)
                animFrame = 0;

            animTimer += Time.deltaTime;
            if (animTimer >= animSpeed)
            {
                animTimer = 0f;
                animFrame = (animFrame + 1) % anim.Length;
            }

            spriteRenderer.sprite = anim[animFrame];
        }

        spriteRenderer.flipX = false;
    }


    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        if (rb != null && locked)
            rb.linearVelocity = Vector2.zero;
    }

    public void SetInputLocked(bool locked) => inputLocked = locked;

    Sprite[] GetCurrentIdleAnimation()
    {
        if (lastDirection == Vector2.down) return idleDown;
        if (lastDirection == Vector2.up) return idleUp;
        if (lastDirection == Vector2.left) return idleLeft;
        if (lastDirection == Vector2.right) return idleRight;
        return idleDown;
    }

    Sprite[] GetCurrentAnimation()
    {
        if (lastDirection == Vector2.down) return walkDown;
        if (lastDirection == Vector2.up) return walkUp;
        if (lastDirection == Vector2.left) return walkLeft;
        if (lastDirection == Vector2.right) return walkRight;
        return walkDown;
    }

    public void SetAutoMove(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            isMoving = true;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                lastDirection = direction.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = direction.y > 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            isMoving = false;
        }
    }
    public void PlaySitDownAnimation()
    {
        StartCoroutine(SitDownSequence());
    }

    public void StandUp()
    {
        isSeated = false;
        isSittingDown = false;
        lastDirection = Vector2.down;
        animFrame = 0;
        animTimer = 0f;
    }

    IEnumerator SitDownSequence()
    {
        isSittingDown = true;
        isSeated = false;

        if (sitDown != null)
        {
            foreach (Sprite frame in sitDown)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(0.15f);
            }
        }

        isSeated = true;
        isSittingDown = false;
    }
    public void SetFacingDirection(Vector2 direction)
    {
        lastDirection = direction;
        animFrame = 0;
        HandleAnimation();
    }

    public void SetLockVertical(bool locked)
    {
        lockVertical = locked;
    }
    public void SetInfiniteHorizontal(bool enabled)
    {
        infiniteHorizontal = enabled;
    }

    public IEnumerator PlayBongAnimation()
    {
        isPlayingBong = true;

        int selected = PlayerPrefs.GetInt("SelectedCharacter", 0);
        Sprite[] bongFrames = selected == 0 ? maleBongFrames : femaleBongFrames;

        if (bongFrames == null || bongFrames.Length == 0) yield break;

        for (int i = 0; i < bongFrames.Length; i++)
        {
            spriteRenderer.sprite = bongFrames[i];
            yield return new WaitForSeconds(bongFrameSpeed);
        }
    }
}
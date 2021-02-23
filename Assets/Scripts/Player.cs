using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // config param
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float timeAfterDeath = 3f;
    [SerializeField] Vector2 deathKick = new Vector2(25f, 25f);

    // states
    public bool isAlive = true;

    // cached component references
    [SerializeField] PhysicsMaterial2D bouncyMaterial;
    Rigidbody2D myRigidbody2D;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidbody2D.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) { return; }

        ClimbLadder();
        Run();
        Jump();
        FlipSprite();
        Die();
    }

    private void Run()
    {
        float controlThrow = Input.GetAxis("Horizontal");
        Vector2 playerVelocity = new Vector2(controlThrow * runSpeed, myRigidbody2D.velocity.y);
        myRigidbody2D.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        if (!myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
        }
        else
        {
            myAnimator.SetBool("isRunning", false);
        }
    }

    private void ClimbLadder()
    {
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        {
            myAnimator.SetBool("isClimbing", false);
            myRigidbody2D.gravityScale = gravityScaleAtStart;
            return; 
        }

        float controlThrow = Input.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(myRigidbody2D.velocity.x, controlThrow * climbSpeed);
        myRigidbody2D.velocity = climbVelocity;
        myRigidbody2D.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);
    }

    private void Jump()
    {
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }

        if (Input.GetButtonDown("Jump"))
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidbody2D.velocity += jumpVelocityToAdd;
        }
    }

    private void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            StartCoroutine(Dying());
        }
    }

    private void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x), 1f);
        }
    }

    private IEnumerator Dying()
    {
        isAlive = false;
        myAnimator.SetTrigger("isDying");
        myRigidbody2D.sharedMaterial = bouncyMaterial;
        Vector2 deathKickScaled = deathKick * new Vector2(-transform.localScale.x, 1f);
        myRigidbody2D.velocity = deathKickScaled;
        yield return new WaitForSeconds(timeAfterDeath);
        myRigidbody2D.sharedMaterial = null;
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    } 
}

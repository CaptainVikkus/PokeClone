using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10;

    [SerializeField]
    private LayerMask collisionLayer;
    [SerializeField]
    private LayerMask bushLayer;

    private bool isMoving;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //If the player isn't moving, get input
        if (!isMoving)
        {
            // Get input in terms of 0, 1 or -1
            Vector2 input;
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //only move in one direction at a time
            if (input.x != 0) { input.y = 0; }

            if (input != Vector2.zero) //movement found
            {
                //Animations
                animator.SetFloat("MoveX", input.x);
                animator.SetFloat("MoveY", input.y);

                //Movement
                Vector3 targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (!DestinationCollides(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        //Doesn't work in Move()
        animator.SetBool("IsMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true; //start moving
        // while distance from target to position is > Epsilon
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            //move player position by speed * deltatime towards target
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        // close to tiny value, set exact
        transform.position = targetPos;
        isMoving = false; //finished moving
    }

    private bool DestinationCollides(Vector3 targetPos)
    {
        //collision at target
        if (Physics2D.OverlapCircle(targetPos, 0.4f, collisionLayer) != null)
        {
            return true;
        }
        return false;
    }
}

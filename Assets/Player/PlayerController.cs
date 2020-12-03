using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float encounterPercent = 10;

    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private LayerMask bushLayer;
    [SerializeField] private LayerMask portalLayer;

    public event Action OnEncountered;

    private bool isMoving;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void HandleUpdate()
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
        // close to the tiny value, set exact
        transform.position = targetPos;
        isMoving = false; //finished moving
        //check the BOOSH
        CheckForEncounter();
        CheckForDoor();
    }

    private void CheckForEncounter()
    {
        //check for boosh
        if (Physics2D.OverlapCircle(transform.position, 0.2f, bushLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= encounterPercent)
            {
                //Debug.Log("Encounter: ");
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }

    private void CheckForDoor()
    {
        var door = Physics2D.OverlapCircle(transform.position, 0.2f, portalLayer);
        if (door != null)
        {
            door.GetComponent<SceneTransition>().LoadScene();
        }
    }

    private bool DestinationCollides(Vector3 targetPos)
    {
        //collision at target
        if (Physics2D.OverlapCircle(targetPos, 0.1f, collisionLayer) != null)
        {
            return true;
        }
        return false;
    }

   

}

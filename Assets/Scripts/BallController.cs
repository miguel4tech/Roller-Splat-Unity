using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    #region VARIABLES
    public Rigidbody rb;
    private float speed = 15;

    public int minSwipeRecognition = 500;

    private bool isTraveling;
    private Vector3 travelDirection;

    //SWIPE variables
    private Vector2 swipePosLastFrame;
    private Vector2 swipePosCurrentFrame;
    private Vector2 currentSwipe;

    private Vector3 nextCollisionPosition;

    private Color solveColor;

    public AudioSource rollingSource;
    [SerializeField] AudioClip rollingSound;

    public ParticleSystem levelUpIndicator;
    #endregion
    private void Start()
    {
        solveColor = Random.ColorHSV(0.5f, 1); // Only take pretty light colors using the HSV color type
        GetComponent<MeshRenderer>().material.color = solveColor;
        levelUpIndicator = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        // Set the balls speed when it should travel
        if (isTraveling) {
            rb.velocity = travelDirection * speed;
            rollingSource.PlayOneShot(rollingSound);
        }

        // Paint the ground
        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up/2), .05f);
        int i = 0;
        // A for loop could be used as well, in order to keep incrementing the value, the i++ was included so the code doesn't break
        while (i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();

            if (ground && !ground.isColored)
            {
                ground.Colored(solveColor);
            }

            i++;
        }

        // Checks if we have reached our destination
        //Also ensuring the ball doesn't move until next collision
        if (nextCollisionPosition != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTraveling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
            }
        }

        if (isTraveling)
            return;
        #region SWIPE controls (applicable in other screen swipe projects with the Swipe global-variables above of course)
        // Swipe mechanism
        if (Input.GetMouseButton(0))
        {
            // Where is the mouse now?
            swipePosCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (swipePosLastFrame != Vector2.zero)
            {

                // Calculate the swipe direction
                currentSwipe = swipePosCurrentFrame - swipePosLastFrame;

                if (currentSwipe.sqrMagnitude < minSwipeRecognition) // Minium amount of swipe recognition
                    return;

                currentSwipe.Normalize(); // Normalize it to only get the direction not the distance (would fake the balls speed)

                // Up/Down swipe
                if (currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back); 
                }   

                // Left/Right swipe
                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);
                }
            }


            swipePosLastFrame = swipePosCurrentFrame;
        }
        //Ensures the last touch point is reset to zero
        if (Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
        #endregion
    }

    //An integral section of SWIPE mechanism exclusive to this game type
    //Is used to instead of collider OnCollision trigger to ensure smooth game play with less possiblibities of an error in detection
    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        // Check with which object we will collide
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTraveling = true;
    }

}

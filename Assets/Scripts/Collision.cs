using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;

    

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset1, bottomOffset2, rightOffset, leftOffset;

    // ledge stuff
    private Color debugCollisionColor = Color.red;
    public bool reachLedge;
    public GameObject ledgeCheckerObject;

    // head bumper correction 
    public Vector2 bumpOuterOffset, bumpInnerOffset, maxCorrectorOffset;
    public bool canCornerCorrect;
    public float cornerCorrectDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {  
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset1, collisionRadius, groundLayer) || Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset1 + new Vector2(0.5f, 0f), collisionRadius, groundLayer);
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer) 
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        wallSide = onRightWall ? -1 : 1;

        
        // Ledge checker
        RaycastHit2D canClimbRight = Physics2D.Raycast(ledgeCheckerObject.transform.position, Vector2.right, 1f);
        RaycastHit2D canClimbLeft = Physics2D.Raycast(ledgeCheckerObject.transform.position, Vector2.left, 1f);

        Debug.DrawRay(ledgeCheckerObject.transform.position, Vector2.right*1f, Color.red);
        Debug.DrawRay(ledgeCheckerObject.transform.position, Vector2.left*1f, Color.red);
        reachLedge = (canClimbRight == false && onRightWall) || (canClimbLeft == false && onLeftWall);


        /*
        // Head bump corrector
        RaycastHit2D headBumperOuterRight = Physics2D.Raycast((Vector2)transform.position + bumpOuterOffset, Vector2.up, 0.2f);
        RaycastHit2D headBumperOuterLeft = Physics2D.Raycast((Vector2)transform.position - bumpOuterOffset, Vector2.up, 0.2f);
        RaycastHit2D headBumperMaxCorrectionRight = Physics2D.Raycast((Vector2)transform.position + bumpInnerOffset, Vector2.up, 0.2f);
        RaycastHit2D headBumperMaxCorrectionLeft = Physics2D.Raycast((Vector2)transform.position - bumpInnerOffset, Vector2.up, 0.2f);

        RaycastHit2D headBumperMeasureRight = Physics2D.Raycast((Vector2)transform.position + maxCorrectorOffset, Vector2.right, 1f);
        if (headBumperMeasureRight.collider != null)
        {
           // float newCornerCorrectDistance = Vector3.Distance(new Vector3(headBumperMeasureRight.x, transform.position.y, 0f) + Vector3.up * )
        }

        RaycastHit2D headBumperMeasureLeft = Physics2D.Raycast((Vector2)transform.position - maxCorrectorOffset, Vector2.left, 1f);
        */



        Debug.DrawRay((Vector2)transform.position + bumpOuterOffset, Vector2.up * 0.2f, Color.blue);
        Debug.DrawRay((Vector2)transform.position - bumpOuterOffset, Vector2.up * 0.2f, Color.blue);
        Debug.DrawRay((Vector2)transform.position + bumpInnerOffset, Vector2.up * 0.2f, Color.blue);
        Debug.DrawRay((Vector2)transform.position - bumpInnerOffset, Vector2.up * 0.2f, Color.blue);
        Debug.DrawRay((Vector2)transform.position + maxCorrectorOffset, Vector2.right*0.3f, Color.blue);
        Debug.DrawRay((Vector2)transform.position - maxCorrectorOffset + new Vector2(0,0.4f), Vector2.left*0.3f, Color.blue);

        //canCornerCorrect = (headBumperOuterRight || headBumperOuterLeft) && (!headBumperMaxCorrectionRight && !headBumperMaxCorrectionLeft);
        //cornerCorrectDistance = ();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset1, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position  + bottomOffset1, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset2, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}

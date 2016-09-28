﻿using UnityEngine;
using System.Collections;


// Enemy willl travel in a u shapped movement starting from start positon
// moving in distance distanceToTravel1, 
// it will then turn in turnDirection.
// moving in distance distanceToTravel2
// finally it will turn, go back distanceToTravel1 and destroy itself
public class Wallmaster : MonoBehaviour {
    // direction to startMovement (will return in the opposite direction)
    public Direction startDirection = Direction.SOUTH;
    // direction to turn after reached specified number of blocks
    public Direction turnDirection = Direction.WEST;
    public Vector3 startPosition = Vector3.zero;
    public float distanceToTravel1 = 1.0f;
    public float distanceToTravel2 = 1.0f;
    public float velocity = 1.0f;
    public Sprite normalSprite;
    public Sprite LinkCapturedSprite;

    private Direction currentDirection = Direction.SOUTH;
    private float startTime;
    private Vector3 endPosition1 = Vector3.zero;
    private Vector3 endPosition2 = Vector3.zero;
    private Vector3 finalEndPosition = Vector3.zero;
    private PlayerControl pc;
    private SpriteRenderer spriteRenderer;
    enum WallMasterState { NORMAL, LINK_CAPTURED};
    WallMasterState currState = WallMasterState.NORMAL;


    // Use this for initialization
    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentDirection = startDirection;
        startTime = Time.time;
        // set up first end position
        switch (startDirection) {
            case Direction.NORTH:
                endPosition1.Set(startPosition.x, startPosition.y + distanceToTravel1, startPosition.z);
                // endPosition1 is the same as startPosition2
                break;
            case Direction.EAST:
                endPosition1.Set(startPosition.x + distanceToTravel1, startPosition.y, startPosition.z);
                break;
            case Direction.SOUTH:
                endPosition1.Set(startPosition.x, startPosition.y - distanceToTravel1, startPosition.z);
                break;
            case Direction.WEST:
                endPosition1.Set(startPosition.x - distanceToTravel1, startPosition.y, startPosition.z);
                break;
        }
        // set up final end position
        switch (turnDirection) {
            case Direction.NORTH:
                // endPosition1 is the same as startPosition2
                endPosition1.Set(endPosition1.x, endPosition1.y + distanceToTravel2, endPosition1.z);
                break;
            case Direction.EAST:
                endPosition1.Set(endPosition1.x + distanceToTravel2, endPosition1.y, endPosition1.z);
                break;
            case Direction.SOUTH:
                endPosition1.Set(endPosition1.x, endPosition1.y - distanceToTravel2, endPosition1.z);
                break;
            case Direction.WEST:
                endPosition1.Set(endPosition1.x - distanceToTravel2, endPosition1.y, endPosition1.z);
                break;
        }
        // set up final end position (travel in the opposite direction)
        switch (startDirection) {
            case Direction.NORTH:
                // finalEndPositionStart is the same as endPosition2
                finalEndPosition.Set(endPosition2.x, endPosition2.y - distanceToTravel1, endPosition2.z);
                break;
            case Direction.EAST:
                finalEndPosition.Set(endPosition2.x - distanceToTravel1, endPosition2.y, endPosition2.z);
                break;
            case Direction.SOUTH:
                finalEndPosition.Set(endPosition2.x, endPosition2.y + distanceToTravel1, endPosition2.z);
                break;
            case Direction.WEST:
                finalEndPosition.Set(endPosition2.x - distanceToTravel1, endPosition2.y, endPosition2.z);
                break;
        }
        pc = PlayerControl.instance;
    }
	
	// Update is called once per frame
	void Update () {
        float u;
        Vector3 currPos;
        if (currentDirection == startDirection) {
            // then we are in the first leg of our journey
            u = (Time.time - startTime) / (velocity / distanceToTravel1);
            currPos = Vector3.Lerp(startPosition, endPosition1, u);
            transform.position = currPos;
            if(u > 1) {
                // then we are at the end of the interpolation
                currentDirection = turnDirection;
                // adjust position, just incase there is a bit of an error from the interpolation
                transform.position = endPosition1;
            }
        } else if(currentDirection == turnDirection) {
            // then we are are turned
            u = (Time.time - startTime) / (velocity / distanceToTravel2);
            currPos = Vector3.Lerp(endPosition1, endPosition2, u);
            transform.position = currPos;
            if (u > 1) {
                currentDirection = UtilityFunctions.reverseDirection(startDirection);
                transform.position = endPosition2;
            }
        } else {
            // we are returning
            u = (Time.time - startTime) / (velocity / distanceToTravel1);
            currPos = Vector3.Lerp(endPosition2, finalEndPosition, u);
            transform.position = currPos;
            if(u > 1) {
                Destroy(this);
                CameraControl.S.ReturnToStart();
            }
        }
	}

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Link") {
            currState = WallMasterState.LINK_CAPTURED;
            spriteRenderer.sprite = LinkCapturedSprite;
        }
    }
}

﻿using UnityEngine;
using System.Collections;
// State Machines are responsible for processing states, notifying them when they're about to begin or conclude, etc.


public class StateMachine
{
	private State _current_state;
	
	public void ChangeState(State new_state)
	{
		if(_current_state != null)
		{
			_current_state.OnFinish();
		}

		_current_state = new_state;
		// States sometimes need to reset their machine. 
		// This reference makes that possible.
		MonoBehaviour.print ("I am in the change state");
		_current_state.state_machine = this;
		_current_state.OnStart();
	}
	
	public void Reset()
	{
		if(_current_state != null)
			_current_state.OnFinish();
		_current_state = null;
	}
	
	public void Update()
	{
		if(_current_state != null)
		{
			float time_delta_fraction = Time.deltaTime / (1.0f / Application.targetFrameRate);
			_current_state.OnUpdate(time_delta_fraction);
		}
	}

	public bool IsFinished()
	{
		return _current_state == null;
	}
}

// A State is merely a bundle of behavior listening to specific events, such as...
// OnUpdate -- Fired every frame of the game.
// OnStart -- Fired once when the state is transitioned to.
// OnFinish -- Fired as the state concludes.
// State Constructors often store data that will be used during the execution of the State.
public class State
{
	// A reference to the State Machine processing the state.
	public StateMachine state_machine;
	
	public virtual void OnStart() {}
	public virtual void OnUpdate(float time_delta_fraction) {} // time_delta_fraction is a float near 1.0 indicating how much more / less time this frame took than expected.
	public virtual void OnFinish() {}
	
	// States may call ConcludeState on themselves to end their processing.
	public void ConcludeState() { state_machine.Reset(); }
}

// A State that takes a renderer and a sprite, and implements idling behavior.
// The state is capable of transitioning to a walking state upon key press.
public class StateIdleWithSprite : State
{
	PlayerControl pc;
	SpriteRenderer renderer;
	Sprite sprite;

	public StateIdleWithSprite(PlayerControl pc, SpriteRenderer renderer, Sprite sprite)
	{
		this.pc = pc;
		this.renderer = renderer;
		this.sprite = sprite;
		MonoBehaviour.print ("idle make");
	}
	
	public override void OnStart()
	{
		MonoBehaviour.print ("idle now");
		renderer.sprite = sprite;
	}
	
	public override void OnUpdate(float time_delta_fraction)
	{
		MonoBehaviour.print ("before I do anything");
		if(pc.current_state == EntityState.ATTACKING)
			return;

		// Transition to walking animations on key press.
		if (Input.GetKeyDown (KeyCode.DownArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_down, 6, KeyCode.DownArrow));
		else if (Input.GetKeyDown (KeyCode.UpArrow) && !Input.GetKeyDown (KeyCode.DownArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_up, 6, KeyCode.UpArrow));
		else if (Input.GetKeyDown (KeyCode.RightArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_right, 6, KeyCode.RightArrow));
		else if (Input.GetKeyDown (KeyCode.LeftArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_left, 6, KeyCode.LeftArrow));
		//don't do anything if 2 arrow keys are pressed
	}
}

// A State for playing an animation until a particular key is released.
// Good for animations such as walking.
public class StatePlayAnimationForHeldKey : State
{
	PlayerControl pc;
	SpriteRenderer renderer;
	KeyCode key;
	Sprite[] animation;
	int animation_length;
	float animation_progression;
	float animation_start_time;
	int fps;
	
	public StatePlayAnimationForHeldKey(PlayerControl pc, SpriteRenderer renderer, Sprite[] animation, int fps, KeyCode key)
	{
		this.pc = pc;
		this.renderer = renderer;
		this.key = key;
		this.animation = animation;
		this.animation_length = animation.Length;
		this.fps = fps;
		MonoBehaviour.print ("Play animation created!");
		
		if(this.animation_length <= 0)
			Debug.LogError("Empty animation submitted to state machine!");
	}
	
	public override void OnStart()
	{
		animation_start_time = Time.time;
	}
	
	public override void OnUpdate(float time_delta_fraction)
	{
		MonoBehaviour.print ("attempting to move");
		if(pc.current_state == EntityState.ATTACKING)
			return;

		if(this.animation_length <= 0)
		{
			Debug.LogError("Empty animation submitted to state machine!");
			return;
		}
		
		// Modulus is necessary so we don't overshoot the length of the animation.
		int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
		renderer.sprite = animation[current_frame_index];
		
		// If another key is pressed, we need to transition to a different walking animation.
		if (Input.GetKeyDown (KeyCode.DownArrow) && !Input.GetKeyDown (KeyCode.UpArrow) && !Input.GetKeyDown (KeyCode.RightArrow) && !Input.GetKeyDown (KeyCode.LeftArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_down, 6, KeyCode.DownArrow));
		else if (Input.GetKeyDown (KeyCode.UpArrow) && !Input.GetKeyDown (KeyCode.DownArrow)  && !Input.GetKeyDown (KeyCode.RightArrow) && !Input.GetKeyDown (KeyCode.LeftArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_up, 6, KeyCode.UpArrow));
		else if (Input.GetKeyDown (KeyCode.RightArrow) && !Input.GetKeyDown (KeyCode.DownArrow)  && !Input.GetKeyDown (KeyCode.UpArrow) && !Input.GetKeyDown (KeyCode.LeftArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_right, 6, KeyCode.RightArrow));
		else if (Input.GetKeyDown (KeyCode.LeftArrow))
			state_machine.ChangeState (new StatePlayAnimationForHeldKey (pc, renderer, pc.link_run_left, 6, KeyCode.LeftArrow));
		// If we detect the specified key has been released, return to the idle state.
		else if(!Input.GetKey(key))
			state_machine.ChangeState(new StateIdleWithSprite(pc, renderer, animation[1]));
		else {
			//run in current direction but don't move don't know how to do that
		}
	}
}

public class StateLinkDoorMovementAnimation : State {
    private PlayerControl pc;
    private SpriteRenderer renderer;
    private Sprite[] animation;
    private int fps;
    private int animation_length;
    private float animation_progression;
    private float animation_start_time;
    private float length;
    private Sprite idleSprite;

    public StateLinkDoorMovementAnimation(PlayerControl pc, SpriteRenderer renderer, Sprite[] animation, int fps, float length) {
        this.pc = pc;
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;
        this.length = length;
        MonoBehaviour.print("Play animation created!");

        if (this.animation_length <= 0)
            Debug.LogError("Empty animation submitted to state machine!");
    }

    public override void OnStart() {
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        if (this.animation_length <= 0) {
            Debug.LogError("Empty animation submitted to state machine!");
            return;
        }
        if (Time.time >= animation_start_time + length) {
            state_machine.ChangeState(new StateIdleWithSprite(pc, renderer, animation[0]));
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];

    }
}

public class StateLinkStunnedMovement : State {
    private PlayerControl pc;
    private float coolDown;
    private float startTime;

    public StateLinkStunnedMovement(PlayerControl pc, float coolDown) {
        this.pc = pc;
        this.coolDown = coolDown;
    }

    public override void OnStart() {
        startTime = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        if(Time.time - startTime > coolDown) {
            state_machine.ChangeState(new StateLinkNormalMovement(pc));
        }
    }
}

public class StateLinkStunnedSprite : State {
    PlayerControl pc;
    SpriteRenderer renderer;
    Sprite sprite;
    float coolDown;
    float startTime;

    public StateLinkStunnedSprite(PlayerControl pc, SpriteRenderer renderer, Sprite sprite, float coolDown) {
        this.pc = pc;
        this.renderer = renderer;
        this.sprite = sprite;
        this.coolDown = coolDown;
        MonoBehaviour.print("idle make");
    }

    public override void OnStart() {
        MonoBehaviour.print("idle now");
        renderer.sprite = sprite;
        startTime = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
       if((Time.time - startTime) >= coolDown) {
            state_machine.ChangeState(new StateIdleWithSprite(pc, renderer, sprite));
        }
    }
}

    // Additional recommended states:
    // StateDeath
    // StateDamaged
    // StateWeaponSwing
    // StateVictory
    //
    // Additional control states:
    // LinkNormalMovement.
    // LinkStunnedState.


    public class StateLinkNormalMovement : State {
    PlayerControl pc;

    public StateLinkNormalMovement(PlayerControl pc) {
        this.pc = pc;
    }

    public override void OnUpdate(float time_delta_fraction) {
        float horizontal_input = Input.GetAxis("Horizontal");
        float vertical_input = Input.GetAxis("Vertical");
        if (horizontal_input != 0.0f) {
            vertical_input = 0.0f;
        }

        pc.GetComponent<Rigidbody>().velocity = new Vector3(horizontal_input, -vertical_input, 0)
                                                                                * pc.walkingVelocity
                                                                                * time_delta_fraction;
        Direction prevDirection = pc.current_direction;
        //Decide the current direction
        if (horizontal_input > 0.0f)
            pc.current_direction = Direction.EAST;
        else if (horizontal_input < 0.0f)
            pc.current_direction = Direction.WEST;
        else if (vertical_input > 0.0f)
            pc.current_direction = Direction.NORTH;
        else if (vertical_input < 0.0f)
            pc.current_direction = Direction.SOUTH;

        // if the direction has changed, have to snap him to the grid
        if (prevDirection != pc.current_direction) {
            if ((prevDirection == Direction.NORTH || prevDirection == Direction.SOUTH)
                && (pc.current_direction == Direction.EAST || pc.current_direction == Direction.WEST)) {
                // if axis is changed from vertical to horizontial
                // we have to fix his postion on the vertical axis
                Vector3 linkPosition = pc.transform.position;
                float posY = linkPosition.y;
                float nonFractionY = Mathf.Floor(posY);
                float fractionY = posY - nonFractionY;
                if (fractionY < 0.25) {
                    posY = nonFractionY;
                } else if (fractionY >= 0.25 && fractionY < 0.75) {
                    posY = nonFractionY + 0.5f;
                } else {
                    posY = nonFractionY + 1.0f;
                }
                linkPosition.Set(linkPosition.x, posY, linkPosition.z);
                pc.transform.position = linkPosition;

            } else if ((prevDirection == Direction.EAST || prevDirection == Direction.WEST)
                && (pc.current_direction == Direction.NORTH || pc.current_direction == Direction.SOUTH)) {
                // If axis is changed from horizontial to 
                // we have to fix his postion on the horizontial axis
                Vector3 linkPosition = pc.transform.position;
                float posX = linkPosition.x;
                float nonFractionX = Mathf.Floor(posX);
                float fractionX = posX - nonFractionX;
                if (fractionX < 0.25) {
                    posX = nonFractionX;
                } else if (fractionX >= 0.25 && fractionX < 0.75) {
                    posX = nonFractionX + 0.5f;
                } else {
                    posX = nonFractionX + 1.0f;
                }
                linkPosition.Set(posX, linkPosition.y, linkPosition.z);
                pc.transform.position = linkPosition;
            }
        }

        //link attack
        //if(Input.GetKeyDown(KeyCode.S) 

    }
}

public class StateEnemyMovementAnimation : State {
    private Enemy enemy;
    private SpriteRenderer renderer;
    private Sprite[] animation;
    private int fps;
    private int animation_length;
    float animation_progression;
    float animation_start_time;

    public StateEnemyMovementAnimation(Enemy enemy, SpriteRenderer renderer, Sprite[] animation, int fps) {
        this.enemy = enemy;
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;
        MonoBehaviour.print("Play animation created!");

        if (this.animation_length <= 0)
            Debug.LogError("Empty animation submitted to state machine!");
    }

    public override void OnStart() {
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        if (this.animation_length <= 0) {
            Debug.LogError("Empty animation submitted to state machine!");
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];

    }
}

// assumes enemy starts on grid
public class StateEnemyMovement : State {
    Enemy enemy;
    Direction direction;
    float timeToCrossTile;
    float turnProbability; // probability of turning for each tile reached
    //Vector3 lastTilePosition;
    //float lastRoundPos = -1.0f; // so we don't check for turning multiple times for the same tile
    float timeLastTile;
    Vector3 posLastTile;
    Vector3 posNextTile;

    public StateEnemyMovement(Enemy enemy, float timeToCrossTile, Direction direction, float turnProbability) {
        this.enemy = enemy;
        this.direction = direction;
        this.timeToCrossTile = timeToCrossTile;
        this.turnProbability = turnProbability;
    }

    public override void OnStart() {
        enemy.currDirection = direction;
        setTileLastAndNext();
        return;
    }



    public override void OnUpdate(float time_delta_fraction) {
        Vector3 currEnemyPosition = enemy.transform.position;
        bool onMainGrid = MoveEnemy();
        if (onMainGrid && shouldEnemyTurn()) {
            state_machine.ChangeState(new StateEnemyMovement(enemy, timeToCrossTile, UtilityFunctions.randomDirection(direction), turnProbability));
        } else {
            setTileLastAndNext();
        }
        return;
    }

    // returns true if we are back on the main grid
    protected bool MoveEnemy() {
        Vector3 currEnemyPosition = enemy.transform.position;
        float u = (Time.time - timeLastTile) / timeToCrossTile;
        if(u > 1.0f) {
            return true;
        }
        // if horizontial
        if (direction == Direction.NORTH || direction == Direction.SOUTH) {
            float newYValue = Mathf.Lerp(posLastTile.y, posNextTile.y, u);
            currEnemyPosition.Set(currEnemyPosition.x, newYValue, currEnemyPosition.z);
          // else is vertical
        } else {
            float newXValue = Mathf.Lerp(posLastTile.x, posNextTile.x, u);
            currEnemyPosition.Set(newXValue, currEnemyPosition.y, currEnemyPosition.z);
        }
        enemy.transform.position = currEnemyPosition;
        return false;
    }

    protected bool shouldEnemyTurn() {
        return (Random.value < turnProbability);
    }

    // sets the last tile to the current position and the next tiel
    // to the correct value relevant to the current position and direction
    protected void setTileLastAndNext() {
        posLastTile = enemy.transform.position;
        switch (direction) {
            case Direction.NORTH:
                posNextTile = new Vector3(posLastTile.x, posLastTile.y + 1.0f, posLastTile.z);
                break;
            case Direction.EAST:
                posNextTile = new Vector3(posLastTile.x + 1.0f, posLastTile.y, posLastTile.z);
                break;
            case Direction.SOUTH:
                posNextTile = new Vector3(posLastTile.x, posLastTile.y - 1.0f, posLastTile.z);
                break;
            case Direction.WEST:
                posNextTile = new Vector3(posLastTile.x - 1.0f, posLastTile.y, posLastTile.z);
                break;
        }
        timeLastTile = Time.time;
    }

    public override void OnFinish() {
        enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}

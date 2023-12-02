using Godot;
using System;
using System.Drawing.Drawing2D;

public class PlayerController : KinematicBody2D
{
    private Vector2 velocity = new Vector2();
    private int speed = 100;
    private int gravity = 400;
    private int jumpHeight = 100;
    private float friction = .1f;
    private float acceleration = 0.5f;
    private bool isDashing = false;
    private int dashSpeed = 500;
    private float dashTimer = .2f;
    private float dashTimerReset = .2f;
    private bool isDashAvailable = true;
    private bool isWallJumping = false;
    private float wallJumpTimer = .45f;
    private float wallJumpTimerTimerReset = .45f;
    private bool canClimb = false;
    private int climbSpeed = 100;
    private bool isClimbing = false;
    private float climbTimer = 5f;
    private float climbTimerReset = 5f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!isDashing && !isWallJumping)
        {
            processMovement(delta);
        }

        // -- jump --
        if (IsOnFloor())
        {
            if (Input.IsActionJustPressed("jump"))
            {
                velocity.y = -jumpHeight;
            }
            canClimb = true;
            isDashAvailable = true;
        }

        if (canClimb)
        {
            processClimb(delta);
        }
        if(!IsOnFloor())
        {
        processWallJump(delta);
        }

        // -- dash
        if (isDashAvailable)
        {
            processDash(delta);
        }
        if (isDashing)
        {
            dashTimer -= delta;
            if (dashTimer <= 0)
            {
                isDashing = false;
                velocity = new Vector2(0, 0);
            }
        }
        else if (!isClimbing)
        {
            // -- falling
            velocity.y += gravity * delta;
        }
        else
        {
            climbTimer -= delta;
            if(climbTimer <= 0)
            {
                isClimbing = false;
                canClimb = false;
                climbTimer = climbTimerReset;
            }
        }
        


        MoveAndSlide(velocity, Vector2.Up);

    }
    private void processClimb(float delta)
    {
        if (Input.IsActionPressed("climb") && (GetNode<RayCast2D>("RayCastLeft").IsColliding() || GetNode<RayCast2D>("RayCastRight").IsColliding() ||
        GetNode<RayCast2D>("RayCastRightClimb").IsColliding() || GetNode<RayCast2D>("RayCastLeftClimb").IsColliding()))
        {
            if (canClimb && !isWallJumping)
            {
                isClimbing = true;
                if (Input.IsActionPressed("ui_up"))
                {
                    velocity.y = -climbSpeed;
                }
                else if (Input.IsActionPressed("ui_down"))
                {
                    velocity.y = climbSpeed;
                }
                else
                {
                    velocity = new Vector2(0, 0);
                }
            }
            else
            {
                isClimbing = false;
            }
        }
        else
        {
            isClimbing = false;
        }
    }
    private void processMovement(float delta)
    {
        int direction = 0;
        // -- move left -- 
        if (Input.IsActionPressed("ui_left"))
        {
            direction -= 1;
            velocity.x -= speed;
        }
        // -- move right -- 
        if (Input.IsActionPressed("ui_right"))
        {
            direction += 1;
            velocity.x += speed;
        }
        if (direction != 0)
        {
            velocity.x = Mathf.Lerp(velocity.x, direction * speed, acceleration);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, friction);
        }
    }

    private void processWallJump(float delta)
    {
        if (Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastLeft").IsColliding())
        {
            velocity.y = -jumpHeight;
            velocity.x = jumpHeight;
            isWallJumping = true;
        }
        else if (Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastRight").IsColliding())
        {
            velocity.y = -jumpHeight;
            velocity.x = -jumpHeight;
            isWallJumping = true;
        }
        if (isWallJumping)
        {
            wallJumpTimer -= delta;
            if (wallJumpTimer <= 0)
            {
                isWallJumping = false;
                wallJumpTimer = wallJumpTimerTimerReset;
            }
        }
    }

    private void processDash(float delta)
    {
        if (Input.IsActionJustPressed("dash"))
        {
            if (Input.IsActionPressed("ui.left"))
            {
                velocity.x = -dashSpeed;
                isDashing = true;
            }
            if (Input.IsActionPressed("ui_right"))
            {
                velocity.x = dashSpeed;
                isDashing = true;
            }
            if (Input.IsActionPressed("ui_up"))
            {
                velocity.y = -dashSpeed;
                isDashing = true;
            }
            if (Input.IsActionPressed("ui_right") && Input.IsActionPressed("ui.up"))
            {
                velocity.x = dashSpeed;
                velocity.y = -dashSpeed;
                isDashing = true;
            }
            if (Input.IsActionPressed("ui_left") && Input.IsActionPressed("ui.up"))
            {
                velocity.x = dashSpeed;
                velocity.y = -dashSpeed;
                isDashing = true;
            }
            dashTimer = dashTimerReset;
            isDashAvailable = false;
        }
    }
}

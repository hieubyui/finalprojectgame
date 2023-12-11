using Godot;
using System;
using System.Drawing.Drawing2D;

public class PlayerController : KinematicBody2D
{
    private Vector2 velocity = new Vector2();
    private int speed = 100;
    private int gravity = 400;
    private int jumpHeight = 150;
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
    private bool isInAir = false;
    [Export]
    public PackedScene GhostPlayerInstance;
    private AnimatedSprite animatedSprite;
    public int Health = 3;
    private int facingDirection = 0;
    private bool isTakingDamage = false;
    [Signal]
    public delegate void Death();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(Health > 0){
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
                    animatedSprite.Play("Jump");
                    isInAir = true;
                }else{
                    isInAir = false;
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
                GhostPlayer ghost = GhostPlayerInstance.Instance() as GhostPlayer;
                Owner.AddChild(ghost);
                ghost.GlobalPosition = this.GlobalPosition;
                ghost.SetHValue(animatedSprite.FlipH);

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
        facingDirection = 0;
        if(!isTakingDamage){
            // -- move left -- 
            if (Input.IsActionPressed("ui_left"))
            {
                facingDirection -= 1;
                animatedSprite.FlipH = true;
            }
            // -- move right -- 
            if (Input.IsActionPressed("ui_right"))
            {
                facingDirection += 1;
                animatedSprite.FlipH = false;
            }
        }
        if (facingDirection != 0)
        {
            velocity.x = Mathf.Lerp(velocity.x, facingDirection * speed, acceleration);
            
            if(!isInAir)
                animatedSprite.Play("Run");
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, friction);
            if(velocity.x < 5 && velocity.x > -5){
                if(!isInAir)
                    animatedSprite.Play("Idle");
                isTakingDamage = false;
            }
        }
    }

    private void processWallJump(float delta)
    {
        if (Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastLeft").IsColliding())
        {
            velocity.y = -jumpHeight;
            velocity.x = jumpHeight;
            isWallJumping = true;
            animatedSprite.FlipH = false;
        }
        else if (Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastRight").IsColliding())
        {
            velocity.y = -jumpHeight;
            velocity.x = -jumpHeight;
            isWallJumping = true;
            animatedSprite.FlipH = true;
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
                velocity.x = -dashSpeed;
                velocity.y = -dashSpeed;
                isDashing = true;
            }
            dashTimer = dashTimerReset;
            isDashAvailable = false;
        }
    }

    public void TakeDamage(){
        GD.Print("Player has taken damage");
        Health -= 1;
        GD.Print("Current Health " +Health);
        velocity = MoveAndSlide(new Vector2(500f * -facingDirection, -80),Vector2.Up);
        isTakingDamage = true;
        animatedSprite.Play("TakeDamage");
        if(Health <= 0){
            Health = 0;
            animatedSprite.Play("Death");
            GD.Print("PLayer has died.");
        }
    }

    private void _on_AnimatedSprite_animation_finished(){
        if(animatedSprite.Animation == "Death"){
            animatedSprite.Stop();
            Hide();
            GD.Print("Animation finished");
            EmitSignal(nameof(Death));
        }
    }

    public void RespawnPlayer(){
        Show();
        Health = 3;
    }




}
using Godot;
using System;

public class ArcherEnemy : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private bool active;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(active){
            if(ableToShoot){
                var spaceState = GetWorld2d().DirectSpaceState;
                Godot.Collections.Dictionary result = spaceState.IntersectRay(this.Position,player.Position, new Godot.Collections.Array);
                if(result != null){
                    if(result.Contains("collider")){
                        this.GetNode<Position2D>("ProjectileSpawn").LookAt(player.Position);
                        if(result["collider"] == player){
                            GD.Print("shooting");
                            Arrow arrow = Arrow.Instance () as Arrow;
                            Owner.AddChild(arrow);
                            arrow.GlobalTransform = this.GetNode<Position2D>("ProjectileSpawn").GlobalTransform;
                            ableToShoot = false;
                            shootTimer = shootTimerReset;
                        }
                    }
                }
            }
        }
    }
}

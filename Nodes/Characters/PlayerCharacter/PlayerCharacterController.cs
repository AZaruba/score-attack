using Godot;
using System;

public partial class PlayerCharacterController : CharacterBody3D
{
  [Export] private InputComponent InputComponent;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private GravityComponent GravityComponent;
  [Export] private CharacterStats Stats;

  public override void _Ready()
  {
    
  }

  public override void _PhysicsProcess(double delta)
  {
    VelocityComponent.SetVelocity(Vector3.Zero);

    Vector3 CameraForward = -GetViewport().GetCamera3D().Basis.Z;
    Vector3 CameraRight = GetViewport().GetCamera3D().Basis.X;
    CameraForward.Y = 0;
    CameraRight.Y = 0;
    Vector3 MotionInput = CameraForward.Normalized() * InputComponent.GetMoveInput() + CameraRight.Normalized() * InputComponent.GetStrafeInput();
    VelocityComponent.AddForce(MotionInput.Normalized() * Stats.MoveSpeed);

    if (IsOnFloor())
    {
      GravityComponent.SetGravity(Vector3.Zero);
    }

    GravityComponent.ApplyVerticalForce(Stats.Gravity * (float)delta);
    if (InputComponent.GetJump())
    {
      GravityComponent.ApplyVerticalForce(Stats.JumpForce);
    }

    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }
}

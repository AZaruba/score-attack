using Godot;
using System;

public partial class InputComponent : Node
{
  private float MoveInput;
  private float StrafeInput;
  private float TimeJumpPressed;
  private float TimeAttackPressed;
  private bool JumpInput;
  private bool AttackInput;
  public override void _Ready()
  {
    
  }

  public override void _PhysicsProcess(double delta)
  {
    MoveInput = Input.GetAxis(InputAction.MoveBackward, InputAction.MoveForward);
    StrafeInput = Input.GetAxis(InputAction.MoveLeft, InputAction.MoveRight);
    JumpInput = Input.IsActionJustPressed(InputAction.Jump);
    AttackInput = Input.IsActionJustPressed(InputAction.Attack);

    if (Input.IsActionPressed(InputAction.Jump))
    {
      TimeJumpPressed += (float)delta;
    }
    else
    {
      TimeJumpPressed = 0;
    }

    if (Input.IsActionPressed(InputAction.Attack))
    {
      TimeJumpPressed += (float)delta;
    }
    else
    {
      TimeJumpPressed = 0;
    }
  }

  public float GetMoveInput() { return MoveInput; }
  public float GetStrafeInput() { return StrafeInput; }
  public float GetJumpHoldTime() { return TimeJumpPressed; }
  public float GetAttackHoldTime() { return TimeAttackPressed; }
  public bool GetJump() { return JumpInput; }
  public bool GetAttack() { return AttackInput; }
}

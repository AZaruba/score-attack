using Godot;
using StateManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Goon : CharacterBody3D
{
  [Export] NavigationAgent3D NavigationAgent;
  [Export] AnimatingBodyComponent AnimatingBodyComponent;
  [Export] VelocityComponent VelocityComponent;
  [Export] RotationComponent RotationComponent;
  [Export] GravityComponent GravityComponent;
  [Export] HealthComponent HealthComponent;
  [Export] StateMachineComponent StateComponent;
  [Export] CharacterStats Stats;

  // is this necessary if the game manager already knows the player state? (Stealth/last known position?)
  //[Signal] public delegate void RequestPlayerPositionEventHandler()
  private Dictionary<StateID, Action<float>> StateActions;

  private Vector3 NextTarget;
  private bool IsMovingToLink = false;
  private bool Arrived = false;

  public override void _Ready()
  {;
    HealthComponent.InitHealth(Stats.MaxHealth);
    InitStateMachine();
    InitSignals();
  }

  public override void _ExitTree()
  {
    TeardownSignals();
  }

  private void InitStateMachine()
  {
    StateActions = new Dictionary<StateID, Action<float>>
    {
      { StateID.APPROACHING, ApproachingState },
      { StateID.ORBITING, OrbitingState },
      { StateID.ENGAGING, EngagingState },
      { StateID.ATTACKING, AttackingState },
      { StateID.HIT_BY_ATTACK, HitRecoveryState }
    };
    StateComponent.AddState(StateID.APPROACHING);
    StateComponent.AddState(StateID.ORBITING);
    StateComponent.AddState(StateID.ENGAGING);
    StateComponent.AddState(StateID.ATTACKING);
    StateComponent.AddState(StateID.HIT_BY_ATTACK);
    StateComponent.AddTransitionToState(StateID.APPROACHING, Command.ORBIT, StateID.ORBITING);
    StateComponent.AddTransitionToState(StateID.ORBITING, Command.ENGAGE, StateID.ENGAGING);
    StateComponent.AddTransitionToState(StateID.ENGAGING, Command.START_ATTACK, StateID.ATTACKING); // 
    StateComponent.AddTransitionToState(StateID.ATTACKING, Command.FINISH_ATTACK, StateID.ENGAGING);
    StateComponent.AddTransitionToState(StateID.ENGAGING, Command.DISENGAGE, StateID.ORBITING);
    StateComponent.AddTransitionToAllStates(Command.GET_HIT, StateID.HIT_BY_ATTACK, [StateID.BLOCKING]);
    StateComponent.Enter(StateID.APPROACHING);
  }

  private void InitSignals()
  {
    HealthComponent.OnHealthZero += OnHealthZero;
  }

  private void TeardownSignals()
  {
    HealthComponent.OnHealthZero -= OnHealthZero;
  }

  public override void _PhysicsProcess(double delta)
  {
    RunStateMachineFunction(StateActions[StateComponent.GetCurrentState()], (float)delta);
    UpdateStateMachine();
    DebugLog.Log(StateComponent.GetCurrentState().ToString(), 0);
    DebugLog.Log($"Target Position: {NavigationAgent.TargetPosition}", 1);
  }

  private void OnLinkReached(Godot.Collections.Dictionary details)
  {
    IsMovingToLink = true;
    NextTarget = (Vector3)details["link_exit_position"];
  }

  private void OnTargetReached()
  {
    // Change to arrived state
    Arrived = true;
  }

  private void OnHitByAttack(Node3D AttackNode)
  {
    GD.Print("hit");
    HealthComponent.Damage(10);
  }

  private void OnHealthZero()
  {
    // TODO: return to object pool
    QueueFree();
  }
  public void RunStateMachineFunction(Action<float> StateFunction, float delta)
  {
    StateFunction(delta);
  }

  private void UpdateStateMachine()
  {
    if (Arrived)
    {
      StateComponent.RunCommand(Command.ORBIT);
    }
  }

  public void UpdateTargetPosition(Vector3 position)
  {
    NavigationAgent.TargetPosition = position;
  }

  private void ApproachingState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, IsOnFloor());
    if (IsMovingToLink)
    {
      // hmm
      if (IsOnFloor())
      {
        GravityComponent.ApplyVerticalForce(Stats.JumpForce);
        IsMovingToLink = false;
      }
    }
    else
    {
      NextTarget = NavigationAgent.GetNextPathPosition();
    }

    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(FlatVelocity);
    Vector3 Direction = NextTarget - Position;
    Direction.Y = 0;

    VelocityComponent.AddForce(Direction.Normalized() * Stats.MoveAcceleration);

    float ToAngle = Basis.Z.SignedAngleTo(VelocityComponent.GetCurrentVelocity(), Vector3.Up);
    Basis = RotationComponent.RotateBasisR(Basis, Vector3.Up, ToAngle);

    //Animate 
    AnimatingBodyComponent.SetAnimationParameter(PCAnimationNames.MoveBlendPath, VelocityComponent.GetCurrentVelocity().Length() / Stats.MoveSpeed);
    
    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }

  private void OrbitingState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, IsOnFloor());
    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(Vector3.Zero);

    Vector3 CurrentOffset = Position - NavigationAgent.TargetPosition;
    // pick a direction, I guess clockwise for testing
    Vector3 OrbitTarget = CurrentOffset.Rotated(Vector3.Up, Mathf.DegToRad(Stats.RotationRate)) + NavigationAgent.TargetPosition;
    VelocityComponent.AddForce((OrbitTarget - Position).Normalized() * Stats.MoveAcceleration);
    DebugLog.Log($"Orbit Dest {(OrbitTarget - Position).ToString()}", 2);
    
    float ToAngle = Basis.Z.SignedAngleTo(CurrentOffset * -1, Vector3.Up);
    Basis = RotationComponent.RotateBasisR(Basis, Vector3.Up, ToAngle);

    // close distance

    if ((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) > 0)
    {
      VelocityComponent.AddForce((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) * -1 * Stats.MoveSpeed * CurrentOffset);
    }

    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();

  }

  private void EngagingState(float delta)
  {
    
  }

  private void AttackingState(float delta)
  {
    
  }

  private void HitRecoveryState(float delta)
  {
    
  }

  // TODO: Waiting state
  private void WaitingState(float delta)
  {
    
  }


}

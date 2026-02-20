using Godot;
using StateManagement;
using System;
using System.Collections.Generic;

public partial class Goon : CharacterBody3D
{
  [Export] NavigationAgent3D NavigationAgent;
  [Export] AnimatingBodyComponent AnimatingBodyComponent;
  [Export] VelocityComponent VelocityComponent;
  [Export] RotationComponent RotationComponent;
  [Export] GravityComponent GravityComponent;
  [Export] HealthComponent HealthComponent;
  [Export] DecisionTreeComponent DecisionTreeComponent;
  [Export] AttackAccumulationComponent AttackAccumulationComponent;
  [Export] WorldResourceBar HealthBar;
  [Export] CharacterStats Stats;

  private StateID LastDecisionState;

  // is this necessary if the game manager already knows the player state? (Stealth/last known position?)
  //[Signal] public delegate void RequestPlayerPositionEventHandler()
  private Dictionary<StateID, Action<float>> StateActions;

  // will each state have much more complicated reactions to getting hit?
  //private Dictionary<StateID, Action<float>> StateOnHitByAttackActions;

  private Vector3 NextTarget;
  private bool IsMovingToLink = false;
  private bool Arrived = false;

  public override void _Ready()
  {;
    HealthComponent.InitHealth(Stats.MaxHealth);
    HealthBar.SetResourceMax(HealthComponent.GetCurrentHealth());
    HealthBar.SetResourceValue(HealthComponent.GetCurrentHealth());
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
      { StateID.HIT_BY_ATTACK, HitRecoveryState },
      { StateID.BLOCKING, BlockingState }
    };

    // Decision Tree version
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddBranch(DecisionID.IDLE); // currently will never get here
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_IN_RANGE, DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_NOT_IN_RANGE, DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddStateLeaf(StateID.GROUNDED, DecisionID.IDLE);
    DecisionTreeComponent.AddStateLeaf(StateID.APPROACHING, DecisionID.TARGET_NOT_IN_RANGE);
    DecisionTreeComponent.AddStateLeaf(StateID.ORBITING, DecisionID.TARGET_IN_RANGE);
    DecisionTreeComponent.SetBehavior(IsTargetNoticed, DecisionID.ROOT);
    DecisionTreeComponent.SetBehavior(IsTargetInRange, DecisionID.TARGET_IS_NOTICED);


  }

  public DecisionID IsTargetNoticed()
  {
    return DecisionID.TARGET_IS_NOTICED;
  }

  public DecisionID IsTargetInRange()
  {
    if (Arrived)
    {
      return DecisionID.TARGET_IN_RANGE;
    }
    return DecisionID.TARGET_NOT_IN_RANGE;
  }

  private void InitSignals()
  {
    HealthComponent.OnHealthZero += OnHealthZero;
    AnimatingBodyComponent.OnCurrentAnimationFinished += OnCurrentAnimationFinished;
    AttackAccumulationComponent.OnThresholdExceeded += OnAttackDamageThresholdHit;
  }

  private void TeardownSignals()
  {
    HealthComponent.OnHealthZero -= OnHealthZero;
    AnimatingBodyComponent.OnCurrentAnimationFinished -= OnCurrentAnimationFinished;
    AttackAccumulationComponent.OnThresholdExceeded -= OnAttackDamageThresholdHit;
  }

  public override void _PhysicsProcess(double delta)
  {
    LastDecisionState = DecisionTreeComponent.MakeDecision();
    RunStateMachineFunction(StateActions[LastDecisionState], (float)delta);
    // UpdateStateMachine();
    DebugLog.Log(LastDecisionState.ToString(), 0);
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
    DebugLog.LogTemp("Got Hit", 2);
    // TODO: investigate simply setting a flag for "eligible for hit"
    if (LastDecisionState != StateID.BLOCKING)
    {
      HealthComponent.Damage(10);
      HealthBar.RevealHealthBar();
      HealthBar.SetResourceValue(HealthComponent.GetCurrentHealth());
      AnimatingBodyComponent.TravelTo("HitStun");
    }
    AttackAccumulationComponent.AddDamage(10);
    //StateComponent.RunCommand(Command.GET_HIT);
  }

  private void OnHealthZero()
  {
    // TODO: return to object pool
    QueueFree();
  }

  private void OnCurrentAnimationFinished()
  {
    // TODO: make this smarter
    // StateComponent.RunCommand(Command.RECOVER_FROM_HIT);
    // StateComponent.RunCommand(Command.FINISH_ATTACK);
  }

  private void OnAttackDamageThresholdHit(int cmd)
  {
    if (LastDecisionState == StateID.BLOCKING)
    {
      //StateComponent.RunCommand(Command.START_ATTACK);
      AnimatingBodyComponent.TravelTo("Punch");
    }
    else
    {
      //StateComponent.RunCommand((Command)cmd);
      AnimatingBodyComponent.TravelTo("Block");
    }
  }

  public void RunStateMachineFunction(Action<float> StateFunction, float delta)
  {
    StateFunction(delta);
  }

  // private void UpdateStateMachine()
  // {
  //   if (Arrived)
  //   {
  //     StateComponent.RunCommand(Command.ORBIT);
  //   }
  //   AnimatingBodyComponent.SetCurrentState(StateComponent.GetCurrentState());
  // }

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

  private void BlockingState(float delta)
  {
    
  }

  // TODO: Waiting state
  private void WaitingState(float delta)
  {
    
  }


}

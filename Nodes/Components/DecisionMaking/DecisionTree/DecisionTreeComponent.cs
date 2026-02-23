using Godot;
using StateManagement;
using System;
using System.Security.Cryptography;

/*
 
  ROOT -> TARGET NOTICED?
    YES -> TARGET IN RANGE?
      YES -> WANT TO ENGAGE?
        YES -> READY TO ATTACK?
          YES -> QUEUE ATTACK
          NO  -> CLOSE DISTANCE (ENGAGE)
        NO  -> LEAF: ORBIT
      NO  -> LEAF: APPROACH
    NO  -> LEAF: STAY IDLE


*/

public partial class DecisionTreeComponent : Node
{
  public DecisionTreeNode<StateID> DecisionTree;
  [Export] public Timer DecisionTimer;

  private StateID LastDecision;

  private bool Locked;
  public override void _Ready()
  {
    DecisionTree = new DecisionTreeNode<StateID>();
    Locked = false;
    DecisionTimer.Timeout += Unlock;
  }

  public override void _PhysicsProcess(double delta)
  {
    DebugLog.Log(Locked.ToString(), 1);
  }

  private void Unlock()
  {
    Locked = false;
  }

  // Return Value: Whether or not the branch was successfully added
  public bool AddBranch(DecisionID NewBranchID, DecisionID ParentID = DecisionID.ROOT)
  {

    return DecisionTree.AddBranch(new DecisionTreeNode<StateID>(NewBranchID), ParentID);
  }

  // Return Value: Whether or not the branch was successfully added
  public bool AddStateLeaf(StateID StateLeaf, DecisionID ID)
  {
    return DecisionTree.AddLeaf(StateLeaf, ID);
  }

  public bool SetBehavior(Func<DecisionID> Decision, DecisionID BranchID)
  {
    return DecisionTree.SetBehavior(Decision, BranchID);
  }

  public bool SetLeafTimer(float Timeout, DecisionID ID)
  {
    return DecisionTree.SetLockTime(Timeout, ID);
  }

  public StateID MakeDecision()
  {
    if (Locked)
    {
      return LastDecision;
    }

    StateID idOut = DecisionTree.GetBehavior(out float LockTime);
    if (LockTime > 0)
    {
      Locked = true;
      DecisionTimer.WaitTime = LockTime;
      DecisionTimer.Start();
    }
    LastDecision = idOut;
    return idOut;
  }
}

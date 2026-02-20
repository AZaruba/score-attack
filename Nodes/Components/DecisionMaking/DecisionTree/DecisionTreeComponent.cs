using Godot;
using StateManagement;
using System;

/*
 
  ROOT -> TARGET NOTICED?
    YES -> TARGET IN RANGE?
      YES -> LEAF: ORBIT
      NO  -> LEAF: APPROACH
    NO  -> LEAF: STAY IDLE


*/

public partial class DecisionTreeComponent : Node
{
  public DecisionTreeNode<StateID> DecisionTree;

  public override void _Ready()
  {
    DecisionTree = new DecisionTreeNode<StateID>();
  }

  public override void _PhysicsProcess(double delta)
  {
    
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

  public StateID MakeDecision()
  {
    return DecisionTree.GetBehavior();
  }
}

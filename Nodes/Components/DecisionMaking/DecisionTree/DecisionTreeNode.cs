
using System;
using System.Collections.Generic;

public enum DecisionID
{
  ERR_DECISION = -1,
  ROOT,
  IDLE,
  TARGET_IS_NOTICED,
  TARGET_IN_RANGE,
  TARGET_NOT_IN_RANGE,
  WANT_TO_ENGAGE,
  WANT_TO_APPROACH,
  WANT_TO_ORBIT,
  ENGAGING,
  WANT_TO_DISENGAGE,
  WANT_TO_BLOCK,
  WANT_TO_COUNTER,
  WANT_TO_ENTER_VIEW,
  WANT_TO_ATTACK,
  PICK_ATTACK
}

public class DecisionTreeNode<T>
{
  private List<DecisionTreeNode<T>> Branches;

  private DecisionID ID;
  private T Leaf;

  public Func<DecisionID> Decision;


  public DecisionTreeNode(DecisionID ID = DecisionID.ROOT)
  {
    this.ID = ID;
    Branches = [];
  }

  public bool AddLeaf(T Leaf, DecisionID Parent)
  {
    if (ID == Parent)
    {
      this.Leaf = Leaf;
      return true;
    }
    else
    {
      foreach(DecisionTreeNode<T> ChildBranch in Branches)
      {
        if (ChildBranch.AddLeaf(Leaf, Parent))
        {
          return true;
        }
      }
    }
    return false;
  }

  public bool AddBranch(DecisionTreeNode<T> Branch, DecisionID Parent = DecisionID.ROOT)
  {
    if (ID == Parent)
    {
      Branches.Add(Branch);
      return true;
    }
    else
    {
      foreach(DecisionTreeNode<T> ChildBranch in Branches)
      {
        if (ChildBranch.AddBranch(Branch, Parent))
        {
          return true;
        }
      }
    }
    return false;
  }

  public bool SetBehavior(Func<DecisionID> Decision, DecisionID BranchID)
  {
    if (ID == BranchID)
    {
      this.Decision = Decision;
      return true;
    }
    else
    {
      foreach(DecisionTreeNode<T> ChildBranch in Branches)
      {
        if (ChildBranch.SetBehavior(Decision, BranchID))
        {
          return true;
        }
      }
    }
    return false;
  }
  
  public T GetBehavior()
  {
    if (Branches.Count == 0)
    {
      return Leaf;
    }

    DecisionID decisionId = Decision();
    foreach(DecisionTreeNode<T> ChildBranch in Branches)
    {
      if (ChildBranch.ID == decisionId)
      {
        return ChildBranch.GetBehavior();
      }
    }

    throw new Exception("Decision Tree failed to reach a leaf");
  }
}
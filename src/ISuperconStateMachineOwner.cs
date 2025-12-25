using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Raele.Supercon2D;

public interface ISuperconStateMachineOwner
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	public static ISuperconStateMachineOwner? GetOrNull(Node node)
	{
		for (Node? ancestor = node.GetParent(); ancestor != null; ancestor = ancestor.GetParent())
		{
			if (ancestor is ISuperconStateMachineOwner stateMachineOwner)
			{
				return stateMachineOwner;
			}
		}
		return null;
	}
	public static bool TryGet(Node node, [NotNullWhen(true)] out ISuperconStateMachineOwner? owner)
	{
		owner = GetOrNull(node);
		return owner != null;
	}

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	public SuperconState? RestState { get; }
	public SuperconStateMachine StateMachine { get; }

	//------------------------------------------------------------------------------------------------------------------
	// ABSTRACT METHODS
	//------------------------------------------------------------------------------------------------------------------

	public Node AsNode();

	//------------------------------------------------------------------------------------------------------------------
	// DEFAULT IMPLEMENTATIONS
	//------------------------------------------------------------------------------------------------------------------

	public SuperconBody2D? Character
		=> this is SuperconBody2D character
			? character
			: ISuperconStateMachineOwner.GetOrNull(this.AsNode())?.Character;

	public void ResetState() => this.StateMachine.QueueTransition(this.RestState);
	public void QueueTransition(string stateName, Variant data = default)
	{
		SuperconState? state = this.AsNode().GetParent().GetNodeOrNull(stateName) as SuperconState;
		if (!string.IsNullOrEmpty(stateName) && state == null)
		{
			GD.PushError($"[{this.GetType().Name} at \"{this.AsNode().GetPath()}\"] {nameof(QueueTransition)}() failed. Cause: SuperconState node not found. State name: '{stateName}'. Does it exists and has the correct script attached?");
			return;
		}
		this.QueueTransition(state, data);
	}
	public void QueueTransition(SuperconState? state, Variant data = default) => this.StateMachine.QueueTransition(state, data);
	public void Stop() => this.StateMachine.Stop();
}

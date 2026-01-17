using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Raele.Supercon2D;

public interface ISuperconStateMachineOwner
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	public static ISuperconStateMachineOwner? GetStateMachineOwnerOf(Node node)
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
	public static bool TryGetStateMachineOwnerOf(Node node, [NotNullWhen(true)] out ISuperconStateMachineOwner? owner)
	{
		owner = GetStateMachineOwnerOf(node);
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
			: ISuperconStateMachineOwner.GetStateMachineOwnerOf(this.AsNode())?.Character;

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

public static class ISuperconStateMachineOwnerExtensions
{
	extension(ISuperconStateMachineOwner self)
	{
		public ISuperconStateMachineOwner AsStateMachineOwner() => self;
	}
}

using System;
using Godot;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconStateLayer : SuperconState, ISuperconStateMachineOwner
{
	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export] public SuperconState? RestState { get; set; }

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintStateChanges = false;

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	public SuperconStateMachine StateMachine { get; } = new();

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// SIGNALS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.StateEntered += this.OnStateEntered;
		this.StateExited += this.OnStateExited;
		this.StateMachine.TransitionCompleted += this.OnStateTransitionCompleted;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.StateEntered -= this.OnStateEntered;
		this.StateExited -= this.OnStateExited;
		this.StateMachine.TransitionCompleted -= this.OnStateTransitionCompleted;
	}

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	Node ISuperconStateMachineOwner.AsNode() => this;
	public ISuperconStateMachineOwner AsStateMachineOwner() => this;

	private void OnStateEntered(SuperconStateMachine.Transition transition)
		=> this.AsStateMachineOwner().QueueTransition(this.RestState);
	private void OnStateExited(SuperconStateMachine.Transition transition)
		=> this.AsStateMachineOwner().Stop();
	private void OnStateTransitionCompleted(SuperconStateMachine.Transition transition)
	{
		if (this.DebugPrintStateChanges) {
			GeneralUtil.DebugLog<SuperconStateLayer>($"🔀 State changed: {transition.ExitState?.Name ?? "<null>"} → {transition.EnterState?.Name ?? "<null>"}");
		}
	}
}

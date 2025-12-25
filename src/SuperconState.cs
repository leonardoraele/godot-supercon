using System;
using Godot;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Node2D, SuperconStateMachine.IState
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public ProcessModeEnum ProcessModeWhenActive = ProcessModeEnum.Inherit;
	[Export] public ProcessModeEnum ProcessModeWhenInactive = ProcessModeEnum.Disabled;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public ISuperconStateMachineOwner? StateMachineOwner => ISuperconStateMachineOwner.GetOrNull(this);

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.StateMachineOwner?.StateMachine.ActiveState == this;
	public bool IsPreviousActiveState => this.StateMachineOwner?.StateMachine.PreviousActiveState == this;
	public TimeSpan ActiveDuration => this.IsActive
		? this.StateMachineOwner?.StateMachine.ActiveStateDuration ?? TimeSpan.Zero
		: TimeSpan.Zero;
	public double ActiveDurationMs => this.ActiveDuration.TotalMilliseconds;
	public SuperconInputMapping? InputMapping => this.StateMachineOwner?.Character?.InputMapping;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateEnteredEventHandler(SuperconStateMachine.Transition transition);
	[Signal] public delegate void StateExitedEventHandler(SuperconStateMachine.Transition transition);

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		if (!Engine.IsEditorHint())
		{
			this.ProcessMode = this.ProcessModeWhenInactive;
		}
	}

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.ToArray();

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		try
		{
			this.EmitSignalStateEntered(transition);
		}
		finally
		{
			if (!transition.IsCanceled)
			{
				this.ProcessMode = this.ProcessModeWhenActive;
			}
		}
	}

	void SuperconStateMachine.IState.ExitState(SuperconStateMachine.Transition transition)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		try
		{
			this.EmitSignalStateExited(transition);
		}
		finally
		{
			if (!transition.IsCanceled)
			{
				this.ProcessMode = this.ProcessModeWhenInactive;
			}
		}
	}

	public void QueueTransition() => this.StateMachineOwner?.StateMachine.QueueTransition(this);
	public void QueueTransition(Variant data) => this.StateMachineOwner?.StateMachine.QueueTransition(this, data);
}

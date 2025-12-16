using System;
using Godot;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Node2D, SuperconStateMachine.IState
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.Flags, "X:1,Y:2")] public byte ResetVelocityOnEnter = 0;
	[Export] public ProcessModeEnum ProcessModeWhenActive = ProcessModeEnum.Inherit;
	[Export] public ProcessModeEnum ProcessModeWhenInactive = ProcessModeEnum.Disabled;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => field ??= this.GetParent<SuperconBody2D>();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.StateMachine.ActiveState == this;
	public bool IsPreviousActiveState => this.StateMachine.PreviousActiveState == this;
	public TimeSpan ActiveDuration => this.IsActive ? this.StateMachine.ActiveStateDuration : TimeSpan.Zero;
	public double ActiveDurationMs => this.ActiveDuration.TotalMilliseconds;
	public SuperconStateMachine StateMachine => this.Character.StateMachine;
	public SuperconInputMapping InputMapping => this.Character.InputMapping;

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

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.ProcessMode = this.ProcessModeWhenActive;
		this.ApplyResetVelocityOnEnter();
		this.EmitSignalStateEntered(transition);
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
			this.ProcessMode = this.ProcessModeWhenInactive;
		}
	}

	public void QueueTransition(Variant data = default) => this.StateMachine.QueueTransition(this, data);

	private void ApplyResetVelocityOnEnter()
	{
		if ((this.ResetVelocityOnEnter & 1) != 0)
		{
			this.Character.VelocityX = 0;
		}
		if ((this.ResetVelocityOnEnter & 2) != 0)
		{
			this.Character.VelocityY = 0;
		}
	}
}

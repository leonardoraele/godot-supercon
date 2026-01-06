using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Raele.GodotUtils;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Node2D, SuperconStateMachine.IState, IActivity
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
	public SuperconInputMapping? InputMapping => this.StateMachineOwner?.Character?.InputMapping;
	public TimeSpan ActiveTimeSpan => this.IsActive
		? this.StateMachineOwner?.StateMachine.ActiveStateDuration ?? TimeSpan.Zero
		: TimeSpan.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateWillEnterEventHandler(SuperconStateMachine.Transition transition, GodotCancellationController cancellationController);
	[Signal] public delegate void StateEnteredEventHandler(SuperconStateMachine.Transition transition);
	[Signal] public delegate void StateWillExitEventHandler(SuperconStateMachine.Transition transition, GodotCancellationController cancellationController);
	[Signal] public delegate void StateExitedEventHandler(SuperconStateMachine.Transition transition);

	event Action<Variant, GodotCancellationController> IActivity.EventWillStart
	{
		add => this.Connect(SignalName.StateWillEnter, value.ToCallable());
		remove => this.Disconnect(SignalName.StateWillEnter, value.ToCallable());
	}
	event Action<Variant> IActivity.EventStarted
	{
		add => this.Connect(SignalName.StateEntered, value.ToCallable());
		remove => this.Disconnect(SignalName.StateEntered, value.ToCallable());
	}
	event Action<Variant, GodotCancellationController> IActivity.EventWillFinish
	{
		add => this.Connect(SignalName.StateWillExit, value.ToCallable());
		remove => this.Disconnect(SignalName.StateWillExit, value.ToCallable());
	}
	event Action<Variant> IActivity.EventFinished
	{
		add => this.Connect(SignalName.StateExited, value.ToCallable());
		remove => this.Disconnect(SignalName.StateExited, value.ToCallable());
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	void IActivity.Start(Variant argument)
	{
		if (this.IsActive || argument.AsGodotObject() is not SuperconStateMachine.Transition transition)
			throw new InvalidOperationException("Cannot start a state that is already active.");
		GodotCancellationController controller = new();
		this.EmitSignalStateWillEnter(transition, controller);
		if (transition.IsCanceled || controller.IsCancellationRequested)
			return;
		this.QueueTransition(argument);
	}
	void IActivity.Finish(Variant reason)
	{
		if (!this.IsActive || reason.AsGodotObject() is not SuperconStateMachine.Transition transition)
			throw new InvalidOperationException("Cannot finish a state that is not active.");
		GodotCancellationController controller = new();
		this.EmitSignalStateWillExit(transition, controller);
		if (transition.IsCanceled || controller.IsCancellationRequested)
			return;
		this.StateMachineOwner?.ResetState();
	}

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

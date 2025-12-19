using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public abstract partial class SuperconStateComponent : Node2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public bool Enabled = true;
	/// <summary>
	/// Time in miliseconds from to start this component after the character has switched to this state.
	/// </summary>
	[Export] public float StartDelayMs = 0f;
	/// <summary>
	/// Time in miliseconds from to stop this component after it has started.
	/// </summary>
	[Export] public float MaxProcessDurationMs = float.PositiveInfinity;
	/// <summary>
	/// If this array is not empty, this component only starts if the SuperconCharacterBody2D has switched to this state
	/// from one of the listed ones.
	/// </summary>
	[Export] public Godot.Collections.Array<SuperconState>? PreviousStateAllowlist;
	/// <summary>
	/// This component won't start if the SuperconCharacterBody2D has switched to this state from one of the listed
	/// ones.
	/// </summary>
	[Export] public Godot.Collections.Array<SuperconState>? PreviousStateForbidlist;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState State => field ??= this.GetParent<SuperconState>();
	public SuperconBody2D Character => this.State.Character;
	public SuperconInputMapping InputMapping => this.Character.InputMapping;
	public SuperconStateMachine StateMachine => this.Character.StateMachine;

	private bool Started = false;
	private bool ShouldProcess =>
		this.Enabled
		&& this.State.ActiveDurationMs >= this.StartDelayMs
		&& this.State.ActiveDurationMs < this.StartDelayMs + this.MaxProcessDurationMs
		&& (
			this.StateMachine.PreviousActiveState == null
			|| this.PreviousStateAllowlist?.Contains(this.StateMachine.PreviousActiveState) != false
			&& this.PreviousStateForbidlist?.Contains(this.StateMachine.PreviousActiveState) != true
		);

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.GetParentOrNull<SuperconState>()?.Connect(SuperconState.SignalName.StateEntered, new Callable(this, MethodName.OnStateEntered));
		this.GetParentOrNull<SuperconState>()?.Connect(SuperconState.SignalName.StateExited, new Callable(this, MethodName.OnStateExited));
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			this.SetPhysicsProcess(false);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.GetParentOrNull<SuperconState>()?.Disconnect(SuperconState.SignalName.StateEntered, new Callable(this, MethodName.OnStateEntered));
		this.GetParentOrNull<SuperconState>()?.Disconnect(SuperconState.SignalName.StateExited, new Callable(this, MethodName.OnStateExited));
	}

	public override void _Process(double delta)
	{
		if (this.ShouldProcess)
		{
			if (!this.Started)
			{
				this.Start();
			}
			this._SuperconProcess(delta);
		}
		else if (this.Started)
		{
			this.Stop();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (this.ShouldProcess)
		{
			this._SuperconPhysicsProcess(delta);
		}
	}

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(this.GetParentOrNull<SuperconState>() == null ? [$"{nameof(SuperconStateComponent)} must be a child of a {nameof(SuperconState)} node."] : [])
			.ToArray();

	public virtual void _SuperconEnter(SuperconStateMachine.Transition transition) {}
	public virtual void _SuperconStart() {}
	public virtual void _SuperconProcess(double delta) {}
	public virtual void _SuperconPhysicsProcess(double delta) {}
	public virtual void _SuperconStop() {}
	public virtual void _SuperconExit(SuperconStateMachine.Transition transition) {}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnStateEntered(SuperconStateMachine.Transition transition)
	{
		this.Started = false;
		this._SuperconEnter(transition);
		if (Mathf.IsZeroApprox(this.StartDelayMs) && this.Enabled)
		{
			this.Start();
		}
	}

	private void OnStateExited(SuperconStateMachine.Transition transition)
	{
		if (this.Started)
		{
			this.Stop();
		}
		this._SuperconExit(transition);
	}

	private void Start()
	{
		this.Started = true;
		this._SuperconStart();
	}

	private void Stop()
	{
		this.Started = false;
		this._SuperconStop();
	}
}

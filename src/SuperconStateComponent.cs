using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public abstract partial class SuperconStateComponent : Node2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public bool Enabled
		{
			get;
			set
			{
				if (Engine.IsEditorHint())
				{
					this.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
				}
				field = value;
			}
		}
		= true;

	[ExportGroup("Process Constraints", "Process")]
	/// <summary>
	/// Time in miliseconds from to start this component after the character has switched to this state.
	/// </summary>
	[Export(PropertyHint.None, "suffix:ms")] public float ProcessStartDelay = 0f;
	/// <summary>
	/// Time in miliseconds from to stop this component after it has started.
	/// </summary>
	[Export(PropertyHint.None, "suffix:ms")] public float ProcessMaxProcessDuration = float.PositiveInfinity;
	/// <summary>
	/// If this array is not empty, this component only starts if the SuperconCharacterBody2D has switched to this state
	/// from one of the listed ones.
	/// </summary>
	[Export] public NodePath?[] ProcessPreviousStateAllowlist = [];
	/// <summary>
	/// This component won't start if the SuperconCharacterBody2D has switched to this state from one of the listed
	/// ones.
	/// </summary>
	[Export] public NodePath?[] ProcessPreviousStateForbidlist = [];

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private bool Started = false;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState? State => this.GetParentOrNull<SuperconState>();
	public ISuperconStateMachineOwner? StateMachineOwner => this.State?.StateMachineOwner;
	public SuperconBody2D? Character => this.StateMachineOwner?.Character;

	private bool ShouldProcess =>
		this.Enabled
		&& (this.State?.ActiveDurationMs ?? 0f) >= this.ProcessStartDelay
		&& (this.State?.ActiveDurationMs ?? 0f) < this.ProcessStartDelay + this.ProcessMaxProcessDuration
		&& this.TestAllowlist()
		&& this.TestForbidlist();

	private IEnumerable<SuperconState> ProcessPreviousStateAllowlistResolved
		=> this.ProcessPreviousStateAllowlist
			.Select(path => this.GetNodeOrNull<SuperconState>(path))
			.OfType<SuperconState>();
	private IEnumerable<SuperconState> ProcessPreviousStateForbidlistResolved
		=> this.ProcessPreviousStateForbidlist
			.Select(path => this.GetNodeOrNull<SuperconState>(path))
			.OfType<SuperconState>();

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
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
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
		if (Engine.IsEditorHint())
		{
			this.SetPhysicsProcess(false);
			return;
		}
		if (this.ShouldProcess)
		{
			this._SuperconPhysicsProcess(delta);
		}
	}

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(
				this.Owner != this && this.GetParentOrNull<SuperconState>() == null
					? [$"{this.GetType().Name} must be a direct child of a {nameof(SuperconState)} node."]
					: []
			)
			.ToArray();

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.ProcessPreviousStateAllowlist):
			case nameof(this.ProcessPreviousStateForbidlist):
				property["hint"] = (long) PropertyHint.ArrayType;
				property["hint_string"] = $"{Variant.Type.NodePath:D}/{PropertyHint.NodePathValidTypes:D}:{nameof(SuperconState)}";
				break;
		}
	}

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
		if (transition.IsCanceled)
		{
			return;
		}
		this.Started = false;
		this._SuperconEnter(transition);
		if (Mathf.IsZeroApprox(this.ProcessStartDelay) && this.Enabled)
		{
			this.Start();
		}
	}

	private void OnStateExited(SuperconStateMachine.Transition transition)
	{
		if (transition.IsCanceled)
		{
			return;
		}
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

	protected async void ConnectStateTransition(string signalName)
	{
		SuperconState selectedState;
		try {
			selectedState = await GeneralUtil.RequestSelectNode<SuperconState>();
		} catch (TaskCanceledException) { return; } // User cancelled the operation. Nothing to do
		Callable callable = new Callable(selectedState, SuperconState.MethodName.QueueTransition);
		this.Connect(signalName, callable, (uint) ConnectFlags.Persist);
		EditorInterface.Singleton.EditNode(this);
	}

	private bool TestAllowlist()
		=> this.StateMachineOwner?.StateMachine.PreviousActiveState == null
			|| this.ProcessPreviousStateAllowlist.Length == 0
			|| this.ProcessPreviousStateAllowlistResolved.Contains(this.StateMachineOwner.StateMachine.PreviousActiveState);

	private bool TestForbidlist()
		=> this.StateMachineOwner?.StateMachine.PreviousActiveState == null
			|| !this.ProcessPreviousStateForbidlistResolved.Contains(this.StateMachineOwner.StateMachine.PreviousActiveState);
}

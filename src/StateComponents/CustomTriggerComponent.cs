using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class CustomTriggerComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node? Context;
	/// <summary>
	/// This value will be available in the expression's context as the 'context' variable.
	/// </summary>
	[Export] public Variant Param = new Variant();
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public bool PhysicsProcess = false;

	[ExportGroup("State Transition")]
	[ExportToolButton("Connect State Transition")] public Callable ToolButtonConnectStateTransition
		=> Callable.From(this.OnToolButtonConnectStateTransitionPressed);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression ExpressionInterpreter = new();

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void TriggeredEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() && this.Context == null)
		{
			this.Context = this.Character;
		}
		this.CompileExpression();
	}

	public override void _SuperconStart()
	{
		base._SuperconStart();
		if (OS.IsDebugBuild())
		{
			this.CompileExpression();
		}
	}

	public override void _SuperconProcess(double delta)
	{
		if (this.PhysicsProcess)
		{
			return;
		}
		if (this.TestExpression())
		{
			this.EmitSignalTriggered();
		}
	}

	public override void _SuperconPhysicsProcess(double delta)
	{
		if (!this.PhysicsProcess)
		{
			return;
		}
		if (this.TestExpression())
		{
			this.EmitSignalTriggered();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void CompileExpression()
	{
		Error error = this.ExpressionInterpreter.Parse(this.Expression, ["context"]);
		if (error != Error.Ok)
		{
			GD.PrintErr($"[{nameof(CustomTriggerComponent)} at \"{this.GetPath()}\"] Failed to parse expression. Error: {this.ExpressionInterpreter.GetErrorText()}");
		}
	}

	private bool TestExpression()
	{
		Variant result;
		try
		{
			result = this.ExpressionInterpreter.Execute([this.Param], this.Context ?? this);
		} catch (Exception e)
		{
			GD.PrintErr($"[{nameof(CustomTriggerComponent)} at \"{this.GetPath()}\"] An exception occured while executing expression. Exception: {e}");
			result = new Variant();
		}
		if (this.ExpressionInterpreter.HasExecuteFailed())
		{
			GD.PrintErr($"[{nameof(CustomTriggerComponent)} at \"{this.GetPath()}\"] Failed to execute expression. Error: {this.ExpressionInterpreter.GetErrorText()}");
			return false;
		} else if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(CustomTriggerComponent)} at \"{this.GetPath()}\"] Failed to test expression. Cause: Expression did not evaluate to a boolean value. Result: {result} ({result.VariantType})");
			return false;
		}
		return result.AsBool();
	}

	private void OnToolButtonConnectStateTransitionPressed() => this.ConnectStateTransition(SignalName.Triggered);
}

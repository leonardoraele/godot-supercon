using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class CustomTransitionComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node? Self;
	/// <summary>
	/// This value will be available in the expression's context as the 'context' variable.
	/// </summary>
	[Export] public Variant ContextVar = new Variant();
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public SuperconState? TransitionOnTrue;

	[ExportGroup("Options")]
	/// <summary>
	/// Minimum duration, in milliseconds, that the condition must be true before the transition is triggered.
	/// </summary>
	[Export] public uint MinDurationMs = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private float ConditionSatisfiedMoment = float.PositiveInfinity;
	private Expression CompiledExpression = new();

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.CompileExpression();
	}

	public override void _SuperconEnter(SuperconStateMachine.Transition transition)
	{
		base._SuperconEnter(transition);
		if (OS.IsDebugBuild())
		{
			this.CompileExpression();
		}
		this.ConditionSatisfiedMoment = float.PositiveInfinity;
	}

	public override void _SuperconProcess(double delta)
	{
		if (this.TransitionOnTrue == null)
		{
			return;
		}
		if (this.TestExpression())
		{
			this.ConditionSatisfiedMoment = Math.Min(this.ConditionSatisfiedMoment, Time.GetTicksMsec());
			if (this.ConditionSatisfiedMoment + this.MinDurationMs <= Time.GetTicksMsec())
			{
				this.StateMachine.QueueTransition(this.TransitionOnTrue);
			}
		}
		else
		{
			this.ConditionSatisfiedMoment = float.PositiveInfinity;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void CompileExpression()
	{
		Error error = this.CompiledExpression.Parse(this.Expression, ["context"]);
		if (error != Error.Ok)
		{
			GD.PrintErr($"[{nameof(CustomTransitionComponent)} at \"{this.GetPath()}\"] Failed to parse expression. Error: {this.CompiledExpression.GetErrorText()}");
		}
	}

	private bool TestExpression()
	{
		Variant result;
		try
		{
			result = this.CompiledExpression.Execute([this.ContextVar], this.Self ?? this);
		} catch (Exception e)
		{
			GD.PrintErr($"[{nameof(CustomTransitionComponent)} at \"{this.GetPath()}\"] An exception occured while executing expression. Exception: {e}");
			result = new Variant();
		}
		if (this.CompiledExpression.HasExecuteFailed())
		{
			GD.PrintErr($"[{nameof(CustomTransitionComponent)} at \"{this.GetPath()}\"] Failed to execute expression. Error: {this.CompiledExpression.GetErrorText()}");
			return false;
		} else if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(CustomTransitionComponent)} at \"{this.GetPath()}\"] Failed to test expression. Cause: Expression did not evaluate to a boolean value. Result: {result} ({result.VariantType})");
			return false;
		}
		return result.AsBool();
	}
}

using System;
using Godot;
using Raele.Supercon2D.StateComponents;

namespace Raele.Supercon2D;

[Tool]
public partial class FacingDirectionComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public FacingDirectionOptions NewFacingDirection
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= FacingDirectionOptions.Neutral;
	[Export] public Node? Self;
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public Variant ContextVar = new Variant();

	[ExportGroup("Trigger Condition", "Trigger")]
	[Export] public TriggerConditionenum TriggerCondition
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= TriggerConditionenum.OnStateEnter;
	[Export] public Node? TriggerSelf;
	[Export(PropertyHint.Expression)] public string TriggerExpression = "";
	[Export] public Variant TriggerContextVar = new Variant();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression ExpressionParser => field ??= new();
	private Expression TriggerExpressionParser => field ??= new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum FacingDirectionOptions {
		Neutral,
		Left,
		Right,
		Invert,
		TowardWall,
		AwayFromWall,
		Expression,
	}

	public enum TriggerConditionenum {
		OnStateEnter,
		OnStateExit,
		EveryFrame,
		IfExpressionIsTrue,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	public override void _Ready()
	{
		base._Ready();
		if (this.NewFacingDirection == FacingDirectionOptions.Expression)
		{
			if (this.ExpressionParser.Parse(this.Expression, ["context"]) != Error.Ok)
			{
				GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Failed to parse expression: {this.Expression}");
			}
		}
		if (this.TriggerCondition == TriggerConditionenum.IfExpressionIsTrue)
		{
			if (this.TriggerExpressionParser.Parse(this.TriggerExpression, ["context"]) != Error.Ok)
			{
				GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Failed to parse trigger expression: {this.TriggerExpression}");
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
		if (
			this.TriggerCondition == TriggerConditionenum.EveryFrame
			|| this.TriggerCondition == TriggerConditionenum.IfExpressionIsTrue && this.EvaluateTriggerExpression()
		)
		{
			this.ApplyFacingDirection();
		}
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? [] : ["Some warning"])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(Expression):
			case nameof(Self):
			case nameof(ContextVar):
				property["usage"] = this.NewFacingDirection == FacingDirectionOptions.Expression
					? (long) PropertyUsageFlags.Default | (int) PropertyUsageFlags.NilIsVariant
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(TriggerExpression):
			case nameof(TriggerSelf):
			case nameof(TriggerContextVar):
				property["usage"] = this.TriggerCondition == TriggerConditionenum.IfExpressionIsTrue
					? (long) PropertyUsageFlags.Default | (int) PropertyUsageFlags.NilIsVariant
					: (long) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	public override void _SuperconEnter(SuperconStateMachine.Transition transition)
	{
		base._SuperconEnter(transition);
		if (this.TriggerCondition == TriggerConditionenum.OnStateEnter)
		{
			this.ApplyFacingDirection();
		}
	}

	public override void _SuperconExit(SuperconStateMachine.Transition transition)
	{
		base._SuperconExit(transition);
		if (this.TriggerCondition == TriggerConditionenum.OnStateExit)
		{
			this.ApplyFacingDirection();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public bool EvaluateTriggerExpression()
	{
		Variant result = new Variant();
		try
		{
			result = this.TriggerExpressionParser.Execute([this.TriggerContextVar], this.TriggerSelf);
		}
		catch (Exception e)
		{
			GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Failed to execute trigger expression: {this.TriggerExpression}. Exception: {e}");
			return false;
		}
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Trigger expression did not return a boolean value. Return: {result} ({result.VariantType})");
			return false;
		}
		return result.AsBool();
	}

	private void ApplyFacingDirection()
	{
		int PreviousFacingDirection = this.Character.FacingDirection;
		this.Character.FacingDirection = this.NewFacingDirection switch
		{
			FacingDirectionOptions.Neutral => 0,
			FacingDirectionOptions.Left => -1,
			FacingDirectionOptions.Right => 1,
			FacingDirectionOptions.Invert => this.Character.FacingDirection * -1,
			FacingDirectionOptions.TowardWall => this.Character.IsOnWall() ? Math.Sign(this.Character.GetWallNormal().X) * -1 : this.Character.FacingDirection,
			FacingDirectionOptions.AwayFromWall => this.Character.IsOnWall() ? Math.Sign(this.Character.GetWallNormal().X) : this.Character.FacingDirection,
			FacingDirectionOptions.Expression => this.EvaluateFacingExpression(@default: this.Character.FacingDirection),
			_ => this.Character.FacingDirection,
		};
		// GD.PrintS(new { State = this.State.Name, IsOnWall = this.Character.IsOnWall(), GetWallNormal = this.Character.GetWallNormal(), PreviousFacingDirection, this.Character.FacingDirection });
	}

	private int EvaluateFacingExpression(int @default)
	{
		Variant result = new Variant();
		try
		{
			result = this.TriggerExpressionParser.Execute([this.TriggerContextVar], this.TriggerSelf);
		}
		catch (Exception e)
		{
			GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Failed to execute trigger expression: {this.TriggerExpression}. Exception: {e}");
			return @default;
		}
		if (result.VariantType != Variant.Type.Int)
		{
			GD.PushError($"[{nameof(FacingDirectionComponent)} at {this.GetPath()}] Trigger expression did not return a boolean value. Return: {result} ({result.VariantType})");
			return @default;
		}
		return result.AsInt32();
	}
}

using System;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class AnimatedSprite2DComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	public enum FlipHEnum
	{
		Never,
		Always,
		IfFacingLeft,
		IfFacingRight,
	}

	public enum PlayWhenEnum
	{
		StateEnter,
		StateExit,
		ExpressionIsTrue,
		StateEnterIfExpressionIsTrue,
		StateExitIfExpressionIsTrue,
	}

	public enum SpeedScaleModeEnum
	{
		Unchanged,
		Constant,
		ExpressionOnce,
		ExpressionEveryFrame,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export(PropertyHint.Enum)] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.IfFacingLeft;
	[Export] public SuperconState? TransitionOnAnimationEnd;

	[ExportGroup("Timing", "Timing")]
	[Export] public PlayWhenEnum TimingPlayAnimationWhen
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= PlayWhenEnum.StateEnter;
	[Export] public Node? TimingSelf;
	[Export(PropertyHint.Expression)] public string TimingExpression = "";
	[Export] public Variant TimingContextVar = new Variant();

	[ExportGroup("Speed Scale", "SpeedScale")]
	[Export] public SpeedScaleModeEnum SpeedScaleMode
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= SpeedScaleModeEnum.Unchanged;
	[Export(PropertyHint.Range, "0.25,4,0.05,or_greater,or_less")] public float SpeedScaleValue = 1f;
	[Export] public Node? SpeedScaleSelf;
	[Export(PropertyHint.Expression)] public string SpeedScaleExpression = "";
	/// <summary>
	/// This value will be available in the expression's context as the 'context' variable.
	/// </summary>
	[Export] public Variant SpeedScaleContextVar = new Variant();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression? ExpressionParser;
	private Expression? SpeedScaleExpressionParser;
	private float OriginalSpeedScale = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private bool ShouldFlipH => this.FlipH switch
	{
		FlipHEnum.Never => false,
		FlipHEnum.Always => true,
		FlipHEnum.IfFacingLeft => this.Character.FacingDirection < 0,
		FlipHEnum.IfFacingRight => this.Character.FacingDirection > 0,
		_ => false,
	};


	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum Type {
	// 	Value1,
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
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
		if (Engine.IsEditorHint() && this.AnimatedSprite == null)
		{
			this.AnimatedSprite = this.Character.GetChildren().OfType<AnimatedSprite2D>().FirstOrDefault();
		}
		if (this.TimingPlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			this.ExpressionParser = new();
			if (this.ExpressionParser?.Parse(this.TimingExpression, ["context"]) != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Failed to parse expression: \"{this.TimingExpression}\"");
			}
		}
		if (
			this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionOnce
			|| this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionEveryFrame
		)
		{
			this.SpeedScaleExpressionParser = new();
			if (this.SpeedScaleExpressionParser?.Parse(this.SpeedScaleExpression, ["context"]) != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Failed to parse speed scale expression: \"{this.SpeedScaleExpression}\"");
			}
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
	// 		.Concat(true ? [] : ["Some warning"])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Animation):
				property["hint_string"] = this.AnimatedSprite?.SpriteFrames?.GetAnimationNames().Join(",") ?? "";
				break;
			case nameof(this.TimingExpression):
			case nameof(this.TimingSelf):
			case nameof(this.TimingContextVar):
				property["usage"] = this.TimingPlayAnimationWhen switch
				{
					PlayWhenEnum.ExpressionIsTrue
						or PlayWhenEnum.StateEnterIfExpressionIsTrue
						or PlayWhenEnum.StateExitIfExpressionIsTrue
						=> (int) PropertyUsageFlags.Default | (int) PropertyUsageFlags.NilIsVariant,
					_ => (int) PropertyUsageFlags.NoEditor,
				};
				break;
			case nameof(this.SpeedScaleValue):
				property["usage"] = this.SpeedScaleMode == SpeedScaleModeEnum.Constant
					? (int) PropertyUsageFlags.Default
					: (int) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.SpeedScaleSelf):
			case nameof(this.SpeedScaleExpression):
			case nameof(this.SpeedScaleContextVar):
				property["usage"] = this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionOnce
					|| this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionEveryFrame
						? (int) PropertyUsageFlags.Default | (int) PropertyUsageFlags.NilIsVariant
						: (int) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconEnter()
	{
		base._SuperconEnter();
		this.OriginalSpeedScale = this.AnimatedSprite?.SpeedScale ?? 1f;
		if (
			this.TimingPlayAnimationWhen == PlayWhenEnum.StateEnter
			|| this.TimingPlayAnimationWhen == PlayWhenEnum.StateEnterIfExpressionIsTrue
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
	}

	public override void _SuperconExit()
	{
		this.AnimatedSprite?.SpeedScale = this.OriginalSpeedScale;
		if (
			this.TimingPlayAnimationWhen == PlayWhenEnum.StateExit
			|| this.TimingPlayAnimationWhen == PlayWhenEnum.StateExitIfExpressionIsTrue
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
		base._SuperconExit();
	}

	public override void _SuperconProcess(double delta)
	{
		base._SuperconProcess(delta);
		if (
			this.AnimatedSprite?.IsPlaying() == true
			&& this.AnimatedSprite?.Animation == this.Animation
		)
		{
			this.AnimatedSprite?.FlipH = this.ShouldFlipH;
			if (this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionEveryFrame)
			{
				this.AnimatedSprite?.SpeedScale = this.EvaluateSpeedScaleExpression();
			}
		}
		else if (this.TimingPlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue && this.EvaluateUserExpression())
		{
			this.Play();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void Play()
	{
		if (this.SpeedScaleMode == SpeedScaleModeEnum.Constant)
		{
			this.AnimatedSprite?.SpeedScale = this.SpeedScaleValue;
		}
		else if (this.SpeedScaleMode == SpeedScaleModeEnum.ExpressionOnce)
		{
			this.AnimatedSprite?.SpeedScale = this.EvaluateSpeedScaleExpression();
		}
		this.AnimatedSprite?.Play(this.Animation);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;
	}

	private float GetSpeedScale()
	{
		return this.SpeedScaleMode switch
		{
			SpeedScaleModeEnum.Constant => this.SpeedScaleValue,
			SpeedScaleModeEnum.ExpressionOnce => this.EvaluateSpeedScaleExpression(),
			_ => this.AnimatedSprite?.SpeedScale ?? 1f,
		};
	}

	private float EvaluateSpeedScaleExpression()
	{
		Variant result;
		try {
			result = this.SpeedScaleExpressionParser?.Execute([this.SpeedScaleContextVar], this.SpeedScaleSelf) ?? new Variant();
		} catch (Exception e) {
			GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Failed to evaluate speed scale expression: \"{this.SpeedScaleExpression}\"", e);
			return 1f;
		}
		if (result.VariantType != Variant.Type.Float && result.VariantType != Variant.Type.Int)
		{
			GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Speed scale expression did not evaluate to a number. Expression \"{this.SpeedScaleExpression}\" returned {result} ({result.VariantType})");
			return 1f;
		}
		return result.AsSingle();
	}

	private bool EvaluateUserExpression()
	{
		Variant result;
		try {
			result = this.ExpressionParser?.Execute([this.TimingContextVar], this.TimingSelf) ?? Variant.From(false);
		} catch (Exception e) {
			GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Failed to evaluate condition: \"{this.TimingExpression}\"", e);
			return false;
		}
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(AnimatedSprite2DComponent)}] Condition did not evaluate to a boolean. Expression \"{this.TimingExpression}\" returned {result} ({result.VariantType})");
			return false;
		}
		return result.AsBool();
	}
}

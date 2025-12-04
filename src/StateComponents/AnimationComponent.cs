using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class AnimationComponent : SuperconStateController
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
		Constant,
		BasedOnAbsoluteVelocityX,
		BasedOnAbsoluteVelocityY,
		BasedOnVelocityMagnitude,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export(PropertyHint.Enum)] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.IfFacingLeft;
	[Export] public PlayWhenEnum PlayAnimationWhen
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= PlayWhenEnum.StateEnter;
	[Export] public Node? Self;
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public Variant ContextVar = new Variant();
	[Export] public SuperconState? TransitionOnAnimationEnd;

	[ExportGroup("Speed Scale", "SpeedScale")]
	[Export(PropertyHint.GroupEnable)] public bool SpeedScaleEnabled = false;
	[Export] public SpeedScaleModeEnum SpeedScaleMode
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= SpeedScaleModeEnum.Constant;
	[Export(PropertyHint.Range, "0.25,4,0.05,or_greater,or_less")] public float SpeedScaleFixedValue = 1f;
	[Export] public Curve? SpeedScaleValueByVelocity;
	[Export] public bool SpeedScaleResetOnAnimationEnd = true;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression ExpressionParser = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private bool ShouldFlipH => this.FlipH switch
	{
		FlipHEnum.Never => false,
		FlipHEnum.Always => true,
		FlipHEnum.IfFacingLeft => this.Character.FacingDirection < 0,
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
		if (this.PlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			if (this.ExpressionParser.Parse(this.Expression, ["context"]) != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)}] Failed to parse expression: \"{this.Expression}\"");
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
			case nameof(this.Expression):
			case nameof(this.Self):
			case nameof(this.ContextVar):
				property["usage"] = this.PlayAnimationWhen switch
				{
					PlayWhenEnum.ExpressionIsTrue
						or PlayWhenEnum.StateEnterIfExpressionIsTrue
						or PlayWhenEnum.StateExitIfExpressionIsTrue
						=> (int) PropertyUsageFlags.Default,
					_ => (int) PropertyUsageFlags.NoEditor,
				};
				break;
			case nameof(this.SpeedScaleFixedValue):
				property["usage"] = this.SpeedScaleEnabled && this.SpeedScaleMode == SpeedScaleModeEnum.Constant
					? (int) PropertyUsageFlags.Default
					: (int) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.SpeedScaleValueByVelocity):
				property["usage"] = this.SpeedScaleEnabled && this.SpeedScaleMode != SpeedScaleModeEnum.Constant
					? (int) PropertyUsageFlags.Default
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
		if (
			this.PlayAnimationWhen == PlayWhenEnum.StateEnter
			|| this.PlayAnimationWhen == PlayWhenEnum.StateEnterIfExpressionIsTrue
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
	}

	public override void _SuperconExit()
	{
		if (
			this.PlayAnimationWhen == PlayWhenEnum.StateExit
			|| this.PlayAnimationWhen == PlayWhenEnum.StateExitIfExpressionIsTrue
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
			this.PlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue
			&& this.AnimatedSprite?.Animation != this.Animation
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
		if (this.FlipH == FlipHEnum.IfFacingLeft && this.AnimatedSprite?.Animation == this.Animation)
		{
			this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		}
		if (this.SpeedScaleEnabled && this.AnimatedSprite?.Animation == this.Animation)
		{
			this.AnimatedSprite.SpeedScale = this.SpeedScaleMode switch
			{
				SpeedScaleModeEnum.BasedOnAbsoluteVelocityX => this.SpeedScaleValueByVelocity?.SampleBaked(Math.Abs(this.Character.Velocity.X)) ?? 1f,
				SpeedScaleModeEnum.BasedOnAbsoluteVelocityY => this.SpeedScaleValueByVelocity?.SampleBaked(Math.Abs(this.Character.Velocity.Y)) ?? 1f,
				SpeedScaleModeEnum.BasedOnVelocityMagnitude => this.SpeedScaleValueByVelocity?.SampleBaked(this.Character.Velocity.Length()) ?? 1f,
				_ => 1f,
			};
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void Play()
	{
		this.AnimatedSprite?.Play(this.Animation);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;

		if (this.SpeedScaleEnabled && this.SpeedScaleMode == SpeedScaleModeEnum.Constant)
		{
			this.AnimatedSprite?.SpeedScale = this.SpeedScaleFixedValue;
		}
		if (this.SpeedScaleEnabled && this.SpeedScaleResetOnAnimationEnd)
		{
			float originalSpeedScale = this.AnimatedSprite?.SpeedScale ?? 1f;
			this.WaitAnimationFinishedOrChanged().ContinueWith(task =>
			{
				if (task.Result == 'C')
				{
					return;
				}
				this.AnimatedSprite?.SpeedScale = originalSpeedScale;
			});
		}
		if (this.TransitionOnAnimationEnd != null)
		{
			this.WaitAnimationFinishedOrChanged().ContinueWith(task =>
			{
				if (task.Result == 'C')
				{
					return;
				}
				this.StateMachine.QueueTransition(this.TransitionOnAnimationEnd);
			});
		}
	}

	private Task<char> WaitAnimationFinishedOrChanged() {
		TaskCompletionSource<char> source = new();
		Callable finished = Callable.From(() => source.SetResult('F'));
		Callable changed = Callable.From(() => source.SetResult('C'));
		this.AnimatedSprite?.Connect(AnimatedSprite2D.SignalName.AnimationFinished, finished);
		this.AnimatedSprite?.Connect(AnimatedSprite2D.SignalName.AnimationChanged, changed);
		source.Task.ContinueWith(_ => Callable.From(() =>
		{
			this.AnimatedSprite?.Disconnect(AnimatedSprite2D.SignalName.AnimationFinished, finished);
			this.AnimatedSprite?.Disconnect(AnimatedSprite2D.SignalName.AnimationChanged, changed);
		}).CallDeferred());
		return source.Task;
	}

	private bool EvaluateUserExpression()
	{
		try {
			Variant result = this.ExpressionParser.Execute([this.ContextVar], this.Self);
			if (result.VariantType != Variant.Type.Bool)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)}] Condition did not evaluate to a boolean: \"{this.Expression}\"");
				return false;
			}
			return result.AsBool();
		} catch (Exception e) {
			GD.PrintErr($"[{nameof(AnimationComponent)}] Failed to evaluate condition: \"{this.Expression}\"", e);
			return false;
		}
	}
}

using System;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class AnimationComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimationPlayer? AnimationPlayer;
	[Export(PropertyHint.Enum)] public string Animation = "";

	[ExportGroup("Playback Options")]
	[Export(PropertyHint.Range, "0.05,8,or_greater,or_less")] public float SpeedScale = 1f;
	[Export] public bool PlayBackwards = false;
	[Export] public float BeginSeekSec = 0f;

	[ExportGroup("Blending")]
	[Export(PropertyHint.GroupEnable)] public bool BlendEnabled;
	[Export] public float BlendTimeMs = 200f;

	[ExportGroup("Timing", "Timing")]
	[Export] public PlayWhenEnum TimingPlayWhen = PlayWhenEnum.StateEnter;
	[Export(PropertyHint.Expression)] public string TimingExpression = "";
	[ExportSubgroup("Expression Options")]
	[Export] public Node? TimingSelf = null;
	[Export] public Variant TimingContextVar = new Variant();
	[Export] public float TimingMinDurationMs = 0f;

	[ExportGroup("State Transition", "Transition")]
	[Export] public SuperconState? TransitionOnAnimationEnd = null;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression? TimingExpressionParser;
	private float TimingDurationAccumulatedTimeMs = 0f;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private int PlayBackwardsInt => this.PlayBackwards ? -1 : 1;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum PlayWhenEnum :  byte
	{
		StateEnter = 1,
		StateExit = 2,
		ExpressionIsTrue = 3,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT OVERRIDES
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
		if (Engine.IsEditorHint() && this.AnimationPlayer == null)
		{
			this.AnimationPlayer = this.Character.GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
		}
		if (this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			this.TimingExpressionParser = new();
			if (this.TimingExpressionParser.Parse(this.TimingExpression, ["context"]) is Error error && error != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Error parsing expression. Error: {error}");
				this.TimingExpressionParser = null;
			}
		}
		this.AnimationPlayer?.AnimationFinished += _ => this.OnAnimationfinished();
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
				property["hint_string"] = this.AnimationPlayer?.GetAnimationList().Join(",") ?? "";
				break;
			case nameof(this.TimingPlayWhen):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.TimingSelf):
			case nameof(this.TimingExpression):
			case nameof(this.TimingMinDurationMs):
				property["usage"] = this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.TimingContextVar):
				property["usage"] = this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue
					? (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.NilIsVariant
					: (long) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OTHER OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconEnter()
	{
		base._SuperconEnter();
		if (this.TimingPlayWhen == PlayWhenEnum.StateEnter)
		{
			this.Activate();
		}
	}

	public override void _SuperconExit()
	{
		base._SuperconExit();
		if (this.TimingPlayWhen == PlayWhenEnum.StateExit)
		{
			this.Activate();
		}
	}

	public override void _SuperconProcess(double delta)
	{
		base._SuperconProcess(delta);
		if (Engine.IsEditorHint() || this.TimingPlayWhen != PlayWhenEnum.ExpressionIsTrue)
		{
			this.SetProcess(false);
		}
		if (this.TestTimingExpression((float) delta))
		{
			this.Activate();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Activate()
	{
		if (this.AnimationPlayer?.IsPlaying() == true && this.AnimationPlayer?.CurrentAnimation == this.Animation)
		{
			return;
		}
		this.AnimationPlayer?.Play("RESET");
		this.AnimationPlayer?.Advance(0f); // Force reset immediately
		this.AnimationPlayer?.Play(
			this.Animation,
			this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : default,
			this.SpeedScale * this.PlayBackwardsInt,
			this.PlayBackwards
		);
		if (!Mathf.IsZeroApprox(this.BeginSeekSec))
		{
			this.AnimationPlayer?.Seek(this.BeginSeekSec, update: true);
		}
	}

	private bool TestTimingExpression(float delta)
	{
		if (this.TimingExpressionParser == null)
		{
			return false;
		}
		Variant result;
		try
		{
			result = this.TimingExpressionParser.Execute([this.TimingContextVar], this.TimingSelf);
		}
		catch (Exception e)
		{
			GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Exception while evaluating timing expression. Exception: {e}");
			return false;
		}
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Timing expression did not evaluate to a boolean. Returned value: {result} ({result.VariantType})");
			return false;
		}
		if (!result.AsBool())
		{
			this.TimingDurationAccumulatedTimeMs = 0f;
			return false;
		}
		this.TimingDurationAccumulatedTimeMs += delta * 1000;
		return this.TimingDurationAccumulatedTimeMs >= this.TimingMinDurationMs;
	}

	private void OnAnimationfinished()
	{
		if (
			!this.State.IsActive
			|| this.AnimationPlayer?.CurrentAnimation != this.Animation
			|| this.TransitionOnAnimationEnd == null
		)
		{
			return;
		}
		this.StateMachine?.QueueTransition(this.TransitionOnAnimationEnd);
	}
}

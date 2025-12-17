using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.StateMachine;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class PlayAnimationComponent : SuperconStateComponent
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
	[Export] public ResetStrategyEnum ResetStrategy = ResetStrategyEnum.BeforePlay;

	[ExportGroup("Playback Options")]
	[Export(PropertyHint.Range, "0.05,8,or_greater,or_less")] public float SpeedScale = 1f;
	[Export] public bool PlayBackwards = false;

	[ExportSubgroup("Sectioning", "Sectioning")]
	[Export(PropertyHint.GroupEnable)] public bool SectioningEnabled = false;
	[Export] public bool SectioningUseMarkers = false;
	[Export(PropertyHint.None, "suffix:s")] public float SectioningStartTimeSec = 0f;
	[Export(PropertyHint.None, "suffix:s")] public float SectioningEndTimeSec = 0f;
	[Export(PropertyHint.Enum)] public string SectioningStartMarker = "";
	[Export(PropertyHint.Enum)] public string SectioningEndMarker = "";

	[ExportGroup("Queueing")]
	[Export(PropertyHint.ArrayType)] public string[]? QueueAnimations;

	[ExportGroup("Blending")]
	[Export(PropertyHint.GroupEnable)] public bool BlendEnabled;
	[Export(PropertyHint.None, "suffix:ms")] public float BlendTimeMs = 200f;

	[ExportGroup("Timing", "Timing")]
	[Export] public TimingStrategyEnum TimingStrategy = TimingStrategyEnum.OnStateEnter;
	[Export] public Node? TimingContext = null;
	[Export] public Variant TimingParamVar = new Variant();
	[Export(PropertyHint.Expression)] public string TimingExpression = "";

	[ExportGroup("State Transition", "Transition")]
	[Export] public SuperconState? TransitionOnAnimationFinished = null;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression? TimingExpressionParser;
	/// <summary>
	/// Because AnimationPlayer has no mechanism to track the progress of a specific played animation, we use this flag
	/// to know whether the currently playing animation is the one started by this component. We set it true when we
	/// play the animation, and we set it false when the AnimationPlayer changes playing animation.
	/// </summary>
	private bool AnimationActive = false;
	/// <summary>
	/// This is a support variable for <see cref="AnimationActive"/>. It tracks the queue of animations that were
	/// started by this component, so that when the AnimationPlayer changes animation we can know whether it's still
	/// playing one of ours or not.
	///
	/// This pair of variables is a workaround for the lack of a proper callback or signal when another animation starts
	/// playing over ours since AnimationFinished is not emitted when another animation plays over an already playing
	/// animation.
	/// </summary>
	private List<string> ActiveAnimationQueue = new();

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private int PlayBackwardsInt => this.PlayBackwards ? -1 : 1;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum TimingStrategyEnum :  byte
	{
		OnStateEnter = 0,
		WhenPlayerIsIdle = 1,
		WhenExpressionIsTrue = 2,
	}

	public enum ResetStrategyEnum : byte
	{
		Never = 0,
		BeforePlay = 1,
		AfterFinished = 2,
		Both = 3,
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
			this.AnimationPlayer ??= this.Character.GetChildren().OfType<AnimationPlayer>().FirstOrDefault()
				?? this.State.GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
		}
		if (this.TimingStrategy == TimingStrategyEnum.WhenExpressionIsTrue)
		{
			this.TimingExpressionParser = new();
			if (this.TimingExpressionParser.Parse(this.TimingExpression, ["param"]) is Error error && error != Error.Ok)
			{
				GD.PrintErr($"[{nameof(PlayAnimationComponent)} at {this.GetPath()}] Error parsing expression. Error: {error}");
				this.TimingExpressionParser = null;
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
			case nameof(this.AnimationPlayer):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.Animation): {
				string[] options = this.AnimationPlayer?.GetAnimationList() ?? [];
				property["hint_string"] = options.Join(",");
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				if (!options.Contains(this.Animation))
				{
					this.Animation = "";
				}
				break;
			} case nameof(this.QueueAnimations): {
				string[] options = this.AnimationPlayer?.GetAnimationList() ?? [];
				string optionsStr = options.Join(",");
				property["hint_string"] = $"String/{PropertyHint.Enum:D}:{optionsStr}";
				this.QueueAnimations = this.QueueAnimations?.Where(a => options.Contains(a)).ToArray() ?? [];
				return;
			}
			case nameof(this.SectioningUseMarkers):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.SectioningStartTimeSec):
			case nameof(this.SectioningEndTimeSec):
				property["usage"] = !this.SectioningUseMarkers
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.SectioningStartMarker):
			case nameof(this.SectioningEndMarker):
				string[] markerNames = this.AnimationPlayer?.HasAnimation(this.Animation) == true
					? this.AnimationPlayer.GetAnimation(this.Animation).GetMarkerNames()
					: [];
				property["hint"] = markerNames.Length > 0 ? (long) PropertyHint.Enum : (long) PropertyHint.None;
				property["hint_string"] = markerNames.Join(",");
				property["usage"] = this.SectioningUseMarkers
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				if (
					markerNames.Length > 0
					&& !string.IsNullOrEmpty(this.SectioningStartMarker)
					&& !markerNames.Contains(this.SectioningStartMarker)
				)
				{
					this.SectioningStartMarker = "";
				}
				else if (
					markerNames.Length > 0
					&& !string.IsNullOrEmpty(this.SectioningEndMarker)
					&& !markerNames.Contains(this.SectioningEndMarker)
				)
				{
					this.SectioningEndMarker = "";
				}
				break;
			case nameof(this.TimingStrategy):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.TimingContext):
			case nameof(this.TimingExpression):
				property["usage"] = this.TimingStrategy == TimingStrategyEnum.WhenExpressionIsTrue
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.TimingParamVar):
				property["usage"] = this.TimingStrategy == TimingStrategyEnum.WhenExpressionIsTrue
					? (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.NilIsVariant
					: (long) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OTHER OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconEnter(SuperconStateMachine.Transition transition)
	{
		base._SuperconEnter(transition);
		this.AnimationActive = false;
		this.AnimationPlayer?.Connect(AnimationPlayer.SignalName.CurrentAnimationChanged, new Callable(this, MethodName.OnCurrentAnimationChanged));
		if (this.TimingStrategy == TimingStrategyEnum.OnStateEnter)
		{
			this.Play();
		}
	}

	public override void _SuperconExit(StateMachine<SuperconState>.Transition transition)
	{
		base._SuperconExit(transition);
		this.ActiveAnimationQueue.Clear();
		this.AnimationPlayer?.Disconnect(AnimationPlayer.SignalName.CurrentAnimationChanged, new Callable(this, MethodName.OnCurrentAnimationChanged));
	}

	public override void _SuperconProcess(double delta)
	{
		base._SuperconProcess(delta);
		if (
			this.TimingStrategy == TimingStrategyEnum.WhenExpressionIsTrue && this.TestTimingExpression((float) delta)
			|| this.TimingStrategy == TimingStrategyEnum.WhenPlayerIsIdle && this.AnimationPlayer?.IsPlaying() == false
		)
		{
			this.Play();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Play()
	{
		if (this.ResetStrategy == ResetStrategyEnum.BeforePlay || this.ResetStrategy == ResetStrategyEnum.Both)
		{
			// Note: Playing the animation doesn't cause the AnimationPlayer to fire signals immediately — it emits only
			// a single signal during idle frame. This means CurrentAnimationChanged and AnimationStarted will never be
			// emitted for this RESET animation, since we immediately play another animation.
			this.AnimationPlayer?.Play("RESET");
			this.AnimationPlayer?.Advance(0f); // Force reset immediately
		}

		if (this.SectioningEnabled)
		{
			if (this.SectioningUseMarkers)
			{
				this.AnimationPlayer?.PlaySectionWithMarkers(
					this.Animation,
					startMarker: this.SectioningStartMarker,
					endMarker: this.SectioningEndMarker,
					customBlend: this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : default,
					customSpeed: this.SpeedScale * this.PlayBackwardsInt,
					fromEnd: this.PlayBackwards
				);
			}
			else
			{
				this.AnimationPlayer?.PlaySection(
					this.Animation,
					startTime: this.SectioningStartTimeSec,
					endTime: this.SectioningEndTimeSec,
					customBlend: this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : default,
					customSpeed: this.SpeedScale * this.PlayBackwardsInt,
					fromEnd: this.PlayBackwards
				);
			}
		}
		else
		{
			this.AnimationPlayer?.Play(
				this.Animation,
				customBlend: this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : default,
				customSpeed: this.SpeedScale * this.PlayBackwardsInt,
				fromEnd: this.PlayBackwards
			);
		}

		foreach (string animation in this.QueueAnimations ?? [])
		{
			this.AnimationPlayer?.Queue(animation);
		}

		this.AnimationActive = true;
		this.ActiveAnimationQueue.Clear();
		this.ActiveAnimationQueue.AddRange([this.Animation, ..this.QueueAnimations ?? []]);
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
			result = this.TimingExpressionParser.Execute([this.TimingParamVar], this.TimingContext);
		}
		catch (Exception e)
		{
			GD.PrintErr($"[{nameof(PlayAnimationComponent)} at {this.GetPath()}] Exception while evaluating timing expression. Exception: {e}");
			return false;
		}
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(PlayAnimationComponent)} at {this.GetPath()}] Timing expression did not evaluate to a boolean. Returned value: {result} ({result.VariantType})");
			return false;
		}
		return result.AsBool();
	}

	private void OnCurrentAnimationChanged(string animationName)
	{
		if (!this.AnimationActive)
		{
			return;
		}
		if (this.ActiveAnimationQueue.Count() > 0)
		{
			if (this.ActiveAnimationQueue[0] == animationName)
			{
				this.ActiveAnimationQueue.RemoveAt(0);
				return;
			}
			else
			{
				this.AnimationActive = false;
				this.ActiveAnimationQueue.Clear();
				return;
			}
		}
		this.AnimationActive = false;
		this.ActiveAnimationQueue.Clear();
		if (this.ResetStrategy == ResetStrategyEnum.AfterFinished || this.ResetStrategy == ResetStrategyEnum.Both)
		{
			this.AnimationPlayer?.Play("RESET");
			this.AnimationPlayer?.Advance(0f); // Force reset immediately
			this.AnimationPlayer?.Stop();
		}
		if (this.TransitionOnAnimationFinished != null)
		{
			this.StateMachine.QueueTransition(this.TransitionOnAnimationFinished);
		}
	}
}

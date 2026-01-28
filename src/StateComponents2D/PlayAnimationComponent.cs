using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_play.png")]
public partial class PlayAnimationComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public string Animation = "";

	[ExportGroup("Custom Animation Player")]
	[Export(PropertyHint.GroupEnable)] public bool CustomAnimationPlayerEnabled
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= false;
	[Export] public AnimationPlayer? CustomAnimationPlayer
		{ get; set { field = value; this.UpdateConfigurationWarnings(); }}

	[ExportGroup("Playback Options")]
	[Export(PropertyHint.Range, "0.05,4,0.05,or_greater,or_less,suffix:x")] public float SpeedScale = 1f;
	[Export] public bool PlayBackwards = false;

	[ExportSubgroup("Sectioning", "Sectioning")]
	[Export(PropertyHint.GroupEnable)] public bool SectioningEnabled = false;
	[Export] public bool SectioningUseMarkers = false;
	[Export] public float SectioningStartTimeSec = 0f;
	[Export] public float SectioningEndTimeSec = 0f;
	[Export] public string SectioningStartMarker = "";
	[Export] public string SectioningEndMarker = "";

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

	[ExportGroup("Reset Strategy")]
	[Export] public ResetStrategyEnum ResetStrategy = ResetStrategyEnum.Never;

	[ExportCategory("🔀 Connect State Transitions")]
	[ExportToolButton("On Animation Finished")]
	public Callable ConnectAnimationFinishedToolButton
		=> Callable.From(this.OnConnectAnimationFinishedToolButtonPressed);

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

	public AnimationPlayer? AnimationPlayer => this.CustomAnimationPlayerEnabled
		? this.CustomAnimationPlayer
		: this.StateMachineOwner?.AsNode().GetChildren().OfType<AnimationPlayer>().FirstOrDefault()
			?? this.Character?.GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
	private int PlayBackwardsInt => this.PlayBackwards ? -1 : 1;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void PlayAnimationEventHandler(string animationName, double customBlend, float customSpeed, bool fromEnd);
	[Signal] public delegate void QueueAnimationEventHandler(string animationName);
	[Signal] public delegate void AnimationFinishedEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum TimingStrategyEnum :  sbyte
	{
		OnStateEnter = 1,
		WhenPlayerIsIdle = 2,
		WhenExpressionIsTrue = 3,
	}

	public enum ResetStrategyEnum : sbyte
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
		if (this.TimingStrategy == TimingStrategyEnum.WhenExpressionIsTrue)
		{
			this.CompileTimingExpression();
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

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(
				this.CustomAnimationPlayerEnabled && this.CustomAnimationPlayer == null
					? [$"Mandatory field {nameof(this.CustomAnimationPlayer)} is not assigned."]
					: []
			)
			.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.CustomAnimationPlayerEnabled):
			case nameof(this.CustomAnimationPlayer):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.Animation): {
				if (this.AnimationPlayer != null)
				{
					string[] options = this.AnimationPlayer?.GetAnimationList() ?? [];
					property["hint"] = (long) PropertyHint.Enum;
					property["hint_string"] = options.Join(",");
					property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
					if (!options.Contains(this.Animation))
					{
						this.Animation = "";
					}
				}
				break;
			} case nameof(this.QueueAnimations): {
				if (this.AnimationPlayer != null)
				{
					string[] options = this.AnimationPlayer?.GetAnimationList() ?? [];
					string optionsStr = options.Join(",");
					property["hint_string"] = $"String/{PropertyHint.Enum:D}:{optionsStr}";
					this.QueueAnimations = this.QueueAnimations?.Where(a => options.Contains(a)).ToArray() ?? [];
				}
				else
				{
					property["hint_string"] = $"String/String";
				}
				break;
			}
			case nameof(this.SectioningUseMarkers):
				if (this.AnimationPlayer != null)
				{
					property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				}
				else
				{
					property["usage"] = (long) PropertyUsageFlags.NoEditor;
					this.SectioningUseMarkers = false;
				}
				break;
			case nameof(this.SectioningStartTimeSec):
			case nameof(this.SectioningEndTimeSec):
				float? length = this.AnimationPlayer?.HasAnimation(this.Animation) == true
					? this.AnimationPlayer.GetAnimation(this.Animation).GetLength()
					: null;
				property["hint"] = length.HasValue ? (long) PropertyHint.Range : (long) PropertyHint.None;
				property["hint_string"] = length.HasValue ? FormattableString.Invariant($"0,{length:N},0.01,suffix:s") : "suffix:s";
				property["usage"] = !this.SectioningUseMarkers
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.SectioningStartMarker):
			case nameof(this.SectioningEndMarker):
				property["usage"] = this.SectioningUseMarkers
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				if (this.AnimationPlayer?.HasAnimation(this.Animation) == true)
				{
					string[] markerNames = this.AnimationPlayer.GetAnimation(this.Animation).GetMarkerNames();
					property["hint"] = (long) PropertyHint.Enum;
					property["hint_string"] = markerNames.Join(",");
					if (!markerNames.Contains(this.SectioningStartMarker)) this.SectioningStartMarker = "";
					if (!markerNames.Contains(this.SectioningEndMarker)) this.SectioningEndMarker = "";
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

	public override void _SuperconStart()
	{
		base._SuperconStart();
		this.AnimationActive = false;
		this.AnimationPlayer?.Connect(AnimationPlayer.SignalName.CurrentAnimationChanged, new Callable(this, MethodName.OnCurrentAnimationChanged));
		if (this.TimingStrategy == TimingStrategyEnum.OnStateEnter)
		{
			this.Play();
		}
	}

	public override void _SuperconStop()
	{
		base._SuperconStop();
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

		double customBlend = this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : -1;
		float customSpeed = this.SpeedScale * this.PlayBackwardsInt;

		if (this.SectioningEnabled)
		{
			if (this.SectioningUseMarkers)
			{
				this.AnimationPlayer?.PlaySectionWithMarkers(
					this.Animation,
					this.SectioningStartMarker,
					this.SectioningEndMarker,
					customBlend,
					customSpeed,
					this.PlayBackwards
				);
			}
			else
			{
				this.AnimationPlayer?.PlaySection(
					this.Animation,
					this.SectioningStartTimeSec,
					this.SectioningEndTimeSec,
					customBlend,
					customSpeed,
					this.PlayBackwards
				);
			}
		}
		else
		{
			this.AnimationPlayer?.Play(this.Animation, customBlend, customSpeed, this.PlayBackwards);
		}

		foreach (string animation in this.QueueAnimations ?? [])
		{
			this.AnimationPlayer?.Queue(animation);
		}

		this.EmitSignalPlayAnimation(this.Animation, customBlend, customSpeed, this.PlayBackwards);

		this.AnimationActive = true;
		this.ActiveAnimationQueue.Clear();
		this.ActiveAnimationQueue.AddRange(this.QueueAnimations ?? []);
	}

	private void CompileTimingExpression()
	{
		this.TimingExpressionParser = new();
		if (this.TimingExpressionParser.Parse(this.TimingExpression, ["param"]) is Error error && error != Error.Ok)
		{
			GD.PrintErr($"[{nameof(PlayAnimationComponent)} at {this.GetPath()}] Error parsing expression. Error: {error}");
			this.TimingExpressionParser = null;
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
		if (this.ActiveAnimationQueue.Count() > 0 && this.ActiveAnimationQueue[0] == animationName)
		{
			this.ActiveAnimationQueue.RemoveAt(0);
			return;
		}
		this.AnimationActive = false;
		this.ActiveAnimationQueue.Clear();
		if (this.ResetStrategy == ResetStrategyEnum.AfterFinished || this.ResetStrategy == ResetStrategyEnum.Both)
		{
			this.AnimationPlayer?.Play("RESET");
			this.AnimationPlayer?.Advance(0f); // Force reset immediately
			this.AnimationPlayer?.Stop();
		}
		this.EmitSignalAnimationFinished();
	}

	private void OnConnectAnimationFinishedToolButtonPressed()
		=> this.ConnectStateTransition(SignalName.AnimationFinished);
}

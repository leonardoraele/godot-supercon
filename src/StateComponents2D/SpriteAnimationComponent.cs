using System.Linq;
using Godot;
using Raele.GodotUtils.StateMachine;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_animated_sprite_3.png")]
public partial class SpriteAnimationComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.Never;

	[ExportGroup("Playback Options")]
	[Export] public float SpeedScale = 1f;
	[Export] public bool PlayBackwards = false;
	[Export] public StopOptionsEnum Stop = StopOptionsEnum.Never;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private bool ShouldFlipH => this.FlipH switch
	{
		FlipHEnum.Never => false,
		FlipHEnum.Always => true,
		FlipHEnum.IfFacingLeft => this.Character?.HorizontalFacingDirection < 0,
		FlipHEnum.IfFacingRight => this.Character?.HorizontalFacingDirection > 0,
		_ => false,
	};

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void PlayAnimationEventHandler(string animationName, float speedScale, bool playBackwards, bool flipH);

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum FlipHEnum
	{
		Never,
		Always,
		IfFacingLeft,
		IfFacingRight,
	}

	public enum StopOptionsEnum
	{
		Never,
		OnDisabled,
		OnStateExit,
	}

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
		if (Engine.IsEditorHint())
		{
			this.AnimatedSprite ??= this.StateMachineOwner?.AsNode().GetChildren().OfType<AnimatedSprite2D>().FirstOrDefault();
				this.Character?.GetChildren().OfType<AnimatedSprite2D>().FirstOrDefault();
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
				if (this.AnimatedSprite?.SpriteFrames != null)
				{
					property["hint"] = (int) PropertyHint.Enum;
					property["hint_string"] = this.AnimatedSprite.SpriteFrames.GetAnimationNames().Join(",");
				}
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconStart()
	{
		base._SuperconStart();
		this.Play();
	}

	public override void _SuperconStop()
	{
		base._SuperconStop();
		if (this.Stop == StopOptionsEnum.OnDisabled)
		{
			this.AnimatedSprite?.Stop();
		}
	}

	public override void _SuperconExit(StateMachine<SuperconState>.Transition transition)
	{
		base._SuperconExit(transition);
		if (this.Stop == StopOptionsEnum.OnStateExit)
		{
			this.AnimatedSprite?.Stop();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void Play()
	{
		this.AnimatedSprite?.Play(this.Animation, this.SpeedScale, this.PlayBackwards);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		this.EmitSignalPlayAnimation(this.Animation, this.SpeedScale, this.PlayBackwards, this.ShouldFlipH);
	}
}

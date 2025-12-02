using System.Linq;
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
		ConditionIsTrue,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export(PropertyHint.Enum)] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.IfFacingLeft;
	[Export(PropertyHint.Range, "0.25,4,0.05,or_greater,or_less")] public float AnimationSpeedScale = 1f;
	[Export] public PlayWhenEnum PlayWhen
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= PlayWhenEnum.StateEnter;
	[Export] public string Condition = "";
	[Export] public Node? Self;
	[Export] public Variant ContextVar = new Variant();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression Expression = new();

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
		if (Engine.IsEditorHint())
		{
			if (this.AnimatedSprite == null)
			{
				this.AnimatedSprite = this.Character.GetChildren().OfType<AnimatedSprite2D>().FirstOrDefault();
			}
		}
		else if (this.PlayWhen == PlayWhenEnum.ConditionIsTrue)
		{
			if (this.Expression.Parse(this.Condition, ["context"]) != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)}] Failed to parse expression: \"{this.Condition}\"");
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
			case nameof(this.Condition):
			case nameof(this.Self):
			case nameof(this.ContextVar):
				property["usage"] = this.PlayWhen == PlayWhenEnum.ConditionIsTrue
					? (int) PropertyUsageFlags.Default
					: (int) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		if (this.PlayWhen == PlayWhenEnum.StateEnter)
		{
			this.Play();
		}
	}

	public override void _ExitState()
	{
		if (this.PlayWhen == PlayWhenEnum.StateExit)
		{
			this.Play();
		}
		base._ExitState();
	}

	public override void _ProcessActive(double delta)
	{
		base._ProcessActive(delta);
		if (this.PlayWhen == PlayWhenEnum.ConditionIsTrue)
		{
			if (this.AnimatedSprite?.Animation != this.Animation)
			{
				Variant result = this.Expression.Execute([this.ContextVar], this.Self);
				if (result.VariantType == Variant.Type.Bool && result.AsBool())
				{
					this.Play();
				}
			}
		}
		if (this.FlipH == FlipHEnum.IfFacingLeft)
		{
			this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void Play()
	{
		this.AnimatedSprite?.Play(this.Animation);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		this.AnimatedSprite?.SpeedScale = this.AnimationSpeedScale;
	}
}

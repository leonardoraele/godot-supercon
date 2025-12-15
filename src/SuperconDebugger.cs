using System.Linq;
using Godot;
using Godot.Collections;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconDebugger : CanvasLayer
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public string ToggleVisibilityInputAction = "ui_menu";
	[Export] public PackedScene? DebuggerInterfaceScene;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => this.GetParent<SuperconBody2D>();

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		this.Visible = !Engine.IsEditorHint() && OS.IsDebugBuild();
		this.ProcessMode = ProcessModeEnum.Always;
		this.DebuggerInterfaceScene = ResourceLoader.Load<PackedScene>($"res://addons/{nameof(Supercon2D)}/SuperconDebuggerInterface.tscn");
		Node node = this.DebuggerInterfaceScene.Instantiate();
		node.Set("debugger", this);
		this.AddChild(node);
	}

	// public override void _Draw()
	// {
	// 	float defaultSpeedBarLength = 16;
	// 	float characterSpeed = this.Character.Velocity.Length();
	// 	float characterMaxSpeed = 1000f; // TODO // FIXME this.Character.MovementSettings.MaxHorizontalSpeedPxPSec;
	// 	float totalSpeedBarLength = defaultSpeedBarLength * (characterSpeed / characterMaxSpeed);
	// 	float cappedSpeedBarLength = Math.Min(defaultSpeedBarLength, totalSpeedBarLength);
	// 	this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * defaultSpeedBarLength, Colors.Gray, -1, true);
	// 	this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * totalSpeedBarLength, Colors.Orange, -1, true);
	// 	this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * cappedSpeedBarLength, Colors.Green, -1, true);
	// 	this.DrawString(ThemeDB.FallbackFont, Vector2.Zero, $"{this.Character.Velocity.Length():N2} px/s", default, default, 8, Colors.Green);

	// 	Vector2 inputPos = Vector2.Down * 32;
	// 	this.DrawLine(inputPos, this.Character.InputMapping.MovementInput * defaultSpeedBarLength + inputPos, Colors.Green, -1, true);
	// 	this.DrawCircle(inputPos, 2, Colors.Green);
	// 	this.DrawArc(inputPos, defaultSpeedBarLength, 0f, 2 * (float) Math.PI, 360, Colors.Green);
	// 	this.DrawString(ThemeDB.FallbackFont, inputPos, $"({this.Character.InputMapping.MovementInput.X:N2}, {this.Character.InputMapping.MovementInput.Y:N2}) {this.Character.InputMapping.MovementInput.Length():N2}/1 {this.Character.InputMapping.MovementInput.Angle():N2}º", default, default, 8, Colors.Green);

	// 	if (this.Character.IsOnFloor()) {
	// 		this.DrawLine(Vector2.Zero, this.Character.GetFloorNormal() * 8, Colors.White, 1, true);
	// 	}
	// 	if (this.Character.IsOnWall()) {
	// 		this.DrawLine(this.Character.Velocity.Normalized() * 32, this.Character.Velocity.Normalized() * 32 + this.Character.GetWallNormal() * 8, Colors.White, 1, true);
	// 	}
	// 	this.DrawString(ThemeDB.FallbackFont, Vector2.Down * 8, this.Character.StateMachine.ActiveState?.Name ?? "null state", default, default, 8, Colors.White);
	// }

	public override void _Input(InputEvent @event)
	{
		if (!OS.IsDebugBuild())
		{
			this.SetProcessInput(false);
			return;
		}
		if (@event.IsActionPressed(this.ToggleVisibilityInputAction)) {
			this.Visible = !this.Visible;
			this.GetChildren().ToList().ForEach(child =>
				child.ProcessMode = this.Visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled
			);
			this.GetViewport().SetInputAsHandled();
		}
	}

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.ToggleVisibilityInputAction):
				InputMap.LoadFromProjectSettings();
				property["hint"] = (long) PropertyHint.Enum;
				property["hint_string"] = string.Join(",", InputMap.GetActions());
				break;
			case nameof(this.DebuggerInterfaceScene):
				property["usage"] = (long) PropertyUsageFlags.ReadOnly | (long) PropertyUsageFlags.Editor;
				break;
		}
	}
}

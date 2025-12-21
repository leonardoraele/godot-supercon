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
		if (!Engine.IsEditorHint())
		{
			Node node = this.DebuggerInterfaceScene.Instantiate();
			node.Set("debugger", this);
			this.AddChild(node);
		}
	}

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

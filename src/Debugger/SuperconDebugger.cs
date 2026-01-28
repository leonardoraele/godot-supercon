using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Raele.Supercon2D.Debugger;

[Tool][GlobalClass]
public partial class SuperconDebugger : CanvasLayer
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.InputName)] public string ToggleVisibilityInputAction = "ui_menu";
	[Export] public PackedScene? DebuggerInterfaceScene;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => this.GetParent<SuperconBody2D>();
	public string StateDescription => string.Join("/", this.GetActiveStates());

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		this.Visible = !Engine.IsEditorHint() && OS.IsDebugBuild();
		this.ProcessMode = ProcessModeEnum.Always;
		this.DebuggerInterfaceScene = ResourceLoader.Load<PackedScene>($"res://addons/{nameof(Supercon2D)}/scenes/{nameof(SuperconDebugger)}.tscn");
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
			case nameof(this.DebuggerInterfaceScene):
				property["usage"] = (long) PropertyUsageFlags.ReadOnly | (long) PropertyUsageFlags.Editor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	private IEnumerable<string> GetActiveStates()
	{
		SuperconState? state = this.Character.ActiveState;
		while (state != null)
		{
			yield return state.Name;
			state = state is ISuperconStateMachineOwner owner ? owner.StateMachine.ActiveState : null;
		}
	}
}


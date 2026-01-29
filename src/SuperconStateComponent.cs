using System.Linq;
using System.Threading.Tasks;
using Godot;
using Raele.GodotUtils.ActivitySystem;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_bg.png")]
public abstract partial class SuperconStateComponent : ActivityComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState? ParentState => this.GetAncestorOrDefault<SuperconState>();
	public SuperconStateMachine? StateMachine => this.GetAncestorOrDefault<ISuperconStateMachineOwner>()?.StateMachine;

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override string[] _GetConfigurationWarnings()
		=> (base._GetConfigurationWarnings() ?? [])
			.AppendIf(this.ParentState == null, $"This node should be a descendant of a {nameof(SuperconState)} node.")
			.ToArray();

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	protected async void ConnectStateTransition(string signalName)
	{
		SuperconState selectedState;
		try {
			selectedState = await GeneralUtil.RequestSelectNode<SuperconState>();
		} catch (TaskCanceledException) { return; } // User cancelled the operation. Nothing to do
		Callable callable = new Callable(selectedState, SuperconState.MethodName.QueueTransition);
		this.Connect(signalName, callable, (uint) ConnectFlags.Persist);
		EditorInterface.Singleton.EditNode(this);
	}
}

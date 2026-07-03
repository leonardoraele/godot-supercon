using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

public partial class SuperconStateMachine : Raele.GodotUtils.StateMachine.StateMachine<SuperconState>
{
	public Node? DebugPrintContext = null;

	public SuperconStateMachine()
	{
		this.TransitionCompleted += transition =>
		{
			if (this.DebugPrintContext == null)
				return;
			string fromState = transition.ExitState?.Name.ToString() ?? "<null>";
			string toState = transition.EnterState?.Name.ToString() ?? "<null>";
			this.DebugPrintContext?.DebugLog(
				$"🔀 State changed: {fromState.BBCBold()} → {toState.BBCBold()}",
				[transition]
			);
		};
	}
}

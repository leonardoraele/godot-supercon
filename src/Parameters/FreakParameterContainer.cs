using System.Collections.Generic;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

public class FreakParameterContainer : IReadOnlyFreakParameterContainer
{
	public static FreakParameterContainer Empty => field ??= new FreakParameterContainer();

	public IReadOnlyFreakParameterContainer? Prototype { get; init; }
	private Dictionary<string, Variant> LocalValues = [];

	public IEnumerable<IFreakParameter> GetParameters()
		=> this.Prototype?.GetParameters() ?? [];

	public void SetParameterValue(string name, Variant value)
		=> this.LocalValues[name] = this.GetParameter(name).RunSetter(this, value);

	public void Clear()
		=> this.LocalValues.Clear();

	public bool HasParameter(string name)
		=> this.Prototype?.HasParameter(name) == true || this.LocalValues.ContainsKey(name);

	public Variant GetParameterValue(string name)
		=> this.LocalValues.TryGetValue(name, out Variant value)
			? this.GetParameter(name).RunGetter(this, value)
			: this.Prototype?.GetParameterValue(name)
				?? throw new KeyNotFoundException($"Failed to get attribute '{name}' of attribute container. Cause: Attribute does not exist. Prototype: {this.Prototype?.ToString() ?? "null"}");

	public IFreakParameter GetParameter(string name)
		=> this.Prototype?.AsReadOnlyParameterContainer().GetParameter(name)
			?? throw new KeyNotFoundException($"Failed to get attribute '{name}' of attribute container. Cause: Attribute does not exist. Prototype: {this.Prototype?.ToString() ?? "null"}");

	/// <summary>
	/// Refresh all attributes by assigning their own values back to them. This forces the execution of their getter and
	/// setter expressions, (if any) as well as type validation, ensuring that all attributes are in a valid state.
	/// </summary>
	public void Refresh()
		=> this.AsReadOnlyParameterContainer()
			.GetParameterNames()
			.ForEach(name => this.SetParameterValue(name, this.GetParameterValue(name)));
}

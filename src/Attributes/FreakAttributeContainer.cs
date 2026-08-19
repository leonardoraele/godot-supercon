using System.Collections.Generic;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

public class FreakAttributeContainer : IReadOnlyFreakAttributeContainer
{
	public static FreakAttributeContainer Empty => field ??= new FreakAttributeContainer();

	public IReadOnlyFreakAttributeContainer? Prototype { get; init; }
	private Dictionary<string, Variant> LocalValues = [];

	public IEnumerable<IFreakAttribute> GetAttributeDefinitions()
		=> this.Prototype?.GetAttributeDefinitions() ?? [];

	public void SetAttributeValue(string name, Variant value)
		=> this.LocalValues[name] = this.GetAttributeDefinition(name).RunSetter(this, value);

	public void Clear()
		=> this.LocalValues.Clear();

	public bool HasAttribute(string name)
		=> this.Prototype?.HasAttribute(name) == true || this.LocalValues.ContainsKey(name);

	public Variant GetAttributeValue(string name)
		=> this.LocalValues.TryGetValue(name, out Variant value)
			? this.GetAttributeDefinition(name).RunGetter(this, value)
			: this.Prototype?.GetAttributeValue(name)
				?? throw new KeyNotFoundException($"Failed to get attribute '{name}' of attribute container. Cause: Attribute does not exist. Prototype: {this.Prototype?.ToString() ?? "null"}");

	public IFreakAttribute GetAttributeDefinition(string name)
		=> this.Prototype?.AsReadOnlyAttributeContainer().GetAttributeDefinition(name)
			?? throw new KeyNotFoundException($"Failed to get attribute '{name}' of attribute container. Cause: Attribute does not exist. Prototype: {this.Prototype?.ToString() ?? "null"}");

	/// <summary>
	/// Refresh all attributes by assigning their own values back to them. This forces the execution of their getter and
	/// setter expressions, (if any) as well as type validation, ensuring that all attributes are in a valid state.
	/// </summary>
	public void Refresh()
		=> this.AsReadOnlyAttributeContainer()
			.GetAttributeNames()
			.ForEach(name => this.SetAttributeValue(name, this.GetAttributeValue(name)));
}

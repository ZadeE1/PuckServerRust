using System.Text.Json.Serialization;
using UnityEngine.InputSystem;

public class KeyBind
{
	[JsonIgnore]
	public InputAction InputAction;

	[JsonIgnore]
	public bool IsComposite => InputAction.bindings[0].isComposite;

	public string ModifierPath { get; set; }

	public string Path { get; set; }

	public string Interactions { get; set; }

	[JsonConstructor]
	public KeyBind()
	{
	}

	public KeyBind(InputAction inputAction)
	{
		InputAction = inputAction;
		Update(InputAction);
	}

	public void Update(InputAction inputAction)
	{
		InputAction = inputAction;
		ModifierPath = (IsComposite ? inputAction.bindings[1].effectivePath : null);
		Path = (IsComposite ? inputAction.bindings[2].effectivePath : inputAction.bindings[0].effectivePath);
		Interactions = inputAction.bindings[0].effectiveInteractions;
	}
}

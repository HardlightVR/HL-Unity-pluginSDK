using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
	AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
	//The name of the bool field that will be in control
	public string ConditionalSourceField = "";

	/// <summary>
	/// TRUE = Hide in inspector / FALSE = Disable in inspector 
	/// </summary>
	public bool HideInInspector = false;
	public bool ReverseConditional = false;

	public string ComparedConditionalField = "";

	public ConditionalHideAttribute(string conditionalSourceField)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = false;
	}

	public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = hideInInspector;
	}

	public ConditionalHideAttribute(string conditionalSourceField, string targetValue, bool hideInInspector = true)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.ComparedConditionalField = targetValue;
		this.HideInInspector = hideInInspector;
	}

	public ConditionalHideAttribute(string conditionalSourceField, bool Reverse, string targetValue, bool hideInInspector = true)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.ComparedConditionalField = targetValue;
		this.ReverseConditional = Reverse;
		this.HideInInspector = hideInInspector;
	}
}
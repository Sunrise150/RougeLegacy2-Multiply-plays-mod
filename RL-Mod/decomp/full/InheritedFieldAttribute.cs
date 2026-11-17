using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class InheritedFieldAttribute : Attribute
{
}

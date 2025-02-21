namespace Game.Common.Utilities;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
public class CommandOptionAttribute : Attribute
{
    public required string Name;
    public string? Description;
}
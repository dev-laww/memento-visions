namespace Game.Common.Utilities;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class CommandOptionAttribute : Attribute
{
    public required string Name;
    public required string Description;
}
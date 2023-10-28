namespace ApiRoutes.EFCore;

public class DatabaseBehaviourConfiguration
{
    public Dictionary<Type, Type> Configurations { get; } = new();
}
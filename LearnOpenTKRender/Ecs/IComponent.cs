namespace LearnOpenTKRender.Ecs;

internal interface IComponent
{
    // Marker interface for components in the ECS (Entity-Component-System) architecture.
    // Components are used to define the data and behavior of entities in the system.
}


class ComponentBase: IComponent
{
    public Entity Entity { get;}


    public ComponentBase(Entity entity)
    {
        Entity = entity;
    }
}
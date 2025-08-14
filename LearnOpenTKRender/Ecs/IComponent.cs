namespace LearnOpenTKRender.Ecs;

internal interface IComponent
{
    // Marker interface for components in the ECS (Entity-Component-System) architecture.
    // Components are used to define the data and behavior of entities in the system.
}


abstract class ComponentBase : IComponent
{
   public Entity Entity { get; internal set; } = null!;

    protected ComponentBase()
    {
        // Entity will be set by Entity.AddComponent<T>()
    }
    
    // 可选：组件初始化方法，在添加到Entity后调用
    public virtual void OnAddedToEntity() { }
    
    // 可选：组件移除方法，在从Entity移除前调用
    public virtual void OnRemovedFromEntity() { }
}
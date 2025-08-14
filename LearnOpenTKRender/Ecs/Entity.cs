namespace LearnOpenTKRender.Ecs;

internal class Entity
{
    public Guid Guid { get; private set; }  

    public List<ComponentBase> Components { get; private set; } = new List<ComponentBase>();
    
    public Entity()
    {
        Guid = Guid.NewGuid();
    }

    public T AddComponent<T>() where T : ComponentBase, new()
    {
        if (HasComponent<T>())
        {
            throw new InvalidOperationException($"Entity already has component of type {typeof(T).Name}");
        }

        var component = new T();
        component.Entity = this;
        Components.Add(component);
        component.OnAddedToEntity();
        return component;
    }

    public T? GetComponent<T>() where T : ComponentBase
    {
        return Components.OfType<T>().FirstOrDefault();
    }

    public bool HasComponent<T>() where T : ComponentBase
    {
        return Components.Any(e => e.GetType() == typeof(T));
    }

    public void RemoveComponent<T>() where T : ComponentBase
    {
        var component = GetComponent<T>();
        if (component != null)
        {
            component.OnRemovedFromEntity();
            Components.Remove(component);
        }
    }

    public IEnumerable<T> GetComponents<T>() where T : ComponentBase
    {
        return Components.OfType<T>();
    }
}
namespace LearnOpenTKRender.Ecs;

internal class Entity
{
    public Guid Guid { get; private set; }  
    public Entity()
    {
        Guid = Guid.NewGuid();
    }



}
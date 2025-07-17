using Robust.Shared.Containers;

namespace Content.Shared.Containers;

public sealed partial class SharedContainerUtilitySystem : EntitySystem
{
    /// <summary>
    /// Enumerates over all contents within the entity.
    /// </summary>
    public IEnumerable<EntityUid> GetContents(EntityUid entity)
    {
        if (!TryComp<ContainerManagerComponent>(entity, out var containerManager))
            yield break;

        List<EntityUid> stack = new();

        foreach (var container in containerManager.Containers.Values)
            stack.AddRange(container.ContainedEntities);

        while (stack.Count > 0)
        {
            entity = stack[^1];
            stack.RemoveAt(stack.Count - 1);
            yield return entity;

            if (!TryComp(entity, out containerManager))
                continue;

            foreach (var container in containerManager.Containers.Values)
                stack.AddRange(container.ContainedEntities);
        }
    }

    /// <summary>
    /// Enumerates over the entity and all contents within the entity.
    /// </summary>
    public IEnumerable<EntityUid> GetContentsAndSelf(EntityUid entity)
    {
        List<EntityUid> stack = new() { entity };

        while (stack.Count > 0)
        {
            entity = stack[^1];
            stack.RemoveAt(stack.Count - 1);
            yield return entity;

            if (!TryComp<ContainerManagerComponent>(entity, out var containerManager))
                continue;

            foreach (var container in containerManager.Containers.Values)
                stack.AddRange(container.ContainedEntities);
        }
    }
}
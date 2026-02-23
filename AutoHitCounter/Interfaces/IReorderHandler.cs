// 

namespace AutoHitCounter.Interfaces;

/// <summary>
/// Implemented by ViewModels that support drag-and-drop reordering
/// in a ListBox via <see cref="AutoHitCounter.Behaviors.SplitListDragDropBehavior"/>.
/// </summary>
public interface IReorderHandler
{
    /// <summary>
    /// Moves a dragged item to the specified index.
    /// The implementing ViewModel is responsible for casting
    /// the item to the correct type and performing the reorder.
    /// </summary>
    void MoveItem(object draggedItem, int dropIndex);
}
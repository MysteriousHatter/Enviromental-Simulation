using System;
using UnityEngine;
using static RecyclableItem;

public class CraftingSystem : MonoBehaviour
{
    private const int GRID_SIZE = 3;
    [SerializeField] public DraggableItem[] grid; // 3x3 Crafting Grid

    public event Action OnCraftingGridChanged; // Notify UI to refresh
    public DraggableItem outputItem; // Current output item

    public CraftingSystem()
    {
        grid = new DraggableItem[GRID_SIZE * GRID_SIZE];
    }

    // Helper Functions
    public DraggableItem GetItem(int x, int y) => grid[x + y];
    public void SetItem(DraggableItem item, int x, int y)
    {
        grid[x + y] = item;
        OnCraftingGridChanged?.Invoke();
        UpdateOutput();
    }
    public void RemoveItem(int x, int y)
    {
        grid[x + y] = null;
        OnCraftingGridChanged?.Invoke();
    }

    // Recipe Matching
    private void UpdateOutput()
    {
        outputItem = GetRecipeOutput();
    }

    private DraggableItem GetRecipeOutput()
    {
        // Define recipes
        var ReedFrogPurifierRecipe = new DraggableItem[] {
             null, RecyclableType.Frog, null,
             null, RecyclableType.Reed, null,
             null, null, null 
        };

        var windTurbineRecipe = new DraggableItem[] {
            null, RecyclableType.Plastic, null,
             RecyclableType.Plastic, RecyclableType.Rings, RecyclableType.Plastic,
             null, RecyclableType.Plastic, null
        };

        // Match recipes
        if (MatchRecipe(ReedFrogPurifierRecipe))
            Debug.Log("Frog");
        if (MatchRecipe(windTurbineRecipe))
            Debug.Log("Turbine");
        return null;
    }

    private bool MatchRecipe(DraggableItem[] recipe)
    {
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                if (grid[x + y]?.type != recipe[x + y] && recipe[x + y] != null)
                    return false;
            }
        }
        return true;
    }
}

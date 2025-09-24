using System;
using System.Collections.Generic;
using static RecyclableItem;

public class CraftingSystem
{
    /*private const int GRID_SIZE = 3;
    private RecyclableItem[,] grid; // 3x3 Crafting Grid

    public event Action OnCraftingGridChanged; // Notify UI to refresh
    public RecyclableItem outputItem; // Current output item

    public CraftingSystem()
    {
        grid = new RecyclableItem[GRID_SIZE, GRID_SIZE];
    }

    // Helper Functions
    public RecyclableItem GetItem(int x, int y) => grid[x, y];
    public void SetItem(RecyclableItem item, int x, int y)
    {
        grid[x, y] = item;
        OnCraftingGridChanged?.Invoke();
        UpdateOutput();
    }
    public void RemoveItem(int x, int y)
    {
        grid[x, y] = null;
        OnCraftingGridChanged?.Invoke();
    }

    // Recipe Matching
    private void UpdateOutput()
    {
        outputItem = GetRecipeOutput();
    }

    private RecyclableItem GetRecipeOutput()
    {
        // Define recipes
        var stickRecipe = new RecyclableType[,] {
            { null, RecyclableType.Paper, null },
            { null, RecyclableType.Paper, null },
            { null, null, null }
        };

        var swordRecipe = new RecyclableType[,] {
            { null, RecyclableType.Glass, null },
            { null, RecyclableType.Glass, null },
            { null, RecyclableType.Plastic, null }
        };

        // Match recipes
        if (MatchRecipe(stickRecipe)) return new RecyclableItem(RecyclableType.Placeholder, 1);
        if (MatchRecipe(swordRecipe)) return new RecyclableItem(RecyclableType.Placeholder, 1);
        return null;
    }

    private bool MatchRecipe(RecyclableType[,] recipe)
    {
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                if (grid[x, y]?.RecyclableType != recipe[x, y] && recipe[x, y] != null)
                    return false;
            }
        }
        return true;
    }*/
}

using System;
using UnityEngine;
using static RecyclableItem;

public class CraftingSystem : MonoBehaviour
{
    private const int GRID_SIZE = 3;
    [SerializeField] public GameObject[] grid; // 3x3 Crafting Grid

    public event Action OnCraftingGridChanged; // Notify UI to refresh
    public GameObject outputItemSlot; // Current output item

    public GameObject[] outputItemPrefabs;

    public CraftingSystem()
    {
        //grid = new GameObject[GRID_SIZE * GRID_SIZE];
    }

    // Helper Functions
    public GameObject GetItem(int x, int y) => grid[x + y];
    public void SetItem(GameObject item, int x, int y)
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
    public void UpdateOutput()
    {
        GetRecipeOutput();
    }

    private GameObject GetRecipeOutput()
    {
        // Define recipes
        var ReedFrogPurifierRecipe = new string[] {
             null,"Frog", null,
             null,"Reed", null,
             null, null, null
        };

        var windTurbineRecipe = new string[] {
            null, "Plastic", null,
             "Plastic", "Rings", "Plastic",
             null, "Plastic", null
        };

        // Match recipes
        if (MatchRecipe(ReedFrogPurifierRecipe))
        {
            if(outputItemSlot.transform.childCount == 0)
                Instantiate(outputItemPrefabs[0], outputItemSlot.transform);
        }

        else if (MatchRecipe(windTurbineRecipe))
        {
            if (outputItemSlot.transform.childCount == 0)
                Instantiate(outputItemPrefabs[1], outputItemSlot.transform);
        }

        else
        {
            if (outputItemSlot.transform.childCount != 0)
                Destroy(outputItemSlot.transform.GetChild(0).gameObject);
        }

        return null;
    }

    private bool MatchRecipe(string[] recipe)
    {
        for (int x = 0; x < GRID_SIZE * GRID_SIZE; x++)
        {  
            //check for draggable item component
            if(grid[x].GetComponentInChildren<DraggableItem>() != null)
            {
                if (recipe[x] != grid[x].GetComponentInChildren<DraggableItem>().recyclableItem.type.ToString() && recipe[x] != null)
                {
                    if (grid[x].GetComponentInChildren<DraggableItem>().recyclableItem.type.ToString() == "Photo")
                    {
                        if (grid[x].GetComponentInChildren<ItemSlot>() != null)
                        {
                            if (recipe[x] != grid[x].GetComponentInChildren<ItemSlot>().animalTag && recipe[x] != null)
                            {
                                Debug.Log("not craft");
                                return false;
                            }
                        }
                        else
                        {
                            Debug.Log("not craft");
                            return false;
                        }
                    }
                }
            }
    
            if (recipe[x] != null && grid[x].GetComponentInChildren<DraggableItem>() == null)
            {
                Debug.Log("not craft");
                return false;
            }

            if (recipe[x] == null && grid[x].GetComponentInChildren<DraggableItem>() != null)
            {
                Debug.Log("not craft");
                return false;
            }
        }

        Debug.Log("crafting");
        return true;
    }

    //wipe crafting grid
    public void WipeGrid()
    {
        for (int x = 0; x < GRID_SIZE * GRID_SIZE; x++)
        {
            if(grid[x].transform.childCount != 0)
                Destroy(grid[x].transform.GetChild(0).gameObject);
        }
    }
}

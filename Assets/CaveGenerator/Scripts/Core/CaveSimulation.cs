using System.Collections.Generic;
using UnityEngine;

public class CaveSimulation
{
    int width;
    int height;
    float percentageStartFloor;
    int floorLimit;
    int wallLimit;
    int border;

    public CaveSimulation(int width, int height, float percentageStartFloor = .5f, int floorLimit = 4, int wallLimit = 4, int border = 1)
    {
        this.percentageStartFloor = percentageStartFloor;
        this.width = width;
        this.height = height;
        this.floorLimit = floorLimit;
        this.wallLimit = wallLimit;
        this.border = border;
    }

    public bool[,] StartSimulation()
    {
        bool[,] grid = CreateInitialGrid();
        grid = CreateSimulation(grid);
        grid = CreateBorder(grid, border);
        return grid;
    }

    public bool[,] Polish(bool[,] grid, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            grid = CreateSimulation(grid);
            grid = CreateBorder(grid, border);
        }
        return grid;
    }

    bool[,] CreateInitialGrid()
    {
        bool[,] grid = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = (Random.value <= percentageStartFloor) ? true : false;
            }
        }
        return grid;
    }

    bool[,] CreateSimulation(bool[,] grid)
    {
        bool[,] newGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int amount = CountNearCells(grid, x, y, true);

                if (grid[x, y])
                {
                    if (amount < wallLimit)
                    {
                        newGrid[x, y] = false;
                    }
                    else
                    {
                        newGrid[x, y] = true;
                    }
                }
                else
                {
                    if (amount < floorLimit)
                    {
                        newGrid[x, y] = true;
                    }
                    else
                    {
                        newGrid[x, y] = false;
                    }
                }
            }
        }

        return newGrid;
    }

    public bool[,] CreateBorder(bool[,] grid, int border)
    {
        bool[,] newGrid = grid;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < border || x >= (width - border))
                {
                    newGrid[x, y] = false;
                }
                else
                {
                    if (y < border || y >= (height - border))
                    {
                        newGrid[x, y] = false;
                    }
                }
            }
        }
        return newGrid;
    }

    public Dictionary<int, List<Vector2>> DetectIslands(bool[,] grid)
    {
        bool[,] visiteds = new bool[width, height];
        Dictionary<int, List<Vector2>> islands = new Dictionary<int, List<Vector2>>();

        int count = 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y])
                {
                    List<Vector2> floors = new List<Vector2>();
                    floors = FloodFill(grid, visiteds, floors, x, y);
                    if (floors.Count > 0)
                    {
                        islands.Add(count, floors);
                        count++;
                    }
                }
            }
        }
        return islands;
    }

    List<Vector2> FloodFill(bool[,] grid, bool[,] visiteds, List<Vector2> list, int x, int y)
    {
        if (!visiteds[x, y] && grid[x, y])
        {
            visiteds[x, y] = true;
            list.Add(new Vector2(x, y));

            if (x < width - 1)
            {
                list = FloodFill(grid, visiteds, list, x + 1, y);
            }
            if (x > 0)
            {
                list = FloodFill(grid, visiteds, list, x - 1, y);
            }
            if (y < height - 1)
            {
                list = FloodFill(grid, visiteds, list, x, y + 1);
            }
            if (y > 0)
            {
                list = FloodFill(grid, visiteds, list, x, y - 1);
            }
        }

        return list;
    }

    public bool[,] DigHorizontal(bool[,] grid, int i, int y, int dx)
    {
        for (int x = i; x < width; x++)
        {
            grid[x, y] = true;
        }
        return grid;
    }

    public bool[,] DigVertical(bool[,] grid, int x, int j, int dy)
    {
        for (int y = j; y < height; y++)
        {
            grid[x, y] = true;
        }
        return grid;
    }

    int CountNearCells(bool[,] grid, int x, int y, bool floor)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx != i || ny != j)
                {
                    if (nx > -1 && ny > -1 && nx < width && ny < height)
                    {
                        if (grid[nx, ny] == floor)
                        {
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }
}

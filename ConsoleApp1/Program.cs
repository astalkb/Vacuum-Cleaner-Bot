using System;
using System.Threading;

public class Map
{
    private enum CellType { Empty, Dirt, Obstacle, Cleaned };
    private CellType[,] _grid;
    public int Width { get; private set; } 
    public int Height { get; private set; }

    public Map(int width, int height)
    {
        this.Width = width;
        this.Height = height;
        _grid = new CellType[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = CellType.Empty;
            }
        }
    }
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 &&  x < this.Width && y >= 0 && y < this.Height;
    }

    public bool IsDirt(int x, int y)
    {
        return IsInBounds(x,y) && _grid[x,y] == CellType.Dirt;
    }
    public bool IsObstacle(int x, int y)
    {
        return IsInBounds(x, y) && _grid[x,y] == CellType.Obstacle;
    }
    
    public void AddObstacle(int x, int y)
    {
        _grid[x, y] = CellType.Obstacle;
    }
    public void AddDirt(int x, int y)
    {
        _grid[x, y] = CellType.Dirt;
    }
    
    public void Clean(int x, int y)
    {
        if (IsInBounds(x, y))
        {
            _grid[x,y] = CellType.Cleaned;
        }
    }
    public void Display(int robotX, int robotY)
    {
        // display the 2d grid, it accepts the location of the robot in x and y
        Console.Clear();
        Console.WriteLine("Vacuum Cleaner robot simulation");
        Console.WriteLine("------------------------------------");
        Console.WriteLine("Legends: #Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");
        
        // displays the grid using loop
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                if (x == robotX && y == robotY)
                {
                    Console.Write("R ");
                }
                else
                {
                    switch (_grid[x, y])
                    {
                        case CellType.Empty: Console.Write(". "); break;
                        case CellType.Dirt: Console.Write("D "); break;
                        case CellType.Obstacle: Console.Write("# "); break;
                        case CellType.Cleaned: Console.Write("C "); break;
                    }
                }
            }
            Console.WriteLine();
        }
        Thread.Sleep(200);
    }
    
}

public class Robot
{
    private readonly Map _map;
    public int X { get; set; }  
    public int Y { get; set; }

    public Robot(Map map)
    {
        this._map = map;
        this.X = 0;
        this.Y = 0;
    }

    public bool Move(int newX, int newY)
    {
        
        if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
        {
            //set the new location
            this.X = newX;
            this.Y = newY;
            //display the map with the robot in its location in the grid
            _map.Display(this.X, this.Y);
            return true;
        }
        else
        {
            // it cannot move
            return false;
        }
    }

    public void CleanCurrentSpot()
    {
        if(_map.IsDirt(this.X, this.Y))
        {
            _map.Clean(this.X, this.Y);
            _map.Display(this.X, this.Y);
        }
    }

    public void StartCleaning()
    {
        Console.WriteLine("Start cleaning the room");
        //flag that determines the direction
        int direction = 1;
        for (int y = 0; y < _map.Height; y++)
        {
            int startX = (direction == 1) ? 0 : _map.Width - 1;
            int endX = (direction == 1) ? _map.Width : -1;

            for (int x = startX; x != endX; x+= direction)
            {
                Move(x, y);
                CleanCurrentSpot();
            }
            direction *= -1; // reverse direction for next row
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Initialize Robot");
        Map map = new Map(20, 10);
        //map.Display(10, 10);

        map.AddDirt(5, 3);
        map.AddDirt(10, 8);
        map.AddObstacle(2, 5);
        map.AddObstacle(12, 1);
        map.Display(11, 8);

        Robot robot = new Robot(map);
        robot.StartCleaning();
        Console.WriteLine("Done .");


        /*
        Console.WriteLine($" Grid Width is {myMap.Width}");
        Console.WriteLine($" Grid Height is {myMap.Height}");
        myMap.Display(2, 3);
        Console.WriteLine("Press any key to exit...");
        */
        Console.ReadKey(); 
    }
}

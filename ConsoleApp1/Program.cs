using System;
using System.Threading;

namespace VacuumRobotSimulation
{
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
        Console.Clear();
        Console.WriteLine("Vacuum Cleaner Robot Simulation");
        Console.WriteLine("------------------------------------");
        Console.WriteLine("Legends: #=Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");
        
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

    // Strategy Interface
    public interface ICleaningStrategy
    {
        void Clean(Robot robot, Map map);
    }

    // Strategy 1: S-Pattern Strategy (Original Logic)
    public class S_PatternStrategy : ICleaningStrategy
{
    public void Clean(Robot robot, Map map)
    {
        Console.WriteLine("Starting S-Pattern Cleaning Strategy");
        Console.WriteLine("Moving in systematic S-pattern across the room...");
        
        int direction = 1;
        
        for (int y = 0; y < map.Height; y++)
        {
            int startX = (direction == 1) ? 0 : map.Width - 1;
            int endX = (direction == 1) ? map.Width : -1;

            for (int x = startX; x != endX; x += direction)
            {
                robot.Move(x, y);
                robot.CleanCurrentSpot();
            }
            direction *= -1;
        }
        
        Console.WriteLine("S-Pattern cleaning completed!");
    }
    }

    // Strategy 2: Random Path Strategy (New Algorithm)
    public class RandomPathStrategy : ICleaningStrategy
{
    private Random _random;
    private int _maxMoves;
    
    public RandomPathStrategy(int maxMoves = 100)
    {
        _random = new Random();
        _maxMoves = maxMoves;
    }
    
    public void Clean(Robot robot, Map map)
    {
        Console.WriteLine("Starting Random Path Cleaning Strategy");
        Console.WriteLine("Moving randomly around the room...");
        
        int moveCount = 0;
        int[] directions = { -1, 0, 1 };
        
        while (moveCount < _maxMoves)
        {
            robot.CleanCurrentSpot();
            
            int attempts = 0;
            bool moved = false;
            
            while (attempts < 8 && !moved)
            {
                int deltaX = directions[_random.Next(directions.Length)];
                int deltaY = directions[_random.Next(directions.Length)];
                
                if (deltaX == 0 && deltaY == 0)
                {
                    attempts++;
                    continue;
                }
                
                int newX = robot.X + deltaX;
                int newY = robot.Y + deltaY;
                
                if (robot.Move(newX, newY))
                {
                    moved = true;
                    moveCount++;
                }
                attempts++;
            }
            
            if (!moved)
            {
                Console.WriteLine("Robot is stuck! Ending random cleaning.");
                break;
            }
        }
        
        Console.WriteLine($"Random path cleaning completed after {moveCount} moves!");
    }
    }

    // Strategy 3: Bonus - Spiral Pattern Strategy
    public class SpiralPatternStrategy : ICleaningStrategy
{
    public void Clean(Robot robot, Map map)
    {
        Console.WriteLine("Starting Spiral Pattern Cleaning Strategy");
        Console.WriteLine("Moving in spiral pattern from outside to inside...");
        
        int top = 0, bottom = map.Height - 1;
        int left = 0, right = map.Width - 1;
        
        while (top <= bottom && left <= right)
        {
            for (int x = left; x <= right; x++)
            {
                robot.Move(x, top);
                robot.CleanCurrentSpot();
            }
            top++;
            
            for (int y = top; y <= bottom; y++)
            {
                robot.Move(right, y);
                robot.CleanCurrentSpot();
            }
            right--;
            
            if (top <= bottom)
            {
                for (int x = right; x >= left; x--)
                {
                    robot.Move(x, bottom);
                    robot.CleanCurrentSpot();
                }
                bottom--;
            }
            
            if (left <= right)
            {
                for (int y = bottom; y >= top; y--)
                {
                    robot.Move(left, y);
                    robot.CleanCurrentSpot();
                }
                left++;
            }
        }
        
        Console.WriteLine("Spiral pattern cleaning completed!");
    }
    }

    public class Robot
{
    private readonly Map _map;
    private ICleaningStrategy _cleaningStrategy;
    
    public int X { get; set; }  
    public int Y { get; set; }

    public Robot(Map map)
    {
        this._map = map;
        this.X = 0;
        this.Y = 0;
    }
    
    public void SetStrategy(ICleaningStrategy strategy)
    {
        _cleaningStrategy = strategy;
        Console.WriteLine($"Strategy changed to: {strategy.GetType().Name}");
    }

    public bool Move(int newX, int newY)
    {
        if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
        {
            this.X = newX;
            this.Y = newY;
            _map.Display(this.X, this.Y);
            return true;
        }
        else
        {
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
        if (_cleaningStrategy == null)
        {
            Console.WriteLine("No cleaning strategy set! Please set a strategy first.");
            return;
        }
        
        Console.WriteLine("Robot starting cleaning process...");
        _cleaningStrategy.Clean(this, _map);
    }
    }

    public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Strategy Pattern Vacuum Robot Demo ===\n");
        
        // Initialize the map and robot
        Map map = new Map(12, 6);
        
        // Add some dirt and obstacles for demonstration
        map.AddDirt(3, 1);
        map.AddDirt(7, 2);
        map.AddDirt(5, 3);
        map.AddDirt(9, 4);
        map.AddDirt(2, 4);
        map.AddObstacle(4, 2);
        map.AddObstacle(8, 1);
        map.AddObstacle(6, 4);
        
        Robot robot = new Robot(map);
        
        Console.WriteLine("Initial map setup:");
        map.Display(robot.X, robot.Y);
        Thread.Sleep(2000);
        
        // Demonstrate Strategy 1: S-Pattern
        Console.WriteLine("\n=== DEMONSTRATION 1: S-Pattern Strategy ===");
        robot.SetStrategy(new S_PatternStrategy());
        robot.StartCleaning();
        
        Console.WriteLine("\nPress any key to continue to next strategy...");
        Console.ReadKey();
        
        // Reset the map for next demonstration
        map = new Map(12, 6);
        map.AddDirt(3, 1);
        map.AddDirt(7, 2);
        map.AddDirt(5, 3);
        map.AddDirt(9, 4);
        map.AddDirt(2, 4);
        map.AddObstacle(4, 2);
        map.AddObstacle(8, 1);
        map.AddObstacle(6, 4);
        robot = new Robot(map);
        
        // Demonstrate Strategy 2: Random Path
        Console.WriteLine("\n=== DEMONSTRATION 2: Random Path Strategy ===");
        robot.SetStrategy(new RandomPathStrategy(50));
        robot.StartCleaning();
        
        Console.WriteLine("\nPress any key to continue to bonus strategy...");
        Console.ReadKey();
        
        // Reset the map for bonus demonstration
        map = new Map(12, 6);
        map.AddDirt(3, 1);
        map.AddDirt(7, 2);
        map.AddDirt(5, 3);
        map.AddDirt(9, 4);
        map.AddDirt(2, 4);
        map.AddObstacle(4, 2);
        map.AddObstacle(8, 1);
        map.AddObstacle(6, 4);
        robot = new Robot(map);
        
        // Demonstrate Bonus Strategy: Spiral Pattern
        Console.WriteLine("\n=== BONUS DEMONSTRATION: Spiral Pattern Strategy ===");
        robot.SetStrategy(new SpiralPatternStrategy());
        robot.StartCleaning();
        
        Console.WriteLine("\n=== All demonstrations completed! ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
}
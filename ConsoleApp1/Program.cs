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
    
    public bool IsEmpty(int x, int y)
    {
        return IsInBounds(x, y) && _grid[x, y] == CellType.Empty;
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

    // Strategy 1: S-Pattern Strategy
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

    // Strategy 2: Random Path Strategy
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
        Console.WriteLine("=== Vacuum Cleaner Robot ===\n");

        int width = ReadPositiveInt("Enter map width: ");
        int height = ReadPositiveInt("Enter map height: ");

        Map map = new Map(width, height);
        Robot robot = new Robot(map);

        int maxPlaceable = width * height - 1; 
        int obstacles = ReadBoundedInt(
            $"Enter number of obstacles (0 to {maxPlaceable}): ", 0, maxPlaceable);

        int remainingAfterObstacles = Math.Max(0, maxPlaceable - obstacles);
        int dirts = ReadBoundedInt(
            $"Enter number of dirt tiles (0 to {remainingAfterObstacles}): ", 0, remainingAfterObstacles);

        PlaceRandomCells(map, obstacles, isObstacle: true);
        PlaceRandomCells(map, dirts, isObstacle: false);

        Console.WriteLine("\nInitial map setup:");
        map.Display(robot.X, robot.Y);

        Console.WriteLine("\nChoose cleaning strategy:");
        Console.WriteLine("1) S-Pattern");
        Console.WriteLine("2) Random");

        int choice = ReadBoundedInt("Select 1 or 2: ", 1, 2);
        if (choice == 1)
        {
            robot.SetStrategy(new S_PatternStrategy());
        }
        else
        {
            int maxSteps = ReadPositiveInt("Enter number of random steps: ");
            robot.SetStrategy(new RandomPathStrategy(maxSteps));
        }

        robot.StartCleaning();

        Console.WriteLine("\nCleaning finished. Press any key to exit...");
        Console.ReadKey();
    }

    private static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (int.TryParse(input, out int value) && value > 0)
            {
                return value;
            }
            Console.WriteLine("Please enter a positive integer.");
        }
    }

    private static int ReadBoundedInt(string prompt, int minInclusive, int maxInclusive)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (int.TryParse(input, out int value) && value >= minInclusive && value <= maxInclusive)
            {
                return value;
            }
            Console.WriteLine($"Please enter an integer between {minInclusive} and {maxInclusive}.");
        }
    }

    private static void PlaceRandomCells(Map map, int count, bool isObstacle)
    {
        if (count <= 0) return;
        Random rng = new Random();
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 20 + 1000;
        while (placed < count && attempts < maxAttempts)
        {
            int x = rng.Next(map.Width);
            int y = rng.Next(map.Height);

            if (x == 0 && y == 0)
            {
                attempts++;
                continue;
            }
            if (map.IsEmpty(x, y))
            {
                if (isObstacle)
                {
                    map.AddObstacle(x, y);
                }
                else
                {
                    map.AddDirt(x, y);
                }
                placed++;
            }
            attempts++;
        }
    }
}
}
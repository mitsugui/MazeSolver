using System.Text;
using MazeSolver;


const int width = 85;
const int height = 65;

var print = args.Any(arg => arg == "-print");

var filePath = args.FirstOrDefault(arg => arg.StartsWith("-file="))?.Substring(6) ?? "input.txt";

var (loaded, cells) = await TryLoadCellsAsync(filePath);

if (!loaded)
{
	Console.WriteLine(@"Try to pass file name in -file=<my file path>. Default file name is input.txt.");
	return;
}

var maze = new Maze(cells, width, height);

const int endRow = height - 1;
const int endCol = width - 1;

var printEachStep = print;
var solver = new MazeSolver.MazeSolver(width, height, 0, 0, endRow, endCol);
while (!solver.IsSolved())
{
	if (printEachStep)
	{
		maze.PrintCount();

		maze.PrintCells();
	}

	maze.NextStep();

	solver.CalculatePossiblePositions(maze.Cells);

	if (printEachStep)
	{
		solver.PrintMovements();

		Console.WriteLine("Press any key to continue to the next step, ESC to quit or END to run to the end.");
		
		var key = Console.ReadKey();
		switch (key.Key)
		{
			case ConsoleKey.Escape:
				return;
			case ConsoleKey.End:
				printEachStep = false;
				break;
		}
	}
}

if (print)
{
	Console.WriteLine("Press p to print solution matrix.");
	
	if (Console.ReadKey().Key == ConsoleKey.P) solver.PrintSolutionMatrix();

	solver.PrintSolution();

	Console.WriteLine("Press p to print game or any other key to continue.");
	if (Console.ReadKey().Key == ConsoleKey.P) solver.PrintGame(maze.Reset());
}

solver.SaveSolution("solution.txt");

Console.WriteLine("Press any key to finish...");
Console.ReadKey();

async Task<(bool Loaded, IReadOnlyList<int> Cells)> TryLoadCellsAsync(string inputTxt)
{
	var loadedCells = new List<int>();
	try
	{
		using var reader = new StreamReader(inputTxt, Encoding.UTF8);

		while (true)
		{
			var rowText = await reader.ReadLineAsync();

			if (rowText == null) break;

			loadedCells.AddRange(rowText.Split(' ')
				.Select(txt => Convert.ToInt32(txt)));
		}
	}
	catch (FileNotFoundException)
	{
		Console.WriteLine($"File {inputTxt} not found.");
		return (false, loadedCells);
	}
	catch (DirectoryNotFoundException)
	{
		Console.WriteLine($"Directory {inputTxt} not found.");
		return (false, loadedCells);
	}

	return (true, loadedCells);
}
using System.Text;

namespace MazeSolver;

public class MazeSolver
{
	public List<Dictionary<(int Row, int Column), ((int ParentRow, int ParentColumn) Parent, char Movement)>> Paths { get; }

	public int Width { get; }

	public int Height { get; }

	public int EndRow { get; }

	public int EndColumn { get; }

	public MazeSolver(int width, int height, int startRow, int startColumn, int endRow, int endColumn)
	{
		Width = width;
		Height = height;
		Paths = new List<Dictionary<(int Row, int Column), ((int ParentRow, int ParentColumn), char Movement)>>
		{
			new() { { (startRow, startColumn), ((-1, -1), '\0') } }
		};
		EndRow = endRow;
		EndColumn = endColumn;
	}

	public void CalculatePossiblePositions(IReadOnlyList<int> mazeCells)
	{
		var lastStep = Paths.Last();

		var currentStep = new Dictionary<(int Row, int Column), ((int ParentRow, int ParentColumn), char Movement)>();
		for (var row = 0; row < Height; row++)
		{
			for (var col = 0; col < Width; col++)
			{
				var currentCell = (row, col);
				if (!lastStep.ContainsKey(currentCell)) continue;
				
				//Can move up
				var upperCell = (row - 1, col);
				if (row > 0 && mazeCells[(row - 1) * Width + col] != Maze.Green)
				{
					currentStep.TryAdd(upperCell, (currentCell, 'U'));
				}
				//Can move down
				var lowerCell = (row + 1, col);
				if (row < Height - 1 && mazeCells[(row + 1) * Width + col] != Maze.Green)
				{
					currentStep.TryAdd(lowerCell, (currentCell, 'D'));
				}
				//Can move left
				var leftCell = (row, col - 1);
				if (col > 0 && mazeCells[row * Width + (col - 1)] != Maze.Green)
				{
					currentStep.TryAdd(leftCell, (currentCell, 'L'));
				}
				//Can move right
				var rightCell = (row, col + 1);
				if (col < Width - 1 && mazeCells[row * Width + (col + 1)] != Maze.Green)
				{
					currentStep.TryAdd(rightCell, (currentCell, 'R'));
				}
			}
		}
		Paths.Add(currentStep);
	}

	public bool IsSolved()
	{
		return Paths.Last().ContainsKey((EndRow, EndColumn));
	}

	public void PrintSolutionMatrix()
	{
		var path = GetPathAndMovements(out var movements);

		Console.CursorVisible = false;
		Console.WriteLine("--- Solution ----");
		Console.Clear();
		for (var idx = 0; idx < path.Count; idx++)
		{
			Console.SetCursorPosition(0, 0);
			Console.Write("|");
			Console.Write(new string('-', 2 * Width));
			Console.WriteLine("|");

			for (var row = 0; row < Height; row++)
			{
				Console.Write("|");
				for (var col = 0; col < Width; col++)
				{
					Console.Write(path[idx] == (row, col) ? $"{movements[idx]} " : "  ");
				}
				Console.WriteLine("|");
			}
			Console.Write("|");
			Console.Write(new string('-', 2 * Width));
			Console.WriteLine("|");
		}
		Console.CursorVisible = true;
	}

	public void PrintSolution()
	{
		var path = GetPathAndMovements(out var movements);
		
		Console.WriteLine("---- Movements --------");
		for (var idx = 0; idx < path.Count; idx++)
		{
			Console.WriteLine(idx < movements.Count
				? $"Step {idx}: {path[idx]} - {movements[idx]}"
				: $"Step {idx}: {path[idx]}");
		}
	}

	public void PrintMovements()
	{
		Console.WriteLine("--- Possible movements ----");
		Console.Write("|");
		Console.Write(new string('-', 2 * Width));
		Console.WriteLine("|");

		var positions = Paths.Last();
		for (var row = 0; row < Height; row++)
		{
			Console.Write("|");
			for (var col = 0; col < Width; col++)
			{
				Console.Write(positions.ContainsKey((row, col)) ? "X " : "  ");
			}
			Console.WriteLine("|");
		}
		Console.Write("|");
		Console.Write(new string('-', 2 * Width));
		Console.WriteLine("|");
	}

	public void PrintGame(Maze maze)
	{
		var path = GetPathAndMovements(out var movements);
		Console.Clear();
		for (var i = 0; i < path.Count; i++)
		{
			Console.SetCursorPosition(0, 0);
			maze.PrintCells(path[i].Row, path[i].Column, i < movements.Count ? movements[i].ToString() : "X");
			maze.NextStep();
		}
	}

	public void SaveSolution(string textFile)
	{
		Console.WriteLine($"Saving solution to {textFile}.");

		using var writer = new StreamWriter(textFile, false, Encoding.UTF8);

		GetPathAndMovements(out var movements);
		foreach (var movement in movements)
		{
			writer.Write($"{movement} ");
		}
	}

	private List<(int Row, int Column)> GetPathAndMovements(out List<char> movements)
	{
		var current = (EndRow, EndColumn);
		var path = new List<(int Row, int Column)> { current };
		movements = new List<char>();
		for (var i = Paths.Count - 1; i >= 0; i--)
		{
			if (!Paths[i].TryGetValue(current, out var parent)
				|| parent.Parent == (-1, -1))
				continue;

			path.Add(parent.Parent);
			movements.Add(parent.Movement);
			current = parent.Parent;
		}

		path.Reverse();
		movements.Reverse();
		return path;
	}
}
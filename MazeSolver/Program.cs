{
	const int width = 8;
	const int height = 7;

	var maze = new Maze(new[]
	{
		Maze.Start, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 1, 0, 1, 1, 0, 0,
		0, 1, 1, 0, 0, 1, 1, 0,
		0, 0, 1, 0, 1, 1, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, Maze.End,
	}, width, height);

	const int endRow = height - 1;
	const int endCol = width - 1;
	var solver = new MazeSolver(width, height, 0, 0);
	while (!solver.IsSolved(endRow, endCol))
	{
		maze.PrintCount();

		maze.PrintCells();
		maze.NextStep();

		solver.CalculatePossiblePositions(maze.Cells);

		solver.PrintMovements();

		Console.WriteLine("Press any key to continue or ESC to quit");
		var key = Console.ReadKey();

		if (key.Key == ConsoleKey.Escape) break;
	}

	solver.PrintSolution(endRow, endCol);

	Console.ReadKey();

	solver.PrintGame(new Maze(new[]
	{
		Maze.Start, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 1, 0, 1, 1, 0, 0,
		0, 1, 1, 0, 0, 1, 1, 0,
		0, 0, 1, 0, 1, 1, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, Maze.End,
	}, width, height), endRow, endCol);
	
	Console.ReadKey();
}

public class Maze
{
	public const int Start = 3;
	public const int End = 4;
	public const int Green = 1;
	public const int White = 0;

	public int Width { get; }

	public int Height { get; }

	public int[] Cells { get; private set; }

	public Maze(int[] cells, int width, int height)
	{
		Width = width;
		Height = height;
		Cells = cells;
	}
	
	public void PrintCells(int? cursorRow = null, int? cursorColumn = null, string cursor = "M")
	{
		Console.WriteLine("--- Cells ----");
		Console.Write("|");
		Console.Write(new string('-', 2 * Width));
		Console.WriteLine("|");
		for (var row = 0; row < Height; row++)
		{
			var isCursorRow = row == cursorRow;

			Console.Write("|");
			for (var col = 0; col < Width; col++)
			{
				var cell = Cells[row * Width + col];
				Console.Write(isCursorRow && col == cursorColumn
					? $"{cursor} "
					: cell == -1
						? "S "
						: cell == -2
							? "E "
							: cell == 0
								? "  "
								: "G ");
			}
			Console.WriteLine("|");
		}
		Console.Write("|");
		Console.Write(new string('-', 2 * Width));
		Console.WriteLine("|");
	}

	public void NextStep()
	{
		Cells = CalculateNextStep(Cells, Width, Height).ToArray();
	}

	public void PrintCount()
	{
		var neighborsCount = CalculateNeighborsCount(Cells, Width, Height)
			.ToArray();

		Console.WriteLine("--- Neighbors count ----");
		Console.Write("|");
		Console.Write(new string('-', 3 * Width));
		Console.WriteLine("|");
		for (var row = 0; row < Height; row++)
		{
			Console.Write("|");
			Console.Write(string.Join(" ", neighborsCount
				.Skip(row * Width)
				.Take(Width)
				.Select(c => c >= 0 ? c.ToString("D2") : c.ToString())));
			Console.WriteLine("|");
		}
		Console.Write("|");
		Console.Write(new string('-', 3 * Width));
		Console.WriteLine("|");
	}

	private static IEnumerable<int> CalculateNextStep(IReadOnlyList<int> cells, int width, int height)
	{
		foreach (var neighborsCount in CalculateNeighborsCount(cells, width, height))
		{
			yield return neighborsCount >= 0
				? neighborsCount >= 2 && neighborsCount <= 3
					? Green
					: White
				: neighborsCount >= -4 && neighborsCount <= -2
					? Green
					: White;
		}
	}

	/// <summary>
	/// Positive values means Green neighbors and negative values means White neighbors
	/// </summary>
	private static IEnumerable<int> CalculateNeighborsCount(IReadOnlyList<int> cells, int width, int height)
	{
		for (var row = 0; row < height; row++)
		{
			for (var col = 0; col < width; col++)
			{
				if (row == 0
					&& col == 0)
				{
					yield return 0;
					continue;
				}

				if (row == height - 1
					&& col == width - 1)
				{
					yield return 0;
					continue;
				}

				var cell = cells[row * width + col];
				yield return cell == White
					? CountNeighbors(cells, width, height, row, col, Green)
					: -CountNeighbors(cells, width, height, row, col, White);
			}
		}
	}

	private static int CountNeighbors(IReadOnlyList<int> cells, int width, int height, int row, int col, int value)
	{
		var count = 0;
		//Count cells from row n-1
		if (row > 0)
		{
			if (col > 0 && cells[(row - 1) * width + (col - 1)] == value) count++;
			if (cells[(row - 1) * width + col] == value) count++;
			if (col < width - 1 && cells[(row - 1) * width + (col + 1)] == value) count++;
		}

		//Count cells from row n (one on the left another on the right)
		if (col > 0 && cells[row * width + (col - 1)] == value) count++;
		if (col < width - 1 && cells[row * width + (col + 1)] == value) count++;
		//Count cells from the last row
		if (row < height - 1)
		{
			if (col > 0 && cells[(row + 1) * width + (col - 1)] == value) count++;
			if (cells[(row + 1) * width + col] == value) count++;
			if (col < width - 1 && cells[(row + 1) * width + (col + 1)] == value) count++;
		}

		//Count cells from the border as if they were surrounded by white cells.
		if (value != 0) return count;

		if (row == 0)
		{
			count += 3; //3 white cells on the top
			if (col == 0) count += 2; //2 white cells on the left
			else if (col == width - 1) count += 2; //2 white cells on the right
		}
		else if (row == height - 1)
		{
			count += 3;
			if (col == 0) count += 2; //2 white cells on the left
			else if (col == width - 1) count += 2; //2 white cells on the right
		}
		else if (col == 0 || col == width - 1)
		{
			count += 3;
		}

		return count;
	}
}

public class MazeSolver
{
	public List<Dictionary<(int Row, int Column), ((int ParentRow, int ParentColumn) Parent, char Movement)>> Paths { get; }

	public int Width { get; }

	public int Height { get; }

	public MazeSolver(int width, int height, int startRow, int startColumn)
	{
		Width = width;
		Height = height;
		Paths = new List<Dictionary<(int Row, int Column), ((int ParentRow, int ParentColumn), char Movement)>>
		{
			new() { { (startRow, startColumn), ((-1, -1), '\0') } }
		};
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

	public bool IsSolved(int endRow, int endColumn)
	{
		return Paths.Last().ContainsKey((endRow, endColumn));
	}

	public void PrintSolution(int endRow, int endColumn)
	{
		var path = GetPathAndMovements(endRow, endColumn, out var movements);

		Console.WriteLine("--- Solution ----");
		for (var idx = 0; idx < path.Count; idx++)
		{
			Console.Write("|");
			Console.Write(new string('-', 2 * Width));
			Console.WriteLine("|");

			for (var row = 0; row < Height; row++)
			{
				Console.Write("|");
				for (var col = 0; col < Width; col++)
				{
					Console.Write(path[idx]  == (row, col) ? idx : " ");
				}
				Console.WriteLine("|");
			}
			Console.Write("|");
			Console.Write(new string('-', 2 * Width));
			Console.WriteLine("|");
		}

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

	public void PrintGame(Maze maze, int endRow, int endColumn)
	{
		var path = GetPathAndMovements(endRow, endColumn, out var movements);
		for (var i = 0; i < path.Count; i++)
		{
			maze.PrintCells(path[i].Row, path[i].Column, i < movements.Count ? movements[i].ToString() : "X");
			maze.NextStep();
		}
	}
	
	private List<(int Row, int Column)> GetPathAndMovements(int endRow, int endColumn, out List<char> movements)
	{
		var current = (endRow, endColumn);
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

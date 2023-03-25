namespace MazeSolver;

public class Maze
{
	private readonly IReadOnlyList<int> _originalCells;

	public const int Start = 3;
	public const int End = 4;
	public const int Green = 1;
	public const int White = 0;

	public int Width { get; }

	public int Height { get; }

	public IReadOnlyList<int> Cells { get; private set; }

	public Maze(IReadOnlyList<int> cells, int width, int height)
	{
		Width = width;
		Height = height;
		Cells = cells;
		_originalCells = cells;
	}

	public Maze Reset()
	{
		Cells = _originalCells;
		return this;
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
				? neighborsCount > 1 && neighborsCount < 5
					? Green
					: White
				: neighborsCount > -5 && neighborsCount < -2
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
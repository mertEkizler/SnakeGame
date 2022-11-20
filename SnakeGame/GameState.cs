using System;
using System.Collections.Generic;

namespace SnakeGame
{
    public class GameState
    {
        public int Rows { get; set; }

        public int Cols { get; set; }

        public GridValue[,] Grid { get; set; }

        public Direction Direction { get; private set; }

        public int Score { get; private set; }

        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> directionChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Direction = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            var rows = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[rows, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(rows, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c); ;
                    }
                }
            }
        }

        private void AddFood()
        {
            var empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            var position = empty[random.Next(empty.Count)];

            Grid[position.Row, position.Col] = GridValue.Food;
        }

        public Position SnakeHeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position SnakeTailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position position)
        {
            snakePositions.AddFirst(position);

            Grid[position.Row, position.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            var tail = snakePositions.Last.Value;

            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (directionChanges.Count == 0)
            {
                return Direction;
            }

            return directionChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDirection)
        {
            if (directionChanges.Count == 2)
            {
                return false;
            }

            var lastDirection = GetLastDirection();
            return newDirection != lastDirection && newDirection != lastDirection.Opposite();
        }

        public void ChangeDirection(Direction direction)
        {
            if (CanChangeDirection(direction))
            {
                directionChanges.AddLast(direction);
            }
        }

        private bool OutsideGrid(Position position)
        {
            return position.Row < 0 || position.Row >= Rows || position.Col < 0 || position.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition))
            {
                return GridValue.Outside;
            }

            if (newHeadPosition == SnakeTailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPosition.Row, newHeadPosition.Col];
        }

        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                Direction = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }

            var newHeadPosition = SnakeHeadPosition().Translate(Direction);
            var hit = WillHit(newHeadPosition);

            if (hit == GridValue.Outside ||hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if(hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPosition);
            }
            else if(hit == GridValue.Food)
            {
                AddHead(newHeadPosition);
                Score++;
                AddFood();
            }
        }
    }
}
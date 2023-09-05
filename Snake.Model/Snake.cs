using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Snake.Persistance;

namespace Snake.Model
{
    public class Snake
    {
        #region Fields

        private LinkedList<(Int32 x, Int32 y)> _body = new LinkedList<(Int32, Int32)>();  // a kígyó teste részeinek pozíciói (a fejét beleszámítva)
        private (Int32 x, Int32 y) _head; // a kígyó fejének pozíciója
        private Direction _currentDirection; // jelenlegi irány
        private Int32 _consumedEggs; // megevett tojások száma
        private SnakeField _field; // a játéktábla

        #endregion

        #region Properties

        /// <summary>
        /// Egy lista, amely a kígyó testének a részeinek a pozícióját tartalmazza, egyesével.
        /// </summary>
        public LinkedList<(Int32 x, Int32 y)> Body { get { return _body; } }

        /// <summary>
        /// A kígyó fejének a pozíciójának a lekérdezése.
        /// </summary>
        public (Int32 x, Int32 y) Head { get { return _head; } private set { _head = value; } }

        /// <summary>
        /// Irány lekérdezése.
        /// </summary>
        public Direction CurrentDirection { get { return _currentDirection; } set { _currentDirection = value; } }

        /// <summary>
        /// Megevett tojások számának lekérdezése.
        /// </summary>
        public Int32 ConsumedEggs { get { return _consumedEggs; } }

        /// <summary>
        /// A játéktábla lekérdezése.
        /// </summary>
        public SnakeField Field { get { return _field; } private set { _field = value; } }

        #endregion

        #region Events

        public event EventHandler<SnakeEventArgs>? Crash;
        public event EventHandler<SnakeEventArgs>? EggConsumed;
        public event EventHandler<SnakeEventArgs>? PositionChange;

        #endregion

        #region Constructors

        /// <summary>
        /// Kígyó példányosítása.
        /// </summary>
        /// <param name="table">A játéktábla, amin példányosítjuk a kígyót.</param>
        public Snake(SnakeField table)
        {
            Int32 x = table.GameSizeX / 2;
            Int32 y = table.GameSizeY / 2;

            _field = table;
            _head = (x, y); // a kígyó feje kezdetben a játéktábla közepén helyezkedik el
            _field.SetType(x, y, FieldType.Snake);

            for (int i = 0; i < 5; i++) // a kígyó hossza kezdetben 5, így felvesszük a kígyó maradék részét is
            {
                _body.AddLast((x, y - i)); // a kígyó vízszintesen fog elhelyezkedni a tábla közepén
                _field.SetType(x, y - i, FieldType.Snake);
            }

            _currentDirection = Direction.None; // a kígyó kezdetben semmilyen irányba se megy
            _consumedEggs = 0; // kezdetben az elfogyasztott tojások száma 0
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Kígyó irányváltása.
        /// </summary>
        public void SwitchDirection(Direction direction)
        {
            switch (_currentDirection)
            {
                case Direction.None:
                    if (direction == Direction.None || direction == Direction.Left) break;
                    else
                    {
                        _currentDirection = direction;
                        break;
                    }
                case Direction.Left:
                    if (direction == Direction.Left || direction == Direction.Right) break;
                    else
                    {
                        _currentDirection = direction;
                        break;
                    }
                case Direction.Right:
                    if (direction == Direction.Right || direction == Direction.Left) break;
                    else
                    {
                        _currentDirection = direction;
                        break;
                    }
                case Direction.Up:
                    if (direction == Direction.Up || direction == Direction.Down) break;
                    else
                    {
                        _currentDirection = direction;
                        break;
                    }
                case Direction.Down:
                    if (direction == Direction.Down || direction == Direction.Up) break;
                    else
                    {
                        _currentDirection = direction;
                        break;
                    }
            }
        }

        /// <summary>
        /// A kígyó mozgatása.
        /// </summary>
        public void Move()
        {
            switch (_currentDirection)
            {
                case Direction.Left:
                    if (_field[_head.x, _head.y - 1] == FieldType.Wall || _field[_head.x, _head.y - 1] == FieldType.Snake)
                    {
                        OnCrash();
                        break;
                    }
                    else if (_field[_head.x, _head.y - 1] == FieldType.Egg)
                    {
                        _field.SetType(_head.x, _head.y - 1, FieldType.Snake);
                        _head = (_head.x, _head.y - 1);
                        _body.AddFirst(_head);

                        OnEggConsumed();
                        break;
                    }
                    else
                    {
                        _field.SetType(_head.x, _head.y - 1, FieldType.Snake);
                        _head = (_head.x, _head.y - 1);
                        _body.AddFirst(_head);

                        _field.SetType(_body.Last().x, _body.Last().y, FieldType.Empty);
                        _body.RemoveLast();
                        break;
                    }
                case Direction.Right:
                    if (_field[_head.x, _head.y + 1] == FieldType.Wall || _field[_head.x, _head.y + 1] == FieldType.Snake)
                    {
                        OnCrash();
                        break;
                    }
                    else if (_field[_head.x, _head.y + 1] == FieldType.Egg)
                    {

                        _field.SetType(_head.x, _head.y + 1, FieldType.Snake);
                        _head = (_head.x, _head.y + 1);
                        _body.AddFirst(_head);


                        OnEggConsumed();
                        break;
                    }
                    else
                    {
                        _field.SetType(_head.x, _head.y + 1, FieldType.Snake);
                        _head = (_head.x, _head.y + 1);
                        _body.AddFirst(_head);

                        _field.SetType(_body.Last().x, _body.Last().y, FieldType.Empty);
                        _body.RemoveLast();
                        break;
                    }
                case Direction.Up:
                    if (_field[_head.x - 1, _head.y] == FieldType.Wall || _field[_head.x - 1, _head.y] == FieldType.Snake)
                    {
                        OnCrash();
                        break;
                    }
                    else if (_field[_head.x - 1, _head.y] == FieldType.Egg)
                    {
                        _field.SetType(_head.x - 1, _head.y, FieldType.Snake);
                        _head = (_head.x - 1, _head.y);
                        _body.AddFirst(_head);


                        OnEggConsumed();
                        break;
                    }
                    else
                    {
                        _field.SetType(_head.x - 1, _head.y, FieldType.Snake);
                        _head = (_head.x - 1, _head.y);
                        _body.AddFirst(_head);

                        _field.SetType(_body.Last().x, _body.Last().y, FieldType.Empty);
                        _body.RemoveLast();
                        break;
                    }
                case Direction.Down:
                    if (_field[_head.x + 1, _head.y] == FieldType.Wall || _field[_head.x + 1, _head.y] == FieldType.Snake)
                    {
                        OnCrash();
                        break;
                    }
                    else if (_field[_head.x + 1, _head.y] == FieldType.Egg)
                    {
                        _field.SetType(_head.x + 1, _head.y, FieldType.Snake);
                        _head = (_head.x + 1, _head.y);
                        _body.AddFirst(_head);

                        OnEggConsumed();
                        break;
                    }
                    else
                    {
                        _field.SetType(_head.x + 1, _head.y, FieldType.Snake);
                        _head = (_head.x + 1, _head.y);
                        _body.AddFirst(_head);

                        _field.SetType(_body.Last().x, _body.Last().y, FieldType.Empty);
                        _body.RemoveLast();
                        break;
                    }
            }

            OnPositionChange();
        }

        #endregion

        #region Private event triggers


        /// <summary>
        /// Tojásfelfalás eseményének kiváltása.
        /// </summary>
        private void OnEggConsumed()
        {
            EggConsumed?.Invoke(this, new SnakeEventArgs(_consumedEggs));
            _consumedEggs++;
        }

        /// <summary>
        /// Ütközés eseményének kiváltása.
        /// </summary>
        private void OnCrash()
        {
            Crash?.Invoke(this, new SnakeEventArgs(_consumedEggs));
        }

        /// <summary>
        /// Pozícióváltás eseményének kiváltása.
        /// </summary>
        private void OnPositionChange()
        {
            PositionChange?.Invoke(this, new SnakeEventArgs(_consumedEggs));
        }

        #endregion
    }
}

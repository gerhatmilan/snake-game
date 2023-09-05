using System;
using System.IO;
using Snake.Persistance;

namespace Snake.Model
{
    public class SnakeGameModel
    {
        #region Fields

        private ISnakeDataAccess _dataAccess; // adatelérés
        private SnakeField _field; // játéktábla
        private Snake _snake; // kígyó
        private Int32 _currentScore; // jelenlegi eredmény
        private Int32 _highScore; // legjobb eredmény
        private Boolean _gameOver; // játék vége állapot
        private String _currentGameField; // jelenlegi játékpálya
        private Boolean _paused;

        #endregion

        #region Properties

        /// <summary>
        /// A pálya lekérdezése.
        /// </summary>
        public SnakeField Field { get { return _field; } }

        /// <summary>
        /// Kígyó lekérdezése.
        /// </summary>
        public Snake Snake { get { return _snake; } }

        /// <summary>
        /// Jelenlegi eredmény lekérdezése.
        /// </summary>
        public Int32 CurrentScore { get { return _currentScore; } set { _currentScore = value; } }

        /// <summary>
        /// Legjobb eredmény lekérdezése.
        /// </summary>
        public Int32 HighScore { get { return _highScore; } set { _highScore = value; } }


        /// <summary>
        /// Játék vége lekérdezése.
        /// </summary>
        public Boolean GameIsOver { get { return _gameOver; } set { _gameOver = value; } }

        /// <summary>
        /// Jelenlegi pálya útvonala.
        /// </summary>
        public String CurrentGameField { get { return _currentGameField; } set { _currentGameField = value; } }

        /// <summary>
        /// Áll-e a játék?
        /// </summary>
        public Boolean Paused { get { return _paused; } set { _paused = value; } }

        #endregion

        #region Events

        /// <summary>
        /// Játék végének eseménye.
        /// </summary>
        public event EventHandler<SnakeEventArgs>? GameOver;

        // <summary>
        /// Tojás felfalásának végének eseménye.
        /// </summary>
        public event EventHandler<SnakeEventArgs>? EggConsumed;

        // <summary>
        /// Tojás felfalásának végének eseménye.
        /// </summary>
        public event EventHandler<SnakeEventArgs>? SnakePositionChange;

        /// <summary>
        /// Játék betöltésének eseménye.
        /// </summary>
        public event EventHandler? GameLoaded;
        #endregion

        #region Constructors

        public SnakeGameModel(ISnakeDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _field = null!; // később példányosítjuk
            _snake = null!; // később példányosítjuk
            _currentGameField = null!; // később példányosítjuk
        }

        #endregion

        #region Public game methods

        /// <summary>
        /// Játék inicializálása.
        /// </summary>
        /// <exception cref="SnakeDataException">A pálya betöltése során hiba lépett fel.</exception>
        public void InitalizeGame()
        {
            try
            {
                _field = _dataAccess.Load(@"GameFields\GameField15x15.txt"); // alapértelmezett pálya
                _snake = new Snake(_field);
                _currentGameField = @"GameFields\GameField15x15.txt";
                _gameOver = false;
                _currentScore = 0;
                _highScore = 0; // az elért legjobb eredményt csak akkor frissítjük, ha másik pályát töltünk be
                _paused = false;
                GenerateEgg(); // tojást generálunk egy random pozícióra

                _snake.Crash += new EventHandler<SnakeEventArgs>(OnGameOver); // feliratkozik a modell a kígyó eseményeire
                _snake.EggConsumed += new EventHandler<SnakeEventArgs>(OnEggConsumed);
                _snake.PositionChange += new EventHandler<SnakeEventArgs>(OnSnakePositionChange);

                OnGameLoaded();
            }
            catch
            {
                throw new SnakeDataException();
            }
        }

        /// <summary>
        /// Játék betöltése aszinkron módon.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// /// <exception cref="SnakeDataException">A pálya betöltése során hiba lépett fel.</exception>
        public async Task LoadGameAsync(String path)
        {
            try
            {
                _field = await _dataAccess.LoadAsync(path);
                _snake = new Snake(_field);
                _gameOver = false;
                _currentScore = 0;
                _paused = false;

                if (path != _currentGameField)
                {
                    _highScore = 0; // az elért legjobb eredményt csak akkor nullázzuk, ha másik pályát töltünk be
                    _currentGameField = path; // ekkor a jelenlegi játékútvonalat is módosítjuk
                }

                GenerateEgg(); // tojást generálunk egy random pozícióra

                _snake.Crash += new EventHandler<SnakeEventArgs>(OnGameOver); // feliratkozik a modell a kígyó eseményeire
                _snake.EggConsumed += new EventHandler<SnakeEventArgs>(OnEggConsumed);
                _snake.PositionChange += new EventHandler<SnakeEventArgs>(OnSnakePositionChange);

                OnGameLoaded();
            }
            catch
            {
                throw new SnakeDataException();
            }
        }

        #endregion

        #region Private game methods

        /// <summary>
        /// Tojás pozíciójának generálása.
        /// </summary>
        private void GenerateEgg()
        {
            Random rand = new Random();
            Int32 x;
            Int32 y;

            do
            {
                x = rand.Next(1, _field.GameSizeX - 1); // vízszintes tengely
                y = rand.Next(1, _field.GameSizeY - 1); // függőleges tengely
            } while (_field[x, y] == FieldType.Wall || _field[x, y] == FieldType.Snake); // ha a generált pozíció helyén fal van, vagy a kígyó része, akkor újrageneráljuk a pozíciót

            _field.SetType(x, y, FieldType.Egg); // ekkor már biztosan üres mezőn vagyunk, tehát beállíthatjuk a mező típusát tojásra.
        }

        #endregion

        #region Private event triggers

        /// <summary>
        /// Játék vége eseményének kiváltása.
        /// </summary>
        private void OnGameOver(object? sender, SnakeEventArgs eventArgs)
        {
            if (eventArgs.EggCount > _highScore) _highScore = eventArgs.EggCount;
            GameIsOver = true;

            GameOver?.Invoke(this, new SnakeEventArgs(eventArgs.EggCount));
        }

        /// <summary>
        /// Tojás felfalásának eseményének kiváltása.
        /// </summary>
        private void OnEggConsumed(object? sender, SnakeEventArgs eventArgs)
        {
            _currentScore++;
            GenerateEgg(); // új tojást generálunk egy random pozícióra
            EggConsumed?.Invoke(this, eventArgs);
        }
        /// <summary>
        /// Kígyó pozícióváltásának eseményének kiváltása.
        /// </summary>
        private void OnSnakePositionChange(object? sender, SnakeEventArgs eventArgs)
        {
            SnakePositionChange?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Játék betöltésének eseményének kiváltása.
        /// </summary>
        private void OnGameLoaded()
        {
            GameLoaded?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
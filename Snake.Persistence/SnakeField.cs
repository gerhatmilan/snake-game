using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Persistance
{
    /// <summary>
    /// Snake játéktábla típusa.
    /// </summary>
    public class SnakeField
    {
        #region Fields

        private Int32 _gameSizeX;
        private Int32 _gameSizeY;
        private FieldType[,] _field;

        #endregion

        #region Properties

        /// <summary>
        /// Játéktábla magasságának lekérdezése.
        /// </summary>
        public Int32 GameSizeX { get { return _gameSizeX; } }

        /// <summary>
        /// Játéktábla hosszának lekérdezése.
        /// </summary>
        public Int32 GameSizeY { get { return _gameSizeY; } }

        /// <summary>
        /// Mező lekérdezése.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <returns>Az x-edik sor y-adik mezőjének típusa.</returns>
        public FieldType this[Int32 x, Int32 y] { get { return GetType(x, y); } }

        #endregion

        #region Constructors

        /// <summary>
        /// Snake játéktábla példányosítása.
        /// </summary>
        /// <param name="gameSizeX">Játéktábla magassága.</param>
        /// <param name="gameSizeY">Játéktábla hossza.</param>
        public SnakeField(Int32 gameSizeX, Int32 gameSizeY)
        {
            if (gameSizeX < 0)
                throw new ArgumentOutOfRangeException(nameof(gameSizeX), "The length of the game field is less than 0.");
            if (gameSizeY < 0)
                throw new ArgumentOutOfRangeException(nameof(gameSizeX), "The height of the game field is less than 0.");

            _gameSizeX = gameSizeX;
            _gameSizeY = gameSizeY;
            _field = new FieldType[gameSizeX, gameSizeY];
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Mező típusának lekérdezése.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <returns>A mező típusa.</returns>
        public FieldType GetType(Int32 x, Int32 y)
        {
            if (x < 0 || x >= _field.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= _field.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return _field[x, y];
        }

        /// <summary>
        /// Mező értékének beállítása.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <param name="type">Típus.</param>
        public void SetType(Int32 x, Int32 y, FieldType type)
        {
            if (x < 0 || x >= _field.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= _field.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            _field[x, y] = type;
        }

        #endregion
    }
}

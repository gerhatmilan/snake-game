using System;
using System.Threading.Tasks;

namespace Snake.Persistance
{
    /// <summary>
    /// Snake fájl kezelő felülete.
    /// </summary>
    public class SnakeFileDataAccess : ISnakeDataAccess
    {
        /// <summary>
        /// Fájl betöltése aszinkron módon.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        public async Task<SnakeField> LoadAsync(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    String line = await reader.ReadLineAsync() ?? String.Empty;
                    String[] numbers = line.Split(' '); // beolvasunk egy sort, és a szóköz mentén széttöredezzük
                    Int32 gameSizeX = Int32.Parse(numbers[0]); // tábla magassága
                    Int32 gameSizeY = Int32.Parse(numbers[1]); // tábla hossza
                    SnakeField field = new SnakeField(gameSizeX, gameSizeY); // létrehozzuk a táblát

                    for (Int32 i = 0; i < gameSizeX; i++)
                    {
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        numbers = line.Split(' ');

                        for (Int32 j = 0; j < gameSizeY; j++)
                        {
                            switch (numbers[j])
                            {
                                case "0":
                                    field.SetType(i, j, FieldType.Empty);
                                    break;
                                case "-1":
                                    field.SetType(i, j, FieldType.Wall);
                                    break;
                            }
                        }
                    }

                    return field;
                }
            }
            catch
            {
                throw new SnakeDataException();
            }
        }

        /// <summary>
        /// Fájl betöltése aszinkron módon.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        public SnakeField Load(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    String line = reader.ReadLine() ?? String.Empty;
                    String[] numbers = line.Split(' '); // beolvasunk egy sort, és a szóköz mentén széttöredezzük
                    Int32 gameSizeX = Int32.Parse(numbers[0]); // tábla magassága
                    Int32 gameSizeY = Int32.Parse(numbers[1]); // tábla hossza
                    SnakeField field = new SnakeField(gameSizeX, gameSizeY); // létrehozzuk a táblát

                    for (Int32 i = 0; i < gameSizeX; i++)
                    {
                        line = reader.ReadLine() ?? String.Empty;
                        numbers = line.Split(' ');

                        for (Int32 j = 0; j < gameSizeY; j++)
                        {
                            switch (numbers[j])
                            {
                                case "0":
                                    field.SetType(i, j, FieldType.Empty);
                                    break;
                                case "-1":
                                    field.SetType(i, j, FieldType.Wall);
                                    break;
                            }
                        }
                    }

                    return field;
                }
            }
            catch
            {
                throw new SnakeDataException();
            }
        }
    }
}
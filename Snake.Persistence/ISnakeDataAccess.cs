using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Persistance
{
    public interface ISnakeDataAccess
    {
        /// <summary>
        /// Fájl betöltése (aszinkron módon)
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        public Task<SnakeField> LoadAsync(String path);

        /// <summary>
        /// Fájl betöltése (szinkron módon)
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        public SnakeField Load(String path);
    }
}

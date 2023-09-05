using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Model
{
    public class SnakeEventArgs : EventArgs
    {
        private Int32 _consumedEggs;

        /// <summary>
        /// Megevett tojások számának lekérdezése.
        /// </summary>
        public Int32 EggCount { get { return _consumedEggs; } }

        /// <summary>
        /// Sudoku eseményargumentum példányosítása.
        /// </summary>
        /// <param name="eggCount">Megevett tojások.</param>
        /// 
        public SnakeEventArgs(Int32 eggCount)
        {
            _consumedEggs = eggCount;
        }
    }
}

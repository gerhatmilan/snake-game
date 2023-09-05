using Microsoft.VisualBasic.FileIO;
using Snake.Persistance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Snake.ViewModel
{
    public class SnakeField : ViewModelBase
    {
        private System.Windows.Media.Color _color;

        public Int32 X { get; set; }
        public Int32 Y { get; set; }

        public System.Windows.Media.Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (value != _color)
                {
                    _color = value;

                    OnPropertyChanged();
                }
            }
        }

        public SnakeField(Int32 x, Int32 y, System.Windows.Media.Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}

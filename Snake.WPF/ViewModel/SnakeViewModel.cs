using Microsoft.VisualBasic.FileIO;
using Snake.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Snake.ViewModel
{
    public class SnakeViewModel : ViewModelBase
    {
        #region Fields

        private SnakeGameModel _model;

        #endregion

        #region Properties
        public DelegateCommand RestartGameCommand { get; private set; }
        public DelegateCommand ChooseFieldCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }
        public DelegateCommand PauseContinueCommand { get; private set; }
        public DelegateCommand ChangeDirectionCommand { get; private set; }
        public ObservableCollection<SnakeField> Fields { get; set; }
        public Int32 GridSizeX { get { return _model.Field.GameSizeX - 2; } }
        public Int32 GridSizeY { get { return _model.Field.GameSizeY - 2; } }
        public Int32 CurrentScore { get { return _model.CurrentScore; } }
        public Int32 HighScore { get { return _model.HighScore; } }


        #endregion

        #region Events

        public event EventHandler? RestartGame;
        public event EventHandler<String>? ChooseField;
        public event EventHandler? ExitGame;
        public event EventHandler? PauseContinue;
        public event EventHandler<String>? ChangeDirection;

        #endregion

        #region Constructors

        public SnakeViewModel(SnakeGameModel model)
        {
            _model = model;
            _model.SnakePositionChange += new EventHandler<SnakeEventArgs>(Model_SnakePositionChange);
            _model.EggConsumed += new EventHandler<SnakeEventArgs>(Model_EggConsumed);
            _model.GameOver += new EventHandler<SnakeEventArgs>(Model_GameOver);
            _model.GameLoaded += new EventHandler(Model_GameLoaded);

            RestartGameCommand = new DelegateCommand(param => OnRestartGame());
            ChooseFieldCommand = new DelegateCommand(param => OnChooseField(param.ToString()));
            ExitGameCommand = new DelegateCommand(param => OnExitGame());
            PauseContinueCommand = new DelegateCommand(param => OnPauseContinue());
            ChangeDirectionCommand = new DelegateCommand(param => OnChangeDirection(param.ToString()));

            Fields = new ObservableCollection<SnakeField>();
            GenerateFields();
        }

        #endregion

        #region Game event handlers

        private void Model_SnakePositionChange(object? sender, SnakeEventArgs e)
        {
            RefreshFields();
        }

        private void Model_EggConsumed(Object? sender, SnakeEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentScore));
        }

        private void Model_GameOver(Object? sender, SnakeEventArgs e)
        {
            OnPropertyChanged(nameof(HighScore));
        }

        private void Model_GameLoaded(Object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(GridSizeX));
            OnPropertyChanged(nameof(GridSizeY));
            OnPropertyChanged(nameof(CurrentScore));
            OnPropertyChanged(nameof(HighScore));

            GenerateFields();
        }

        #endregion

        #region Event methods

        private void OnRestartGame()
        {
            RestartGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnChooseField(String param)
        {
            ChooseField?.Invoke(this, param);
        }
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnPauseContinue()
        {
            PauseContinue?.Invoke(this, EventArgs.Empty);
        }
        private void OnChangeDirection(String param)
        {
            ChangeDirection?.Invoke(this, param);
        }

        #endregion

        #region Private methods 

        private void RefreshFields()
        {
            // inicializáljuk a mezőket
            // 1-től megyünk GameSize - 1 -ig, a falak szélét nem akarjuk megjeleníteni (az ablak széle lesz a fal)
            for (Int32 i = 1; i < _model.Field.GameSizeX - 1; i++)
            {
                for (Int32 j = 1; j < _model.Field.GameSizeY - 1; j++)
                {
                    System.Windows.Media.Color color = Colors.White;
                    switch (_model.Field.GetType(i, j))
                    {
                        case Persistance.FieldType.Empty:
                            color = System.Windows.Media.Color.FromArgb(255, 1, 0, 23);
                            break;
                        case Persistance.FieldType.Snake:
                            color = Colors.Green;
                            break;
                        case Persistance.FieldType.Wall:
                            color = System.Windows.Media.Color.FromArgb(255, 1, 77, 254);
                            break;
                        case Persistance.FieldType.Egg:
                            color = Colors.Yellow;
                            break;
                    }

                    Fields[(i - 1) * GridSizeY + (j - 1)].Color = color;
                }
            }
        }
        
        public void GenerateFields()
        {
            Fields.Clear();

            // inicializáljuk a mezőket
            // 1-től megyünk GameSize - 1 -ig, a falak szélét nem akarjuk megjeleníteni (az ablak széle lesz a fal)
            for (Int32 i = 1; i < _model.Field.GameSizeX - 1; i++)
            {
                for (Int32 j = 1; j < _model.Field.GameSizeY - 1; j++)
                {

                    Fields.Add(new SnakeField(i - 1, j - 1, Colors.White));
                }
            }

            RefreshFields();
        }

        #endregion
    }
}

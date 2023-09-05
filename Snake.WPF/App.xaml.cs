using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Snake.Model;
using Snake.Persistance;
using Snake.View;
using Snake.ViewModel;

namespace Snake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private SnakeGameModel _model = null!;
        private SnakeViewModel _viewModel = null!;
        private MainWindow _view = null!;
        private DispatcherTimer _timer = null!;

        #endregion

        #region Constructors

        /// <summary>
        /// Alkalmazás példányosítása.
        /// </summary>
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handlers

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            // modell létrehozása
            _model = new SnakeGameModel(new SnakeFileDataAccess());
            _model.GameOver += new EventHandler<SnakeEventArgs>(Model_GameOver);
            _model.InitalizeGame();

            // nézemodell létrehozása
            _viewModel = new SnakeViewModel(_model);
            _viewModel.RestartGame += new EventHandler(ViewModel_RestartGame);
            _viewModel.ChooseField += new EventHandler<String>(ViewModel_ChooseField);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.PauseContinue += new EventHandler(ViewModel_PauseContinue);
            _viewModel.ChangeDirection += new EventHandler<String>(ViewModel_ChangeDirection);

            // nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer(DispatcherPriority.Send);
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += new EventHandler(Timer_Tick);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _model.Snake.Move();
        }

        #endregion

        #region View event handlers

        /// <summary>
        /// Nézet bezárásának eseménykezelője.
        /// </summary>
        private void View_Closing(object? sender, CancelEventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;
            _timer.Stop();

            // megkérdezzük, hogy biztos ki szeretne-e lépni
            if (MessageBox.Show("Are you sure?", "Snake Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást

                if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                    _timer.Start();
            }
        }

        #endregion

        #region ViewModel event handlers

        private async void ViewModel_RestartGame(object? sender, EventArgs e)
        {
            _timer.Stop();
            try
            {
                await _model.LoadGameAsync(_model.CurrentGameField);
            }
            catch (SnakeDataException)
            {
                if (MessageBox.Show("An error has occured while loading the game!" + Environment.NewLine
                    + "The field file might be missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    _view.Close();
                }
            }
        }

        private async void ViewModel_ChooseField(object? sender, String field)
        {
            _timer.Stop();
            try
            {
                await _model.LoadGameAsync($@"GameFields\GameField{field}.txt");
            }
            catch (SnakeDataException)
            {
                if (MessageBox.Show("An error has occured while loading the game!" + Environment.NewLine
                    + "The field file might be missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    _view.Close();
                }
            }
        }

        private void ViewModel_ExitGame(object? sender, System.EventArgs e)
        {
            _view.Close();
        }

        private void ViewModel_PauseContinue(object? sender, System.EventArgs e)
        {
            // ha még nem indult el a kígyó
            if (_model.Snake.CurrentDirection == Direction.None) return;

            // ha már elindult a kígyó, de még nem ütközött és jelenleg nem szünetel a játék
            if (!_model.GameIsOver && !_model.Paused)
            {
                _timer.Stop();
                _model.Paused = true;
                return;
            }

            // ha már elindult a kígyó, de még nem ütközött és jelenleg szünetel a játék
            if (!_model.GameIsOver && _model.Paused)
            {
                _timer.Start();
                _model.Paused = false;
                return;
            }
        }

        private void ViewModel_ChangeDirection(object? sender, String direction)
        {
            switch (direction) // megkapjuk a billentyűt
            {
                case "Left":
                    // ha a játék szünetel, nem történik semmi
                    if (_model.Paused) break;

                    // ha még nem ütközött a kígyó (játék eleje állapot)
                    if (!_model.GameIsOver)
                    {
                        // ha még nincs elindítva, elindítjuk a kígyót
                        if (!_timer.IsEnabled) _timer.Start();
                        _model.Snake.SwitchDirection(Direction.Left); // változik az irány
                    }
                    break;
                case "Right":
                    if (_model.Paused) break;
                    if (!_model.GameIsOver)
                    {
                        if (!_timer.IsEnabled) _timer.Start();
                        _model.Snake.SwitchDirection(Direction.Right);
                    }
                    break;
                case "Up":
                    if (_model.Paused) break;
                    if (!_model.GameIsOver)
                    {
                        if (!_timer.IsEnabled) _timer.Start();
                        _model.Snake.SwitchDirection(Direction.Up);
                    }
                    break;
                case "Down":
                    if (_model.Paused) break;
                    if (!_model.GameIsOver)
                    {
                        if (!_timer.IsEnabled) _timer.Start();
                        _model.Snake.SwitchDirection(Direction.Down);
                    }
                    break;
            }
        }

        #endregion

        #region Model event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private async void Model_GameOver(object? sender, SnakeEventArgs e)
        {
            _timer.Stop();

            MessageBoxResult result = MessageBox.Show($"The snake crashed!" + Environment.NewLine +
                $"Score: {e.EggCount}" + Environment.NewLine +
                "Do you want to try again?", "Snake", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _model.LoadGameAsync(_model.CurrentGameField);
                }
                catch (SnakeDataException)
                {
                    if (MessageBox.Show("An error has occured while loading the game!" + Environment.NewLine
                    + "The field file might be missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        _view.Close();
                    }
                }
            }
        }

        #endregion
    }
}

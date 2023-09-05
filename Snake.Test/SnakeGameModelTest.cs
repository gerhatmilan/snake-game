using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snake.Model;
using Snake.Persistance;
using Moq;
using System.ComponentModel;
using System.Reflection;

namespace Snake.Test
{
    [TestClass]
    public class SnakeGameModelTest
    {

        private SnakeGameModel _model = null!; // a tesztelend� modell
        private SnakeField _mockedField = null!; // mockolt j�t�kmez�
        private Mock<ISnakeDataAccess> _mock = null!; // az adatel�r�s mock-ja

        // ez a met�dusra az�rt van sz�ks�g, hogy ha egy tesztesetben egym�s ut�n t�lten�nk be p�ly�kat (amiken k�zben m�dos�tutunk is), ne v�ltozzon
        // az eredeti _mockedField p�ld�ny
        private SnakeField Copy(SnakeField fieldToCopy)
        {
            SnakeField copyField = new SnakeField(fieldToCopy.GameSizeX, fieldToCopy.GameSizeY);

            for (int i = 0; i < fieldToCopy.GameSizeX; i++)
            {
                for (int j = 0; j < fieldToCopy.GameSizeY; j++)
                {
                    copyField.SetType(i, j, fieldToCopy.GetType(i, j));
                }
            }

            return copyField;
        }

        [TestInitialize]
        public void Initialize()
        {
            // a tesztelend� modell p�lya 11x11-es, a p�ly�k sz�lei falak
            _mockedField = new SnakeField(11, 11);
            for (int i = 0; i < _mockedField.GameSizeX; i++)
            {
                for (int j = 0; j < _mockedField.GameSizeY; j++)
                {
                    if (i == 0 || i == _mockedField.GameSizeX - 1 || j == 0 || j == _mockedField.GameSizeY - 1)
                    {
                        _mockedField.SetType(i, j, FieldType.Wall);
                    }
                    else
                    {
                        _mockedField.SetType(i, j, FieldType.Empty);
                    }
                }
            }

            _mock = new Mock<ISnakeDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(Copy(_mockedField)));
            _mock.Setup(mock => mock.Load(It.IsAny<String>()))
                .Returns(() => Copy(_mockedField));
            // a mock a LoadAsync �s Load m�veletben b�rmilyen param�terre a fenti j�t�kt�bl�t fogja visszaadni

            _model = new SnakeGameModel(_mock.Object);
            // p�ld�nyos�tjuk a modellt a mock objektummal

            _model.GameOver += new EventHandler<SnakeEventArgs>(Model_GameOver);
            _model.EggConsumed += new EventHandler<SnakeEventArgs>(Model_EggConsumed);
        }

        #region Model object tests

        [TestMethod]
        public void SnakeGameModelInitializeGameTest()
        {
            _model.InitalizeGame();

            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (i == _model.Field.GameSizeX / 2 && (j == _model.Field.GameSizeY / 2 || j == _model.Field.GameSizeY / 2 - 1
                        || j == _model.Field.GameSizeY / 2 - 2 || j == _model.Field.GameSizeY / 2 - 3 || j == _model.Field.GameSizeY / 2 - 4))
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a k�z�ps� cella �s a t�le balra lev� 4 cella t�pusa k�gy� kell legyen, hiszen inicializ�l�sn�l a k�gy� legener�l�dott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelel� mez��rt�kek �ll�t�dnak be (egy mez� kiv�tel�vel, ahova m�r toj�s gener�l�dott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag p�ld�nyosul
            Assert.AreEqual(@"GameFields\GameField15x15.txt", _model.CurrentGameField); // alap�rtelmezett p�lya 15x15
            Assert.AreEqual(0, _model.CurrentScore);
            Assert.AreEqual(0, _model.HighScore);
            Assert.AreEqual(false, _model.GameIsOver);

            Int32 eggCount = 0;
            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (_model.Field.GetType(i, j) == FieldType.Egg)
                    {
                        eggCount++;
                    }
                }
            }

            Assert.AreEqual(1, eggCount); // pontosan egy darab toj�s gener�l�dott
        }

        [TestMethod]
        public async Task SnakeGameModelLoadGameAsyncWhenLoadingDifferentField()
        {
            _model.InitalizeGame(); // el�sz�r bet�ltj�k az alap�rtelmezett p�ly�t, ekkor az aktu�lis p�lya a 15x15-�s lesz
            _model.HighScore = 2; // ezzel azt szimul�ljuk hogy m�r v�gigment a j�t�k legal�bb egyszer �s az eddigi legmagasabb pontsz�m 2

            await _model.LoadGameAsync(@"GameFields\GameField19x19.txt"); // bet�lt�nk egy 15x15-t�l elt�r� p�ly�t, ekkor a high scorenak null�z�dnia kell, valamint az el�r�si �t is v�ltozik

            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (i == _model.Field.GameSizeX / 2 && (j == _model.Field.GameSizeY / 2 || j == _model.Field.GameSizeY / 2 - 1
                        || j == _model.Field.GameSizeY / 2 - 2 || j == _model.Field.GameSizeY / 2 - 3 || j == _model.Field.GameSizeY / 2 - 4))
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a k�z�ps� cella �s a t�le balra lev� 4 cella t�pusa k�gy� kell legyen, hiszen inicializ�l�sn�l a k�gy� legener�l�dott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelel� mez��rt�kek �ll�t�dnak be (egy mez� kiv�tel�vel, ahova m�r toj�s gener�l�dott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag p�ld�nyosul
            Assert.AreEqual(@"GameFields\GameField19x19.txt", _model.CurrentGameField);
            Assert.AreEqual(0, _model.CurrentScore);
            Assert.AreEqual(0, _model.HighScore); // high score null�z�dik �j p�lya bet�lt�s�n�l
            Assert.AreEqual(false, _model.GameIsOver);


            Int32 eggCount = 0;
            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (_model.Field.GetType(i, j) == FieldType.Egg)
                    {
                        eggCount++;
                    }
                }
            }

            Assert.AreEqual(1, eggCount); // pontosan egy darab toj�s gener�l�dott
        }

        [TestMethod]
        public async Task SnakeGameModelLoadGameAsyncWhenLoadingSameField()
        {
            _model.InitalizeGame(); // el�sz�r bet�ltj�k az alap�rtelmezett p�ly�t, ekkor az aktu�lis p�lya a 15x15-�s lesz

            _model.HighScore = 2; // ezzel azt szimul�ljuk hogy m�r v�gigment a j�t�k legal�bb egyszer �s az eddigi legmagasabb pontsz�m 2
         
            await _model.LoadGameAsync(@"GameFields\GameField15x15.txt"); // bet�ltj�k a 15x15-�s p�ly�t �jra, ekkor a high score nem null�z�dik, az el�r�si �t nem v�ltozik

            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (i == _model.Field.GameSizeX / 2 && (j == _model.Field.GameSizeY / 2 || j == _model.Field.GameSizeY / 2 - 1
                        || j == _model.Field.GameSizeY / 2 - 2 || j == _model.Field.GameSizeY / 2 - 3 || j == _model.Field.GameSizeY / 2 - 4))
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a k�z�ps� cella �s a t�le balra lev� 4 cella t�pusa k�gy� kell legyen, hiszen inicializ�l�sn�l a k�gy� legener�l�dott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelel� mez��rt�kek �ll�t�dnak be (egy mez� kiv�tel�vel, ahova m�r toj�s gener�l�dott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag p�ld�nyosul
            Assert.AreEqual(@"GameFields\GameField15x15.txt", _model.CurrentGameField);
            Assert.AreEqual(0, _model.CurrentScore);
            Assert.AreEqual(2, _model.HighScore); // a legmagasabb el�rt pontsz�m �jraind�t�s ut�n is megmarad
            Assert.AreEqual(false, _model.GameIsOver);

            Int32 eggCount = 0;
            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (_model.Field.GetType(i, j) == FieldType.Egg)
                    {
                        eggCount++;
                    }
                }
            }

            Assert.AreEqual(1, eggCount); // pontosan egy darab toj�s gener�l�dott
        }

        #endregion

        #region Snake object tests

        [TestMethod]
        public void SnakeSwitchDirectionTest()
        {
            _model.InitalizeGame(); // inicializ�ljuk a j�t�kot, ezzel a k�gy�t is
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.None); // kezdetben a k�gy� "nem megy semerre"

            _model.Snake.SwitchDirection(Direction.Left);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.None); // balra nem tud elindulni

            _model.Snake.SwitchDirection(Direction.Right);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // az ir�ny megv�ltozott

            _model.Snake.SwitchDirection(Direction.Right);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // ha ugyan arra akar ir�nyt v�ltani, nincs v�ltoz�s

            _model.Snake.SwitchDirection(Direction.Left);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // 180 fokban nem tud megfordulni
        }

        [TestMethod]
        public void SnakeMoveToEmptyFieldTest()
        {
            _model.InitalizeGame(); // inicializ�ljuk a j�t�kot, ezzel a k�gy�t is

            _model.Snake.Move();
            Assert.IsTrue(_model.Snake.Head == (_model.Field.GameSizeX / 2, _model.Field.GameSizeY / 2)); // mivel nincs be�ll�tva ir�ny, a k�gy� nem mozdult semerre (a feje poz�ci�ja m�g mindig a j�t�kp�lya k�zepe)

            _model.Snake.SwitchDirection(Direction.Right);
            _model.Snake.Move();

            // a k�gy� egyel jobbra mozdult
            Assert.IsTrue(_model.Snake.Head == (_model.Field.GameSizeX / 2, _model.Field.GameSizeY / 2 + 1)); // a k�gy� feje egyel jobbra mozdult
            // mivel a k�gy� kezdetben 5 hossz�, a p�lya 11x11-es, a k�gy� v�g�nek a poz�ci�ja kezdetben v�zszintesen a p�lya k�zepe, f�gg�legesen a 1. oszlop (a 0. oszlop fal) 
            Assert.IsTrue(_model.Snake.Body.Last!.Value == (_model.Field.GameSizeX / 2, 2)); // a k�gy� v�ge mozdul�s ut�n m�r a 2. oszlopban lesz
        }

        [TestMethod]
        public void SnakeMoveToEggField()
        {
            _model.InitalizeGame(); // inicializ�ljuk a j�t�kot, ezzel a k�gy�t is

            // a tesztel�shez a k�gy�t�l egyel jobbra l�v� mez�t be�ll�tjuk toj�sra
            _model.Field.SetType(_model.Snake.Head.x, _model.Snake.Head.y + 1, FieldType.Egg);
            _model.Snake.SwitchDirection(Direction.Right);
            _model.Snake.Move();

            Assert.AreEqual(1, _model.CurrentScore); // n� a pontsz�m egyel
        }

        #endregion

        private void Model_GameOver(Object? sender, SnakeEventArgs e)
        {
            Assert.IsTrue(_model.GameIsOver);
            Assert.IsTrue(_model.HighScore >= _model.CurrentScore);

            // a k�gy� ir�ny�t�l f�gg�en a szomsz�dos mez� vagy fal, vagy a k�gy� r�sze
            switch (_model.Snake.CurrentDirection)
            {
                case Direction.Left:
                    Assert.IsTrue(_model.Field.GetType(_model.Snake.Head.x, _model.Snake.Head.y - 1) == FieldType.Wall
                        || _model.Field.GetType(_model.Snake.Head.x, _model.Snake.Head.y - 1) == FieldType.Snake);
                    break;
                case Direction.Right:
                    Assert.IsTrue(_model.Field.GetType(_model.Snake.Head.x, _model.Snake.Head.y + 1) == FieldType.Wall
                        || _model.Field.GetType(_model.Snake.Head.x, _model.Snake.Head.y + 1) == FieldType.Snake);
                    break;
                case Direction.Up:
                    Assert.IsTrue(_model.Field.GetType(_model.Snake.Head.x - 1, _model.Snake.Head.y) == FieldType.Wall
                        || _model.Field.GetType(_model.Snake.Head.x - 1, _model.Snake.Head.y) == FieldType.Snake);
                    break;
                case Direction.Down:
                    Assert.IsTrue(_model.Field.GetType(_model.Snake.Head.x + 1, _model.Snake.Head.y) == FieldType.Wall
                        || _model.Field.GetType(_model.Snake.Head.x + 1, _model.Snake.Head.y) == FieldType.Snake);
                    break;
            }
        }

        private void Model_EggConsumed(Object? sender, SnakeEventArgs e)
        {
            Assert.AreEqual(e.EggCount + 1, _model.CurrentScore); // n�tt a pontsz�m egyel az el�z�h�z k�pest
        }
    }
}
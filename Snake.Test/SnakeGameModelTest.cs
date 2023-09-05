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

        private SnakeGameModel _model = null!; // a tesztelendõ modell
        private SnakeField _mockedField = null!; // mockolt játékmezõ
        private Mock<ISnakeDataAccess> _mock = null!; // az adatelérés mock-ja

        // ez a metódusra azért van szükség, hogy ha egy tesztesetben egymás után töltenénk be pályákat (amiken közben módosítutunk is), ne változzon
        // az eredeti _mockedField példány
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
            // a tesztelendõ modell pálya 11x11-es, a pályák szélei falak
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
            // a mock a LoadAsync és Load mûveletben bármilyen paraméterre a fenti játéktáblát fogja visszaadni

            _model = new SnakeGameModel(_mock.Object);
            // példányosítjuk a modellt a mock objektummal

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
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a középsõ cella és a tõle balra levõ 4 cella típusa kígyó kell legyen, hiszen inicializálásnál a kígyó legenerálódott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelelõ mezõértékek állítódnak be (egy mezõ kivételével, ahova már tojás generálódott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag példányosul
            Assert.AreEqual(@"GameFields\GameField15x15.txt", _model.CurrentGameField); // alapértelmezett pálya 15x15
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

            Assert.AreEqual(1, eggCount); // pontosan egy darab tojás generálódott
        }

        [TestMethod]
        public async Task SnakeGameModelLoadGameAsyncWhenLoadingDifferentField()
        {
            _model.InitalizeGame(); // elõször betöltjük az alapértelmezett pályát, ekkor az aktuális pálya a 15x15-ös lesz
            _model.HighScore = 2; // ezzel azt szimuláljuk hogy már végigment a játék legalább egyszer és az eddigi legmagasabb pontszám 2

            await _model.LoadGameAsync(@"GameFields\GameField19x19.txt"); // betöltünk egy 15x15-tõl eltérõ pályát, ekkor a high scorenak nullázódnia kell, valamint az elérési út is változik

            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (i == _model.Field.GameSizeX / 2 && (j == _model.Field.GameSizeY / 2 || j == _model.Field.GameSizeY / 2 - 1
                        || j == _model.Field.GameSizeY / 2 - 2 || j == _model.Field.GameSizeY / 2 - 3 || j == _model.Field.GameSizeY / 2 - 4))
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a középsõ cella és a tõle balra levõ 4 cella típusa kígyó kell legyen, hiszen inicializálásnál a kígyó legenerálódott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelelõ mezõértékek állítódnak be (egy mezõ kivételével, ahova már tojás generálódott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag példányosul
            Assert.AreEqual(@"GameFields\GameField19x19.txt", _model.CurrentGameField);
            Assert.AreEqual(0, _model.CurrentScore);
            Assert.AreEqual(0, _model.HighScore); // high score nullázódik új pálya betöltésénél
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

            Assert.AreEqual(1, eggCount); // pontosan egy darab tojás generálódott
        }

        [TestMethod]
        public async Task SnakeGameModelLoadGameAsyncWhenLoadingSameField()
        {
            _model.InitalizeGame(); // elõször betöltjük az alapértelmezett pályát, ekkor az aktuális pálya a 15x15-ös lesz

            _model.HighScore = 2; // ezzel azt szimuláljuk hogy már végigment a játék legalább egyszer és az eddigi legmagasabb pontszám 2
         
            await _model.LoadGameAsync(@"GameFields\GameField15x15.txt"); // betöltjük a 15x15-ös pályát újra, ekkor a high score nem nullázódik, az elérési út nem változik

            for (int i = 0; i < _model.Field.GameSizeX; i++)
            {
                for (int j = 0; j < _model.Field.GameSizeY; j++)
                {
                    if (i == _model.Field.GameSizeX / 2 && (j == _model.Field.GameSizeY / 2 || j == _model.Field.GameSizeY / 2 - 1
                        || j == _model.Field.GameSizeY / 2 - 2 || j == _model.Field.GameSizeY / 2 - 3 || j == _model.Field.GameSizeY / 2 - 4))
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == FieldType.Snake); // a középsõ cella és a tõle balra levõ 4 cella típusa kígyó kell legyen, hiszen inicializálásnál a kígyó legenerálódott
                    }
                    else
                    {
                        Assert.IsTrue(_model.Field.GetType(i, j) == _mockedField.GetType(i, j) || _model.Field.GetType(i, j) == FieldType.Egg); // a megfelelõ mezõértékek állítódnak be (egy mezõ kivételével, ahova már tojás generálódott)

                    }
                }
            }

            Assert.AreNotEqual(null, _model.Snake); // _snake adattag példányosul
            Assert.AreEqual(@"GameFields\GameField15x15.txt", _model.CurrentGameField);
            Assert.AreEqual(0, _model.CurrentScore);
            Assert.AreEqual(2, _model.HighScore); // a legmagasabb elért pontszám újraindítás után is megmarad
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

            Assert.AreEqual(1, eggCount); // pontosan egy darab tojás generálódott
        }

        #endregion

        #region Snake object tests

        [TestMethod]
        public void SnakeSwitchDirectionTest()
        {
            _model.InitalizeGame(); // inicializáljuk a játékot, ezzel a kígyót is
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.None); // kezdetben a kígyó "nem megy semerre"

            _model.Snake.SwitchDirection(Direction.Left);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.None); // balra nem tud elindulni

            _model.Snake.SwitchDirection(Direction.Right);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // az irány megváltozott

            _model.Snake.SwitchDirection(Direction.Right);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // ha ugyan arra akar irányt váltani, nincs változás

            _model.Snake.SwitchDirection(Direction.Left);
            Assert.IsTrue(_model.Snake.CurrentDirection == Direction.Right); // 180 fokban nem tud megfordulni
        }

        [TestMethod]
        public void SnakeMoveToEmptyFieldTest()
        {
            _model.InitalizeGame(); // inicializáljuk a játékot, ezzel a kígyót is

            _model.Snake.Move();
            Assert.IsTrue(_model.Snake.Head == (_model.Field.GameSizeX / 2, _model.Field.GameSizeY / 2)); // mivel nincs beállítva irány, a kígyó nem mozdult semerre (a feje pozíciója még mindig a játékpálya közepe)

            _model.Snake.SwitchDirection(Direction.Right);
            _model.Snake.Move();

            // a kígyó egyel jobbra mozdult
            Assert.IsTrue(_model.Snake.Head == (_model.Field.GameSizeX / 2, _model.Field.GameSizeY / 2 + 1)); // a kígyó feje egyel jobbra mozdult
            // mivel a kígyó kezdetben 5 hosszú, a pálya 11x11-es, a kígyó végének a pozíciója kezdetben vízszintesen a pálya közepe, függõlegesen a 1. oszlop (a 0. oszlop fal) 
            Assert.IsTrue(_model.Snake.Body.Last!.Value == (_model.Field.GameSizeX / 2, 2)); // a kígyó vége mozdulás után már a 2. oszlopban lesz
        }

        [TestMethod]
        public void SnakeMoveToEggField()
        {
            _model.InitalizeGame(); // inicializáljuk a játékot, ezzel a kígyót is

            // a teszteléshez a kígyótól egyel jobbra lévõ mezõt beállítjuk tojásra
            _model.Field.SetType(_model.Snake.Head.x, _model.Snake.Head.y + 1, FieldType.Egg);
            _model.Snake.SwitchDirection(Direction.Right);
            _model.Snake.Move();

            Assert.AreEqual(1, _model.CurrentScore); // nõ a pontszám egyel
        }

        #endregion

        private void Model_GameOver(Object? sender, SnakeEventArgs e)
        {
            Assert.IsTrue(_model.GameIsOver);
            Assert.IsTrue(_model.HighScore >= _model.CurrentScore);

            // a kígyó irányától függõen a szomszédos mezõ vagy fal, vagy a kígyó része
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
            Assert.AreEqual(e.EggCount + 1, _model.CurrentScore); // nõtt a pontszám egyel az elõzõhöz képest
        }
    }
}
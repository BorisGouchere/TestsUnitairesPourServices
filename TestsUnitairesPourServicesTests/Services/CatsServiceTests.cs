using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Models;
using TestsUnitairesPourServices.Exceptions;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        ApplicationDBContext _db;

        private const int CAT_HOUSE_ID = 1;
        private const int CAT_WITHOUT_HOUSE_ID = 2;
        private const int FIRST_HOUSE_ID = 1;
        private const int SECOND_HOUSE_ID = 2;

        [TestInitialize]
        public void Init()
        {
            string dbName = Guid.NewGuid().ToString();
            DbContextOptions<ApplicationDBContext> options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .UseLazyLoadingProxies(true)
                .Options;
            _db = new ApplicationDBContext(options);
            // Ajout des données de tests

            House firstHouse = new House
            {
                Id = FIRST_HOUSE_ID,
                OwnerName = "Proprio",
                Address = "130 Rue des Chats"
            };

            House secondHouse = new House()
            {
                Id = SECOND_HOUSE_ID,
                OwnerName = "Propio2",
                Address = "240 Rue des chats"
            };
            _db.Add(firstHouse);
            _db.Add(secondHouse);
            _db.SaveChanges();

            Cat catWithHouse = new Cat()
            {
                Id = CAT_HOUSE_ID,
                Name = "Chat Dragon",
                Age = 30,
                House = firstHouse
            };

            Cat catWithoutHouse = new Cat()
            {
                Id = CAT_WITHOUT_HOUSE_ID,
                Name = "Chat Guerrier",
                Age = 15
            };
            _db.Add(catWithHouse);
            _db.Add(catWithoutHouse);
            _db.SaveChanges();

        }

        [TestCleanup]
        public void Dispose()
        {
            _db.Dispose();
        }

        [TestMethod()]
        public void MoveTest()
        {
            var catsService = new CatsService(_db);
            var cat = _db.Cat.Find(CAT_HOUSE_ID);
            var house = _db.House.Find(FIRST_HOUSE_ID);
            var houseToMove = _db.House.Find(SECOND_HOUSE_ID);
            var catMove = catsService.Move(cat.Id, house, houseToMove);
            Assert.IsNotNull(catMove);
        }

        [TestMethod]
        public void MoveNotFoundCatTest() 
        {
            var catsService = new CatsService(_db);
            var house = _db.House.Find(FIRST_HOUSE_ID);
            var houseTomove = _db.House.Find(SECOND_HOUSE_ID);
            Assert.IsNull(catsService.Move(5, house, houseTomove));
        }

        [TestMethod]
        public void MoveCatWithoutHouseTest() 
        {
            var catsService = new CatsService(_db);
            var house = _db.House.Find(FIRST_HOUSE_ID);
            var houseTomove = _db.House.Find(SECOND_HOUSE_ID);
            Exception e = Assert.ThrowsException<WildCatException>(() => catsService.Move(CAT_WITHOUT_HOUSE_ID, house, houseTomove));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);
        }

        [TestMethod]
        public void MoveCatWrongHouseTest() 
        {
            var catsService = new CatsService(_db);
            var house = _db.House.Find(FIRST_HOUSE_ID);
            var houseTomove = _db.House.Find(SECOND_HOUSE_ID);
            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => catsService.Move(CAT_HOUSE_ID, houseTomove, house));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);
        }
    }
}
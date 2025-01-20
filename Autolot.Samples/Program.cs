using System;
using Microsoft.Data.SqlClient;

namespace AutoLot.Samples;

class Program{

    //var context = new ApplicationDbContextFactory().CreateDbContext(null);
    //context.Makes.Add(new Make { Name = "Lemon" });
    //context.SaveChanges();

    static void Main(string[] args){
        Console.WriteLine("More Fun with Entity Framework Core");
        
        ClearSampleData();
        AddRecords();
        Console.WriteLine("\n");
        ClearSampleData();
        LoadMakeAndCarData();
        Console.WriteLine("\n");
        QueryData();
        Console.WriteLine("\n");
        FilterData();
        Console.WriteLine("\n");
        SortData();
        Console.WriteLine("\n");
        Paging();
        Console.WriteLine("\n");
        SingleRecordQueries();
        Console.WriteLine("\n");
        Aggregation();
        Console.WriteLine("\n");
        AnyAndAll();
        Console.WriteLine("\n");
        GetDataFromStoredProcedure();
        Console.WriteLine("\n");
        RelatedData();
        Console.WriteLine("\n");
        UpdateRecords();
        Console.WriteLine("\n");
        DeleteRecords();
        Console.WriteLine("\n");
        QueryFilter();
        Console.WriteLine("\n");
        RelatedDataWithQueryFilter();
        Console.WriteLine("\n");
        UsingSqlRaw();
        Console.WriteLine("\n");
        Projections();
        Console.WriteLine("\n");
        AddCar();
        Console.WriteLine("\n");
        //ClearSampleData();
        //AddCarWithDefaultsSet();
        //Console.WriteLine("\n");
        //UpdateACar();
        //Console.WriteLine("\n");
        //UseCompiledDbContext();

    }

            

static void ClearSampleData()
{
    //The factory is not meant to be used like this, but it’s demo code :-)
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var entities = new[]
    {
        typeof(Driver).FullName,
        typeof(Car).FullName,
        typeof(Make).FullName,
    };
    /*var serviceCollection = new ServiceCollection();
    //serviceCollection.AddEntityFrameworkDesignTimeServices();
    serviceCollection.AddDbContextDesignTimeServices(context);
    var serviceProvider = serviceCollection.BuildServiceProvider();
    var designTimeModel = serviceProvider.GetService<IModel>();*/


    foreach (var entityName in entities)
    {
        var entity = context.Model.FindEntityType(entityName);
        var tableName = entity.GetTableName();
        var schemaName = entity.GetSchema();
        context.Database.ExecuteSqlRaw($"DELETE FROM {schemaName}.{tableName}");
        context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (\"{schemaName}.{tableName}\", RESEED, 0);");
        /*if (entity.IsTemporal())
        {
            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var trans = context.Database.BeginTransaction();
                var designTimeEntity = designTimeModel.FindEntityType(entityName);
                var historySchema = designTimeEntity.GetHistoryTableSchema();
                var historyTable = designTimeEntity.GetHistoryTableName();
                context.Database.ExecuteSqlRaw($"ALTER TABLE {schemaName}.{tableName} SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"DELETE FROM {historySchema}.{historyTable}");
                context.Database.ExecuteSqlRaw($"ALTER TABLE {schemaName}.{tableName} SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE={historySchema}.{historyTable}))");
                trans.Commit();
            });
        }*/
    }
}
static void AddRecords()
{
    //The factory is not meant to be used like this, but it’s demo code :-)
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var newMake = new Make
    {
        Name = "BMW"
    };
    Console.WriteLine($"State of the {newMake.Name} is {context.Entry(newMake).State}");
    context.Makes.Add(newMake);
    Console.WriteLine($"State of the {newMake.Name} is {context.Entry(newMake).State}");
    context.SaveChanges();
    Console.WriteLine($"State of the {newMake.Name} is {context.Entry(newMake).State}");
    var newCar = new Car()
    {
        Color = "Blue",
        DateBuilt = new DateTime(2016, 12, 01),
        IsDrivable = true,
        PetName = "Bluesmobile",
        MakeId = newMake.Id
    };
    Console.WriteLine($"State of the {newCar.PetName} is {context.Entry(newCar).State}");
    context.Cars.Attach(newCar);
    Console.WriteLine($"State of the {newCar.PetName} is {context.Entry(newCar).State}");
    context.SaveChanges();
    Console.WriteLine($"State of the {newCar.PetName} is {context.Entry(newCar).State}");

    var cars = new List<Car>
    {
        new() { Color = "Yellow", MakeId = newMake.Id, PetName = "Herbie" },
        new() { Color = "White", MakeId = newMake.Id, PetName = "Mach 5" },
        new() { Color = "Pink", MakeId = newMake.Id, PetName = "Avon" },
        new() { Color = "Blue", MakeId = newMake.Id, PetName = "Blueberry" },
    };
    context.Cars.AddRange(cars);
    context.SaveChanges();
    IEntityType metadata = context.Model.FindEntityType(typeof(Car).FullName);
    var schema = metadata.GetSchema();
    var tableName = metadata.GetTableName();

    var strategy = context.Database.CreateExecutionStrategy();
    strategy.Execute(() =>
    {
        using var trans = context.Database.BeginTransaction();
        try
        {
            context.Database.ExecuteSqlRaw(
                $"SET IDENTITY_INSERT {schema}.{tableName} ON");
            var anotherNewCar = new Car()
            {
                Id = 27,
                Color = "Blue",
                DateBuilt = new DateTime(2016, 12, 01),
                IsDrivable = true,
                PetName = "Bluesmobile",
                MakeId = newMake.Id
            };
            context.Cars.Add(anotherNewCar);
            context.SaveChanges();
            trans.Commit();
            Console.WriteLine($"Insert succeeded");
        }
        catch (Exception ex)
        {
            trans.Rollback();
            Console.WriteLine($"Insert failed: {ex.Message}");
        }
        finally
        {
            context.Database.ExecuteSqlRaw(
                $"SET IDENTITY_INSERT {schema}.{tableName} OFF");
        }
    });
    //Object graph
    var anotherMake = new Make { Name = "Honda" };
    var car = new Car { Color = "Yellow", MakeId = newMake.Id, PetName = "Herbie" };
    //Cast the Cars property to List<Car> from IEnumerable<Car>
    ((List<Car>)anotherMake.Cars).Add(car);
    context.Makes.Add(anotherMake);
    context.SaveChanges();
    //M2M
    context.ChangeTracker.Clear();
    ClearSampleData();
    LoadMakeAndCarData();
    var drivers = new List<Driver>
    {
        new() { PersonInfo = new Person { FirstName = "Fred", LastName = "Flinstone" } },
        new() { PersonInfo = new Person { FirstName = "Wilma", LastName = "Flinstone" } },
        new() { PersonInfo = new Person { FirstName = "BamBam", LastName = "Flinstone" } },
        new() { PersonInfo = new Person { FirstName = "Barney", LastName = "Rubble" } },
        new() { PersonInfo = new Person { FirstName = "Betty", LastName = "Rubble" } },
        new() { PersonInfo = new Person { FirstName = "Pebbles", LastName = "Rubble" } }
    };
    var carsForM2M = context.Cars.Take(2).ToList();
    //Cast the IEnumerable to a List to access the Add method
    //Range support works with LINQ to Objects, but is not translatable to SQL calls
    ((List<Driver>)carsForM2M[0].Drivers).AddRange(drivers.Take(..3));
    ((List<Driver>)carsForM2M[1].Drivers).AddRange(drivers.Take(3..));
    context.SaveChanges();

}
static void LoadMakeAndCarData()
{
    //The factory is not meant to be used like this, but it’s demo code :-)
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    List<Make> makes = new()
    {
        new() { Name = "VW" },
        new() { Name = "Ford" },
        new() { Name = "Saab" },
        new() { Name = "Yugo" },
        new() { Name = "BMW" },
        new() { Name = "Pinto" },
    };
    context.Makes.AddRange(makes);
    context.SaveChanges();

    List<Car> inventory = new()
    {
        new() { MakeId = 1, Color = "Black", PetName = "Zippy" },
        new() { MakeId = 2, Color = "Rust", PetName = "Rusty" },
        new() { MakeId = 3, Color = "Black", PetName = "Mel" },
        new() { MakeId = 4, Color = "Yellow", PetName = "Clunker" },
        new() { MakeId = 5, Color = "Black", PetName = "Bimmer" },
        new() { MakeId = 5, Color = "Green", PetName = "Hank" },
        new() { MakeId = 5, Color = "Pink", PetName = "Pinky" },
        new() { MakeId = 6, Color = "Black", PetName = "Pete" },
        new() { MakeId = 4, Color = "Brown", PetName = "Brownie" },
        new() { MakeId = 1, Color = "Rust", PetName = "Lemon", IsDrivable = false },
    };
    context.Cars.AddRange(inventory);
    context.SaveChanges();
    //context.Cars.Add(new() { MakeId = 1, Color = "Rust", PetName = "Lemon", IsDrivable = false});
    //context.SaveChanges();
}

static void QueryData(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    IQueryable<Car> cars = context.Cars;
    foreach(Car c in cars){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();
    context.ChangeTracker.Clear();
    List<Car> cars2 = context.Cars.ToList();
    foreach(Car c in cars2){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();
}

static void FilterData(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    IQueryable<Car> cars = context.Cars.Where(c => c.Color == "Yellow");
    Console.WriteLine("Yellow Cars");
    foreach(Car c in cars){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    context.ChangeTracker.Clear();
    Console.WriteLine();

    IQueryable<Car> cars1 = context.Cars.Where(c => c.Color == "Yellow" && c.PetName == "Clunker");
    Console.WriteLine("Yellow cars and clunkers");
    foreach(Car c in cars1){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars2 = context.Cars.Where(c => c.Color == "Yellow").Where(c => c.PetName == "Clunker");
    Console.WriteLine("Yellow cars and clunkers");
    foreach(Car c in cars2){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars3 = context.Cars.Where(c => c.Color == "Yellow" || c.PetName == "Clunker");
    Console.WriteLine("Yellow cars or clunkers");
    foreach(Car c in cars3){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars4 = context.Cars.Where(c => !string.IsNullOrWhiteSpace(c.Color));
    Console.WriteLine("Cars with colors");
    foreach(Car c in cars4){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();
}

static void SortData(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    IQueryable<Car> cars = context.Cars.OrderBy(c => c.Color);
    Console.WriteLine("Cars ordered by color.");
    foreach(Car c in cars){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    context.ChangeTracker.Clear();
    Console.WriteLine();

    IQueryable<Car> cars1 = context.Cars.OrderBy(c => c.Color).ThenBy(c => c.PetName);
    Console.WriteLine("Cars ordered by color then by pet name.");
    foreach(Car c in cars1){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars2 = context.Cars.OrderByDescending(c => c.Color);
    Console.WriteLine("Cars ordered by color descending.");
    foreach(Car c in cars2){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars3 = context.Cars.OrderBy(c => c.Color).ThenByDescending(c => c.PetName);
    Console.WriteLine("Cars ordered by color then Petname descending.");
    foreach(Car c in cars3){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();

    IQueryable<Car> cars4 = context.Cars.OrderBy(c => c.Color).ThenBy(c => c.PetName).Reverse();
    Console.WriteLine("Cars ordered by color then by pet name in reverse.");
    foreach(Car c in cars4){
        Console.WriteLine($"{c.PetName} is {c.Color}");
    }
    Console.WriteLine();
}

static void Paging(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    Console.WriteLine("Paging");
    context.Cars.Skip(2).ToList();
    context.ChangeTracker.Clear();
    context.Cars.Take(2).ToList();
    context.ChangeTracker.Clear();
    context.Cars.Skip(2).Take(2).ToList();
}

static void SingleRecordQueries(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    Console.WriteLine("Single Record with Database sort");
    var firstCar = context.Cars.First();
    Console.WriteLine($"{firstCar.PetName} is {firstCar.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();


    Console.WriteLine("Single Record with OrderBy sort");
    var firstCarByColor = context.Cars.OrderBy(c => c.Color).First();
    Console.WriteLine($"{firstCarByColor.PetName} is {firstCarByColor.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();


    Console.WriteLine("Single Record with Where clause");
    var firstCarIdThree = context.Cars.Where(c => c.Id == 3).First();
    Console.WriteLine($"{firstCarIdThree.PetName} is {firstCarIdThree.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    Console.WriteLine("Single Record using First as Where clause");
    var firstCarIdThreeI = context.Cars.First(c => c.Id == 3);
    Console.WriteLine($"{firstCarIdThreeI.PetName} is {firstCarIdThreeI.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    Console.WriteLine("Return Default(null) when no record is found"); 
    var firstCarNotFoundDefault = context.Cars.FirstOrDefault(c => c.Id == 27);
    Console.WriteLine(firstCarNotFoundDefault == null);
    Console.WriteLine();

    Console.WriteLine("Get last record sorted by color");
    var lastCar = context.Cars.OrderBy(c => c.Color).Last();
    Console.WriteLine($"{lastCar.PetName} is {lastCar.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    Console.WriteLine("Get single record");
    var singleCar = context.Cars.Single(c => c.Id == 3);
    //var singleCar = context.Cars.Where(c => c.Id == 3).Single();
    Console.WriteLine($"{singleCar.PetName} is {singleCar.Color}.");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    /*Console.WriteLine("Exception when more than one record is found.");
    try{
        context.Cars.Where(c => c.Id > 1).Single();
    }catch(Exception ex){
        Console.WriteLine(ex.Message);
    }
    context.ChangeTracker.Clear();

    Console.WriteLine("Exception when specified record is not found.");
    try{
        context.Cars.Single(c => c.Id == 27);
    }catch(Exception ex){
        Console.WriteLine(ex.Message);
    }*/

    var defaultWhenSingleNotFoundCar = context.Cars.SingleOrDefault(c => c.Id == 27);
    context.ChangeTracker.Clear();

    var foundCar = context.Cars.Find(27);
    context.ChangeTracker.Clear();

       
}

static void Aggregation(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var count = context.Cars.Count();
    Console.WriteLine($"There are {count} cars");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    var countByMake = context.Cars.Where(c => c.MakeId == 1).Count();
    Console.WriteLine($"Count: {countByMake}");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    var countByMake2 = context.Cars.Count(c => c.MakeId == 1);
    Console.WriteLine($"Count: {countByMake}");
    context.ChangeTracker.Clear();
    Console.WriteLine();

    var max = context.Cars.Max(c => c.Id);
    var min = context.Cars.Min(c => c.Id);
    var avg = context.Cars.Average(c => c.Id);
    Console.WriteLine($"Max Id: {max}, Min Id: {min}, Average Id: {avg}");
    Console.WriteLine();
}

static void AnyAndAll(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var resultAny = context.Cars.Any(x => x.Id == 1);
    var resultAnyWithWhere = context.Cars.IgnoreQueryFilters().Where(x => x.Id == 1).Any();
    Console.WriteLine($"Exist? {resultAny}");
    Console.WriteLine($"Exist? {resultAnyWithWhere}");
    Console.WriteLine();
    var resultAll = context.Cars.All(x => x.Color == "Yellow");
    Console.WriteLine($"All? {resultAll}");
    context.ChangeTracker.Clear();
}

static void GetDataFromStoredProcedure(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var cars = context.Cars.IgnoreQueryFilters().ToList();
    foreach(Car c in cars){
        Console.WriteLine($"PetName : {GetPetName(context, c.Id)}");
    }
    context.ChangeTracker.Clear();
}

static string GetPetName(ApplicationDbContext context, int id)
{
    var parameterId = new SqlParameter
    {
        ParameterName = "@carId",
        SqlDbType = System.Data.SqlDbType.Int,
        Value = id,
    };

    var parameterName = new SqlParameter
    {
        ParameterName = "@petName",
        SqlDbType = System.Data.SqlDbType.NVarChar,
        Size = 50,
        Direction = System.Data.ParameterDirection.Output
    };

    var result = context.Database
        .ExecuteSqlRaw("EXEC [dbo].[GetPetName] @carId, @petName OUTPUT", parameterId, parameterName);
    return (string)parameterName.Value;
}

static void RelatedData(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var carsWithMake = context.Cars.Include(c => c.MakeNavigation).ToList();
    context.ChangeTracker.Clear();

    var makesWithCarsAndDrivers = context.Makes.Include(c => c.Cars).ThenInclude(d => d.Drivers).ToList();
    context.ChangeTracker.Clear();

    var orderedMakes = context.Makes.Include(c => c.Cars).ThenInclude(d => d.Drivers).OrderBy(n => n.Name).ToList();
    context.ChangeTracker.Clear();

    var makesWithYellowCars = context.Makes.Include(x => x.Cars.Where(x => x.Color == "Yellow")).ToList();
    context.ChangeTracker.Clear();

    var splitMakes = context.Makes.AsSplitQuery().Include(x => x.Cars.Where(x => x.Color == "Yellow")).ToList();
    context.ChangeTracker.Clear();

    var carsAndDrivers = context.Cars.Include(d => d.Drivers).Where(d => d.Drivers.Any());
    context.ChangeTracker.Clear();

    var car = context.Cars.First(x => x.Id == 1);
    context.Entry(car).Reference(x => x.MakeNavigation).Load();
    context.Entry(car).Collection(x => x.Drivers).Query().Load();
    context.ChangeTracker.Clear();
}

static void UpdateRecords(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var car = context.Cars.First();
    car.Color = "Green";
    context.SaveChanges();

    context.ChangeTracker.Clear();

    var carToUpdate = context.Cars.AsNoTracking().First(x => x.Id == 1);
    carToUpdate.Color = "Orange";
    context.Cars.Update(carToUpdate);
    context.SaveChanges();
    context.ChangeTracker.Clear();

    var carToUpdateII = context.Cars.AsNoTracking().First(x => x.Id == 1);
    carToUpdateII.Color = "Orange";
    context.Entry(carToUpdateII).State = EntityState.Modified;
    context.SaveChanges();
    context.ChangeTracker.Clear();

}

static void DeleteRecords(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    ClearSampleData();
    LoadMakeAndCarData();
    var car = context.Cars.First(x => x.Color != "Green");
    context.Cars.Remove(car);
    context.SaveChanges();
    Console.WriteLine($"{car.PetName}'s state is {context.Entry(car).State}");
    context.ChangeTracker.Clear();

    var carToDelete = context.Cars.AsNoTracking().First(x => x.Color != "Green");
    context.Cars.Remove(carToDelete);
    context.SaveChanges();
    context.ChangeTracker.Clear();

    var carToDeleteII = context.Cars.AsNoTracking().First(x => x.Color != "Green");
    context.Entry(carToDeleteII).State = EntityState.Deleted;
    context.SaveChanges();
    context.ChangeTracker.Clear();
}

static void QueryFilter(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var cars = context.Cars.ToList();
    Console.WriteLine($"Total no of driveable cars: {cars.Count}");
    Console.WriteLine("All Driveable cars: ");
    foreach(Car c in cars){
        Console.WriteLine($"{c.PetName} {c.Color} {c.MakeId}");
    }
    context.ChangeTracker.Clear();
    Console.WriteLine();
    var allCars = context.Cars.IgnoreQueryFilters().ToList();
    Console.WriteLine($"Total no of cars: {allCars.Count}");
    Console.WriteLine("All cars: ");
    foreach(Car c in allCars){
        Console.WriteLine($"{c.PetName} {c.Color} {c.MakeId}");
    }
    var radios = context.Radios.ToList();
    var allRadios = context.Radios.IgnoreQueryFilters().ToList();
    Console.WriteLine($"Total no of radios having tweeters: {radios.Count}");
    Console.WriteLine($"Total no of radios: {allRadios.Count}");
    context.ChangeTracker.Clear();
}

static void RelatedDataWithQueryFilter(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    var make = context.Makes.First(x => x.Id == 1);
    context.Entry(make).Collection(c => c.Cars).Load();
    context.Entry(make).Collection(c => c.Cars).Query().IgnoreQueryFilters().Load();
    context.ChangeTracker.Clear();
}

static void UsingSqlRaw(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    IEntityType metadata = context.Model.FindEntityType(typeof(Car).FullName);
    Console.WriteLine($"Schema: {metadata.GetSchema()}");
    Console.WriteLine($"Table Name: {metadata.GetTableName()}");
    context.ChangeTracker.Clear();
    /*int carId = 1;
    var car = context
    .Cars
    .FromSqlInterpolated($"SELECT * from dbo.Inventory where Id = {carId} ")
    .Include(x => x.MakeNavigation)
    .First();*/
}

static void Projections(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    List<int> ids = context.Cars.Select(x => x.Id).ToList();

    var vms = context.Cars.Select(x => new CarMakeViewModel {
        CarId = x.Id,
        Color = x.Color,
        DateBuilt = x.DateBuilt.GetValueOrDefault(new DateTime(2024, 01, 01)),
        Make = x.MakeNavigation.Name,
        MakeId = x.MakeId,
        IsDrivable = x.IsDrivable,
        PetName = x.PetName,
        Display = x.Display
    });

    foreach(CarMakeViewModel c in vms){
        Console.WriteLine($"{c.PetName} is a {c.Make}");
    }

    
} 

static void AddCar(){
        var context = new ApplicationDbContextFactory().CreateDbContext(null);
        var car = new Car{
            Color = "Green",
            PetName = "Light Speed",
            MakeId = 2050
        };
        context.Cars.Add(car);
        Console.WriteLine($"Car named {car.PetName} has been added to the database.");
        context.ChangeTracker.Clear();
    }

static void AddCarWithDefaultsSet(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
        var car = new Car{
            Color = "Green",
            PetName = "Light Speed",
            MakeId = 2050,
            IsDrivable = true,
            DateBuilt = new DateTime(2023, 01, 01)
        };
        context.Cars.Add(car);
        
        context.SaveChanges();
        context.ChangeTracker.Clear();
}

static void UpdateACar(){
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
         var car = context.Cars.First(c => c.MakeId == 2050);
         car.Color = "White";
         context.SaveChanges();
         context.ChangeTracker.Clear();
        
        
}

}
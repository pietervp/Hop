# Hop #
## Usage ##
Hop works as an extension to IDbConnection. It propagates as one extension method 'Hop()' which returns an IHop instance.
The IHop interface in it's turn has basic CRUD extension methods defined. This approach keeps the 'true' IDbConnection members clean, and separates all the extensions to an external helper class.

### Dynamic Schema creation ###
Hop will dynamically create tables/columns when it is possible/needed. This only works for value typed columns (except System.String). Hop will try to infer a primary key by searching for a property named Id, or a property decorated with IdAttribute. All the beheviour in this matter can be modified by overriding the service-methods in HopBase.

### Read ###

The basic usage of the Read extension methods is as follows:

    GetSqlConnection().Hop().ReadAll<Beer>();
    GetSqlConnection().Hop().Read<Beer>("Name = 'Jupiler'")

To refresh an existing instance (given the Id property is valid)

    GetSqlConnection().Hop().Read(existingBeerInstance);

#### How it works

Hop works by generating type specific code when it comes to Materializing a DataReader instance. This means the lib generates IL opcodes that represent a class deriving from the generic Materializer<T> class. In this way we can reduce Reflection costs for Read operations.

#### Extra: Read to Tuple<> ####

In Hop, it is possible to do a read to Tuples:

    GetSqlConnection().Hop().ReadTuples<Tuple<int, string>, Beer>("Id, Name");

### Insert ###

    GetSqlConnection().Hop().InsertSingle(new Beer() { Name = "Petrus" });

#### How it works ####
  
Inserting works by generating a SQL script which will insert the correct values to the database. In the same script the current Identity is retrieved, and the Id properties of the instances are updated to the correct value.

### Update ###

Update method works as expected:

    var beer = Beers.FirstOrDefault();
    beer.Name = "Heineken";
    
    GetSqlConnection().Hop().UpdateSingle(beer);

### Delete ###

So does the delete method:

    //this line actually deletes all records from the beer table
    GetSqlConnection().Hop().Delete<Beer>();

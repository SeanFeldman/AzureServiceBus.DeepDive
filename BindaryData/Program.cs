var dataString = new BinaryData("Loki");
Console.WriteLine(dataString);

var person = new Person("Loki", "Sanekat-Feldman");
var data = new BinaryData(person);
Console.WriteLine(data.ToString());

var deserialized = data.ToObjectFromJson<Person>();
Console.WriteLine(deserialized.ToString());

record Person(string FirstName, string LastName);
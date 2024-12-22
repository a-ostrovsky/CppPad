namespace CppPad.UniqueIdentifier;

public class IdGenerator
{
    private static readonly List<string> Adjectives =
    [
        "Happy", "Blue", "Fast", "Green", "Big", "Strong", "Bright", "Quick", "Smart", "Brave", "Calm",
        "Gentle", "Kind", "Bold", "Clever", "Curious", "Diligent", "Eager", "Faithful", "Generous",
        "Jolly", "Lively", "Mighty", "Noble", "Proud", "Tender", "Witty", "Zany", "Cheerful", "Vibrant",
        "Energetic", "Radiant", "Serene", "Wise", "Courageous", "Inventive", "Meticulous", "Optimistic",
        "Loyal", "Charitable", "Merry", "Animated", "Powerful", "Regal", "Playful", "Humorous", "Quirky",
        "Adventurous", "Patient", "Ambitious", "Content", "Resourceful", "Tolerant", "Friendly", "Sincere",
        "Trustworthy"
    ];

    private static readonly List<string> Nouns =
    [
        "Dog", "Cat", "Sky", "Car", "Tree", "Bird", "House", "River", "Mountain", "Cloud", "Flower",
        "Star", "Sun", "Moon", "Ocean", "Forest", "Book", "Road", "Bridge", "Fire", "Water", "Wind",
        "Stone", "Rain", "Lion", "Tiger", "Bear", "Wolf", "Fox", "Eagle", "Shark", "Whale", "Dolphin",
        "Horse", "Elephant", "Giraffe", "Kangaroo", "Panda", "Zebra", "Butterfly", "Dragon", "Phoenix",
        "Laptop", "Phone", "Tablet", "Chair", "Table", "Lamp", "Pen", "Notebook", "Clock", "Watch",
        "Bag", "Bottle", "Cup", "Plate", "Fork", "Spoon", "Knife", "Glass", "Mirror", "Window", "Door",
        "Wall", "Ceiling", "Floor", "Roof", "Garden", "Yard", "Fence", "Gate", "Path", "Trail", "Hill",
        "Valley", "Cave", "Desert", "Island", "Beach", "Wave", "Tide", "Current", "Stream", "Pond",
        "Lake", "Swamp", "Marsh", "Glacier", "Volcano", "Canyon", "Cliff", "Peak", "Summit", "Plateau",
        "Plain", "Field", "Meadow", "Pasture", "Grove"
    ];

    private static readonly ThreadLocal<Random> Random = new(() => new Random());

    public static Identifier GenerateUniqueId()
    {
        var adjective = Adjectives[Random.Value!.Next(Adjectives.Count)];
        var noun = Nouns[Random.Value.Next(Nouns.Count)];
        var shortGuid = ToShortGuid(Guid.NewGuid());
        var id = new Identifier($"{adjective}_{noun}_{shortGuid}");
        return id;
    }

    private static string ToShortGuid(Guid guid)
    {
        var base64 = Convert.ToBase64String(guid.ToByteArray());
        return base64.Replace("/", "_").Replace("+", "-").TrimEnd('=');
    }
}
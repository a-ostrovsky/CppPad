namespace CppPad.Common;

public class IdGenerator
{
    private static readonly List<string> Adjectives =
    [
        "Happy", "Sad", "Blue", "Red", "Fast", "Slow", "Green", "Yellow", "Big", "Small", "Strong",
        "Weak", "Bright", "Dark", "Quiet", "Loud", "Quick", "Lazy", "Smart", "Brave", "Calm",
        "Gentle", "Kind", "Bold", "Fierce", "Shy", "Clever", "Curious", "Diligent", "Eager",
        "Faithful", "Generous", "Grumpy", "Jolly", "Lively", "Mighty", "Noble", "Proud", "Silly",
        "Tender", "Witty", "Zany"
    ];

    private static readonly List<string> Nouns =
    [
        "Dog", "Cat", "Sky", "Car", "Tree", "Bird", "House", "River", "Mountain", "Cloud", "Flower",
        "Star", "Sun", "Moon", "Ocean", "Forest", "Book", "Road", "Bridge", "Fire", "Water", "Wind",
        "Stone", "Rain", "Lion", "Tiger", "Bear", "Wolf", "Fox", "Eagle", "Shark", "Whale",
        "Dolphin", "Horse", "Elephant", "Giraffe", "Kangaroo", "Panda", "Zebra", "Butterfly",
        "Dragon",
        "Phoenix"
    ];

    private static readonly ThreadLocal<Random> Random = new(() => new Random());

    public static Identifier GenerateUniqueId()
    {
        var adjective1 = Adjectives[Random.Value!.Next(Adjectives.Count)];
        string adjective2;
        do
        {
            adjective2 = Adjectives[Random.Value.Next(Adjectives.Count)];
        } while (adjective2 == adjective1);

        var noun = Nouns[Random.Value.Next(Nouns.Count)];
        var id = new Identifier($"{adjective1}_{adjective2}_{noun}_{Guid.NewGuid():N}");
        return id;
    }
}
### __LinkedDictionary__

The LinkedDictionary class works pretty much the same as a regular Dictionary, except all mutations will appear in FIFO order during iteration:
```csharp
var dictionary = new LinkedDictionary<int, char>();
for (int i = 65; i < 70; i++)
{
    dictionary.Add(i, (char)i);
}

dictionary.Remove(67);
dictionary.Add(85, (char)85);
dictionary[66] = 'Q';

foreach (var key in dictionary.Keys)
{
    Console.Write("[{0}, {1}], ", key, dictionary[key]);
}
// prints "[65, A], [68, D], [69, E], [85, U], [66, Q]"
```
namespace Models
{
    public class CommandAndEvent
    {
        public string CommandText { get; set; }
        public string WaitForEventType { get; set; }
    }

    public class CommandAndEvents
    {
        public List<CommandAndEvent> Commands { get; set; } = new();
    }
}

namespace Scratch
{
    public static class Helper
    {
        public static Tuple<string?, string?, string?> GetUserInput()
        {
            Console.Write("Enter OrchestrationInstance Id: ");
            var instanceId1 = Console.ReadLine();
            Console.Write("Enter event: ");
            var eventName = Console.ReadLine();
            Console.Write("Enter eventText: ");
            var eventData = Console.ReadLine();

            return Tuple.Create(instanceId1, eventName, eventData);
        }
    }
}

namespace AutoPrintr.Helpers
{
    internal class ShowControlMessage
    {
        public string Caption { get; set; }
        public object Data { get; set; }
        public ControlMessageType Type { get; set; }

        public ShowControlMessage(ControlMessageType type)
        {
            Type = type;
        }
    }
}
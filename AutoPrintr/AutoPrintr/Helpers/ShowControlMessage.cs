namespace AutoPrintr.Helpers
{
    public class ShowControlMessage
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
namespace AutoPrintr.Core.Models
{
    public enum JobState : byte
    {
        New,
        Processing,
        Downloading,
        Downloaded,
        Printing,
        Printed,
        Error
    }
}
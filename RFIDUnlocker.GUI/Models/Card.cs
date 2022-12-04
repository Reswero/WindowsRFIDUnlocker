using System.Text.Json.Serialization;

namespace RFIDUnlocker.GUI.Models
{
    internal struct Card
    {
        public string UID { get; init; }
        public string? Name { get; set; }

        public Card(string uid)
        {
            UID = uid;
        }
    }
}

namespace InteractiveItinerary.Entities
{
    public readonly record struct Location
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public string Address { get; }

        public Location(double latitude, double longitude, string address)
        {
            if (latitude < -90 || latitude > 90)
                    throw new ArgumentOutOfRangeException(
                    nameof(latitude), "Latitude must be between -90 and 90.");

            if (longitude < -180 || longitude > 180)    
                    throw new ArgumentOutOfRangeException(
                    nameof(longitude), "Longitude must be between -180 and 180.");

            Latitude = latitude;
            Longitude = longitude;
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }

}

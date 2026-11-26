namespace InteractiveItinerary.Exceptions
{
	public class ItineraryStateConflictException : InvalidOperationException
	{
		public ItineraryStateConflictException(string message)
			: base(message)
		{
		}
	}
}
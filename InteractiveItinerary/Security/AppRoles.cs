namespace InteractiveItinerary.Security
{
    /// <summary>
    /// Application roles. Currently <see cref="Editor"/> is the role allowed to create itineraries.
    /// </summary>
    public static class AppRoles
    {
        public const string Editor = "Editor";
        public const string Viewer = "Viewer";
    }

    public static class AuthorizationPolicies
    {
        public const string CanCreateItinerary = "CanCreateItinerary";
    }
}

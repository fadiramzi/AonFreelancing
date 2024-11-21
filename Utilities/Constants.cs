namespace AonFreelancing.Utilities
{
    public class Constants
    {
        //User types
        public const string USER_TYPE_FREELANCER = "FREELANCER";
        public const string USER_TYPE_CLIENT= "CLIENT";

        // Env
        public const string ENV_SIT = "SIT";

        // JWT
        public const string JWT_KEY = "Key";
        public const string JWT_ISSUER = "Issuer";
        public const string JWT_AUDIENCE = "Audience";
        public const string JWT_EXPIRATION = "ExpireInMinutes";

        // Project status
        public const string PROJECT_STATUS_AVAILABLE = "Available";
        public const string PROJECT_STATUS_CLOSED = "Closed";

        // Project price type
        public const string PROJECT_PRICE_TYPE_FIXED = "Fixed";
        public const string PROJECT_PRICE_TYPE_PER_HOUR = "PerHour";

        // Bids status
        public const string BID_STATUS_PENDING = "pending";
        public const string BID_STATUS_APPROVED = "approved";

        // Tasks status
        public const string TASK_STATUS_TODO = "to-do";
        public const string TASK_STATUS_IN_PROGRESS = "in-progress";
        public const string TASK_STATUS_IN_REVIEW = "in-review";
        public const string TASK_STATUS_DONE = "done";
    }
}

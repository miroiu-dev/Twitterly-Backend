namespace Twitter.ApiErrors
{
    static class SignUpError
    {
        public const string USER_EXISTS = "The user you are attempting to sign up has already signed up";
    }
    static class SignInError
    {
        public const string INVALID_USER_PASSWORD = "The email and/or password used for authentication are invalid";
        public const string ACCESS_DENIED = "When using web-based authentication, the resource server denies access per OAuth2 specifications";
    }
    static class ResetError
    {
        public const string USER_DOESNT_EXIST = "The provided email is not associated with an account";
        public const string SOMETHIG_WENT_WRONG = "Ooops! Something went wrong, please try again later";
        public const string INVALID_TOKEN = "This session expired. Please try again later";
    }
    record AuthenticationError(string Message);
}

using Google.Apis.Auth;

namespace CustomUser_Auth.Helpers.Services;

public class GoogleAuthService
{
    private readonly string _googleClientId;

    public GoogleAuthService()
    {
        // Get your Google Client ID from appsettings or environment variable
        _googleClientId = Environment.GetEnvironmentVariable("APP_CLIENT_ID");
    }

    public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            // The default validator is based on the Google OAuth2 API
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            // Verify the token's audience
            if ((string)validPayload.Audience != _googleClientId)
            {
                throw new Exception("Invalid audience");
            }

            return validPayload;  // Return the payload which contains user info
        }
        catch (Exception ex)
        {
            // Token is invalid or could not be verified
            throw new Exception("Token verification failed", ex);
        }
    }
}
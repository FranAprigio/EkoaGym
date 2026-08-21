namespace EkoaGym.Mobile.Services.Auth;

// App-wide authentication state. AuthDelegatingHandler raises AuthenticationExpired when a silent
// refresh fails; a top-level component (MainLayout) subscribes to it and navigates to Login.
public class AuthState
{
    public bool IsAuthenticated { get; private set; }

    public event Action? Changed;
    public event Action? AuthenticationExpired;

    public void SignIn()
    {
        IsAuthenticated = true;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        IsAuthenticated = false;
        Changed?.Invoke();
        AuthenticationExpired?.Invoke();
    }
}

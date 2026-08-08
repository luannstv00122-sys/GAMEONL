using System;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebaseAuthService : MonoBehaviour
{
    public static FirebaseAuthService Instance { get; private set; }

    public FirebaseUser CurrentUser =>
        FirebaseBootstrap.IsReady
            ? FirebaseBootstrap.Instance.Auth.CurrentUser
            : null;

    public bool IsLoggedIn => CurrentUser != null;

    public event Action<FirebaseUser> OnLoginSuccess;
    public event Action<FirebaseUser> OnRegisterSuccess;
    public event Action<string> OnAuthError;
    public event Action OnLogout;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(string email, string password)
    {
        if (!ValidateFirebase())
            return;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            OnAuthError?.Invoke("Email/password không được để trống.");
            return;
        }

        FirebaseBootstrap.Instance.Auth
            .CreateUserWithEmailAndPasswordAsync(
                email.Trim(),
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    OnAuthError?.Invoke("Đăng ký bị hủy.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string error =
                        task.Exception?.GetBaseException().Message
                        ?? "Đăng ký thất bại.";

                    Debug.LogError(error);
                    OnAuthError?.Invoke(error);
                    return;
                }

                FirebaseUser user = task.Result.User;

                Debug.Log(
                    $"[Firebase Auth] Register OK: {user.UserId}"
                );

                OnRegisterSuccess?.Invoke(user);
            });
    }

    public void Login(string email, string password)
    {
        if (!ValidateFirebase())
            return;

        FirebaseBootstrap.Instance.Auth
            .SignInWithEmailAndPasswordAsync(
                email.Trim(),
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    OnAuthError?.Invoke("Đăng nhập bị hủy.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string error =
                        task.Exception?.GetBaseException().Message
                        ?? "Đăng nhập thất bại.";

                    Debug.LogError(error);
                    OnAuthError?.Invoke(error);
                    return;
                }

                FirebaseUser user = task.Result.User;

                Debug.Log(
                    $"[Firebase Auth] Login OK: {user.UserId}"
                );

                OnLoginSuccess?.Invoke(user);
            });
    }

public void LoginAnonymous()
{
    if (!FirebaseBootstrap.IsReady)
    {
        Debug.LogError("[Firebase Auth] Firebase chưa READY");
        return;
    }

    var auth = FirebaseBootstrap.Instance.Auth;

    // Đã có user từ lần chơi trước thì dùng luôn.
    if (auth.CurrentUser != null)
    {
        Debug.Log(
            "[Firebase Auth] Existing UID: " +
            auth.CurrentUser.UserId
        );

        OnLoginSuccess?.Invoke(auth.CurrentUser);
        return;
    }

    auth.SignInAnonymouslyAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(
                    "[Firebase Auth] Anonymous login lỗi:\n" +
                    task.Exception
                );

                return;
            }

            FirebaseUser user = task.Result.User;

            Debug.Log(
                "[Firebase Auth] Anonymous Login OK: " +
                user.UserId
            );

            OnLoginSuccess?.Invoke(user);
        });
}
    public void Logout()
    {
        if (!ValidateFirebase())
            return;

        FirebaseBootstrap.Instance.Auth.SignOut();

        Debug.Log("[Firebase Auth] Logout");
        OnLogout?.Invoke();
    }

    public void SendPasswordReset(string email)
    {
        if (!ValidateFirebase())
            return;

        FirebaseBootstrap.Instance.Auth
            .SendPasswordResetEmailAsync(email.Trim())
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    OnAuthError?.Invoke(
                        task.Exception?.GetBaseException().Message
                        ?? "Không gửi được email reset."
                    );

                    return;
                }

                Debug.Log("[Firebase Auth] Password reset email sent.");
            });
    }

    private bool ValidateFirebase()
    {
        if (!FirebaseBootstrap.IsReady ||
            FirebaseBootstrap.Instance == null)
        {
            OnAuthError?.Invoke(
                "Firebase chưa READY. Chờ khởi tạo xong."
            );

            return false;
        }

        return true;
    }
}

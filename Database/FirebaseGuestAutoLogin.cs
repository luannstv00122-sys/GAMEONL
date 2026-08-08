using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebaseGuestAutoLogin : MonoBehaviour
{
    private bool subscribed;
    private bool loginStarted;

    private void Start()
    {
        TryLoginOrWait();
    }

    private void TryLoginOrWait()
    {
        FirebaseBootstrap bootstrap =
            FirebaseBootstrap.Instance;

        if (bootstrap != null &&
            FirebaseBootstrap.IsReady &&
            bootstrap.Auth != null &&
            bootstrap.Database != null &&
            bootstrap.Root != null)
        {
            Unsubscribe();
            LoginGuest();
            return;
        }

        if (bootstrap != null && !subscribed)
        {
            bootstrap.OnFirebaseReady += OnFirebaseReady;
            subscribed = true;
        }

        Debug.Log(
            "[Firebase Guest] Đang chờ Firebase READY..."
        );
    }

    private void OnFirebaseReady()
    {
        Unsubscribe();

        if (loginStarted)
            return;

        LoginGuest();
    }

    private void LoginGuest()
    {
        if (loginStarted)
            return;

        FirebaseBootstrap bootstrap =
            FirebaseBootstrap.Instance;

        if (bootstrap == null ||
            !FirebaseBootstrap.IsReady ||
            bootstrap.Auth == null ||
            bootstrap.Root == null)
        {
            loginStarted = false;
            TryLoginOrWait();
            return;
        }

        loginStarted = true;

        FirebaseAuth auth = bootstrap.Auth;

        FirebaseUser currentUser =
            auth.CurrentUser;

        if (currentUser != null)
        {
            Debug.Log(
                "[Firebase Guest] UID cũ: " +
                currentUser.UserId
            );

            CreateOrUpdateProfile(
                currentUser
            );

            return;
        }

        auth.SignInAnonymouslyAsync()
            .ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCanceled)
                    {
                        loginStarted = false;

                        Debug.LogError(
                            "[Firebase Guest] Anonymous login bị hủy."
                        );

                        return;
                    }

                    if (task.IsFaulted)
                    {
                        loginStarted = false;

                        Debug.LogError(
                            "[Firebase Guest] Anonymous login lỗi."
                        );

                        if (task.Exception != null)
                        {
                            Debug.LogException(
                                task.Exception
                            );
                        }

                        return;
                    }

                    FirebaseUser user =
                        task.Result.User;

                    if (user == null)
                    {
                        loginStarted = false;

                        Debug.LogError(
                            "[Firebase Guest] Firebase không trả về user."
                        );

                        return;
                    }

                    Debug.Log(
                        "[Firebase Guest] Anonymous Login OK: " +
                        user.UserId
                    );

                    CreateOrUpdateProfile(
                        user
                    );
                }
            );
    }

    private void CreateOrUpdateProfile(
        FirebaseUser user)
    {
        FirebaseBootstrap bootstrap =
            FirebaseBootstrap.Instance;

        if (bootstrap == null ||
            bootstrap.Root == null)
        {
            Debug.LogError(
                "[Firebase Guest] Database Root chưa sẵn sàng."
            );

            return;
        }

        DatabaseReference profileRef =
            bootstrap.Root
                .Child("profiles")
                .Child(user.UserId);

        var data =
            new Dictionary<string, object>
            {
                { "uid", user.UserId },
                { "displayName", "Guest" },
                { "avatarId", 0 },
                { "updatedAt", ServerValue.Timestamp }
            };

        profileRef
            .UpdateChildrenAsync(data)
            .ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError(
                            "[Firebase Guest] Save profile bị hủy."
                        );

                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError(
                            "[Firebase Guest] Save profile lỗi."
                        );

                        if (task.Exception != null)
                        {
                            Debug.LogException(
                                task.Exception
                            );
                        }

                        return;
                    }

                    Debug.Log(
                        "[Firebase Guest] PROFILE SAVED ✅ UID: " +
                        user.UserId
                    );
                }
            );
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        FirebaseBootstrap bootstrap =
            FirebaseBootstrap.Instance;

        if (bootstrap != null)
        {
            bootstrap.OnFirebaseReady -= OnFirebaseReady;
        }

        subscribed = false;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
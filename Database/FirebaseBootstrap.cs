using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebaseBootstrap : MonoBehaviour
{
    public static FirebaseBootstrap Instance { get; private set; }

    public static bool IsReady { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseDatabase Database { get; private set; }
    public DatabaseReference Root { get; private set; }

    public event Action OnFirebaseReady;
    public event Action<string> OnFirebaseError;

    private bool initializationStarted;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Quan trọng khi Unity tắt Domain Reload.
        IsReady = false;
        Auth = null;
        Database = null;
        Root = null;

        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        if (initializationStarted)
            return;

        initializationStarted = true;

        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCanceled ||
                        task.IsFaulted)
                    {
                        initializationStarted = false;

                        string error =
                            task.Exception != null
                                ? task.Exception.ToString()
                                : "Firebase dependency check failed.";

                        Debug.LogError(error);
                        OnFirebaseError?.Invoke(error);
                        return;
                    }

                    DependencyStatus status =
                        task.Result;

                    if (status !=
                        DependencyStatus.Available)
                    {
                        initializationStarted = false;

                        string error =
                            "Firebase dependencies unavailable: " +
                            status;

                        Debug.LogError(error);
                        OnFirebaseError?.Invoke(error);
                        return;
                    }

                    try
                    {
                        _ = FirebaseApp.DefaultInstance;

                        Auth =
                            FirebaseAuth.DefaultInstance;

                        Database =
                            FirebaseDatabase.DefaultInstance;

                        Root =
                            Database.RootReference;

                        IsReady = true;

                        Debug.Log(
                            "[Firebase] READY"
                        );

                        OnFirebaseReady?.Invoke();
                    }
                    catch (Exception exception)
                    {
                        initializationStarted = false;
                        IsReady = false;

                        Debug.LogException(
                            exception
                        );

                        OnFirebaseError?.Invoke(
                            exception.Message
                        );
                    }
                }
            );
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        IsReady = false;
        Auth = null;
        Database = null;
        Root = null;
        Instance = null;
    }
}
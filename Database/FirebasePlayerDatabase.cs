using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebasePlayerDatabase : MonoBehaviour
{
    public static FirebasePlayerDatabase Instance { get; private set; }

    public event Action<PlayerProfile> OnProfileLoaded;
    public event Action<PlayerProfile> OnProfileChanged;
    public event Action<string> OnDatabaseError;

    private DatabaseReference listeningReference;

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

    public void SaveMyProfile(
        string displayName,
        int avatarId)
    {
        if (!TryGetUid(out string uid))
            return;

        displayName = (displayName ?? string.Empty).Trim();

        if (displayName.Length < 3 ||
            displayName.Length > 20)
        {
            OnDatabaseError?.Invoke(
                "Tên player phải từ 3 đến 20 ký tự."
            );

            return;
        }

        if (avatarId < 0 || avatarId > 999)
        {
            OnDatabaseError?.Invoke(
                "avatarId không hợp lệ."
            );

            return;
        }

        DatabaseReference profileRef =
            FirebaseBootstrap.Instance.Root
                .Child("profiles")
                .Child(uid);

        var updates =
            new Dictionary<string, object>
            {
                { "uid", uid },
                { "displayName", displayName },
                { "avatarId", avatarId },
                { "updatedAt", ServerValue.Timestamp }
            };

        profileRef
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    string error =
                        task.Exception?.GetBaseException().Message
                        ?? "Không save được profile.";

                    Debug.LogError(error);
                    OnDatabaseError?.Invoke(error);
                    return;
                }

                Debug.Log(
                    $"[Firebase DB] Saved profile: {uid}"
                );
            });
    }

    public void LoadMyProfile()
    {
        if (!TryGetUid(out string uid))
            return;

        FirebaseBootstrap.Instance.Root
            .Child("profiles")
            .Child(uid)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    string error =
                        task.Exception?.GetBaseException().Message
                        ?? "Không load được profile.";

                    Debug.LogError(error);
                    OnDatabaseError?.Invoke(error);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    OnDatabaseError?.Invoke(
                        "Player chưa có profile."
                    );

                    return;
                }

                PlayerProfile profile =
                    SnapshotToProfile(snapshot);

                OnProfileLoaded?.Invoke(profile);

                Debug.Log(
                    $"[Firebase DB] Loaded: {profile.displayName}"
                );
            });
    }

    public void StartListenMyProfile()
    {
        StopListenMyProfile();

        if (!TryGetUid(out string uid))
            return;

        listeningReference =
            FirebaseBootstrap.Instance.Root
                .Child("profiles")
                .Child(uid);

        listeningReference.ValueChanged += OnProfileValueChanged;

        Debug.Log(
            $"[Firebase DB] Listening profile realtime: {uid}"
        );
    }

    public void StopListenMyProfile()
    {
        if (listeningReference == null)
            return;

        listeningReference.ValueChanged -= OnProfileValueChanged;
        listeningReference = null;
    }

    private void OnProfileValueChanged(
        object sender,
        ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            OnDatabaseError?.Invoke(
                args.DatabaseError.Message
            );

            return;
        }

        if (args.Snapshot == null ||
            !args.Snapshot.Exists)
        {
            return;
        }

        PlayerProfile profile =
            SnapshotToProfile(args.Snapshot);

        OnProfileChanged?.Invoke(profile);

        Debug.Log(
            $"[Firebase DB] Realtime changed: {profile.displayName}"
        );
    }

    private static PlayerProfile SnapshotToProfile(
        DataSnapshot snapshot)
    {
        var profile =
            new PlayerProfile
            {
                uid =
                    snapshot.Child("uid").Value?.ToString()
                    ?? string.Empty,

                displayName =
                    snapshot.Child("displayName").Value?.ToString()
                    ?? string.Empty,

                avatarId =
                    ToInt(snapshot.Child("avatarId").Value),

                updatedAt =
                    ToLong(snapshot.Child("updatedAt").Value)
            };

        return profile;
    }

    private bool TryGetUid(out string uid)
    {
        uid = null;

        if (!FirebaseBootstrap.IsReady ||
            FirebaseBootstrap.Instance == null)
        {
            OnDatabaseError?.Invoke(
                "Firebase chưa READY."
            );

            return false;
        }

        var user =
            FirebaseBootstrap.Instance.Auth.CurrentUser;

        if (user == null)
        {
            OnDatabaseError?.Invoke(
                "Player chưa đăng nhập."
            );

            return false;
        }

        uid = user.UserId;
        return true;
    }

    private static int ToInt(object value)
    {
        if (value == null)
            return 0;

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return 0;
        }
    }

    private static long ToLong(object value)
    {
        if (value == null)
            return 0;

        try
        {
            return Convert.ToInt64(value);
        }
        catch
        {
            return 0;
        }
    }

    private void OnDestroy()
    {
        StopListenMyProfile();
    }
}

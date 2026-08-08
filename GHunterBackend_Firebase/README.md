# G-Hunter Backend Firebase

Backend ASP.NET Core thật, không SQL Server.

## 1. Lấy Service Account
Firebase Console -> Cài đặt dự án -> Tài khoản dịch vụ / Service accounts -> Firebase Admin SDK -> Tạo khóa riêng tư mới.
Đổi tên file tải về thành `firebase-admin.json` và đặt tại:
`GHunterBackend/secrets/firebase-admin.json`

TUYỆT ĐỐI không bỏ key này vào Unity và không commit GitHub.

## 2. Chạy backend
Mở terminal trong folder `GHunterBackend`:

```bash
dotnet restore
dotnet run
```

Test: `http://127.0.0.1:5077/api/health`

## 3. Nối Unity
Copy `UnityClient/GHunterBackendClient.cs` vào `Assets/Database/` và gắn lên `FirebaseSystem`.
Game đã Anonymous login thì gọi:
- `GHunterBackendClient.Instance.LoadMySecureData()`
- `GHunterBackendClient.Instance.SelectCharacter(0)`

Backend xác minh Firebase ID Token trước khi lấy UID.

## 4. Database
Sau lần gọi `/api/player/me`, Firebase sẽ có:

securePlayers/{UID}
- uid
- selectedCharacter
- level
- xp
- coins
- createdAt
- updatedAt

Unity không có API tự set coins/xp/level. Dữ liệu đó để backend quyết định.

## 5. Backup
Backend tự backup toàn Realtime Database khi startup và mỗi 30 phút vào `GHunterBackend/Backups/`, giữ 20 bản mới nhất.

## Release
Dev: Unity -> http://127.0.0.1:5077
Release: Unity -> HTTPS API trên VPS/cloud -> Firebase

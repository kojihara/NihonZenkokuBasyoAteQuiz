using UnityEngine;
using UniRx.Async;

#if UNITY_ANDROID || UNITY_EDITOR
using Firebase.Auth;
#endif
/// <summary>
/// Firebaseのログイン状態を扱うモデルクラス
/// </summary>
public class AuthModel
{
#if UNITY_ANDROID || UNITY_EDITOR
    private FirebaseAuth auth;

    public AuthModel()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public async UniTask SignInAnonymouslyAsync()
    {
        await auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
        });
    }
#endif
}

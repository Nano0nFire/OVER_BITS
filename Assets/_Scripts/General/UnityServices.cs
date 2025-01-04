using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Cysharp.Threading.Tasks;
public class UnityServicesManager : MonoBehaviour
{
    public static async UniTask InitUnityServices()
    {
        await UnityServices.InitializeAsync();
        // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();
    }

    public static void Logout()
    {
        AuthenticationService.Instance.SignOut(true);
        // VivoxService.Instance.LogoutAsync();
    }
    private void OnDestroy()
    {
        Logout();
    }
}

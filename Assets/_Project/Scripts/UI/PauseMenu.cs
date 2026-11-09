using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMenu menu;

    private GameServices services;
    private bool isOpen;

    public void Construct(GameServices gameServices)
    {
        services = gameServices;
        if (root != null) root.SetActive(false);
        if (menu != null)
        {
            menu.Picked += OnMenuPicked;
        }
    }

    private void Update()
    {
        if (services == null) return;

        if (MenuInput.Cancel)
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                TryOpen();
            }
        }
    }

    private void TryOpen()
    {
        if (services.Scenes.IsBusy) return;
        if (services.Scenes.CurrentScene != SceneFlow.WorldScene) return;
        if (services.Input.IsLocked) return;
        if (services.Dialogue != null && services.Dialogue.IsRunning) return;

        isOpen = true;
        services.Input.Lock();
        root.SetActive(true);
        menu.Open(new[] { "Lanjut", "Simpan sekarang", "Kembali ke judul" });
    }

    public void Close()
    {
        isOpen = false;
        if (menu != null) menu.Close();
        if (root != null) root.SetActive(false);
        services?.Input.Unlock();
    }

    private void OnMenuPicked(int index)
    {
        switch (index)
        {
            case 0: // Lanjut
                Close();
                break;
            case 1: // Simpan sekarang
                SaveGame();
                break;
            case 2: // Kembali ke judul
                Close();
                services.Scenes.GoTo(SceneFlow.TitleScene);
                break;
        }
    }

    private void SaveGame()
    {
        PlayerMovement pm = Object.FindAnyObjectByType<PlayerMovement>();
        Vector3 pos = pm != null ? pm.transform.position : Vector3.zero;
        bool saved = services.Save.Save(SaveMapper.Capture(services, pos));
        if (saved)
        {
            services.Toast.Show("Tersimpan");
        }
        else
        {
            services.Toast.Show("Gagal menyimpan");
        }
    }
}

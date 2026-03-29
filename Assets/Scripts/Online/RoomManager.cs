using UnityEngine;
using Fusion; // Thư viện chính của Photon Fusion để dùng NetworkObject, NetworkRunner...
using TMPro;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    // NetworkRunner: "Trái tim" của hệ thống mạng. Nó quản lý kết nối, đồng bộ và các thực thể mạng.
    private NetworkRunner _runner; 

    [Header("UI Setup")]
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput;
    public GameObject menuPanel;

    [Header("Spawning")]
    // NetworkObject: Một đối tượng có ID định danh duy nhất trên toàn mạng để mọi máy đều nhận diện được.
    public NetworkObject playerPrefab; 

    // Biến static: Để lưu tên cục bộ trên máy này trước khi truyền nó vào Network thông qua PlayerNameHandler.
    public static string LocalPlayerName = "Guest"; 

    public async void JoinOrCreateRoom()
    {
        // Xử lý Tên Người Chơi (Cục bộ - Local)
        LocalPlayerName = !string.IsNullOrEmpty(playerNameInput.text) 
            ? playerNameInput.text 
            : "Guest_" + Random.Range(100, 999);

        // SessionName: Tên của phiên chơi (phòng). Những ai có cùng SessionName sẽ được gom vào chung 1 Network.
        string finalRoomName = string.IsNullOrEmpty(roomNameInput.text) 
            ? "Public_Lobby" 
            : roomNameInput.text;

        // 1. Khởi tạo Runner (Thực thể điều hành mạng)
        if (_runner == null)
        {
            // AddComponent<NetworkRunner>: Gắn bộ điều khiển mạng vào Object này để bắt đầu làm việc với Photon.
            _runner = gameObject.AddComponent<NetworkRunner>();

            // ProvideInput = true: Cho phép máy này gửi dữ liệu bàn phím/chuột (Input) lên mạng để điều khiển nhân vật.
            _runner.ProvideInput = true;
        }

        // 2. StartGame: Lệnh quan trọng nhất để bắt đầu kết nối với Cloud của Photon.
        var result = await _runner.StartGame(new StartGameArgs()
        {
            // GameMode.Shared: Chế độ chia sẻ quyền điều khiển (Phù hợp game đơn giản, ít tốn tài nguyên cho máy i5-6300U).
            GameMode = GameMode.Shared,

            // SessionName: Gán tên phòng bạn đã xử lý ở trên.
            SessionName = finalRoomName, 

            // SceneRef: Chuyển chỉ số Scene của Unity thành định dạng mà Fusion hiểu để đồng bộ Scene giữa các người chơi.
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),

            // NetworkSceneManagerDefault: Component giúp tự động chuyển cảnh cho mọi người chơi khi Host/Master thay đổi Scene.
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // result.Ok: Kiểm tra xem việc kết nối tới Session (phòng) trên Cloud đã thành công hay chưa.
        if (result.Ok)
        {
            Debug.Log($"Vào phòng: {finalRoomName} thành công!");
            if (menuPanel != null) menuPanel.SetActive(false); // Ẩn UI sau khi đã ở trong Network.

            // Task.Delay: Đợi 0.1 giây để đảm bảo Runner đã hoàn tất việc khởi tạo môi trường mạng (Network Environment).
            await System.Threading.Tasks.Task.Delay(100); 

            // IsRunning: Kiểm tra lần cuối xem Runner có còn đang hoạt động ổn định không.
            if (_runner.IsRunning)
            {
                // Runner.Spawn: Lệnh sinh ra một vật thể mạng (Player). 
                // Nó sẽ tạo ra Object trên TẤT CẢ các máy đang ở chung trong Session này.
                // _runner.LocalPlayer: Gán quyền điều khiển (Input Authority) cho máy vừa nhấn nút.
                float x = Random.Range(-10, 10);
                float z = Random.Range(-10, 10);
                _runner.Spawn(playerPrefab, new Vector3(x, 10, z), Quaternion.identity, _runner.LocalPlayer);
            }
        }
    }
}
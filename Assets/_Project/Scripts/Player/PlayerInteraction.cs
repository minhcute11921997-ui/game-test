using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // Biến này để nhớ xem ta có đang đứng gần đồ vật nào không
    private bool canInteract = false;
    private string targetName = "";

    void Update()
    {
        // Kiểm tra 2 điều kiện: Đang đứng gần vật thể VÀ người chơi bấm phím E
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            // Thay vì in ra Console, sau này ta sẽ gọi hàm mở UI hoặc ném bóng ở đây
            Debug.Log("Bạn vừa tương tác thành công với: " + targetName);
        }
    }

    // Hàm này tự động chạy khi nhân vật BƯỚC VÀO vùng Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật thể ta vừa chạm vào có Tag là Interactable không
        if (collision.CompareTag("Interactable"))
        {
            canInteract = true;
            targetName = collision.gameObject.name;
            Debug.Log("Đã vào tầm tương tác. Nhấn E để nói chuyện!");
        }
    }

    // Hàm này tự động chạy khi nhân vật BƯỚC RA KHỎI vùng Trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            canInteract = false;
            targetName = "";
            Debug.Log("Đã đi ra xa.");
        }
    }
}
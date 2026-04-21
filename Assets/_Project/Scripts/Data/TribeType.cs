/// <summary>
/// Sprint 9: Định nghĩa 9 tộc theo GDD.
/// Colossus và Gunner là tộc Legacy — khi bị hạ sẽ để lại hiệu ứng.
/// </summary>
public enum TribeType
{
    None        = 0,
    Mechanical,     // Cơ Khí
    Supernatural,   // Siêu Nhiên
    Beast,          // Thú
    Ancient,        // Cổ Đại
    Angel,          // Thiên Thần
    Warrior,        // Chiến Binh
    Legendary,      // Huyền Thoại
    Colossus,       // Khổng Lồ (Legacy — KO để lại DEF buff cho đồng đội)
    Gunner,         // Pháo Thủ  (Legacy — KO để lại oanh tạc liên tục lên kẻ địch)
}

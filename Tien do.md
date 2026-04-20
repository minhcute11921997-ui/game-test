# Tiến Độ Dự Án

---

## ✅ Các Tính Năng Đã Hoàn Thiện

### Overworld
- [x] Di chuyển 8 hướng Overworld & Camera Damp
- [x] Vật lý bản đồ và ranh giới
- [x] Hệ sinh thái cỏ (GrassZoneManager) tạo bóng đen ngẫu nhiên
- [x] AI Quái vật (Nhát gan / Hung dữ)
- [x] Data Assets ThingData tạo hàng loạt quái
- [x] Tương tác tầm xa (Bóng thu phục / Đạn khiêu chiến)
- [x] Minigame thu phục (% bắt & xóa object)
- [x] Scene Bridge (Cầu nối dữ liệu từ Overworld sang Battle)
- [x] Chuyển cảnh Battle tự động khi va chạm
- [x] Hệ thống Layer và Tag hoàn chỉnh để Radar quét mục tiêu

### Battle — Scene Flow
- [x] Projectile trigger hoạt động
- [x] BattleScene đã load đúng
- [x] Scene flow chạy thật: Overworld → bắn trúng Shadow/Thing → chuyển sang BattleScene

### Battle — Data Bridge
- [x] `GlobalBattleBridge.encounteredThing` — lưu monster vừa gặp ngoài map, BattleScene đọc lại đúng dữ liệu
- [x] `GlobalPlayerBridge.activeThing` — pet mặc định người chơi (tạm thời, chưa có party system)

### Battle — Spawn
- [x] BattleScene spawn đúng quái đã gặp (Slime / Wolf / Dragon đều đúng)
- [x] HP battle đọc đúng từ `ThingData.hp` — không hardcode
  - Slime.hp = 100 | Wolf.hp = 180 | Dragon.hp = 400
- [x] BattleScene có đủ 2 phe: bên trái = pet người chơi, bên phải = quái encounter

### Battle — Grid System *(mới hoàn thành)*
- [x] Tạo struct `GridPos` (col, row) — tọa độ logic cho toàn bộ hệ thống lưới
- [x] Tạo `BattleGridConfig` ScriptableObject — cấu hình kích thước sân (8×8×2 sân, 2 cột gap)
- [x] Tạo `BattleGridManager` — xây lưới 18×8 bằng Tilemap, quản lý vị trí entity, highlight ô
- [x] Tạo `BattleEntity` — component gắn vào mọi Thing trong battle (thay thế BattleEnemy cũ)
- [x] Lưới 3 vùng màu hiển thị đúng: xanh lá (sân trái) | xám (gap) | đỏ (sân phải)
- [x] Camera tự động fit toàn bộ lưới qua `FitCamera()` — đọc Cell Size thực tế từ Grid component
- [x] Entity spawn đúng vị trí ô lưới qua `PlaceEntity()` — snap tự động vào tâm ô
- [x] Console log HP đúng khi spawn: `[BattleEntity] Spawn {name} | Team {id} | HP {hp}`

---

## 🔧 Cấu Hình Kỹ Thuật Hiện Tại

| Thông số | Giá trị |
|---|---|
| PPU (Pixels Per Unit) | 32 |
| Cell Size (Grid) | (1, 1, 0) |
| Kích thước lưới | 18 cột × 8 hàng |
| Sân trái (Team 0) | col 0–7 |
| Gap | col 8–9 |
| Sân phải (Team 1) | col 10–17 |
| Camera Size | ~4.8 (tự tính qua FitCamera) |
| Camera Position | X=9, Y=4, Z=-10 |

---

## 📁 Files Đã Tạo (Session này)

| File | Thư mục | Mô tả |
|---|---|---|
| `GridPos.cs` | `Assets/_Project/Scripts/Combat/` | Struct tọa độ ô lưới |
| `BattleGridConfig.cs` | `Assets/_Project/Scripts/Combat/` | ScriptableObject config lưới |
| `BattleGridManager.cs` | `Assets/_Project/Scripts/Combat/` | Manager xây & quản lý lưới |
| `BattleEntity.cs` | `Assets/_Project/Scripts/Combat/` | Component Thing trong battle |
| `BattleGridConfig` (asset) | `Assets/_Project/ScriptableObjects/` | Config instance (boardCols=8, boardRows=8, gapCols=2) |
| `Tile_Left` / `Tile_Right` / `Tile_Gap` / `Tile_Highlight` | `Assets/_Project/Art/Tiles/` | 4 tile màu cho lưới |

---

## 🔜 Việc Tiếp Theo

- [ ] Hệ thống nhập lệnh (Command Phase) — chọn di chuyển & tấn công tọa độ
- [ ] Thực thi lệnh đồng thời (Execution Phase)
- [ ] Phán xét Speed — Thing nhanh hơn ra chiêu trước
- [ ] Highlight ô di chuyển hợp lệ khi chọn Thing
- [ ] Animation di chuyển Entity mượt trên lưới
- [ ] Party system thay thế GlobalPlayerBridge tạm

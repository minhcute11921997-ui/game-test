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

### Battle — Data Bridge *(đã refactor)*
- [x] `RuntimeGameState.CurrentEnemy` — lưu monster vừa gặp, BattleScene đọc đúng
- [x] `RuntimeGameState.Party` — danh sách Thing của người chơi
- [x] `RuntimeGameState.ActiveThing` — helper lấy Thing đầu tiên trong party
- [x] `SetStarterThing` — set Thing mặc định vào Party khi bắt đầu game
- [x] Xóa `GlobalPlayerBridge` — không còn dùng nữa, tất cả tập trung về `RuntimeGameState`
- [x] Xóa `BattlePlayerLoader` + `BattleEnemyLoader` — không còn cần, `BattleManager` xử lý spawn

### Battle — Spawn
- [x] BattleScene spawn đúng quái đã gặp (Slime / Wolf / Dragon đều đúng)
- [x] HP battle đọc đúng từ `ThingData.hp` — không hardcode
- [x] BattleScene có đủ 2 phe: bên trái = pet người chơi, bên phải = quái encounter
- [x] Spawn đọc từ `RuntimeGameState` thay vì test fields kéo tay

### Battle — Grid System
- [x] Struct `GridPos` (col, row) — tọa độ logic cho toàn bộ hệ thống lưới
- [x] `BattleGridConfig` ScriptableObject — cấu hình kích thước sân (8×8×2 sân, 2 cột gap)
- [x] `BattleGridManager` — xây lưới 18×8 bằng Tilemap, quản lý vị trí entity, highlight ô
- [x] `BattleEntity` — component gắn vào mọi Thing trong battle
- [x] Lưới 3 vùng màu: xanh lá (sân trái) | xám (gap) | đỏ (sân phải)
- [x] Camera tự fit toàn bộ lưới qua `FitCamera()`
- [x] Entity spawn đúng vị trí ô lưới qua `PlaceEntity()` — snap tâm ô
- [x] `MoveEntity()` update cả data lẫn `transform.position` đồng thời
- [x] `MoveEntitySmooth()` — di chuyển mượt Lerp (sẵn sàng dùng cho Sprint 2)
- [x] `ShowHighlight()` / `ClearHighlight()` / `IsHighlighted()` — quản lý highlight tilemap
- [x] `ShowMovableRange()` — highlight vùng di chuyển hợp lệ theo MoveRange của entity

### Battle — Command Phase *(mới hoàn thành)*
- [x] `BattlePhaseManager` — quản lý vòng đời phase: CommandPhase → ExecutionPhase → JudgePhase → loop
- [x] `CommandPhaseController` — nhận input người chơi theo 3 bước: SelectUnit → SelectMove → SelectAttack
- [x] Highlight ô di chuyển hợp lệ (hình vuông Chebyshev, giới hạn trong sân của phe)
- [x] Highlight ô tấn công hợp lệ (toàn bộ ô có entity địch)
- [x] Click chọn Thing → hiện vùng di chuyển → click ô → hiện vùng tấn công → click xác nhận
- [x] Camera Z fix — `mousePos.z = Mathf.Abs(Camera.main.transform.position.z)` đảm bảo click đúng ô
- [x] `BattleCommand` — lưu lệnh (moveTarget + attackTarget) cho mỗi entity mỗi lượt
- [x] Enemy tự động submit lệnh đứng yên (AI placeholder)
- [x] `SubmitCommand` tự động trigger ExecutionPhase khi đủ 2 lệnh
- [x] Entity thực sự di chuyển tới ô đã chọn sau ExecutionPhase

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

## 📁 Files Quan Trọng

| File | Thư mục | Mô tả |
|---|---|---|
| `GridPos.cs` | `Scripts/Combat/` | Struct tọa độ ô lưới |
| `BattleGridConfig.cs` | `Scripts/Combat/` | ScriptableObject config lưới |
| `BattleGridManager.cs` | `Scripts/Combat/` | Manager xây & quản lý lưới |
| `BattleEntity.cs` | `Scripts/Combat/` | Component Thing trong battle |
| `BattleManager.cs` | `Scripts/Combat/` | Spawn 2 phe từ RuntimeGameState |
| `BattlePhaseManager.cs` | `Scripts/Combat/` | Quản lý vòng đời phase |
| `CommandPhaseController.cs` | `Scripts/Combat/` | Input người chơi theo phase |
| `BattleCommand.cs` | `Scripts/Combat/` | Data lệnh mỗi lượt |
| `RuntimeGameState.cs` | `Scripts/Core/` | Bridge data Overworld → Battle |
| `SetStarterThing.cs` | `Scripts/` | Set Thing mặc định khi bắt đầu |

---

## 🔜 Việc Tiếp Theo (Sprint 2)

- [ ] **Combat Calculation** — tính sát thương theo 6 chỉ số (Atk, Def, Sp.Atk, Sp.Def, Speed, HP)
- [ ] **Bảng tương khắc 9 Hệ** — Hỏa/Mộc/Thủy/Thổ/Lôi/Phong/Băng/Quang/Ám
- [ ] **Speed Judge** — Thing nhanh hơn ra chiêu trước trong ExecutionPhase
- [ ] **Animation di chuyển mượt** — dùng `MoveEntitySmooth()` thay teleport trong ExecutionPhase
- [ ] **Animation tấn công** — hiệu ứng khi Thing tấn công
- [ ] **Enemy AI** — địch chọn lệnh di chuyển + tấn công thực sự thay vì đứng yên
- [ ] **Party System** — hỗ trợ 2vs2, quản lý danh sách Thing đã thu phục
- [ ] **Gacha Kỹ Năng** — hệ thống lên cấp mở skill

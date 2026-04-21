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
- [x] **Grid mờ (border 1px)** — `TilemapGrid` layer riêng, tự tạo sprite runtime qua `CreateGridTile()`
- [x] **Tắt màu nền 2 sân** — tắt TilemapRenderer của TilemapLeft / TilemapRight để highlight rõ hơn

### Battle — Command Phase
- [x] `BattlePhaseManager` — quản lý vòng đời phase: CommandPhase → ExecutionPhase → JudgePhase → loop
- [x] `CommandPhaseController` — nhận input người chơi theo 3 bước: SelectUnit → SelectMove → SelectAttack
- [x] Highlight ô di chuyển hợp lệ (hình vuông Chebyshev, giới hạn trong sân của phe)
- [x] Highlight ô tấn công hợp lệ (toàn bộ ô có thể nhắm trong sân địch)
- [x] Click chọn Thing → hiện vùng di chuyển → click ô → hiện vùng tấn công → click xác nhận
- [x] Camera Z fix — `mousePos.z = Mathf.Abs(Camera.main.transform.position.z)` đảm bảo click đúng ô
- [x] `BattleCommand` — lưu lệnh (moveTarget + attackTarget) cho mỗi entity mỗi lượt
- [x] Enemy tự động submit lệnh đứng yên (AI placeholder)
- [x] `SubmitCommand` tự động trigger ExecutionPhase khi đủ 2 lệnh
- [x] Entity thực sự di chuyển tới ô đã chọn sau ExecutionPhase

### Battle — Combat Calculation *(Sprint 2 — hoàn thành)*
- [x] **`MoveData` ScriptableObject** — lưu thông tin chiêu thức: tên, hệ, loại (Physical/Special/Status), sức mạnh, độ chính xác, PP
- [x] **Enum `ElementType`** — 9 hệ: Neutral / Hỏa / Mộc / Thủy / Thổ / Lôi / Phong / Băng / Quang / Ám
- [x] **Enum `AttackShape`** — 5 hình dạng AoE: Single / Cross / Square2x2 / Square3x3 / Line
- [x] **`CombatCalculator` (static class)** — tính sát thương theo công thức:
  - Base: `((BasePower × Atk / Def) / 50 + 2)`
  - × STAB x1.2 nếu move cùng hệ với attacker
  - × Bảng tương khắc 9 hệ (x0.5 / x1.0 / x2.0)
  - × Chí mạng x1.5 (6.25% cơ bản)
  - × Random ±5%
- [x] **Bảng tương khắc 9 hệ** — `TypeChart[10,10]` đầy đủ trong `CombatCalculator`
- [x] **Speed Judge** — sort theo Speed giảm dần, thing nhanh hơn tấn công trước trong JudgePhase
- [x] **AoE damage** — `GetAoECells()` tính tất cả ô bị ảnh hưởng, `BeginJudgePhase` áp damage cho từng ô
- [x] **AoE hover preview** — di chuột vào vùng tấn công hiện shape AoE real-time theo `move.shape`
- [x] **`ThingData` mở rộng** — thêm `spAtk`, `spDef`, `elementType`, `defaultMove`
- [x] **`BattleEntity` mở rộng** — thêm `Data` property, `GetMove()`, `TakeDamage()` với log HP %
- [x] Combat pipeline hoạt động end-to-end: Input → Command → Execute → Judge → Damage → Loop

### Battle — Visual Feedback *(Sprint 3 — hoàn thành)*
- [x] **Animation di chuyển mượt** — `BattlePhaseManager` dùng coroutine `MoveEntitySmooth()`, chờ tất cả entity xong mới sang JudgePhase
- [x] **`BattlePhaseManager` refactor** — `BeginExecutionPhase` đổi thành `IEnumerator`, `SubmitCommand` dùng `StartCoroutine`
- [x] **Floating damage number** — `DamagePopup.cs` + Prefab trong `Assets/Resources/`; số bay lên, fade out 0.8s, màu vàng khi chí mạng
- [x] **HP Bar** — `EntityHpBar.cs` + Prefab trong `Assets/Resources/`; World Space Canvas theo đầu entity, đổi màu xanh/vàng/đỏ theo % máu
- [x] **`BattleEntity` mở rộng Sprint 3** — thêm `CurrentHp` property, `hpBar` field, `TakeDamage(dmg, isCrit)` gọi popup + cập nhật bar
- [x] **`BattleManager` mở rộng Sprint 3** — spawn HP bar từ `Resources.Load` sau khi tạo entity, gắn vào `entity.hpBar`
- [x] **`BattleResultManager`** — kiểm tra toàn bộ entity 1 phe = 0 HP sau mỗi lượt → hiện Win/Lose panel → load về Overworld
- [x] **`CheckBattleEndThenLoop()`** — sau JudgePhase chờ 0.5s cho popup kịp hiện, rồi check kết thúc hoặc loop lượt mới

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
| `BattleGridManager.cs` | `Scripts/Combat/` | Manager xây & quản lý lưới, AoE cells, grid mờ |
| `BattleEntity.cs` | `Scripts/Combat/` | Component Thing trong battle, TakeDamage, GetMove, HP bar |
| `BattleManager.cs` | `Scripts/Combat/` | Spawn 2 phe + HP bar từ RuntimeGameState |
| `BattlePhaseManager.cs` | `Scripts/Combat/` | Quản lý vòng đời phase, smooth move coroutine, battle end check |
| `BattleResultManager.cs` | `Scripts/Combat/` | Kiểm tra thắng/thua, hiện panel, load Overworld |
| `CommandPhaseController.cs` | `Scripts/Combat/` | Input người chơi + AoE hover preview |
| `BattleCommand.cs` | `Scripts/Combat/` | Data lệnh mỗi lượt |
| `CombatCalculator.cs` | `Scripts/Combat/` | Static class tính sát thương + bảng tương khắc |
| `DamagePopup.cs` | `Scripts/UI/` | Floating damage number, fade out 0.8s |
| `EntityHpBar.cs` | `Scripts/UI/` | HP bar World Space theo đầu entity |
| `MoveData.cs` | `Scripts/Data/` | ScriptableObject chiêu thức (hệ, shape, power) |
| `ThingData.cs` | `Scripts/Data/` | ScriptableObject stats Thing (có spAtk, spDef, elementType) |
| `RuntimeGameState.cs` | `Scripts/Core/` | Bridge data Overworld → Battle |
| `SetStarterThing.cs` | `Scripts/` | Set Thing mặc định khi bắt đầu |

---

## 📦 Assets/Resources/ (Prefabs load bằng code)

| Prefab | Mô tả |
|---|---|
| `DamagePopup.prefab` | World Space Canvas + TMP text, gắn script `DamagePopup` |
| `HpBar.prefab` | World Space Canvas + Fill Image, gắn script `EntityHpBar` |

---

## 🔜 Việc Tiếp Theo (Sprint 4)

### Ưu tiên cao
- [ ] **Win/Lose Panel** — tạo 2 UI Panel trong BattleScene, kéo vào `BattleResultManager`, hiện khi battle kết thúc
- [ ] **Enemy AI thực sự** — địch tự chọn ô di chuyển gần nhất + tấn công Thing khắc hệ nếu có

### Ưu tiên trung bình
- [ ] **Animation tấn công** — hit flash + knockback nhỏ khi Thing bị đánh
- [ ] **Party System 2vs2** — quản lý danh sách Thing đã thu phục, chiến đấu 2 bên mỗi bên 2 thing

### Ưu tiên sau
- [ ] **Gacha Kỹ Năng** — hệ thống lên cấp mở skill ngẫu nhiên, lưu vào kho
- [ ] **Âm thanh** — SFX tấn công, nhạc nền battle

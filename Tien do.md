# Tiến Độ Dự Án

> Cập nhật: đối chiếu trực tiếp từng dòng code tất cả file `.cs` trong `Assets/_Project/Scripts` + các sửa đổi trong session hiện tại.

---

## ✅ Overworld

### Di chuyển & Camera

- [x] **Di chuyển 8 hướng** — `PlayerMovement.cs` dùng `GetAxisRaw` Horizontal + Vertical, normalize vector, `Rigidbody2D.MovePosition` → đúng 8 hướng mượt
- [x] **Animator 8 hướng** — Set `MoveX`, `MoveY`, `Speed` cho Animator; flip sprite khi đi trái
- [x] **Vật lý Rigidbody2D** — `gravityScale = 0`, `Interpolate` để tránh rung
- [ ] **Camera Damp** — _Không tìm thấy script camera trong thư mục_; nhiều khả năng đang dùng Cinemachine hoặc component riêng chưa vào Scripts/

### Tương tác Player

- [x] **Tương tác vật thể (PlayerInteraction.cs)** — Phát hiện tag `Interactable` qua trigger, nhấn `E` để tương tác; hiện chỉ `Debug.Log`, chưa mở UI thật
- [x] **Chế độ săn bắt (PlayerActions.cs)** — 2 chế độ `Ball` / `Gun`, nhấn `Q`/`R` để đổi chế độ
- [x] **Hồng tâm (Reticle)** — Bám sát chuột, giới hạn `maxRange = 3f`; đổi màu xanh/đỏ theo trong/ngoài tầm
- [x] **Bắn đạn / bóng** — Click trái bắn về tâm hồng tâm, xoay đúng hướng bay, hai prefab riêng (Ball / Bullet)

### Hệ sinh thái quái (Overworld)

- [x] **GrassZoneManager.cs** — Spawn quái trong zone bằng coroutine heartbeat, delay ngẫu nhiên `minSpawnDelay–maxSpawnDelay`, giới hạn `maxThings`, validate điểm spawn (grassLayer, blockLayer, không đè nhau)
- [x] **Weighted random spawn** — `spawnWeight` trong `ThingData`, GetWeightedRandomThing() chính xác
- [x] **AI quái vật 2 kiểu** — `ShadowRoaming.cs`: `isFleeingType` (nhát gan — chạy xa khỏi player) và `isAggressiveType` (hung dữ — lao vào player); random 80%/20% nếu không đặt tay
- [x] **Wander tự động** — PickNewDirection() ngẫu nhiên góc, timer 1.5–4s, đổi hướng khi va MonsterBoundary
- [x] **Detection Range** — CheckForPlayer() dùng `Vector2.Distance`, chuyển sang chaseSpeed khi vào range

### Encounter

- [x] **EncounterProjectile.cs** — Bay đến điểm đích, `OverlapCircle` tại đích để tìm Monster layer
- [x] **Thu phục (Ball)** — `captureChance = 40f`, `Random.Range(0–100)`, thành công → `RuntimeGameState.Party.Add()` + `thing.OnCaptured()`
- [x] **Khiêu chiến (Gun)** — `StartEncounterBattle()` → set `RuntimeGameState.CurrentEnemy` → `SceneManager.LoadScene("BattleScene")`
- [x] **Tấn công bất ngờ** — `ShadowRoaming.OnCollisionEnter2D` khi `isAggressiveType` đụng Player → tự chuyển sang BattleScene
- [x] **Dọn dẹp sau encounter** — `thing.OnCaptured()` → `parentManager.OnThingRemoved()` → giảm `currentActiveThings`, `Destroy(gameObject)`

---

## ✅ Data Layer

- [x] **ThingData.cs** — ScriptableObject đầy đủ: `thingName`, `prefab`, `battlePrefab`, `spawnWeight`, `elementType`, `aiDifficulty`, `archetype`, `defaultMove`, `moves` (List), `AllMoves` property, `hp`, `attack`, `defense`, `spAtk`, `spDef`, `speed`, `level`, `luck`, `moveRange`; **thêm `footprint` (ThingFootprint enum)** để cấu hình kích thước chiếm ô
- [x] **MoveData.cs** — ScriptableObject đầy đủ với tất cả enums: `ElementType` (10 hệ), `MoveCategory` (Physical/Special/Status/Weather/Terrain), `StatusSubType`, `AttackShape` (5 dạng), `WeatherType` (None/Blizzard/MagneticField), `TerrainEffectType` (None/ThornTrap/BurnMark); **thêm `Luck` vào enum `StatType`** để chiêu buff có thể tăng luck
- [x] **AIDifficulty.cs** — Enum `AIDifficulty` (Easy/Medium/Hard/Ultra) + `ThingArchetype` (Attacker/Defender/Setup)
- [ ] **ThingCollection.cs** — File tồn tại nhưng _thân class rỗng_, chưa implement
- [ ] **SaveLoad scripts** — Thư mục `Scripts/SaveLoad/` tồn tại nhưng _không có file `.cs` nào_

---

## ✅ Core / Bridge

- [x] **RuntimeGameState.cs** — Static class: `CurrentEnemy`, `Party` (List<ThingData>), `ActiveThing` (Party[0]), `ResetForNewSession()`; thêm `BookInventory` (List<BookEntry>), `AddBook()`, `UseBook()`
- [x] **SetStarterThing.cs** — Gán `starterThing` vào Party trong `Awake()` nếu Party chưa có dữ liệu (đảm bảo trước `BattleManager.Start()`)

---

## ✅ Battle — Scene Flow & Spawn

- [x] **BattleManager.cs** — Đọc `RuntimeGameState.ActiveThing` (team 0, col 3, row 4) và `CurrentEnemy` (team 1, col 14, row 4), dùng `battlePrefab` nếu có, fallback nếu không
- [x] **HP bar spawn** — Instantiate `hpBarPrefab`, gọi `hpBar.Init(entity.transform)` + `SetHp()`
- [x] **BattleResultManager.cs** — `CheckBattleEnd()` tìm team 0 và team 1 bằng `FindObjectsByType`, trả về Overworld sau `0.6s` qua `SceneManager.LoadScene("OverworldScene")`

---

## ✅ Battle — Grid System

- [x] **GridPos.cs** — Struct tọa độ ô lưới (col, row)
- [x] **BattleGridConfig.cs** — ScriptableObject: `boardCols=8`, `boardRows=8`, `gapCols=2` → tổng 18 cột; helpers `GetTeam()`, `IsWalkable()`, `IsInBounds()`, `IsGap()`
- [x] **BattleGridManager.cs** — Singleton, build 18×8 grid với 7 Tilemaps (Left/Right/Gap/Highlight/Grid/Terrain/**Footprint**), entity tracking qua `Dictionary<GridPos, BattleEntity>`
- [x] **Grid mờ border 1px** — `CreateGridTile()` sinh sprite 32×32 runtime, border trắng 25% trong suốt
- [x] **Terrain tiles runtime** — `CreateSolidColorTile()` tạo tile màu tùy ý (burn = cam 55%, thorn = xanh lá 55%)
- [x] **Footprint tiles runtime** — `tileFootprintPlayer` (xanh 35%), `tileFootprintEnemy` (đỏ 35%) sinh runtime; `ShowFootprint()` / `ClearFootprint()` / `RefreshAllFootprints()` sau mỗi lần di chuyển
- [x] **FitCamera()** — Tính orthographicSize theo gridWidth/gridHeight × 1.1 margin, center chính xác
- [x] **PlaceEntity / MoveEntity** — Cập nhật toàn bộ ô trong footprint của entity vào `_occupied` dict; **PlaceEntity & MoveEntity giờ nhận biết footprint đa ô**
- [x] **MoveEntitySmooth()** — Coroutine Lerp với `duration`; update `_occupied` footprint ngay đầu rồi lerp visual; gọi `RefreshAllFootprints()` sau di chuyển
- [x] **GetFootprintCells()** — Trả về danh sách ô chiếm dựa trên `ThingFootprint`: `Size1x1` (1 ô), `Size2x2` (4 ô góc phải-trên), `Size3x3` (9 ô tâm), `Cross1` (tâm + 4 ô chữ thập); lọc ô ngoài bảng
- [x] **GetAllEntities()** — Dùng `HashSet` để loại trùng entity chiếm nhiều ô
- [x] **ShowHighlight / ShowHighlightColored / ShowAoEPreview / ClearHighlight** — Đầy đủ
- [x] **GetAoECells()** — Hỗ trợ tất cả 5 AttackShape: Single, Cross, Square2x2, Square3x3, Line

---

## ✅ Battle — Command Phase

- [x] **BattlePhaseManager.cs** — Singleton, enum `BattlePhase` (Idle/Command/Execution/Judge/Result), quản lý `_commands` dict, auto-trigger ExecutionPhase khi đủ lệnh
- [x] **Vòng đời phase** — Command → Execution → Judge → (Result hoặc loop Command); gọi `entity.OnTurnStart()` và `TerrainManager.OnTurnStart()` đầu mỗi lượt
- [x] **CommandPhaseController.cs** — Singleton, 3 bước: `SelectMove → SelectSkill → SelectAttack`
- [x] **Chebyshev move range** — `GetReachableCells()` dùng `Mathf.Max(|dc|,|dr|)`, chỉ trong sân của team
- [x] **Attack range** — Toàn bộ sân địch; Weather Both → không cần chọn ô; Terrain → có thể chọn ô trong cả sân địch hoặc tùy `weatherTarget`
- [x] **StepBack()** — Lùi qua 3 bước đầy đủ; lùi qua nhiều unit nếu có
- [x] **AoE hover preview** — `UpdateAoEPreview()` real-time mỗi frame khi hover ô tấn công, color-coded theo `MoveCategory`
- [x] **MoveSelectionUI** — Panel sinh `MoveButtonUI` động, `OnCancelButtonClicked → StepBack()`
- [x] **BattleCommand struct** — `moveTarget`, `attackTarget`, `HasAttack`; static factories `StayAndAttack / MoveOnly / MoveAndAttack`
- [x] **PP Management** — `GetCurrentPP()` dùng `TryGetValue`, khởi tạo PP dict từ `AllMoves`; tracking PP per move trong trận
- [x] **Input Lock/Unlock** — Lock input khi chuyển sang lượt enemy AI, unlock khi quay lại lượt player; tránh click nhầm khi AI đang xử lý

---

## ✅ Battle — Enemy AI (EnemyAIBrain.cs)

- [x] **Tách file riêng** — `EnemyAIBrain` là static class độc lập, CommandPhaseController không chứa AI cũ
- [x] **4 mức độ khó** — Easy / Medium / Hard / Ultra với logic riêng cho từng mức
- [x] **3 archetype** — Attacker (85% chọn chiêu cao nhất), Defender (buff mỗi 3 lượt), Setup (status 2 lượt đầu)
- [x] **PickMove theo difficulty** — Easy: 15% bỏ lượt + chiêu yếu nhất; Medium: random usable move; Hard/Ultra: tìm chiêu khắc type trước
- [x] **PickMoveTarget theo difficulty** — Easy: di chuyển hàng ngang; Medium: random; Hard: tiên đoán vị trí; Ultra: tránh AoE của player (85%)
- [x] **PickAttackTarget** — Hard/Ultra: dự đoán vị trí + AoE center tối ưu; Easy/Medium: ưu tiên target HP thấp nhất
- [x] **FilterOutHarmfulTerrain** — AI tránh ô địa hình có hại khi di chuyển
- [x] **FindFinishTarget** — Ưu tiên tấn công target có thể kill ngay lượt này
- [x] **Enemy Force Idle (Debug)** — `BattleDebugController.enemyForceIdle` flag; khi bật enemy luôn submit lệnh đứng yên để test chiêu thức; không ảnh hưởng build production

---

## ✅ Battle — Combat Calculation

- [x] **HP Formula V2** — `MaxHP = floor(Base × 8 × Level / 100) + Level + 150`
- [x] **Damage Formula** — `BaseDmg = floor(((2×Level/5 + 2) × Power × Atk/Def) / 10)`
- [x] **STAB x1.2** — Áp dụng khi chiêu cùng hệ với attacker, bỏ qua Neutral
- [x] **Bảng tương khắc 10 hệ** — `TypeChart[10,10]`, hệ số **1.35 / 1.0 / 0.75 / 0.0**
- [x] **Crit Rate động** — `5 + EffectiveLuck/10 %`, nhân `x1.5` khi trúng
- [x] **Evasion Rate** — `EffectiveLuck/10 %`; mặc định `luck=0` → 0% né
- [x] **RNG 0.9–1.0** — Áp dụng cho mọi đòn
- [x] **Stat Stage Multiplier** — `Max(2,2+stage) / Max(2,2-stage)`, stage -6 ↔ +6
- [x] **AoE Falloff 3×3** — Tâm x1.0 / Cận tâm x0.8 / Góc chéo x0.7
- [x] **AoE Falloff 2×2** — RNG kép: chung × [0.85–1.0] → tổng [0.765–1.0]
- [x] **Physical / Special split** — Dùng `attack/defense` hoặc `spAtk/spDef` tùy `MoveCategory`
- [x] **Speed sort trong JudgePhase** — Sort giảm dần `EffectiveSpeed` (đã tính stage) _(đã sửa từ `Speed` base)_
- [x] **Luck buff system** — `_luckBonus` flat, 1 stage = +50 luck; `EffectiveLuck = Data.luck + _luckBonus`; dùng `EffectiveLuck` trong crit và evasion
- [x] **ResolveTargetsWithDistance** — Tính `distType` nhỏ nhất (0/1/2) cho mỗi entity bị trúng; entity chiếm nhiều ô nhận dame theo ô gần tâm nhất (mạnh nhất)

---

## ✅ Battle — Effects System

- [x] **KnockbackEffect** — `MoveEffect` subclass với `pushDistance`; đẩy target lùi theo hướng từ attacker → target; dừng khi gặp tường/ô bị chiếm/ngoài bảng
- [x] **Knockback + AoE** — Set `aoeShape` và `aoeRadius` của `KnockbackEffect` giống `DamageEffect` trong Inspector để đẩy đúng toàn bộ target trong vùng chiêu
- [x] **CalcKnockbackDir** — Tính hướng đẩy 8 hướng từ `attacker.GridPos → target.GridPos`, normalize về (-1/0/+1)
- [x] **FindKnockbackDest** — Duyệt từng bước theo hướng, dừng khi out of bounds / không walkable / ô đã có entity

---

## ✅ Battle — Terrain & Weather

### Địa Hình (TerrainManager.cs)

- [x] **PlaceTerrain()** — Đặt ô địa hình lên `tilemapTerrain`, lưu dict với duration; **lỗi đã sửa**: `BuildTerrainMoveData()` trong `BattlePhaseManager` thiếu copy `effects`; đã thêm `tmp.effects = new List<MoveEffect> { te }`
- [x] **Bẫy Gai (ThornTrap)** — Entity bước vào → `LockMovementNextTurn()`; `OnTurnEnd` chỉ lock khi `CanMove == true`
- [x] **Vết Cháy (BurnMark)** — Entity bước vào: nhận 10% MaxHP damage; ở lại cuối lượt: nhận thêm 10% MaxHP damage; tính type multiplier Fire vs defender
- [x] **OnTurnEnd** — Giảm duration, xóa tile và dict entry khi hết hạn
- [x] **OnTurnStart** — Hook để reset `_enteredThisTurn` đầu lượt
- [ ] **Terrain highlight trên sân** — `tilemapTerrain` cần kéo vào Inspector
- [x] **Terrain gắn Owner Team** — `PlaceTerrain()` lưu thêm `ownerTeamId` để phân biệt địa hình của phe nào đặt

### Thời Tiết (WeatherManager.cs)

- [x] **Bão Tuyết (Blizzard)** — `GetEffectiveAoE()` expand AoE khi Blizzard active; **lỗi đã sửa**: `BuildWeatherMoveData()` thiếu copy `effects`; đã thêm `tmp.effects = new List<MoveEffect> { we }`
- [x] **Từ Trường (MagneticField)** — `IsMagneticFieldActive(teamId)` per team; `_moveCountThisCycle` đếm 3 lượt → khoá lượt 3
- [x] **ApplyWeather()** — Áp thời tiết mới với duration
- [x] **OnTurnEnd** — Giảm duration, reset về `None` khi hết
- [x] **WeatherTarget** — Both / TeamLeft / TeamRight; auto submit nếu Both
- [ ] **Weather Visual Effect** — Cần tạo `WeatherVFXController.cs` + Particle prefab cho Blizzard / MagneticField

---

## ✅ Battle — Visual Feedback

- [x] **DamagePopup.cs** — Float up `1.5f` trong `0.8s`, fade alpha, chí mạng: **bold + màu vàng**
- [x] **EntityHpBar.cs** — World Space Canvas, bám `_target.position + Vector3.up * 0.8f`, đổi màu xanh → vàng → đỏ theo % HP
- [x] **MoveButtonUI.cs** — Màu nền per category, icon hệ nguyên tố, power/PP display; hỗ trợ Weather và Terrain
- [x] **MoveSelectionUI.cs** — Singleton panel, sinh button động từ `AllMoves`, Cancel → StepBack

---

## ✅ Battle — Action Panel & Book System

- [x] **BattleActionPanel.cs** — Panel 3 nút: Fight (mở MoveSelectionUI), Flee (thoát trận với xác suất), Capture (mở BookSelectionUI); player turn bị skip nếu flee/capture thất bại; **Cancel trong BookSelectionUI → quay lại BattleActionPanel** (ESC/RClick cần thêm `Update()` vào BookSelectionUI)
- [x] **BookSelectionUI.cs** — UI danh sách sách trong inventory, hiển thị tên + bonus, xử lý inventory rỗng; nút Cancel → `_onChosen(null)` → back về BattleActionPanel
- [x] **BookData.cs** — ScriptableObject dữ liệu sách: tên, mô tả, hệ số tăng tỷ lệ bắt
- [x] **BookEntry.cs** — Struct lưu 1 entry sách trong inventory (BookData + số lượng)
- [x] **Capture chance theo level địch** — `Clamp(60 - enemyLevel * 3 + book.captureRateBonus, 5, 95)`
- [x] **RuntimeGameState mở rộng** — Thêm `BookInventory` (List<BookEntry>), `AddBook()`, `UseBook()` quản lý book inventory xuyên scene

---

## ✅ Battle — Footprint System

- [x] **ThingFootprint enum** — `Size1x1`, `Size2x2`, `Size3x3`, `Cross1`; khai báo trong `ThingData.cs`
- [x] **ThingData.footprint field** — Header "Phạm Vi Chiếm Ô", mặc định `Size1x1`
- [x] **GetFootprintCells()** — `BattleGridManager` tính danh sách ô chiếm theo từng shape; lọc ô ngoài bảng
- [x] **PlaceEntity / MoveEntity / RemoveEntity** — Toàn bộ đọc footprint; entity 2×2 chiếm 4 ô trong `_occupied`
- [x] **MoveEntitySmooth() footprint-aware** — Xóa footprint cũ → ghi footprint mới → lerp visual → `RefreshAllFootprints()`
- [x] **GetAllEntities() dedup** — HashSet loại trùng entity chiếm nhiều ô
- [x] **tilemapFootprint** — Tilemap riêng (field `tilemapFootprint`) vẽ màu entity chiếm; player xanh 35%, enemy đỏ 35%
- [x] **ShowFootprint / ClearFootprint / RefreshAllFootprints** — Tự động refresh sau mỗi lần di chuyển

---

## 🔧 Cấu Hình Kỹ Thuật Hiện Tại

| Thông số              | Giá trị                              |
| --------------------- | ------------------------------------ |
| PPU (Pixels Per Unit) | 32                                   |
| Grid Cell Size        | (1, 1, 0)                            |
| Kích thước lưới       | 18 cột × 8 hàng                      |
| Sân trái (Team 0)     | col 0–7                              |
| Gap                   | col 8–9                              |
| Sân phải (Team 1)     | col 10–17                            |
| Camera Size           | tự tính qua FitCamera() + 10% margin |
| Player spawn          | col 3, row 4                         |
| Enemy spawn           | col 14, row 4                        |
| STAB multiplier       | ×1.2                                 |
| TypeChart multiplier  | 1.35 / 1.0 / 0.75 / 0.0              |
| Crit multiplier       | ×1.5                                 |
| Base crit rate        | 5% + EffectiveLuck/10                |
| Luck buff per stage   | +50 flat                             |

---

## 📁 Files Quan Trọng

| File                        | Thư mục                     | Chức năng chính                                        |
| --------------------------- | --------------------------- | ------------------------------------------------------ |
| `PlayerMovement.cs`         | `Scripts/Player/`           | Di chuyển 8 hướng, Rigidbody2D, Animator               |
| `PlayerActions.cs`          | `Scripts/Player/`           | Chế độ Ball/Gun, hồng tâm, bắn projectile              |
| `PlayerInteraction.cs`      | `Scripts/Player/`           | Trigger tương tác (tag Interactable, nhấn E)           |
| `GrassZoneManager.cs`       | `Scripts/World/Encounters/` | Spawn zone, weighted random, heartbeat                 |
| `ShadowRoaming.cs`          | `Scripts/World/Encounters/` | AI quái Nhát gan/Hung dữ, wander, aggro                |
| `EncounterProjectile.cs`    | `Scripts/World/Encounters/` | Bay đến đích, thu phục hoặc khiêu chiến                |
| `RuntimeGameState.cs`       | `Scripts/Core/`             | Bridge data xuyên scene                                |
| `SetStarterThing.cs`        | `Scripts/`                  | Nạp pet mặc định vào Party                             |
| `ThingData.cs`              | `Scripts/Data/`             | ScriptableObject stats Thing + footprint               |
| `MoveData.cs`               | `Scripts/Data/`             | ScriptableObject chiêu thức + enums (có StatType.Luck) |
| `AIDifficulty.cs`           | `Scripts/Data/`             | Enum difficulty + archetype                            |
| `GridPos.cs`                | `Scripts/Combat/`           | Struct tọa độ ô lưới                                   |
| `BattleGridConfig.cs`       | `Scripts/Combat/`           | ScriptableObject cấu hình lưới                         |
| `BattleGridManager.cs`      | `Scripts/Combat/`           | Singleton build/quản lý lưới, tiles, AoE, footprint   |
| `BattleEntity.cs`           | `Scripts/Combat/`           | HP, EffectiveLuck, \_luckBonus, stages                 |
| `BattleManager.cs`          | `Scripts/Combat/`           | Spawn 2 phe từ RuntimeGameState                        |
| `BattlePhaseManager.cs`     | `Scripts/Combat/`           | Vòng đời phase, execution, judge, weather/terrain      |
| `BattleResultManager.cs`    | `Scripts/Combat/`           | Check kết thúc trận, về Overworld                      |
| `BattleCommand.cs`          | `Scripts/Combat/`           | Struct lệnh mỗi lượt                                   |
| `BattleDebugController.cs`  | `Scripts/Combat/`           | MonoBehaviour debug: `enemyForceIdle` flag cho testing |
| `CommandPhaseController.cs` | `Scripts/Combat/`           | Input người chơi, AoE preview, StepBack                |
| `CombatCalculator.cs`       | `Scripts/Combat/`           | Công thức GDD: HP, damage, STAB, type, crit, falloff   |
| `EnemyAIBrain.cs`           | `Scripts/Combat/`           | AI địch 4 mức × 3 archetype; check forceIdle đầu Decide() |
| `TerrainManager.cs`         | `Scripts/Combat/`           | Địa hình: đặt, hiệu ứng, hết hạn                       |
| `WeatherManager.cs`         | `Scripts/Combat/`           | Thời tiết: Blizzard, MagneticField, duration           |
| `DamagePopup.cs`            | `Scripts/UI/`               | Số sát thương bay, crit vàng/bold                      |
| `EntityHpBar.cs`            | `Scripts/UI/`               | HP bar World Space theo đầu entity                     |
| `MoveButtonUI.cs`           | `Scripts/UI/`               | Nút chiêu styled per category                          |
| `MoveSelectionUI.cs`        | `Scripts/UI/`               | Panel chọn chiêu, Cancel → StepBack                    |
| `BattleActionPanel.cs`      | `Scripts/UI/`               | Panel Fight/Flee/Capture trong trận chiến              |
| `BookSelectionUI.cs`        | `Scripts/UI/`               | UI chọn sách khi thu phục                              |
| `BookData.cs`               | `Scripts/Data/`             | ScriptableObject dữ liệu sách                          |
| `BookEntry.cs`              | `Scripts/Data/`             | Entry sách trong inventory                             |
| `MoveEffect.cs`             | `Scripts/Data/`             | Class hiệu ứng chiêu thức (tách từ MoveData); có KnockbackEffect |
| `EffectResult.cs`           | `Scripts/Data/`             | Struct kết quả sau áp effect                           |

---

## ⏳ Chưa Làm / Còn Thiếu

| Hạng mục                      | Trạng thái                                                                                                         |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| PlayerInteraction thật        | Hiện chỉ `Debug.Log`, chưa mở UI/action                                                                            |
| ThingCollection.cs            | Class rỗng, chưa implement                                                                                         |
| SaveLoad system               | Thư mục tồn tại nhưng chưa có file `.cs` nào                                                                       |
| tilemapTerrain Inspector      | Phải kéo tay Tilemap_Terrain vào field `tilemapTerrain` của `BattleGridManager`; nếu null → terrain không hiển thị |
| tilemapFootprint Inspector    | Phải kéo tay Tilemap_Footprint vào field `tilemapFootprint` của `BattleGridManager`; nếu null → footprint không hiển thị |
| Weather Visual Effect         | Cần tạo `WeatherVFXController.cs` + Particle System prefab cho Blizzard / MagneticField                            |
| Multi-unit battle             | Hiện spawn mỗi phe 1 unit (col cố định)                                                                            |
| Terrain AoE preview tách biệt | `tilemapTerrainPreview` chưa tạo; preview hiện đang đè lên highlight valid cells khi di chuột                      |
| ESC/RClick back BookSelectionUI | Nút Cancel đã hoạt động; cần thêm `Update()` vào `BookSelectionUI` để bắt ESC / RClick                           |

X Thêm tùy chọn phạm vi cho things như chiếm 1x1, 2x2, hay 3x3 ✅ (đã làm — ThingFootprint + GetFootprintCells)
X Thêm effect đẩy lùi vào move (đẩy lùi so với tâm chiêu hiện tại với things địch) ✅ (đã làm — KnockbackEffect + pushDistance; set aoeShape giống DamageEffect trong Inspector)
X Thêm cơ chế enemy đứng yên để test chiêu ✅ (đã làm — BattleDebugController.enemyForceIdle)

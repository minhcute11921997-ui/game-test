# Tiến Độ Dự Án

> Cập nhật: đối chiếu trực tiếp từng dòng code tất cả 29 file `.cs` trong `Assets/_Project/Scripts`.

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

- [x] **ThingData.cs** — ScriptableObject đầy đủ: `thingName`, `prefab`, `battlePrefab`, `spawnWeight`, `elementType`, `aiDifficulty`, `archetype`, `defaultMove`, `moves` (List), `AllMoves` property, `hp`, `attack`, `defense`, `spAtk`, `spDef`, `speed`, `level`, `luck`, `moveRange`
- [x] **MoveData.cs** — ScriptableObject đầy đủ với tất cả enums: `ElementType` (10 hệ), `MoveCategory` (Physical/Special/Status/Weather/Terrain), `StatusSubType`, `AttackShape` (5 dạng), `WeatherType` (None/Blizzard/MagneticField), `TerrainEffectType` (None/ThornTrap/BurnMark), `WeatherTarget`
- [x] **AIDifficulty.cs** — Enum `AIDifficulty` (Easy/Medium/Hard/Ultra) + `ThingArchetype` (Attacker/Defender/Setup)
- [ ] **ThingCollection.cs** — File tồn tại nhưng _thân class rỗng_, chưa implement
- [ ] **SaveLoad scripts** — Thư mục `Scripts/SaveLoad/` tồn tại nhưng _không có file `.cs` nào_

---

## ✅ Core / Bridge

- [x] **RuntimeGameState.cs** — Static class: `CurrentEnemy`, `Party` (List<ThingData>), `ActiveThing` (Party[0]), `ResetForNewSession()`
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
- [x] **BattleGridManager.cs** — Singleton, build 18×8 grid với 6 Tilemaps (Left/Right/Gap/Highlight/Grid/Terrain), entity tracking qua `Dictionary<GridPos, BattleEntity>`
- [x] **Grid mờ border 1px** — `CreateGridTile()` sinh sprite 32×32 runtime, border trắng 25% trong suốt
- [x] **Terrain tiles runtime** — `CreateSolidColorTile()` tạo tile màu tùy ý (burn = cam 55%, thorn = xanh lá 55%)
- [x] **FitCamera()** — Tính orthographicSize theo gridWidth/gridHeight × 1.1 margin, center chính xác
- [x] **PlaceEntity / MoveEntity** — Update cả `_occupied` dict và `transform.position` đồng thời
- [x] **MoveEntitySmooth()** — Coroutine Lerp với `duration`; có callback `onArrived`
- [x] **ShowHighlight / ShowHighlightColored / ShowAoEPreview / ClearHighlight** — Đầy đủ
- [x] **GetAoECells()** — Hỗ trợ tất cả 5 AttackShape: Single, Cross (vòng lặp radius theo 4 hướng), Square2x2, Square3x3, Line

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

---

## ✅ Battle — Combat Calculation

- [x] **HP Formula V2** — `MaxHP = floor(Base × 8 × Level / 100) + Level + 150`
- [x] **Damage Formula** — `BaseDmg = floor(((2×Level/5 + 2) × Power × Atk/Def) / 10)`
- [x] **STAB x1.2** — Áp dụng khi chiêu cùng hệ với attacker, bỏ qua Neutral _(lưu ý: code dùng 1.2f, không phải 1.5f như một số ghi chú trước)_
- [x] **Bảng tương khắc 9 hệ** — `TypeChart[10,10]`, hệ số **1.35 / 1.0 / 0.75 / 0.0** _(lưu ý: code dùng 1.35/0.75 thay vì 2.0/0.5 — cân bằng game hơn)_
- [x] **Crit Rate động** — `5 + Luck/10 %`, nhân `x1.5` khi trúng
- [x] **Evasion Rate** — `Luck/10 %`
- [x] **RNG 0.9–1.0** — Áp dụng cho mọi đòn
- [x] **Stat Stage Multiplier** — `Max(2,2+stage) / Max(2,2-stage)`, stage -6 ↔ +6
- [x] **AoE Falloff 3×3** — Tâm x1.0 / Cận tâm x0.8 / Góc chéo x0.7
- [x] **AoE Falloff 2×2** — RNG kép: chung × [0.85–1.0] → tổng [0.765–1.0]
- [x] **Physical / Special split** — Dùng `attack/defense` hoặc `spAtk/spDef` tùy `MoveCategory`
- [x] **Speed sort trong JudgePhase** — Sort giảm dần Speed, entity nhanh hơn ra đòn trước

---

## ✅ Battle — Terrain & Weather

### Địa Hình (TerrainManager.cs)

- [x] **PlaceTerrain()** — Đặt ô địa hình lên `tilemapTerrain`, lưu dict với duration
- [x] **Bẫy Gai (ThornTrap)** — Entity bước vào → `LockMovementNextTurn()` trên `BattleEntity`; `OnTurnEnd` chỉ lock khi `CanMove == true` để tránh lock mãi
- [x] **Vết Cháy (BurnMark)** — `TerrainEffectType` và tile tồn tại; effect runtime cần xác nhận trong `TerrainManager.OnEntityEnterCell`
- [x] **OnTurnEnd** — Giảm duration, xóa tile và dict entry khi hết hạn; áp địa hình lên tất cả entity còn sống
- [x] **OnTurnStart** — Hook để reset flag nếu cần đầu lượt

### Thời Tiết (WeatherManager.cs)

- [x] **Bão Tuyết (Blizzard)** — `GetEffectiveAoE()` expand AoE khi Blizzard active (ví dụ Single → Square3x3)
- [x] **Từ Trường (MagneticField)** — `IsMagneticFieldActive(teamId)` per team; `BattleEntity._moveCountThisCycle` đếm 3 lượt di chuyển liên tiếp → khoá lượt 3
- [x] **ApplyWeather()** — Áp thời tiết mới với duration
- [x] **OnTurnEnd** — Giảm duration, reset về `None` khi hết
- [x] **WeatherTarget** — Both / TeamLeft / TeamRight; `CommandPhaseController` tự submit nếu Both (không cần chọn ô)

---

## ✅ Battle — Visual Feedback

- [x] **DamagePopup.cs** — `Resources.Load("DamagePopup")`, float up `1.5f` trong `0.8s`, fade alpha, chí mạng: **bold + màu vàng**
- [x] **EntityHpBar.cs** — World Space Canvas, `LateUpdate` bám `_target.position + Vector3.up * 0.8f`, đổi màu xanh → vàng → đỏ theo % HP
- [x] **MoveButtonUI.cs** — Nút chiêu đầy đủ: màu nền per category, icon hệ nguyên tố, power/PP display; hỗ trợ Weather (xanh biển) và Terrain (xanh lá đậm)
- [x] **MoveSelectionUI.cs** — Singleton panel, sinh button động từ `AllMoves`, Show/Hide, Cancel → StepBack

---

## 🔧 Cấu Hình Kỹ Thuật Hiện Tại

| Thông số                            | Giá trị                              |
| ----------------------------------- | ------------------------------------ |
| PPU (Pixels Per Unit)               | 32                                   |
| Grid Cell Size                      | (1, 1, 0)                            |
| Kích thước lưới                     | 18 cột × 8 hàng                      |
| Sân trái (Team 0)                   | col 0–7                              |
| Gap                                 | col 8–9                              |
| Sân phải (Team 1)                   | col 10–17                            |
| Camera Size                         | tự tính qua FitCamera() + 10% margin |
| Player spawn                        | col 3, row 4                         |
| Enemy spawn                         | col 14, row 4                        |
| STAB multiplier (code thực tế)      | ×1.2                                 |
| TypeChart multiplier (code thực tế) | 1.35 / 1.0 / 0.75 / 0.0              |
| Crit multiplier                     | ×1.5                                 |
| Base crit rate                      | 5% + Luck/10                         |

---

## 📁 Files Quan Trọng

| File                        | Thư mục                     | Chức năng chính                                            |
| --------------------------- | --------------------------- | ---------------------------------------------------------- |
| `PlayerMovement.cs`         | `Scripts/Player/`           | Di chuyển 8 hướng, Rigidbody2D, Animator                   |
| `PlayerActions.cs`          | `Scripts/Player/`           | Chế độ Ball/Gun, hồng tâm, bắn projectile                  |
| `PlayerInteraction.cs`      | `Scripts/Player/`           | Trigger tương tác (tag Interactable, nhấn E)               |
| `GrassZoneManager.cs`       | `Scripts/World/Encounters/` | Spawn zone, weighted random, heartbeat                     |
| `ShadowRoaming.cs`          | `Scripts/World/Encounters/` | AI quái Nhát gan/Hung dữ, wander, aggro                    |
| `EncounterProjectile.cs`    | `Scripts/World/Encounters/` | Bay đến đích, thu phục hoặc khiêu chiến                    |
| `RuntimeGameState.cs`       | `Scripts/Core/`             | Bridge data xuyên scene                                    |
| `SetStarterThing.cs`        | `Scripts/`                  | Nạp pet mặc định vào Party                                 |
| `ThingData.cs`              | `Scripts/Data/`             | ScriptableObject stats Thing                               |
| `MoveData.cs`               | `Scripts/Data/`             | ScriptableObject chiêu thức + enums                        |
| `AIDifficulty.cs`           | `Scripts/Data/`             | Enum difficulty + archetype                                |
| `GridPos.cs`                | `Scripts/Combat/`           | Struct tọa độ ô lưới                                       |
| `BattleGridConfig.cs`       | `Scripts/Combat/`           | ScriptableObject cấu hình lưới                             |
| `BattleGridManager.cs`      | `Scripts/Combat/`           | Singleton build/quản lý lưới, tiles, AoE                   |
| `BattleEntity.cs`           | `Scripts/Combat/`           | Component entity: HP, terrain/weather hooks, AI state      |
| `BattleManager.cs`          | `Scripts/Combat/`           | Spawn 2 phe từ RuntimeGameState                            |
| `BattlePhaseManager.cs`     | `Scripts/Combat/`           | Vòng đời phase, execution, judge, weather/terrain dispatch |
| `BattleResultManager.cs`    | `Scripts/Combat/`           | Check kết thúc trận, về Overworld                          |
| `BattleCommand.cs`          | `Scripts/Combat/`           | Struct lệnh mỗi lượt                                       |
| `CommandPhaseController.cs` | `Scripts/Combat/`           | Input người chơi, AoE preview, StepBack                    |
| `CombatCalculator.cs`       | `Scripts/Combat/`           | Công thức GDD: HP, damage, STAB, type, crit, falloff       |
| `EnemyAIBrain.cs`           | `Scripts/Combat/`           | AI địch 4 mức × 3 archetype                                |
| `TerrainManager.cs`         | `Scripts/Combat/`           | Địa hình: đặt, hiệu ứng, hết hạn                           |
| `WeatherManager.cs`         | `Scripts/Combat/`           | Thời tiết: Blizzard, MagneticField, duration               |
| `DamagePopup.cs`            | `Scripts/UI/`               | Số sát thương bay, crit vàng/bold                          |
| `EntityHpBar.cs`            | `Scripts/UI/`               | HP bar World Space theo đầu entity                         |
| `MoveButtonUI.cs`           | `Scripts/UI/`               | Nút chiêu styled per category                              |
| `MoveSelectionUI.cs`        | `Scripts/UI/`               | Panel chọn chiêu, Cancel → StepBack                        |

---

## ⏳ Chưa Làm / Còn Thiếu

| Hạng mục                    | Trạng thái                                                                      |
| --------------------------- | ------------------------------------------------------------------------------- |
| PlayerInteraction thật      | Hiện chỉ `Debug.Log`, chưa mở UI/action                                         |
| ThingCollection.cs          | Class rỗng, chưa implement                                                      |
| SaveLoad system             | Thư mục tồn tại nhưng chưa có file `.cs` nào                                    |
| BurnMark effect logic       | Tile tồn tại, chưa xác nhận effect thật trong `OnEntityEnterCell`               |
| Multi-unit battle           | Hiện spawn mỗi phe 1 unit (col cố định)                                         |
| Buff/Debuff stat stage thật | Công thức GetStageMultiplier() có nhưng chưa thấy nơi gọi để áp dụng lên entity |
| Heal logic                  | StatusSubType.Heal tồn tại nhưng chưa thấy handler trong JudgePhase             |

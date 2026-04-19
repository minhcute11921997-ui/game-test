Các tính năng đã hoàn thiện
:
[x] Di chuyển 8 hướng Overworld & Camera Damp.
[x] Vật lý bản đồ và ranh giới.
[x] Hệ sinh thái cỏ (GrassZoneManager) tạo bóng đen ngẫu nhiên.
[x] AI Quái vật (Nhát gan/Hung dữ).
[x] Data Assets ThingData tạo hàng loạt quái.
[x] Tương tác tầm xa (Bóng thu phục / Đạn khiêu chiến).
[x] Minigame thu phục (% bắt & xóa object).
[x] Scene Bridge (Cầu nối dữ liệu từ Overworld sang Battle).
[x] Chuyển cảnh Battle tự động khi va chạm.
[x] Hệ thống Layer và Tag hoàn chỉnh để Radar quét mục tiêu.

Đã hoàn thành flow:

Overworld
→ bắn trúng Shadow / Thing
→ chuyển sang BattleScene
Chi tiết:
Projectile trigger hoạt động
BattleScene đã load đúng
Scene flow chạy thật
2. GlobalBattleBridge hoạt động

Đã dùng static bridge để truyền dữ liệu giữa scene:

GlobalBattleBridge.encounteredThing
Công dụng:
Lưu monster vừa gặp ngoài map
BattleScene đọc lại đúng dữ liệu
3. BattleScene spawn đúng quái đã gặp

Khi encounter:

Slime gặp -> Battle spawn Slime
Wolf gặp -> Battle spawn Wolf
Dragon gặp -> Battle spawn Dragon
Hệ thống dùng:
ThingData.battlePrefab
4. HP battle đọc đúng từ ThingData

Không còn hardcode.

ThingData.hp

được dùng làm máu battle.

Ví dụ:
Slime.hp = 100
Wolf.hp = 180
Dragon.hp = 400
5. Player Starter Prototype

Đã tạo hệ tạm:

GlobalPlayerBridge.activeThing

Để gán pet mặc định của người chơi khi chưa có party system.

6. BattleScene có đủ 2 phe

Hiện tại BattleScene đã spawn:

Bên trái = pet người chơi
Bên phải = quái encounter

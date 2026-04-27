📋 11 Bản Đồ Chiến Thuật — Bản Chốt Cuối Cùng
Bản Đồ 1 — Đồng Cỏ Tinh Thể (Crystal Fields)
Địa hình: Mở, thiên cơ động

Cơ chế:

Các chiêu terrian bị ẩn ui khỏi màn hình

Mảnh Crystal spawn ngẫu nhiên sau khi bị ăn

Snowball hồi máu: Viên 1 hồi 1% MaxHP, viên 2 hồi 2%, viên 3 hồi 3%... cộng dồn theo số viên toàn đội đã ăn

Lợi thế: Pokemon có kỹ năng di chuyển linh hoạt (di chuyển 2 ô, đánh xong lùi 1) farm Crystal hiệu quả nhất

Bản Đồ 2 — Xưởng Máy Rèn (Iron Foundry)
Địa hình: Nguy hiểm, tính giờ

Cơ chế:

Ô Sắt nóng dần qua từng lượt

Chu kỳ 3 lượt: Cuối lượt 3 → ô Sắt phát nổ cross pattern 4 cạnh trực tiếp (không chéo), gây 10% MaxHP dame chuẩn → đếm lại từ đầu

Chiến thuật: Ép góc địch vào gần sắt sắp nổ

Bản Đồ 3 — Đỉnh Núi Băng Giá (Frozen Peaks)
Địa hình: Khắc nghiệt, hao mòn tài nguyên

Cơ chế:

Chu kỳ 3 lượt: Bão tuyết quét toàn bộ lưới 18x8 → trừ 1 PP tất cả chiêu của mọi Pokemon đang trên sân

Chiến thuật: Thiên đường Mecha (PP mở rộng). Đội phụ thuộc chiêu PP thấp phải ra chiêu nhanh hoặc bị đóng băng kho đạn

Bản Đồ 4 — Phế Tích Tự Động (Ancient Ruins)
Địa hình: Tầm nhìn biến đổi liên tục

Cơ chế:

Nhiều cột đá chặn LoS chiêu thẳng

Chu kỳ 3 lượt: Toàn bộ cột đá biến mất 1 lượt → sân trống hoàn toàn → hiện lại

Chiến thuật: Tích trữ chiêu mạnh, xả đúng lượt trống để xuyên sân

Bản Đồ 5 — Đầm Lầy Sương Mù (Gloomy Mire)
Địa hình: Tâm lý chiến Battleship

Cơ chế:

True Blind: 2 bên không nhìn thấy Pokemon của nhau

Delay 2 lượt: Trúng chiêu không báo ngay — 2 lượt sau ô bị trúng mới phát sáng báo vị trí địch (của 2 lượt trước)

Chiến thuật: Gunner và Supernatural lên ngôi. Di chuyển sau mỗi lần tấn công là bắt buộc

Bản Đồ 6 — Đảo Gió Trời (Sky Island)
Địa hình: Quỹ đạo bị bẻ cong

Cơ chế:

Chu kỳ 3 lượt: Gió flip giữa 2 chiều

Isometric (live): Đông ↔ Tây

Test grid ngang: Lên ↔ Xuống

25% tỷ lệ lệch mỗi chiêu khi gió đang thổi

Chiêu lệch 1 ô tại vị trí tung chiêu với không bị lệch ( lệch cùng với hướng gió thổi)

Bản Đồ 7 — Thảo Nguyên Trống (The Empty Plains)
Địa hình: Thuần kỹ năng

Cơ chế: Không có vật cản, không hiệu ứng môi trường

Chiến thuật: Chỉ số gốc, Hệ nguyên tố và Tộc quyết định tất cả

Bản Đồ 8 — Đền Thờ Nhật Thực (Eclipse Temple)
Địa hình: Positioning puzzle

Cơ chế:

Chu kỳ 2 lượt: Sân luân phiên Ánh Sáng ↔ Bóng Tối

Spawn vùng (reset mỗi lần đổi pha), mỗi nửa sân 8×8 độc lập:

Random 2 tâm vùng 2×2 Trắng — các ô không liền kề nhau

Loại vùng Trắng + buffer 1 ô xung quanh khỏi pool

Random 2 tâm vùng 2×2 Đen từ pool còn lại
→ Trắng và Đen không bao giờ đè nhau

Hiệu ứng khi tung chiêu:

Vùng trùng pha sân → không trừ PP

Vùng trái pha sân → chiêu đổi sang H của thing tung chiêu

Ngoài vùng → không có hiệu ứng

Overlap Logic (đè cả 2 vùng): Ưu tiên vùng trùng pha hiện tại,

Bản Đồ 9 — Đấu Trường Tâm Lý (The Colosseum)
Địa hình: Minigame tâm lý

Cơ chế:

Chu kỳ 4 lượt: Trận tạm dừng → bảng Kéo/Búa/Bao hiện lên

Bên thắng: +1 tầm di chuyển toàn bộ Pokemon trên sân trong 2 lượt tiếp theo

Chiến thuật: ảnh hưởng tầm di chuyển

Bản Đồ 10 — Đại Dương Sâu Thẳm (The Deep Ocean)
Địa hình: Áp lực sinh tồn

Cơ chế:

Pokemon đứng trên sân > 5 lượt liên tiếp → từ lượt thứ 6 nhận 10% MaxHP dame chuẩn mỗi lượt

Reset chỉ khi Pokemon được thu hồi và thay bằng con khác (\_turnsOnField về 0)

Chiến thuật: Buộc xoay tua liên tục. Thiên địch của đội đứng yên (Angel chưa đủ 4 lượt vẫn an toàn, Giant cần tính thời điểm rút)

Bản Đồ 11 — Vách Núi Cheo Leo (Treacherous Cliffs)
Địa hình: Displacement mạnh

Cơ chế:

Mọi chiêu trúng đều tạo Knockback 1 ô ra xa tâm chiêu

Chạm rìa sân → nhận dame + tự động rút về

Chiến thuật: Positioning cực kỳ quan trọng, tránh bị dồn vào góc

🗂️ Hạ Tầng Kỹ Thuật cần thêm cho Bản Đồ
System Dùng cho
TurnCycleManager Đỉnh Băng, Xưởng Rèn, Phế Tích, Đảo Gió, Đền Nhật Thực, Đấu Trường
CrystalSpawnManager Đồng Cỏ
DamageDelayQueue Đầm Lầy (delay 2 lượt)
WindDirectionSystem Đảo Gió
ZoneSpawnManager + ZoneOverlapLogic Đền Nhật Thực
KnockbackSystem Vách Núi (đã có base từ hitbox)
\_turnsOnField counter Đại Dương, Angel, Vách Núi
Minigame UI (KBB) Đấu Trường

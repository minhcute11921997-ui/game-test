1. TỔNG QUAN DỰ ÁN
   Thể loại: Tactical RPG / Grid-based Battler (Nhập vai Chiến thuật trên lưới) kết hợp thu thập quái vật và Auto-chess
   .
   Lối chơi cốt lõi: Các monster được gọi là Thing, sẽ thu thập Thing, dàn trận chiến thuật 1vs1 hoặc 2vs2 trên đấu trường lưới chia phe độc lập
   .
   Engine: Unity
   .
   Đồ họa: 2D Pixel Art đồng nhất điểm ảnh (Pixel Perfect), hệ màu AAP-Splendor128
   .
2. HỆ THỐNG ĐỒ HỌA (ART DIRECTION)
   Góc nhìn Lai ghép (Hybrid Perspective): Bản đồ Overworld dùng góc Top-down 3/4 (lưới vuông 32x32px), trong khi Đấu trường dùng góc Isometric chéo 45 độ (lưới thoi 64x32px)
   .
   Pixel Perfect Cứng: Không dùng code để scale (phóng to/thu nhỏ) đối tượng, PPU cố định
   .
   Quy chuẩn Kích thước: Phân loại Thing từ hạng XS (Canvas 40x48px) đến XXL/Boss (Canvas 208x208px) trên đấu trường
   , và kích thước tương ứng thu nhỏ lại trên Overworld
   .
   Quy tắc phối màu: Không dùng màu đen thuần cho viền, bắt buộc có 3 tông màu sáng tối (base/shadow/highlight)
   . Bóng (shadow) được tách riêng thành file ảnh trong suốt đặt dưới chân
   .
3. CƠ CHẾ CHIẾN ĐẤU CỐT LÕI (CORE COMBAT)
   Cấu trúc Sân đấu: 2 sân lưới 8x8 độc lập cho 2 phe (tổng 16x8), không được bước sang sân đối phương
   .
   Hệ thống Lượt đi đồng thời:
   Giai đoạn 1 (Nhập lệnh): Hai bên bí mật chọn lệnh di chuyển và tấn công tọa độ
   .
   Giai đoạn 2 (Thực thi): Đồng thời di chuyển
   .
   Giai đoạn 3 (Phán xét): Thing có Tốc độ (Speed) cao hơn sẽ tung chiêu trước
   .
   Giao diện Tối giản (Minimal UI): Không hiện số máu chính xác (trừ PvP) hay Icon Tộc/Hệ trên đầu
   . Hiện khung Tinh Thể (Crystal Aura) quanh thanh máu để báo hiệu chất lượng nguyên liệu
   .
   Hover-Sync Move UI: Rê chuột trên lưới, mũi tên báo hiệu tương khắc (↑ Xanh, ↓ Đỏ) sẽ hiển thị ngay tại bảng tên chiêu thức để giữ sạch đồ họa
   . Thẻ chiêu thức được phân loại màu nền theo loại sát thương/buff
   .
4. HỆ THỐNG CHỈ SỐ, TỘC & HỆ (STATS, SYNERGY)
   6 Chỉ số cơ bản: HP, Attack, Defense, Sp. Atk, Sp. Def, Speed, MoveRange
   . Tổng chỉ số cơ bản (BST) ngang nhau ở cùng cấp tiến hóa, chỉ phân bổ khác nhau
   .
   Tương khắc Hệ (9 Hệ): Hỏa, Mộc, Thủy, Thổ, Lôi, Phong, Băng, Quang, Ám
   . Nhận thưởng STAB (x1.2 sát thương) khi dùng chiêu trùng hệ
   .
   Spec Tộc — Bản Chốt Cuối Cùng
   2.1 Tộc Cơ Khí (Mecha)
   Mốc 2/3/4 con: +2/+3/+4 PP cho tất cả chiêu của toàn team khi bắt đầu trận

2.2 Tộc Siêu Nhiên (Supernatural)
Chỉ áp dụng cho thing Supernatural khi tung chiêu

Shape Single, Cross, Square2x2, Square3x3 mở rộng thêm 1 ô xung quanh — Line không mở rộng

Cross chỉ mở rộng 4 hướng thẳng, không chéo

Ô rìa chỉ gây dame thuần (không knockback/terrain/weather)

Dame rìa = dame gốc × 30%/40%/50% mốc 2/3/4, tính từ dame gốc trước falloff

2.3 Tộc Thú (Feral)
Khi thing Feral tung chiêu Terrain → lan tỏa 1 ô ngẫu nhiên cùng loại tại ô kế cạnh trống trong cùng sân

Terrain do thing Feral đặt có duration +1/+1/+2 lượt theo mốc 2/3/4

2.4 Tộc Cổ Đại (Ancient)
Mỗi loài Ancient trong đội mang flat stat bonus cộng thẳng vào base stat toàn team khi bắt đầu trận

Tối đa 4 loài × số stat nhỏ (set trực tiếp trên ThingData)

Cộng dồn từ nhiều loài khác nhau

2.5 Tộc Thiên Thần (Angel)
Đếm số lượt đứng trên sân từ khi spawn (\_turnsOnField tăng mỗi OnTurnStart)

Sau đủ 4 lượt → tất cả chiêu Physical và Special của thing Angel đó được cộng +20/+30/+40 Power vào runtime copy

Chỉ kích hoạt 1 lần duy nhất

UI MoveButtonUI hiển thị đúng power mới sau buff

Chỉ áp dụng cho thing Angel đã đủ 4 lượt
nếu thu về thì reset lại lượt đếm
mỗi thing angel có bộ đếm riêng

2.6 Tộc Chiến Binh (Warrior)
Điều kiện: HP > 60% — tụt ≤60% mất hiệu ứng ngay lập tức

Mốc 1 con: Kháng hoàn toàn debuff từ địch + buff/heal từ đồng đội (tự buff/heal bản thân vẫn nhận)

Mốc 2-3 con: Thêm lifesteal 8% dame cuối gây ra — hồi vào chính con tung chiêu, xử lý trước heal cuối lượt

Mốc 4 con: Lifesteal 15%

Chỉ áp dụng cho thing Warrior

2.7 Tộc Khổng Lồ (Giant)
Footprint 3×3, không bị knockback

Di chuyển lượt này → lượt sau không được di chuyển (\_movedLastTurn flag)

Mốc 2/3/4 con: Giảm hit rate toàn sân ta 8%/10%/12% — không cộng dồn nếu 2 Giant

Khi Giant chết/về sách → hiệu ứng kéo dài thêm 3 lượt qua PersistentEffectManager

2.8 Tộc Pháo Thủ (Gunner)
Cuối lượt: random 4/6/9 ô 1×1 trên lưới 18×8 (kể cả gap)

Trúng thing địch: gây 8% MaxHP dame (cộng dồn)

Trúng thing ta: hồi 6% MaxHP (cộng dồn, cap 18%/lượt per entity)

Không cộng dồn số vùng nếu có 2 Gunner trên sân cùng lúc

Khi Gunner chết/về sách → hiệu ứng kéo dài thêm 4 lượt qua PersistentEffectManager

2.9 Tộc Huyền Thoại (Legendary)
Passive aura: Khi thing Legendary tấn công trúng non-Legendary → target bị giảm 10% Def/SpDef trong 2 lượt

Mốc 1/3 con: Giảm 10%/15% dame nhận vào thing Legendary từ non-Legendary

Không cộng dồn nếu có 2 Legendary trên sân

. 5. PHÁT TRIỂN SỨC MẠNH & KỸ NĂNG (PROGRESSION)
Lên cấp & Tiến hóa: Max level 100
. Tiến hóa làm tăng lớn tổng chỉ số (BST)
.
Gacha Kỹ Năng: Lên cấp sẽ Gacha ngẫu nhiên chiêu thức mới, có thể lưu vào "Kho lưu trữ" để mua lại sau
.
Độ tinh thông Kỹ năng (Move Mastery): Nâng cấp chiêu thức ngẫu nhiên hệ số từ x0.9 đến x1.1 (tối đa 3 khe). Cần dùng tiền tệ game để "Tẩy luyện" (Reroll) thành x1.1
. 6. KHÁM PHÁ & THU THẬP (OVERWORLD & LOOTING)
Hệ sinh thái Động: Thing trốn trong bụi cỏ dạng Bóng đen (Shadows)
. Người chơi có thể đặt Mồi nhử (Bait) để tăng tỷ lệ ra quái hiếm
.
Tương tác ngắm bắn trên Map: Phím A để ném bóng (thu phục), Phím B để bắn đạn (vào trận lấy nguyên liệu)
.
Cơ chế Clean Kill (Đánh giá Sao): Hạ gục 1-3 Hit giữ nguyên 3 Sao. Dùng chiêu khắc hệ, đánh nhiều Hit, hoặc dính Thiêu Đốt/Trúng Độc sẽ làm hỏng nguyên liệu (tụt Sao)
. Hiệu ứng Ngủ/Tê liệt tính là 0 Hit
.
Cơ chế Thu phục (Minigame): Ném bóng có vùng ảnh hưởng 3x3. Trúng tâm 1x1 tỷ lệ bắt cao nhất (Perfect Hit). Máu địch càng thấp tỷ lệ càng cao
. Lệnh ném bóng là Hành động ưu tiên (Giai đoạn 0)
. 7. QUẢN LÝ CỨ ĐIỂM & KINH TẾ (BASE BUILDING)
Resort Sinh Thái: Xây chuồng trại theo Hệ cho Thing
.
Kinh tế & Chế tạo: Thing ở nhà tạo ra nguyên liệu thụ động. Ghép nguyên liệu và chiến lợi phẩm 3 Sao để chế bóng/mồi nhử xịn
.
Du lịch: Thing hiếm tạo Điểm Hấp Dẫn kéo NPC đến tham quan và vứt tiền xu. Có thể thuê NPC tự động hóa thu hoạch
. 8. HỆ THỐNG GUILD, NHIỆM VỤ & THI ĐẤU (GUILD & EXAMINER)
Phân cấp (Rank F đến S): Thăng hạng thông qua tích lũy Quota (Nhiệm vụ) và đánh bại Giám khảo (Examiner)
.
Hệ thống Giám Khảo: Cần đặt lịch hẹn qua Trung tâm Guild, chờ 2-3 ngày in-game
. Người chơi có thể đi thu thập "Tình báo" (Intel) tại quán trọ để biết con bài tẩy, map ưa thích của Giám khảo
.
Phân Khu Guild: Thợ Săn (Chiến đấu), Sinh Thái (Bảo tồn), Kỹ Thuật (Chế tạo). Nhiệm vụ và cách NPC đối xử sẽ thay đổi theo phân khu người chơi chọn
.
Drafting Esports: Thể thức chuyên nghiệp "Mang 6 Chọn 4" ra sân
. 10. CỐT TRUYỆN & THẾ GIỚI LORE (STORYLINE)
Khởi đầu (Học viện Tinh thể): Nhập vai học sinh năm cuối. Dùng "Cuốn Sách Phong Ấn" (Grimoire) và các Trang giấy thay vì Pokeball để thu phục
.
Bài kiểm tra: Phục hồi khu Resort, đấu giải vòng bảng hạn chế cấp độ (Soft Level Cap), ép thắng bằng chiến thuật
.
Tốt nghiệp: Thành tích thi đấu Tứ kết -> Chung kết quyết định bằng cấp và nhận Skin Grimoire lung linh tương ứng
. Sau đó gia nhập Guild tham gia thế giới rộng lớn.
Cốt lõi Thế giới: Năng lượng từ Đại tinh thể (Primal Crystals) tạo ra biến thể vùng miền (Regional Variants) và hình thành các Tộc/Hệ. Các thế lực tội phạm muốn khai thác tinh thể tạo ra quái vật cuồng nộ
. Các vùng xuất hiện tinh thể đột biến (Crystal Surge) gia tăng quái hiếm

11. AI enemy

Kiến Trúc Tổng Quan
text
AIBrain = DifficultyProfile × ThingArchetype

Mỗi lượt AI quyết định 3 thứ độc lập:

1. moveTarget — ô sẽ di chuyển đến
2. attackTarget — ô sẽ bắn vào (độc lập hoàn toàn với vị trí đứng)
3. move (skill) — chiêu sẽ dùng
   Nguyên Tắc Chung (Mọi Difficulty)
   Né terrain bất lợi: Ở mọi difficulty, khi chọn moveTarget đều loại trừ ô có terrain gây hại — mức độ ưu tiên tăng dần theo difficulty.

2vs2 — Ưu tiên target: Mặc định nhắm theo difficulty logic bình thường. Ngoại lệ: nếu 1 thing player đang HP thấp (ngưỡng sắp chết) VÀ chiêu đang dùng khắc hệ thing đó → ưu tiên bắn vào nó để kết liễu.

Trục 1 — 4 Difficulty Profile
🟢 EASY — "Hoang Dã Bản Năng"
Quyết định Logic
Di chuyển Chọn 1 hướng cố định đầu battle (random) · 80% giữ hướng, 20% đổi random · Loại trừ ô terrain bất lợi khi có thể
Attack target 70% bắn ô hiện tại player · 30% bắn random 1 ô trong MoveRange của player
Chọn chiêu Luôn chọn chiêu power thấp nhất · 15% bỏ lượt
🟡 MEDIUM — "Học Hỏi"
Quyết định Logic
Di chuyển Random hoàn toàn trong ô hợp lệ · Loại trừ ô terrain bất lợi khi có thể
Attack target 50% bắn ô hiện tại player · 50% bắn random 1 ô trong MoveRange của player
Chọn chiêu Random đều trong toàn bộ moves[]
🔴 HARD — "Chiến Thuật Cơ Bản"
Quyết định Logic
Di chuyển Tính hướng từ vị trí enemy → ô player đã bắn vào lượt trước (8 hướng) · 65% di chuyển random vào ô trong MoveRange thuộc 7 hướng còn lại · 35% di chuyển random vào ô trong MoveRange thuộc hướng đó · Sau đó loại trừ terrain bất lợi
Attack target Predicted Position: predictedPos = player.CurrentPos + player.LastMoveDir · Nếu AoE → tìm tâm cover nhiều ô MoveRange player nhất
Chọn chiêu Ưu tiên chiêu khắc hệ player · Không có → chiêu power cao nhất
🔴🔴 ULTRA — "Omniscient Có Kiểm Soát"
Quyết định Logic
Di chuyển Biết trước AoE thật của player lượt này · 85% → ô tốt nhất ngoài AoE · 15% → ô tốt nhất trong AoE (vẫn chọn ô có lợi nhất trong vùng bị trúng) · Loại trừ terrain bất lợi
Attack target 85% bắn ô player sẽ đến · 15% bắn ô player hiện tại
Chọn chiêu Khắc hệ → ưu tiên tuyệt đối · Xen chiêu buff/debuff tối đa 1-2 lần cả trận (buffCount ≤ 2), sau đó chỉ damage
Trục 2 — 3 Thing Archetype
⚔️ Attacker
text
Chọn chiêu:
85% → chiêu damage cao nhất (sau filter difficulty)
15% → random trong moves[] còn lại

Di chuyển: theo difficulty bình thường, không ghi đè
Attack target: theo difficulty bình thường, không ghi đè
🛡️ Defender
text
Chọn chiêu:
Xen chiêu buff Def/SpDef sau mỗi 2 lượt liên tiếp damage
Các lượt còn lại → theo difficulty bình thường

Di chuyển:
Ở MỌI difficulty: nâng ưu tiên né terrain + né AoE lên cao hơn mặc định
(EASY/MEDIUM thường bỏ qua AoE nhưng Defender thì không)

Attack target: theo difficulty bình thường, không ghi đè
🔧 Setup
text
Chọn chiêu:
2 lượt đầu: luôn dùng chiêu buff/debuff
Sau đó: theo difficulty bình thường

Attack target (chỉ archetype này ghi đè — với chiêu buff/hỗ trợ):
Chiêu buff Atk/SpAtk:
→ Thing enemy Attacker chưa có buff loại đó → ưu tiên
→ Không có hoặc đã buff rồi → thing enemy bất kỳ chưa có buff
Hồi máu → ưu tiên thing enemy HP thấp nhất
Buff Def/SpDef → ưu tiên thing enemy đang bị player nhắm nhiều nhất
Debuff → nhắm thing player có stat cao nhất tương ứng
Chiêu damage → nhắm theo difficulty logic bình thường

1. TỔNG QUAN DỰ ÁN
Thể loại: Tactical RPG / Grid-based Battler (Nhập vai Chiến thuật trên lưới) kết hợp thu thập quái vật và Auto-chess
.
Lối chơi cốt lõi: Thu thập Pokemon, dàn trận chiến thuật 1vs1 hoặc 2vs2 trên đấu trường lưới chia phe độc lập
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
Quy chuẩn Kích thước: Phân loại Pokemon từ hạng XS (Canvas 40x48px) đến XXL/Boss (Canvas 208x208px) trên đấu trường
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
Giai đoạn 3 (Phán xét): Pokemon có Tốc độ (Speed) cao hơn sẽ tung chiêu trước
.
Giao diện Tối giản (Minimal UI): Không hiện số máu chính xác (trừ PvP) hay Icon Tộc/Hệ trên đầu
. Hiện khung Tinh Thể (Crystal Aura) quanh thanh máu để báo hiệu chất lượng nguyên liệu
.
Hover-Sync Move UI: Rê chuột trên lưới, mũi tên báo hiệu tương khắc (↑ Xanh, ↓ Đỏ) sẽ hiển thị ngay tại bảng tên chiêu thức để giữ sạch đồ họa
. Thẻ chiêu thức được phân loại màu nền theo loại sát thương/buff
.
4. HỆ THỐNG CHỈ SỐ, TỘC & HỆ (STATS, SYNERGY)
6 Chỉ số cơ bản: HP, Attack, Defense, Sp. Atk, Sp. Def, Speed
. Tổng chỉ số cơ bản (BST) ngang nhau ở cùng cấp tiến hóa, chỉ phân bổ khác nhau
.
Tương khắc Hệ (9 Hệ): Hỏa, Mộc, Thủy, Thổ, Lôi, Phong, Băng, Quang, Ám
. Nhận thưởng STAB (x1.2 sát thương) khi dùng chiêu trùng hệ
.
Tộc (9 Tộc): Cơ chế cộng hưởng giống Auto-Chess
.
Tộc Nội Tại: Cơ Khí, Siêu Nhiên, Thú, Cổ Đại, Thiên Thần, Chiến Binh, Huyền Thoại (Kích hoạt vĩnh viễn)
.
Tộc Hiện Diện (Legacy): Khổng Lồ, Pháo Thủ (Để lại hiệu ứng buff/oanh tạc từ 4-5 lượt sau khi rời sân)
.
5. PHÁT TRIỂN SỨC MẠNH & KỸ NĂNG (PROGRESSION)
Lên cấp & Tiến hóa: Max level 100
. Tiến hóa làm tăng lớn tổng chỉ số (BST)
.
Gacha Kỹ Năng: Lên cấp sẽ Gacha ngẫu nhiên chiêu thức mới, có thể lưu vào "Kho lưu trữ" để mua lại sau
.
Độ tinh thông Kỹ năng (Move Mastery): Nâng cấp chiêu thức ngẫu nhiên hệ số từ x0.9 đến x1.1 (tối đa 3 khe). Cần dùng tiền tệ game để "Tẩy luyện" (Reroll) thành x1.1
.
6. KHÁM PHÁ & THU THẬP (OVERWORLD & LOOTING)
Hệ sinh thái Động: Pokemon trốn trong bụi cỏ dạng Bóng đen (Shadows)
. Người chơi có thể đặt Mồi nhử (Bait) để tăng tỷ lệ ra quái hiếm
.
Tương tác ngắm bắn trên Map: Phím A để ném bóng (thu phục), Phím B để bắn đạn (vào trận lấy nguyên liệu)
.
Cơ chế Clean Kill (Đánh giá Sao): Hạ gục 1-3 Hit giữ nguyên 3 Sao. Dùng chiêu khắc hệ, đánh nhiều Hit, hoặc dính Thiêu Đốt/Trúng Độc sẽ làm hỏng nguyên liệu (tụt Sao)
. Hiệu ứng Ngủ/Tê liệt tính là 0 Hit
.
Cơ chế Thu phục (Minigame): Ném bóng có vùng ảnh hưởng 3x3. Trúng tâm 1x1 tỷ lệ bắt cao nhất (Perfect Hit). Máu địch càng thấp tỷ lệ càng cao
. Lệnh ném bóng là Hành động ưu tiên (Giai đoạn 0)
.
7. QUẢN LÝ CỨ ĐIỂM & KINH TẾ (BASE BUILDING)
Resort Sinh Thái: Xây chuồng trại theo Hệ cho Pokemon
.
Kinh tế & Chế tạo: Pokemon ở nhà tạo ra nguyên liệu thụ động. Ghép nguyên liệu và chiến lợi phẩm 3 Sao để chế bóng/mồi nhử xịn
.
Du lịch: Pokemon hiếm tạo Điểm Hấp Dẫn kéo NPC đến tham quan và vứt tiền xu. Có thể thuê NPC tự động hóa thu hoạch
.
8. HỆ THỐNG GUILD, NHIỆM VỤ & THI ĐẤU (GUILD & EXAMINER)
Phân cấp (Rank F đến S): Thăng hạng thông qua tích lũy Quota (Nhiệm vụ) và đánh bại Giám khảo (Examiner)
.
Hệ thống Giám Khảo: Cần đặt lịch hẹn qua Trung tâm Guild, chờ 2-3 ngày in-game
. Người chơi có thể đi thu thập "Tình báo" (Intel) tại quán trọ để biết con bài tẩy, map ưa thích của Giám khảo
.
Phân Khu Guild: Thợ Săn (Chiến đấu), Sinh Thái (Bảo tồn), Kỹ Thuật (Chế tạo). Nhiệm vụ và cách NPC đối xử sẽ thay đổi theo phân khu người chơi chọn
.
Drafting Esports: Thể thức chuyên nghiệp "Mang 6 Chọn 4" ra sân
. Có cơ chế Cấm/Chọn 1 trong 6 loại Bản đồ
.
9. BẢN ĐỒ CHIẾN THUẬT ĐA DẠNG (11 TACTICAL MAPS)
Game có 11 map với cơ chế xoay tua luật lệ đặc thù nhằm ép người chơi thay đổi đội hình
:
Đồng cỏ Tinh Thể: Ăn crystal hồi máu snowball
.
Xưởng Máy Rèn: Ô sắt nổ sát thương diện rộng mỗi 3 lượt
.
Đỉnh Núi Băng Giá: Bão tuyết trừ P.P chiêu thức mỗi 3 lượt
.
Phế Tích Tự Động: Cột đá chắn tầm nhìn xuất hiện/biến mất
.
Đầm Lầy Sương Mù: Mù hoàn toàn (True Blind), sát thương trúng sau 2 lượt mới báo hiệu vị trí
.
Đảo Gió Trời: Gió bẻ cong quỹ đạo đạn đạo
.
Thảo Nguyên Trống: Không vật cản, thuần kỹ năng
.
Đền Thờ Nhật Thực: Giải đố vị trí trên các ô Đen/Trắng để nhận Buff
.
Đấu Trường Tâm Lý: Minigame Kéo-Búa-Bao giành buff Tốc độ
.
Đại Dương Sâu Thẳm: Đứng trên sân quá 4 lượt sẽ bị Ngạt Thở mất máu, ép đổi người
.
Vách Núi Cheo Leo: Chiêu thức gây Đẩy Lùi (Knockback), rớt khỏi rìa chịu sát thương và phải thu hồi
.
10. CỐT TRUYỆN & THẾ GIỚI LORE (STORYLINE)
Khởi đầu (Học viện Tinh thể): Nhập vai học sinh năm cuối. Dùng "Cuốn Sách Phong Ấn" (Grimoire) và các Trang giấy thay vì Pokeball để thu phục
.
Bài kiểm tra: Phục hồi khu Resort, đấu giải vòng bảng hạn chế cấp độ (Soft Level Cap), ép thắng bằng chiến thuật
.
Tốt nghiệp: Thành tích thi đấu Tứ kết -> Chung kết quyết định bằng cấp và nhận Skin Grimoire lung linh tương ứng
. Sau đó gia nhập Guild tham gia thế giới rộng lớn.
Cốt lõi Thế giới: Năng lượng từ Đại tinh thể (Primal Crystals) tạo ra biến thể vùng miền (Regional Variants) và hình thành các Tộc/Hệ. Các thế lực tội phạm muốn khai thác tinh thể tạo ra quái vật cuồng nộ
. Các vùng xuất hiện tinh thể đột biến (Crystal Surge) gia tăng quái hiếm
.

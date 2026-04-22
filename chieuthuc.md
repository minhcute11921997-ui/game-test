Tóm tắt kiến trúc chiêu thức trong dự án:

Cách hoạt động tổng quan
Mọi chiêu thức đều là ScriptableObject MoveData — không có logic nào nằm trong script chiêu. Code đọc thông số từ data rồi tự xử lý. Thêm chiêu mới = tạo file data mới, không sửa code.

5 Hình Dạng AoE hiện có
Single — 1 ô duy nhất, không falloff, damage 100%.

Cross — 4 hướng Đông/Tây/Bắc/Nam cố định từ tâm. Field aoeRadius quyết định đi xa bao nhiêu ô mỗi hướng. aoeRadius = 1 ra 5 ô, aoeRadius = 2 ra 9 ô. Không falloff, tất cả ô nhận damage như nhau.

Square2x2 — 4 ô hình vuông. Có falloff RNG kép: mỗi ô nhận chung × (0.85–1.0) nên tổng dao động 0.765–1.0. Cảm giác "nổ tung" không đều.

Square3x3 — 9 ô hình vuông. Falloff cố định theo khoảng cách từ tâm: tâm ×1.0, 4 ô giữa cạnh ×0.8, 4 ô góc ×0.7.

Line — đường thẳng từ entity đến ô chọn. Tất cả ô trên đường đó bị trúng, không falloff.

Pipeline damage mỗi lượt
text
MoveData.attackShape
    → GetAoECells() lấy danh sách ô
    → BattlePhaseManager tính cellDistanceType từng ô
    → CombatCalculator.Calculate() cho từng ô
    → TakeDamage() trừ HP
Công thức damage
text
BaseDmg = floor(((2×Level/5+2) × Power × Atk/Def) / 10)
Final    = BaseDmg × TypeEff × STAB × RNG × Crit × Falloff
TypeEff: ×2.0 khắc / ×0.5 bị khắc / ×1.0 trung lập

STAB: ×1.5 nếu hệ chiêu = hệ entity

RNG: 0.9–1.0 mỗi lần đánh

Crit: ×1.5, tỉ lệ = 5 + Luck/10 %
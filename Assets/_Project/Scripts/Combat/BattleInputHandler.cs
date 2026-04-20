using UnityEngine;

public class BattleInputHandler : MonoBehaviour
{
    private BattleEntity _selected;
    private bool _isMoving = false;      // đang chạy coroutine di chuyển
    private bool _waitingForMove = false; // đã chọn entity, chờ click ô

    void Update()
    {
        if (_isMoving) return; // chặn input khi đang di chuyển
        if (!Input.GetMouseButtonDown(0)) return;

        var grid = BattleGridManager.Instance;
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;
        GridPos clicked = grid.WorldToGrid(world);

        if (!_waitingForMove)
        {
            // Chưa chọn entity — thử click vào Thing team 0
            BattleEntity entity = grid.GetEntityAt(clicked);
            if (entity != null && entity.TeamId == 0)
            {
                _selected = entity;
                _waitingForMove = true;
                grid.ClearHighlight();
                grid.ShowMovableRange(entity, entity.MoveRange);
            }
        }
        else
        {
            if (grid.IsHighlighted(clicked))
            {
                // Click vào ô hợp lệ → di chuyển
                grid.ClearHighlight();
                _isMoving = true;
                StartCoroutine(grid.MoveEntitySmooth(_selected, clicked, onDone: () =>
                {
                    _isMoving = false;
                    _waitingForMove = false;
                    // Bỏ comment nếu muốn tự show range ở vị trí mới sau khi di chuyển
                    var moved = _selected;
                    grid.ShowMovableRange(moved, moved.MoveRange);
                    _selected = null;
                    
                }));
            }
            else
            {
                // Click chỗ không hợp lệ → hủy chọn
                grid.ClearHighlight();
                _waitingForMove = false;
                _selected = null;
            }
        }
    }
}
using UnityEngine;

public class BattleInputHandler : MonoBehaviour
{
    private BattleEntity _selected;
    private bool _waitingForMove = false;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var grid = BattleGridManager.Instance;
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;
        GridPos clicked = grid.WorldToGrid(world);

        if (!_waitingForMove)
        {
            // Click vào entity team 0 (player) → chọn và show range
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
            // Click vào ô highlight → di chuyển
            if (grid.IsHighlighted(clicked))
            {
                grid.ClearHighlight();
                StartCoroutine(grid.MoveEntitySmooth(_selected, clicked, onDone: () =>
                {
                    _waitingForMove = false;
                    _selected = null;
                }));
            }
            else
            {
                // Click chỗ khác → hủy chọn
                grid.ClearHighlight();
                _waitingForMove = false;
                _selected = null;
            }
        }
    }
}
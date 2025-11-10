using UnityEngine;

public class PileManager : MonoBehaviour
{
    [Tooltip("씬에 소환할 Rigidbody Prefab을 할당하세요.")]
    [SerializeField] private GameObject itemPrefab;

    [Tooltip("총 몇 개의 오브젝트를 소환할지 설정합니다.")]
    [SerializeField] private int pileCount = 10;

    [Tooltip("오브젝트가 소환될 영역의 가로/세로 크기입니다.")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(5f, 5f);

    // ✨ 새로 추가된 Offset 설정 ✨
    [Header("Spawn Offset Settings")]
    [Tooltip("각 오브젝트가 이전 오브젝트 대비 최소한 벌어져야 할 간격입니다.")]
    [SerializeField] private Vector2 minOffset = new Vector2(0.5f, 0.5f);
    
    // ✨ 그리드 소환을 위한 토글 ✨
    [Tooltip("체크하면 minOffset 크기로 일정한 그리드 형태로 소환됩니다.")]
    [SerializeField] private bool useGridLayout = false;

    void Start()
    {
        SpawnItems();
    }

    private void SpawnItems()
    {
        Vector3 centerPosition = transform.position - new Vector3(spawnAreaSize.x, spawnAreaSize.y) * 0.5f;

        // 그리드 소환을 위한 변수
        Vector3 currentOffsetPosition = centerPosition;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(pileCount)); // 대략적인 열 수 계산
        int currentRow = 0;
        int currentCol = 0;

        for (int i = 0; i < pileCount; i++)
        {
            Vector3 spawnPosition;

            if (useGridLayout)
            {
                // 1. 그리드 레이아웃 (Fixed Offset)
                float xOffset = currentCol * minOffset.x;
                float yOffset = currentRow * minOffset.y;
                
                spawnPosition = new Vector3(
                    centerPosition.x + xOffset,
                    centerPosition.y + yOffset,
                    centerPosition.z
                );

                currentCol++;
                if (currentCol >= cols)
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
            else
            {
                // 2. 무작위 소환 (Random Offset)
                // Random.Range를 사용하되, minOffset을 최소 간격처럼 활용
                float randomX = Random.Range(-spawnAreaSize.x / 2f + minOffset.x, spawnAreaSize.x / 2f - minOffset.x);
                float randomY = Random.Range(-spawnAreaSize.y / 2f + minOffset.y, spawnAreaSize.y / 2f - minOffset.y);
                
                // 중심 위치 + 무작위 오프셋
                spawnPosition = new Vector3(
                    centerPosition.x + randomX,
                    centerPosition.y + randomY,
                    centerPosition.z
                );
            }

            // 오브젝트 생성
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"{pileCount}개의 {itemPrefab.name} 오브젝트가 씬에 소환되었습니다.");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));
    }
}
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    // 조작할 Rigidbody를 저장하는 변수
    private Rigidbody2D selectedRigidbody = null;

    // 객체를 드래그하기 위한 오프셋 (Vector2로 명확히)
    private Vector2 dragOffset;
    
    // 우클릭 힘 계산을 위한 마우스 다운 위치 (스크린 좌표)
    private Vector3 rmbDownPosition;

    // 우클릭으로 가해지는 힘의 크기를 조절하는 계수
    [SerializeField] private float throwForce = 500f;

    [SerializeField] private string tagName;

    // 캐싱된 메인 카메라
    private Camera mainCamera;

    private void Start()
    {
        // 카메라 캐싱: 성능 최적화
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is missing the 'MainCamera' tag!");
        }
    }

    void Update()
    {
        // 좌클릭 드래그 중인 경우, Raycast를 수행하지 않고 드래그 로직만 처리
        if (selectedRigidbody != null && Input.GetMouseButton(0))
        {
            HandleDragMove();
        }
        else
        {
            // 클릭 다운 시점에만 객체 선택 시도
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                TrySelectRigidbody();
            }
        }
        
        // 우클릭 Up 시점에 던지기 처리
        if (Input.GetMouseButtonUp(1))
        {
            HandleRightClickThrow();
        }
        
        // 좌클릭 Up 시점에 해제 처리
        if (Input.GetMouseButtonUp(0))
        {
            HandleLeftClickRelease();
        }
    }

    // --- Rigidbody 선택 로직 ---
    private void TrySelectRigidbody()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // LayerMask를 사용한 2D Raycast: 특정 레이어의 객체만 검출 (성능 및 정확도 향상)
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0.1f);

        if (hit.collider != null)
        {
            if (hit.transform.tag != tagName) return;

            selectedRigidbody = hit.collider.GetComponent<Rigidbody2D>();
            
            if (selectedRigidbody != null)
            {
                // 오프셋 계산
                dragOffset = (Vector2)selectedRigidbody.transform.position - hit.point;

                // 우클릭이라면 다운 위치 기록
                if (Input.GetMouseButtonDown(1))
                {
                    rmbDownPosition = Input.mousePosition;
                    // 우클릭 선택 후 바로 힘을 가할 준비를 위해 Kinematic은 설정하지 않음
                }
                
                // 좌클릭이라면 드래그를 시작할 준비
                if (Input.GetMouseButtonDown(0))
                {
                    // 드래그 시작 시 Kinematic 설정
                    selectedRigidbody.bodyType = RigidbodyType2D.Kinematic; 
                }
            }
        }
    }

    // --- 좌클릭 드래그 이동 로직 ---
    private void HandleDragMove()
    {
        // Rigidbody가 null이 아닌 것은 이미 Updtae()에서 확인됨
        
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // MovePosition은 물리 업데이트에서 호출하는 것이 좋지만, 
        // 사용자 입력 반응을 위해 Update에서 호출하고 Rigidbody를 Kinematic으로 제어합니다.
        selectedRigidbody.MovePosition(mouseWorldPosition + dragOffset);
    }
    
    // --- 좌클릭 해제 로직 ---
    private void HandleLeftClickRelease()
    {
        if (selectedRigidbody != null)
        {
            // Kinematic 해제 및 변수 초기화
            selectedRigidbody.bodyType = RigidbodyType2D.Dynamic;
            selectedRigidbody = null;
        }
    }


    // --- 우클릭 던지기 로직 ---
    private void HandleRightClickThrow()
    {
        // 우클릭 Up 시점에 selectedRigidbody가 선택된 상태였는지 확인
        if (selectedRigidbody != null)
        {
            // Down 위치에서 Up 위치로의 벡터 계산 (스크린 좌표)
            Vector3 rmbUpPosition = Input.mousePosition;
            Vector3 dragVector = rmbUpPosition - rmbDownPosition;
            
            // 힘을 가하기 전에 Kinematic이 아닌지 확인
            if (selectedRigidbody.bodyType != RigidbodyType2D.Kinematic)
            {
                // 스크린 좌표의 차이 벡터를 월드 좌표계의 힘 벡터로 변환하여 AddForce에 사용
                // 벡터의 크기와 방향을 기반으로 힘을 가합니다.
                Vector2 forceDirection = new Vector2(dragVector.x, dragVector.y);

                // ForceMode2D.Impulse를 사용해 순간적인 힘을 가합니다.
                selectedRigidbody.AddForce(forceDirection * throwForce, ForceMode2D.Impulse);
            }
            
            // 해제 및 변수 초기화
            selectedRigidbody = null;
        }
    }
}
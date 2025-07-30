using UnityEngine;

namespace TacticsArena
{
    /// <summary>
    /// Base class cho tất cả entities trong game TacticsArena
    /// Có thể extend cho Champions, Buildings, Effects, etc.
    /// </summary>
    public class TEntity : MonoBehaviour
    {
        [Header("Entity Info")]
        public string entityName = "Entity";
        public int entityID;
        
        [Header("Transform")]
        public Vector3 targetPosition;
        public bool isMoving = false;
        
        [Header("State")]
        public bool isActive = true;
        public bool isInteractable = true;
        
        protected virtual void Start()
        {
            Initialize();
        }
        
        protected virtual void Update()
        {
            if (isMoving)
            {
                HandleMovement();
            }
        }
        
        protected virtual void Initialize()
        {
            // Override trong subclasses
            targetPosition = transform.position;
        }
        
        protected virtual void HandleMovement()
        {
            float moveSpeed = 5f;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                isMoving = false;
                OnMovementComplete();
            }
        }
        
        protected virtual void OnMovementComplete()
        {
            // Override trong subclasses
        }
        
        public virtual void MoveTo(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;
        }
        
        public virtual void SetActive(bool active)
        {
            isActive = active;
            gameObject.SetActive(active);
        }
        
        protected virtual void OnMouseDown()
        {
            if (isInteractable)
            {
                OnEntityClicked();
            }
        }
        
        protected virtual void OnEntityClicked()
        {
            Debug.Log($"{entityName} clicked!");
        }
    }
}

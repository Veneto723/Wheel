namespace Runtime.ObjectPool {
    public class ObjectPool<T> {
        public bool Used { get; private set; }
        public T Item { get; set; }

        public void Consume() {
            Used = true;
        }
        
        public void Release() {
            Used = false;
        }
    }
}
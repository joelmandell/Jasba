window.offlineStorage = {
  db: null,
  
  async init() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open('SBAProDB', 1);
      
      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };
      
      request.onupgradeneeded = (event) => {
        const db = event.target.result;
        
        if (!db.objectStoreNames.contains('inspections')) {
          db.createObjectStore('inspections', { keyPath: 'id' });
        }
      };
    });
  },
  
  async saveInspection(inspection) {
    const transaction = this.db.transaction(['inspections'], 'readwrite');
    const store = transaction.objectStore('inspections');
    return store.put(inspection);
  },
  
  async getInspections() {
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['inspections'], 'readonly');
      const store = transaction.objectStore('inspections');
      const request = store.getAll();
      
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }
};

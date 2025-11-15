// SBA Pro Offline Storage Module
// Provides IndexedDB wrapper for offline inspection data storage

window.offlineStorage = {
  db: null,
  dbName: 'SBAProDB',
  dbVersion: 1,

  // Initialize the database
  async init() {
    return new Promise((resolve, reject) => {
      console.log('[OfflineStorage] Initializing IndexedDB...');
      const request = indexedDB.open(this.dbName, this.dbVersion);

      request.onerror = () => {
        console.error('[OfflineStorage] Database error:', request.error);
        reject(request.error);
      };

      request.onsuccess = () => {
        this.db = request.result;
        console.log('[OfflineStorage] Database opened successfully');
        resolve();
      };

      request.onupgradeneeded = (event) => {
        console.log('[OfflineStorage] Upgrading database...');
        const db = event.target.result;

        // Store for pending inspection saves
        if (!db.objectStoreNames.contains('pending')) {
          const pendingStore = db.createObjectStore('pending', { keyPath: 'id', autoIncrement: true });
          pendingStore.createIndex('timestamp', 'timestamp', { unique: false });
          pendingStore.createIndex('roundId', 'roundId', { unique: false });
          console.log('[OfflineStorage] Created pending store');
        }

        // Store for cached inspection data
        if (!db.objectStoreNames.contains('inspections')) {
          const inspectionsStore = db.createObjectStore('inspections', { keyPath: 'id' });
          inspectionsStore.createIndex('roundId', 'roundId', { unique: false });
          inspectionsStore.createIndex('timestamp', 'timestamp', { unique: false });
          console.log('[OfflineStorage] Created inspections store');
        }

        // Store for cached round data
        if (!db.objectStoreNames.contains('rounds')) {
          const roundsStore = db.createObjectStore('rounds', { keyPath: 'id' });
          roundsStore.createIndex('timestamp', 'timestamp', { unique: false });
          console.log('[OfflineStorage] Created rounds store');
        }
      };
    });
  },

  // Save inspection result to pending queue
  async savePendingInspection(roundId, objectId, inspectionData) {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['pending'], 'readwrite');
      const store = transaction.objectStore('pending');

      const item = {
        roundId: roundId,
        objectId: objectId,
        data: inspectionData,
        timestamp: new Date().toISOString()
      };

      const request = store.add(item);

      request.onsuccess = () => {
        console.log('[OfflineStorage] Saved pending inspection:', request.result);
        resolve(request.result);
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error saving pending inspection:', request.error);
        reject(request.error);
      };
    });
  },

  // Get all pending inspections
  async getPendingInspections() {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['pending'], 'readonly');
      const store = transaction.objectStore('pending');
      const request = store.getAll();

      request.onsuccess = () => {
        console.log('[OfflineStorage] Retrieved pending inspections:', request.result.length);
        resolve(request.result);
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error getting pending inspections:', request.error);
        reject(request.error);
      };
    });
  },

  // Get pending inspections count
  async getPendingCount() {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['pending'], 'readonly');
      const store = transaction.objectStore('pending');
      const request = store.count();

      request.onsuccess = () => {
        resolve(request.result);
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error counting pending inspections:', request.error);
        reject(request.error);
      };
    });
  },

  // Delete a pending inspection
  async deletePendingInspection(id) {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['pending'], 'readwrite');
      const store = transaction.objectStore('pending');
      const request = store.delete(id);

      request.onsuccess = () => {
        console.log('[OfflineStorage] Deleted pending inspection:', id);
        resolve();
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error deleting pending inspection:', request.error);
        reject(request.error);
      };
    });
  },

  // Clear all pending inspections
  async clearPending() {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['pending'], 'readwrite');
      const store = transaction.objectStore('pending');
      const request = store.clear();

      request.onsuccess = () => {
        console.log('[OfflineStorage] Cleared all pending inspections');
        resolve();
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error clearing pending:', request.error);
        reject(request.error);
      };
    });
  },

  // Cache inspection round data
  async cacheRound(roundId, roundData) {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['rounds'], 'readwrite');
      const store = transaction.objectStore('rounds');

      const item = {
        id: roundId,
        data: roundData,
        timestamp: new Date().toISOString()
      };

      const request = store.put(item);

      request.onsuccess = () => {
        console.log('[OfflineStorage] Cached round:', roundId);
        resolve();
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error caching round:', request.error);
        reject(request.error);
      };
    });
  },

  // Get cached round data
  async getCachedRound(roundId) {
    if (!this.db) {
      await this.init();
    }

    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(['rounds'], 'readonly');
      const store = transaction.objectStore('rounds');
      const request = store.get(roundId);

      request.onsuccess = () => {
        if (request.result) {
          console.log('[OfflineStorage] Retrieved cached round:', roundId);
          resolve(request.result.data);
        } else {
          resolve(null);
        }
      };

      request.onerror = () => {
        console.error('[OfflineStorage] Error getting cached round:', request.error);
        reject(request.error);
      };
    });
  },

  // Check if online
  isOnline() {
    return navigator.onLine;
  },

  // Legacy compatibility - save inspection
  async saveInspection(inspection) {
    const transaction = this.db.transaction(['inspections'], 'readwrite');
    const store = transaction.objectStore('inspections');
    return store.put(inspection);
  },

  // Legacy compatibility - get inspections
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

// Initialize on load
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    window.offlineStorage.init().catch(err => {
      console.error('[OfflineStorage] Failed to initialize:', err);
    });
  });
} else {
  window.offlineStorage.init().catch(err => {
    console.error('[OfflineStorage] Failed to initialize:', err);
  });
}

console.log('[OfflineStorage] Module loaded');

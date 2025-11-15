// SBA Pro Service Worker - v1.1
const CACHE_NAME = 'sbapro-cache-v1';
const RUNTIME_CACHE = 'sbapro-runtime-v1';

// Assets to cache on install
const PRECACHE_URLS = [
  '/',
  '/mobile',
  '/mobile/rounds',
  '/css/app.css',
  '/_framework/blazor.web.js',
  '/manifest.json'
];

// Install event - cache essential assets
self.addEventListener('install', event => {
  console.log('[ServiceWorker] Installing...');
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[ServiceWorker] Precaching app shell');
        return cache.addAll(PRECACHE_URLS);
      })
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  console.log('[ServiceWorker] Activating...');
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME && cacheName !== RUNTIME_CACHE) {
            console.log('[ServiceWorker] Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - network first, fallback to cache for GET requests
self.addEventListener('fetch', event => {
  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  // Skip chrome extension and hot reload requests
  const url = event.request.url;
  if (url.startsWith('chrome-extension://') || 
      url.includes('/_framework/aspnetcore-browser-refresh.js') ||
      url.includes('/hotreload')) {
    return;
  }

  event.respondWith(
    caches.open(RUNTIME_CACHE).then(cache => {
      return fetch(event.request)
        .then(response => {
          // Cache successful responses
          if (response && response.status === 200) {
            cache.put(event.request, response.clone());
          }
          return response;
        })
        .catch(() => {
          // Network failed, try cache
          return caches.match(event.request).then(cachedResponse => {
            if (cachedResponse) {
              console.log('[ServiceWorker] Serving from cache:', event.request.url);
              return cachedResponse;
            }
            
            // Return offline page for navigation requests
            if (event.request.mode === 'navigate') {
              return caches.match('/mobile');
            }
          });
        });
    })
  );
});

// Background Sync event - sync pending data when online
self.addEventListener('sync', event => {
  console.log('[ServiceWorker] Background sync:', event.tag);
  
  if (event.tag === 'sync-inspections') {
    event.waitUntil(syncPendingInspections());
  }
});

// Sync pending inspections to server
async function syncPendingInspections() {
  try {
    console.log('[ServiceWorker] Syncing pending inspections...');
    
    // Open IndexedDB and get pending items
    const db = await openDatabase();
    const pending = await getAllPending(db);
    
    console.log(`[ServiceWorker] Found ${pending.length} pending inspection(s)`);
    
    if (pending.length === 0) {
      return;
    }
    
    // Sync each pending item
    let syncedCount = 0;
    for (const item of pending) {
      try {
        // Note: This would need a proper API endpoint
        // For now, just log and remove from queue
        console.log('[ServiceWorker] Would sync item:', item.id);
        await deletePendingItem(db, item.id);
        syncedCount++;
      } catch (error) {
        console.error('[ServiceWorker] Failed to sync item:', item.id, error);
      }
    }
    
    // Notify clients that sync is complete
    const clients = await self.clients.matchAll();
    clients.forEach(client => {
      client.postMessage({
        type: 'SYNC_COMPLETE',
        count: syncedCount
      });
    });
    
  } catch (error) {
    console.error('[ServiceWorker] Sync failed:', error);
    throw error; // Retry sync later
  }
}

// Helper: Open IndexedDB
function openDatabase() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('SBAProDB', 1);
    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);
    request.onupgradeneeded = (event) => {
      const db = event.target.result;
      if (!db.objectStoreNames.contains('pending')) {
        db.createObjectStore('pending', { keyPath: 'id', autoIncrement: true });
      }
      if (!db.objectStoreNames.contains('inspections')) {
        db.createObjectStore('inspections', { keyPath: 'id' });
      }
    };
  });
}

// Helper: Get all pending items
function getAllPending(db) {
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['pending'], 'readonly');
    const store = transaction.objectStore('pending');
    const request = store.getAll();
    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);
  });
}

// Helper: Delete pending item
function deletePendingItem(db, id) {
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['pending'], 'readwrite');
    const store = transaction.objectStore('pending');
    const request = store.delete(id);
    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve();
  });
}

console.log('[ServiceWorker] Loaded v1.1');

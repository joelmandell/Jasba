# 05-Advanced-Features: Offline Capability

## Objective
Implement offline-first inspection execution using Service Workers and IndexedDB.

## Prerequisites
- Completed: Phase 4 (WebApp Layer)
- Understanding of Service Workers
- Understanding of Progressive Web Apps (PWA)

## Overview

Offline capability allows inspectors to:
- Execute inspections without internet connection
- Store inspection data locally
- Automatically sync when connection restored
- Continue working in remote locations

## Instructions

### 1. Configure PWA Support

**File**: `src/SBAPro.WebApp/wwwroot/manifest.json`

```json
{
  "name": "SBA Pro",
  "short_name": "SBAPro",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#3b82f6",
  "icons": [
    {
      "src": "icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```

### 2. Create Service Worker

**File**: `src/SBAPro.WebApp/wwwroot/service-worker.js`

```javascript
const CACHE_NAME = 'sbapro-v1';
const urlsToCache = [
  '/',
  '/css/app.css',
  '/js/site.js',
  '/_framework/blazor.web.js'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => response || fetch(event.request))
  );
});
```

### 3. Implement Local Storage

Use IndexedDB for storing inspection data:

```javascript
// wwwroot/js/offline-storage.js
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
    const transaction = this.db.transaction(['inspections'], 'readonly');
    const store = transaction.objectStore('inspections');
    return store.getAll();
  }
};
```

### 4. Sync When Online

```javascript
// Background sync
self.addEventListener('sync', event => {
  if (event.tag === 'sync-inspections') {
    event.waitUntil(syncInspections());
  }
});

async function syncInspections() {
  // Get pending inspections from IndexedDB
  // POST to server
  // Clear local storage on success
}
```

## Validation

1. Start inspection round
2. Turn off network connection
3. Complete inspection offline
4. Verify data stored locally
5. Turn on network connection
6. Verify data syncs to server

## Success Criteria

✅ Service worker installs correctly  
✅ Can execute inspections offline  
✅ Data stored in IndexedDB  
✅ Background sync works  
✅ Data integrity maintained  

## Next Steps

Proceed to 02-email-reminders.md

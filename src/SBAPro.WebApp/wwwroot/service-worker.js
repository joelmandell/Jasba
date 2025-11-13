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

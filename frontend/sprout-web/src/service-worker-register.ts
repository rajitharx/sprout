export function registerServiceWorker() {
  if (!('serviceWorker' in navigator)) {
    console.log('Service Worker not supported');
    return;
  }

  window.addEventListener('load', async () => {
    try {
      const registration = await navigator.serviceWorker.register('/service-worker.js', {
        scope: '/',
      });
      console.log('Service Worker registered:', registration);

      registration.addEventListener('updatefound', () => {
        const newWorker = registration.installing;
        if (newWorker) {
          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'activated') {
              console.log('New Service Worker activated');
              // Optionally notify user of update
            }
          });
        }
      });
    } catch (error) {
      console.log('Service Worker registration failed:', error);
    }
  });
}

export function unregisterServiceWorker() {
  if ('serviceWorker' in navigator) {
    navigator.serviceWorker.getRegistrations().then((registrations) => {
      registrations.forEach((registration) => registration.unregister());
    });
  }
}

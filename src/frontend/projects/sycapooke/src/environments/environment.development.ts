/** Development: ng serve proxies /api to Sycapooke.Api (projects/sycapooke/proxy.conf.json), so cookies stay same-origin. */
export const environment = {
  apiBaseUrl: '/api',
  analytics: { scriptUrl: 'https://analytics.sycapooke.invalid/script.js', cookieNames: ['_sya'] },
};

/** The marketing build: absolute links go to the application at app.sycapooke.com (ADR platform/0017). */
export const environment = {
  apiBaseUrl: '/api',
  appOrigin: 'https://app.sycapooke.com',
  /** The analytics provider is not chosen yet (ADR privacy/0001): this origin never resolves. */
  analytics: { scriptUrl: 'https://analytics.sycapooke.invalid/script.js', cookieNames: ['_sya'] },
};

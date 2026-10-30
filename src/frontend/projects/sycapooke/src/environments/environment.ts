export const environment = {
  apiBaseUrl: '/api',
  /** Empty: links stay inside this build. The marketing build points this at the app host. */
  appOrigin: '',
  /** The analytics provider is not chosen yet (ADR privacy/0001): this origin never resolves. */
  analytics: { scriptUrl: 'https://analytics.sycapooke.invalid/script.js', cookieNames: ['_sya'] },
};

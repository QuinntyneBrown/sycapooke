/** The marketing build (www.sycapooke.com): links into the application go to the app host (ADR platform/0015). */
export const environment = {
  apiBaseUrl: '/api',
  appOrigin: 'https://app.sycapooke.com',
  /** The analytics provider is not chosen yet (ADR privacy/0001): this origin never resolves. */
  analytics: { scriptUrl: 'https://analytics.sycapooke.invalid/script.js', cookieNames: ['_sya'] },
};

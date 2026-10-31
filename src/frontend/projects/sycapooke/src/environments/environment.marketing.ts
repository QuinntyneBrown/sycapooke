/** The Azure marketing build: absolute links leave /welcome/ for the application (ADR platform/0016). */
export const environment = {
  apiBaseUrl: '/api',
  appOrigin: 'https://sycapooke.canadacentral.cloudapp.azure.com',
  /** The analytics provider is not chosen yet (ADR privacy/0001): this origin never resolves. */
  analytics: { scriptUrl: 'https://analytics.sycapooke.invalid/script.js', cookieNames: ['_sya'] },
};

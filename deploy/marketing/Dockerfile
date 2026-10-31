# Builds the marketing-only Angular build (/ is the marketing page) and serves it with Caddy on port 80 behind
# the edge proxy. Deployed on its own by .github/workflows/deploy-marketing.yml (ADR platform/0015).
# Build context: the repository root.
FROM node:22-alpine AS build
WORKDIR /src
COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci
COPY src/frontend/ .
RUN npm run build:marketing -- --base-href /welcome/

FROM caddy:2-alpine
COPY deploy/marketing/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /src/dist/sycapooke/browser /srv/marketing
# The same files at / for sycapooke.com and www.sycapooke.com (ADR platform/0017): only the base href differs.
RUN cp -r /srv/marketing /srv/marketing-root \
 && sed -i 's|<base href="/welcome/">|<base href="/">|' /srv/marketing-root/*.html \
 && grep -q '<base href="/">' /srv/marketing-root/index.html
EXPOSE 80 8080

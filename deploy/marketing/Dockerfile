# Builds the marketing-only Angular build (/ is the marketing page) and serves it with Caddy on port 80 behind
# the edge proxy. Deployed on its own by .github/workflows/deploy-marketing.yml (ADR platform/0015).
# Build context: the repository root.
FROM node:22-alpine AS build
WORKDIR /src
COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci
COPY src/frontend/ .
RUN npm run build:marketing

FROM caddy:2-alpine
COPY deploy/marketing/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /src/dist/sycapooke/browser /srv/marketing
EXPOSE 80

# Builds the full Angular application (production configuration) and serves it with Caddy on port 80 behind the
# edge proxy, proxying /api and /photos to the API. Deployed with the backend by
# .github/workflows/deploy-app.yml (ADR platform/0015). Build context: the repository root.
FROM node:22-alpine AS build
WORKDIR /src
COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci
COPY src/frontend/ .
RUN npm run build:all

FROM caddy:2-alpine
COPY deploy/app/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /src/dist/sycapooke/browser /srv/app
EXPOSE 80

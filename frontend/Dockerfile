# Stage 1 — build Vite app
FROM node:22-alpine AS build
WORKDIR /app

# VITE_API_BASE_URL is baked into the JS bundle at build time.
# The browser calls the API from the host, not from inside Docker,
# so we point to the host-exposed API port (8080) rather than api:8080.
ARG VITE_API_BASE_URL=http://localhost:8080
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Stage 2 — serve with nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

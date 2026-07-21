# ---- Stage 1: build the Vite production bundle ----
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies against the lockfile for reproducible builds.
COPY package*.json ./
RUN npm ci

# Build. VITE_API_BASE_URL is baked in at build time; override per environment:
#   docker build --build-arg VITE_API_BASE_URL=https://your-api/api .
ARG VITE_API_BASE_URL=http://localhost:5238/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
COPY . .
RUN npm run build

# ---- Stage 2: serve static files with nginx ----
FROM nginx:alpine AS serve
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

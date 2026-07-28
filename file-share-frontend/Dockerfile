# --- STAGE 1: Build the Vue App ---
FROM node:22 AS build-stage
WORKDIR /app

ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Copy package files and install dependencies
COPY package*.json ./
RUN npm install

# Copy the Vue source code and build for production
COPY . .
RUN npm run build

# --- STAGE 2: Serve with Nginx ---
FROM nginx:stable-alpine AS production-stage

COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

ENV RESOLVER="127.0.0.11"
ENV BACKEND_URL="http://api:8080"

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
